//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(EndConfigurationManager))]
    class EndConfigurationManagerEditor : BoomSportsEditor
    {
        private ReorderableList list;

        int current_configuration_showing = 0;
        EndConfigurationManager myTarget;
        SerializedProperty state;
        SerializedProperty endConfigurationsScriptableObject;
        SerializedProperty end_reelstrips_to_display_sequence;
        SerializedProperty displayConfigurationInUse;
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
            list = new ReorderableList(serializedObject,serializedObject.FindProperty("end_reelstrips_to_display_sequence"),true,true,true,true);
            displayConfigurationInUse = serializedObject.FindProperty("displayConfigurationInUse");
            endConfigurationsScriptableObject = serializedObject.FindProperty("endConfigurationsScriptableObject");
        }
        public override async void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("EndConfigurationManager Properties");
            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            
            BoomEditorUtilities.DrawUILine(Color.white);
            if(endConfigurationsScriptableObject.type != null)
            {
                EditorGUILayout.LabelField("End Configuration Manager Controls");
                if (GUILayout.Button("Generate feature configuration to spin"))
                {
                    myTarget.AddConfigurationToSequence(Features.freespin);
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate Reelstrips Basegame"))
                {
                    await myTarget.GenerateMultipleEndReelStripsConfiguration(GameModes.baseGame,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate Reelstrips Free-Spins"))
                {
                    await myTarget.GenerateMultipleEndReelStripsConfiguration(GameModes.freeSpin,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate Reelstrips Overlay-Spins"))
                {
                    await myTarget.GenerateMultipleEndReelStripsConfiguration(GameModes.overlaySpin,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if(GUILayout.Button("Clear end_reelstrips_to_display_sequence"))
                {
                    myTarget.ClearConfigurations();
                }
            
                if (GUILayout.Button("Pop Reel Configuration Test"))
                {
                    Debug.Log(String.Format("current configuration was set with reelstrip length of {0}",myTarget.pop_end_reelstrips_to_display_sequence.Length));
                    serializedObject.Update();
                    displayConfigurationInUse = serializedObject.FindProperty("displayConfigurationInUse");
                }
                if (GUILayout.Button("Set Matrix to Display End Reel Configuration"))
                {
                    myTarget.SetMatrixToReelConfiguration();
                }
            }
            base.OnInspectorGUI();
        }


    }
#endif
    [System.Serializable]
    public partial class EndConfigurationManager : MonoBehaviour
    {
        internal StripConfigurationObject configurationObject
        {
            get
            {
                if (_matrix == null)
                    //TODO hardcoded - will change
                    _matrix = transform.parent.parent.GetComponentInChildren<StripConfigurationObject>();
                return _matrix;
            }
        }
        [SerializeField]
        private StripConfigurationObject _matrix;
        public EndConfigurationsScriptableObject endConfigurationsScriptableObject;
        /// <summary>
        /// The current reelstrip display configuration
        /// </summary>
        internal StripSpinStruct[] displayConfigurationInUse
        {
            get
            {
                return endConfigurationsScriptableObject.currentReelstripConfiguration;
            }
            set
            {
                endConfigurationsScriptableObject.currentReelstripConfiguration = value;
            }
        }
        private StripSpinStruct[] nextConfiguration;
        //Ending Reelstrips current
        public StripSpinStruct[] pop_end_reelstrips_to_display_sequence
        {
            get
            {
                try
                {
                    SetAndRemoveConfiguration(0);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                    GenerateMultipleEndReelStripsConfiguration(StateManager.enCurrentMode, 20);
                    SetAndRemoveConfiguration(0);
                }
                return endConfigurationsScriptableObject.currentReelstripConfiguration;
            }
        }

        private void SetAndRemoveConfiguration(int v)
        {
            if (v > endConfigurationsScriptableObject.endReelstripsPerState[StateManager.enCurrentMode].data.Count)
            {
                GenerateMultipleEndReelStripsConfiguration(StateManager.enCurrentMode, v);
            }
            //Save the strip used into the backlog
            if (displayConfigurationInUse?.Length > 0) ;
            SaveReelstripUsed(displayConfigurationInUse);
            //TODO Validate Data in Reel Strip then Generate if no valid data found
            SetCurrentConfigurationTo(endConfigurationsScriptableObject.endReelstripsPerState[StateManager.enCurrentMode].data[v].data);
            endConfigurationsScriptableObject.endReelstripsPerState[StateManager.enCurrentMode].data.RemoveAt(v);
        }

        private void SaveReelstripUsed(StripSpinStruct[] current_reelstrip_configuration)
        {
            endConfigurationsScriptableObject.AddReelstripToUsedList(current_reelstrip_configuration);
        }

        private void SetCurrentConfigurationTo(StripSpinStruct[] reelstrips)
        {
            displayConfigurationInUse = reelstrips;
        }

        private void SetEndingReelStripToDisplay(StripSpinStruct[] reelstrips_to_display)
        {
            nextConfiguration = reelstrips_to_display;
        }

        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateMultipleEndReelStripsConfiguration(GameModes gameState, int amount)
        {
            if (!endConfigurationsScriptableObject.endReelstripsPerState.ContainsKey(gameState))
                endConfigurationsScriptableObject.endReelstripsPerState[gameState] = new GameStateConfigurationStorage();
            if (endConfigurationsScriptableObject.endReelstripsPerState[gameState].data == null)
                endConfigurationsScriptableObject.endReelstripsPerState[gameState].data = new List<SpinConfigurationStorage>();
            for (int i = 0; i < amount; i++)
            {
                endConfigurationsScriptableObject.endReelstripsPerState[gameState].data.Add(new SpinConfigurationStorage(GenerateStrips(gameState, configurationObject.configurationSettings.displayZones).Result));
            }
        }
        /// <summary>
        /// Generates Strips with random configuration - TODO - add logic to add symbols based on spin type - stepper, constant, etc...
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="displayZones"></param>
        /// <returns></returns>
        internal async Task<StripSpinStruct[]> GenerateStrips(GameModes gameState, ConfigurationDisplayZonesStruct[] displayZones)
        {
            StripSpinStruct[] output = new StripSpinStruct[displayZones.Length];
            for (int reel = 0; reel < displayZones.Length; reel++)
            {
                output[reel] = new StripSpinStruct(await GenerateEndingStrip(gameState, displayZones[reel]));
            }
            return output;
        }
        internal async Task<StripSpinStruct[]> GenerateStrips(GameModes gameState, ConfigurationDisplayZonesStruct[] displayZones, Features featureToGenerate)
        {
            StripSpinStruct[] output = new StripSpinStruct[displayZones.Length];
            for (int strip = 0; strip < displayZones.Length; strip++)
            {
                output[strip] = new StripSpinStruct(await GenerateEndingStrip(gameState, displayZones[strip], featureToGenerate, strip, displayZones.Length));
            }
            return output;
        }

        private async Task<NodeDisplaySymbol[]> GenerateEndingStrip(GameModes mode, ConfigurationDisplayZonesStruct configurationDisplayZonesStruct)
        {
            List<NodeDisplaySymbol> output = new List<NodeDisplaySymbol>();
            //Debug.LogWarning($"reelStripManager.configurationGroupDisplayZones.totalPositions = {configurationDisplayZonesStruct.totalPositions}");
            //Generate a symbol for each display zone slot
            for (int i = 0; i < configurationDisplayZonesStruct.displayZonesPositionsTotal; i++)
            {
                output.Add(await GetRandomWeightedSymbol(mode));
            }
            return output.ToArray();
        }

        private async Task<NodeDisplaySymbol[]> GenerateEndingStrip(GameModes mode, ConfigurationDisplayZonesStruct configurationDisplayZonesStruct,Features featureToGenerate, int strip, int lengthGroupManagers)
        {
            List<NodeDisplaySymbol> output = new List<NodeDisplaySymbol>();
            //Debug.LogWarning($"reelStripManager.configurationGroupDisplayZones.totalPositions = {configurationDisplayZonesStruct.totalPositions}");

            //Get ConfigurationDisplayZonesStruct Spin Type - Generate Symbols based on how many Symbols replace per spin

            //Generate a symbol for each display zone slot
            for (int i = 0; i < configurationDisplayZonesStruct.displayZonesPositionsTotal; i++)
            {
                output.Add(await GetRandomWeightedSymbol(mode));
            }
            return output.ToArray();
        }

        /// <summary>
        /// Generate a random symbol based on weights defined
        /// </summary>
        /// <returns></returns>
        public async Task<NodeDisplaySymbol> GetRandomWeightedSymbol(GameModes currentMode)
        {
            int symbol = await configurationObject.DrawRandomSymbol(currentMode);
            return await GetNodeDisplaySymbol(symbol);
        }
        /// <summary>
        /// Generate a random symbol based on weights defined
        /// </summary>
        /// <returns></returns>
        public async Task<NodeDisplaySymbol> GetNodeDisplaySymbol(int symbol)
        {
            NodeDisplaySymbol output = new NodeDisplaySymbol();
            //Debug.LogWarning($"End Configuration Draw Random Symbol returned {symbol}");
            if (configurationObject.isSymbolOverlay(symbol))
            {
                output.SetOverlaySymbolTo(symbol);
                output.AddFeaturesTo(configurationObject.GetSymbolFeatures(symbol));
                while (configurationObject.isSymbolOverlay(symbol))
                {
                    symbol = await configurationObject.DrawRandomSymbol();//symbol_weights_per_state[StateManager.enCurrentMode].intDistribution.Draw();
                    //Set Overlay feature in list and freespin
                }
            }
            if (configurationObject.isFeatureSymbol(symbol))
            {
                output.AddFeaturesTo(configurationObject.GetSymbolFeatures(symbol));
            }
            if (configurationObject.isWildSymbol(symbol))
            {
                output.SetWildTo(symbol);
            }
            output.primarySymbol = symbol;
            //Debug.Log(String.Format("Symbol Generated form Weighted Distribution is {0}", ((Symbol)output).ToString()));
            return output;
        }

        internal StripSpinStruct[] UseNextConfigurationInList()
        {
            return pop_end_reelstrips_to_display_sequence;
        }

        internal StripSpinStruct[] GetCurrentConfiguration()
        {
            //if(current_reelstrip_configuration.Length < 1)
            //{
            //    if (end_reelstrips_to_display_sequence.Length < 1)
            //        GenerateMultipleEndReelStripsConfiguration(5);
            //    return pop_end_reelstrips_to_display_sequence;
            //}
            //else
            //{
            //    return current_reelstrip_configuration;
            //}
            return displayConfigurationInUse;
        }
        /// <summary>
        /// Sets the matrix to the display current reelstrip configuration
        /// </summary>
        internal void SetMatrixToReelConfiguration()
        {
            configurationObject.SetSymbolsToDisplayOnConfigurationObjectTo(displayConfigurationInUse);
        }

        internal void AddConfigurationToSequence(GameModes gameState, StripSpinStruct[] configuration)
        {
            if (endConfigurationsScriptableObject.endReelstripsPerState[gameState] == null)
                endConfigurationsScriptableObject.endReelstripsPerState[gameState] = new GameStateConfigurationStorage();
            //if valid configuration then add and move on
            endConfigurationsScriptableObject.endReelstripsPerState[gameState].data.Insert(0, new SpinConfigurationStorage(configuration));
        }

        internal void AddConfigurationToSequence(Features feature)
        {
            StripSpinStruct[] configuration = new StripSpinStruct[0];
            switch (feature)
            {
                case Features.freespin:
                    //configuration = new StripSpinStruct[configurationObject.configurationGroupManagers.Length];
                    //for (int i = 0; i < configuration.Length; i++)
                    //{
                    //    //configuration[i].displaySymbols = new NodeDisplaySymbol[3]
                    //    //{
                    //    //    new NodeDisplaySymbol(i % 2 == 0
                    //    //? (int)Symbol.SA01 : (int)Symbol.RO03),
                    //    //    new NodeDisplaySymbol((int)Symbol.RO01),
                    //    //    new NodeDisplaySymbol((int)Symbol.RO02)
                    //    //};

                    //    //if (i % 2 == 0)
                    //    //{
                    //    //    configuration[i].displaySymbols[0].AddFeature(Features.freespin);
                    //    //}
                    //}
                    //AddConfigurationToSequence(GameModes.baseGame, configuration);
                    break;
                case Features.overlay:
                    //configuration = new StripSpinStruct[configurationObject.configurationGroupManagers.Length];
                    //for (int i = 0; i < configuration.Length; i++)
                    //{
                    //    configuration[i].displaySymbols = new NodeDisplaySymbol[3]
                    //    {
                    //        new NodeDisplaySymbol(i % 2 == 0
                    //    ? (int)Symbol.SA02 : (int)Symbol.RO03),
                    //        new NodeDisplaySymbol((int)Symbol.RO01),
                    //        new NodeDisplaySymbol((int)Symbol.RO02)
                    //    };
                    //    if (i % 2 == 0)
                    //    {
                    //        configuration[i].displaySymbols[0].primary_symbol = (int)Symbol.RO03;
                    //        configuration[i].displaySymbols[0].SetOverlaySymbolTo((int)Symbol.SA02);
                    //        configuration[i].displaySymbols[0].AddFeature(Features.overlay);
                    //        configuration[i].displaySymbols[0].is_overlay = true;
                    //    }
                    //}
                    //AddConfigurationToSequence(GameModes.baseGame, configuration);
                    break;
                default:
                    break;
            }
        }
        internal void ClearConfigurations()
        {
            foreach (KeyValuePair<GameModes, GameStateConfigurationStorage> weight in endConfigurationsScriptableObject.endReelstripsPerState)
            {
                weight.Value.data.Clear();
            }
        }

        internal StripSpinStruct[] GenerateFeatureConfigurationAndAddToStateNextSpin(GameModes gameState, Features featureToTest)
        {
            StripSpinStruct[] output = GenerateStrips(gameState, configurationObject.configurationSettings.displayZones,featureToTest).Result;
            endConfigurationsScriptableObject.endReelstripsPerState[gameState].data.Add(new SpinConfigurationStorage(output));
            return output;
        }
    }
}