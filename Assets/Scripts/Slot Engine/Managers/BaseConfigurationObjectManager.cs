//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BoomSports.Prototype.Managers
{
    /// <summary>
    /// Base Configuration Object Class - Strip Configuration (Spin grid in a strip approach) - Grid Configuration (WYSIWYG, Independant Reels)- 
    /// </summary>
    public class BaseConfigurationObjectManager : MonoBehaviour
    {
        /// <summary>
        /// Holds the reference to all managers
        /// </summary>
        public ManagersReferenceScript managers
        {
            get
            {
                if (_managers == null)
                    _managers = GameObject.FindObjectOfType<ManagersReferenceScript>();
                return _managers;
            }
        }
        [SerializeField]
        internal ManagersReferenceScript _managers;
        /// <summary>
        /// Configuration Group Managers that make up the matrix - Generated with ConfigurationGenerator Script - Order determines strip spin delay and Symbol Evaluation Logic
        /// </summary>
        [SerializeField]
        internal BaseObjectGroupManager[] configurationGroupManagers;
        /// <summary>
        /// Symbol information for the matrix - Loaded from each game folder
        /// </summary>
        public SymbolScriptableObject symbolDataScriptableObject;
        /// <summary>
        /// Holds reference for the symbol weights prefab and other utility prefabs
        /// </summary>
        public CorePrefabsReferencesScriptableObject corePrefabs;
        /// <summary>
        /// Sets the symbol weights via Symbol Data by state
        /// </summary>
        [SerializeField]
        public ModeWeights[] symbolWeightsByState;
        /// <summary>
        /// Configuration settings used to build ConfigurationObject;
        /// </summary>
        [SerializeField]
        public ConfigurationSettingsScriptableObject configurationSettings;
        /// <summary>
        /// List of wining and losing slots
        /// </summary>
        [SerializeField]
        internal List<BaseObjectManager> winning_slots, losing_slots;
        internal string[] supportedSymbols
        {
            get
            {
                string[] symbols = new string[symbolDataScriptableObject.symbols.Length];
                for (int symbol = 0; symbol < symbols.Length; symbol++)
                {
                    symbols[symbol] = symbolDataScriptableObject.symbols[symbol].symbolName;
                }
                return symbols;
            }
        }

        public async virtual void Start()
        {
            //Initialize game mode
            StaticStateManager.SetGameModeActiveTo(GameModes.baseGame);
            await CheckSymbolWeightsWork();
            //On Play editor referenced state machines loos reference. Temp Solution to build on game start. TODO find way to store info between play and edit mode - Has to do with prefabs
            InitializeStateMachine();
            //Initialize Machine and Player  Information
            managers.machineInfoManager.InitializeTestMachineValues(10000.0f, 0.0f, managers.machineInfoManager.machineInfoScriptableObject.supported_bet_amounts.Length / 2 - 1, 0, 0);
            //Debug.Log(String.Format("Initial pop of end_configiuration_manager = {0}", print_string));
            //This is temporary - we need to initialize the slot engine in a different scene then when preloading is done swithc to demo_attract.
            SyncCurrentSymbolDisplayedToPresentationID();
            managers.endConfigurationManager.ClearConfigurations();
            await managers.endConfigurationManager.GenerateMultipleDisplayConfigurations(GameModes.baseGame,20);

            StaticStateManager.SetStateTo(States.Idle_Intro);
        }
        /// <summary>
        /// Use Next Configuration from endConfigurationManager
        /// </summary>
        /// <returns></returns>
        internal async Task SpinStartGroupManagers()
        {
            //The end reel configuration is set when spin starts to the next item in the list
            DisplayConfigurationContainer nextConfiguration = managers.endConfigurationManager.UseNextConfigurationInList();
            //Evaluation is ran over those symbols and if there is a bonus trigger the matrix will be set into display bonus state
            //managers.evaluationManager.EvaluateWinningSymbolsFromCurrentConfiguration();

            await SpinStartGroupManagers(nextConfiguration);
        }
        /// <summary>
        /// Start the group managers spin and set to Configuraiton
        /// </summary>
        /// <param name="displayConfigurationToUse">Configuration to set the objects to display</param>
        /// <returns></returns>
        internal async Task SpinStartGroupManagers(DisplayConfigurationContainer displayConfigurationToUse)
        {
            int[] spinObjectsOrder = managers.spinManager.baseSpinSettingsScriptableObject.GetStopObjectOrder<BaseObjectGroupManager>(ref managers.configurationObject.configurationGroupManagers);
            //Set the end configuraiton manager current configuraiton in use to displayConfiguration
            managers.endConfigurationManager._displayConfigurationInUse = displayConfigurationToUse;
            //Debug.Log($"Spinning objects in order {String.Join("|", spinObjectsOrder)}");
            //Spin the reels - if there is a delay between reels then wait delay amount
            for (int i = 0; i < spinObjectsOrder.Length; i++)
            {
                //Each group will Start their spin and evaluate based on spin type. Slots need to evaluate incoming symbol for events or outgoing symbol events
                await configurationGroupManagers[spinObjectsOrder[i]].StartSpin();
            }
        }

        internal async Task SpinStopGroupManagers()
        {
            //Get the end display configuration and set per reel
            DisplayConfigurationContainer displayConfiguration = EndConfigurationManager.displayConfigurationInUse;

            //Get Order of stopping configuration objects from baseSpinSettingsScriptableObject - Independant reels will have to be taken into consideration
            int[] orderStopObjects = managers.spinManager.baseSpinSettingsScriptableObject.GetStopObjectOrder<BaseObjectGroupManager>(ref managers.configurationObject.configurationGroupManagers);
            //Determine whether to stop reels forwards or backwards.
            for (int i = 0; i < orderStopObjects.Length; i++)
            {
                //If reel strip delays are enabled wait between strips to stop
                if (managers.spinManager.baseSpinSettingsScriptableObject.delayStartEnabled)
                {
                    await configurationGroupManagers[orderStopObjects[i]].StopReel();
                }
                else
                {
                    configurationGroupManagers[orderStopObjects[i]].StopReel();
                }
            }
            //Wait for all reels to be in spin.end state before continuing
            await WaitForGroupManagersToStopSpin(configurationGroupManagers);
            //ensure symbols have proper id's set
            SyncCurrentSymbolDisplayedToPresentationID();
            //Evaluate the reels - 
            managers.evaluationManager.EvaluateWinningSymbolsFromCurrentConfiguration();
        }

        internal void SyncCurrentSymbolDisplayedToPresentationID()
        {
            for (int i = 0; i < configurationGroupManagers.Length; i++)
            {
                configurationGroupManagers[i].SyncDisplaySymbolInformation();
            }
        }

        internal int GetPaddingBeforeStrip(int stripColumn)
        {
            return configurationSettings.displayZones[stripColumn].paddingBefore;
        }

        private async Task WaitForGroupManagersToStopSpin(BaseObjectGroupManager[] groupManagers)
        {
            bool lock_task = true;
            while (lock_task)
            {
                for (int i = 0; i < groupManagers.Length; i++)
                {
                    if (groupManagers[i].currentSpinState == SpinStates.spin_end)
                    {
                        if (i == groupManagers.Length - 1)
                        {
                            lock_task = false;
                            break;
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                        break;
                    }
                }
            }
        }

        private void InitializeStateMachine()
        {
            SetAllSlotAnimatorSyncStates();
            SetSubStatesAllSlotAnimatorStateMachines();
            SetManagerStateMachineSubStates();
        }

        internal void SetAllSlotAnimatorSyncStates()
        {
            for (int group = 0; group < configurationGroupManagers.Length; group++)
            {
                configurationGroupManagers[group].SetAllSlotContainersAnimatorSyncStates();
            }
        }

        internal void SetSubStatesAllSlotAnimatorStateMachines()
        {
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                configurationGroupManagers[reel].SetAllSlotContainersSubAnimatorStates();
            }
        }

        internal void SetManagerStateMachineSubStates()
        {
            string[] keys = GatherKeysFromSubStates();
            AnimatorSubStateMachine[] values = GatherValuesFromSubStates();
            _managers.animatorStateMachineMaster.SetSubStateMachinesTo(keys, values);
        }

        private string[] GatherKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                keys.AddRange(configurationGroupManagers[reel].ReturnAllKeysFromSubStates());
            }
            return keys.ToArray();
        }
        private AnimatorSubStateMachine[] GatherValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                values.AddRange(configurationGroupManagers[reel].ReturnAllValuesFromSubStates());
            }
            return values.ToArray();
        }

        private async Task CheckSymbolWeightsWork()
        {
            int symbol_weight_pass_check = -1;
            try
            {
                symbol_weight_pass_check = DrawRandomSymbolFromCurrentMode();
            }
            catch
            {
                await SetSymbolWeightsByState();
                symbol_weight_pass_check = DrawRandomSymbolFromCurrentMode();
                Debug.Log("Weights are in");
            }
        }

        internal async Task SetSymbolWeightsByState()
        {
            symbol_weight_state temp;
            Dictionary<GameModes, List<float>> symboWeightsByState = new Dictionary<GameModes, List<float>>();
            for (int symbol = 0; symbol < symbolDataScriptableObject.symbols.Length; symbol++)
            {
                for (int weight_state = 0; weight_state < symbolDataScriptableObject.symbols[symbol].symbolWeights.Length; weight_state++)
                {
                    temp = symbolDataScriptableObject.symbols[symbol].symbolWeights[weight_state];
                    if (!symboWeightsByState.ContainsKey(temp.gameState))
                    {
                        symboWeightsByState[temp.gameState] = new List<float>();
                    }
                    symboWeightsByState[temp.gameState].Add(temp.symbolWeightInfo);
                }
            }
            await AddSymbolStateWeightByDict(symboWeightsByState);
        }

        internal int GetIndexOfGroupManager(BaseObjectGroupManager baseObjectGroupManager)
        {
            for (int i = 0; i < configurationGroupManagers.Length; i++)
            {
                if(configurationGroupManagers[i] == baseObjectGroupManager)
                {
                    return i;
                }
            }
            Debug.Log($"object group manager {baseObjectGroupManager.gameObject.name} not in configuration object group managers array. Returning -1 for index");
            return -1;
        }

        private async Task AddSymbolStateWeightByDict(Dictionary<GameModes, List<float>> symbol_weight_state)
        {
            symbolWeightsByState = new ModeWeights[symbol_weight_state.Keys.Count];
            int counter = -1;
            ModeWeights temp;
            WeightsForMode temp2;
            WeightsDistributionScriptableObject temp4;
            foreach (KeyValuePair<GameModes, List<float>> item in symbol_weight_state)
            {
                counter += 1;
                temp = new ModeWeights();
                temp.gameMode = item.Key;
                temp2 = new WeightsForMode();
                temp2.symbolWeights = item.Value;
                temp4 = LoadFromResourcesWeights(item.Key);
                if (temp4.intDistribution.Items.Count > 0)
                    temp4.intDistribution.ClearItems();
                for (int weight = 0; weight < temp2.symbolWeights.Count; weight++)
                {
                    temp4.intDistribution.Add(weight, temp2.symbolWeights[weight]);
                }
                for (int weight = 0; weight < temp4.intDistribution.Items.Count; weight++)
                {
                    temp4.intDistribution.Items[weight].Weight = temp2.symbolWeights[weight];
                }
                temp2.weightDistributionScriptableObject = temp4;
                temp.weightsForModeDistribution = temp2;
                symbolWeightsByState[counter] = temp;
            }
        }
        /// <summary>
        /// Loads the weights scriptable object from resources folder
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private WeightsDistributionScriptableObject LoadFromResourcesWeights(GameModes mode)
        {
            return Resources.Load($"Core/ScriptableObjects/WeightObjects/{mode}") as WeightsDistributionScriptableObject;
        }

        private WeightsDistributionScriptableObject FindDistributionFromResources(GameModes key)
        {
            Debug.Log(String.Format("Loading Resources/Core/ScriptableObjects/Weights/{0}", key.ToString()));
            return Resources.Load(String.Format("Core/ScriptableObjects/Weights/{0}", key.ToString())) as WeightsDistributionScriptableObject;
        }

        /// <summary>
        /// Draws a random symbol based on weights of current mode
        /// </summary>
        /// <returns>symbol int</returns>
        internal async Task<int> DrawRandomSymbol()
        {
            if (Application.isPlaying)
                return await DrawRandomSymbol(StaticStateManager.enCurrentMode);
            else
                return await DrawRandomSymbol(GameModes.baseGame);
        }

        internal async Task<int> DrawRandomSymbol(GameModes gameMode)
        {
            for (int i = 0; i < symbolWeightsByState.Length; i++)
            {
                if (symbolWeightsByState[i].gameMode == gameMode)
                {
                    if (symbolWeightsByState[i].weightsForModeDistribution.weightDistributionScriptableObject.intDistribution.Items.Count == 0)
                    {
                        Debug.Log("No Items in ");
                        symbolWeightsByState[i].weightsForModeDistribution.SetWeightsForInt(symbolWeightsByState[i].weightsForModeDistribution.symbolWeights);
                        //await symbolWeightsByState[i].weightsForModeDistribution.weightDistributionScriptableObject.SyncWeights();
                    }
                    return symbolWeightsByState[i].weightsForModeDistribution.weightDistributionScriptableObject.intDistribution.Draw();
                }
            }
            Debug.Log($"Game Mode {gameMode.ToString()} doesn't have valid weights to draw from");
            return -1;
        }



        internal void SetConfigurationSettings(ConfigurationSettingsScriptableObject configurationGeneratorSettings)
        {
            configurationSettings = configurationGeneratorSettings;
        }
        internal int DrawRandomSymbolFromCurrentMode()
        {
            //Debug.Log($"Drawing random symbol for state {StateManager.enCurrentMode}");
            return DrawRandomSymbol(StaticStateManager.enCurrentMode).Result;
        }

        internal Animator SetAnimatorFeatureTriggerAndReturn(SuffixTreeNodeInfo SuffixTreeNodeInfo)
        {
            Animator output = SetOverlayFeatureAndReturnAnimatorFromNode(SuffixTreeNodeInfo);
            return output;
        }

        private Animator SetOverlayFeatureAndReturnAnimatorFromNode(SuffixTreeNodeInfo SuffixTreeNodeInfo)
        {
            throw new Exception("Implement Feature");
            //return configurationGroupManagers[SuffixTreeNodeInfo.column].GetSlotsDecending()[configurationGroupManagers[SuffixTreeNodeInfo.column].stripInfo.stripDisplayZonesSetting.paddingBefore + SuffixTreeNodeInfo.row].SetOverlayAnimatorToFeatureAndGet();
        }

        internal bool isSymbolOverlay(int symbol)
        {
            return managers.evaluationManager.DoesSymbolActivateFeature(symbolDataScriptableObject.symbols[symbol], Features.overlay);
        }

        internal bool isWildSymbol(int symbol)
        {
            return managers.evaluationManager.DoesSymbolActivateFeature(symbolDataScriptableObject.symbols[symbol], Features.wild);
        }

        internal virtual async Task WaitForSymbolToResolveState(string state)
        {
            await Task.CompletedTask;
        }

        internal bool isFeatureSymbol(int symbol)
        {
            return managers.evaluationManager.IsSymbolFeatureSymbol(symbolDataScriptableObject.symbols[symbol]);
        }
        /// <summary>
        /// Sets all symbol animators to REsolve Win States
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="v"></param>
        internal void SetSlotsToResolveWinLose(ref List<BaseObjectManager> slots, bool v)
        {
            for (int slot = 0; slot < slots.Count; slot++)
            {
                if (v)
                    slots[slot].SetSymbolResolveWin();
                else
                    slots[slot].SetSymbolResolveToLose();
            }
        }
    }
}
