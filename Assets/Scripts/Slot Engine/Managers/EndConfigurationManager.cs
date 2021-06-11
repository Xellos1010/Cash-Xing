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
namespace BoomSports.Prototype.Managers
{
#if UNITY_EDITOR
    [CustomEditor(typeof(EndConfigurationManager))]
    class EndConfigurationManagerEditor : BoomSportsEditor
    {
        EndConfigurationManager myTarget;
        SerializedProperty state;
        SerializedProperty endConfigurationsScriptableObject;
        SerializedProperty displayConfigurationInUse;
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
            displayConfigurationInUse = serializedObject.FindProperty("displayConfigurationInUse");
            endConfigurationsScriptableObject = serializedObject.FindProperty("endConfigurationsScriptableObject");
        }
        public override async void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("EndConfigurationManager Properties");
            EditorGUILayout.EnumPopup(StaticStateManager.enCurrentState);
            
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
                    await myTarget.GenerateMultipleDisplayConfigurations(GameModes.baseGame,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate Reelstrips Free-Spins"))
                {
                    await myTarget.GenerateMultipleDisplayConfigurations(GameModes.freeSpin,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Generate Reelstrips Overlay-Spins"))
                {
                    await myTarget.GenerateMultipleDisplayConfigurations(GameModes.overlaySpin,20);
                    serializedObject.ApplyModifiedProperties();
                }
                if(GUILayout.Button("Clear end_reelstrips_to_display_sequence"))
                {
                    myTarget.ClearConfigurations();
                }
                if (GUILayout.Button("Pop Reel Configuration Test"))
                {
                    Debug.Log(String.Format("current configuration was set with reelstrip length of {0}",myTarget.popEndDisplayConfiguration.configuration.Length));
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
    public class EndConfigurationManager : BaseBoomSportsManager
    {
        /*Static Accessors used to have clean calls from scripts - will not allow multiple slot games in 1 scene. Would have to Unload Current Slot Game and Load Next Slot Game.*/
        public static DisplayConfigurationContainer displayConfigurationInUse
        {
            get
            {
                //Debug.Log(instance.gameObject.name);
                //Debug.Log(instance._displayConfigurationInUse.configuration.Length);

                return instance._displayConfigurationInUse;
            }
        }

        //Will need to refactor reference if more than 1 slot game will be present in scene
        public static EndConfigurationManager instance
        {
            get
            {
                if (_instance == null)
                    //Will not work if more than 1 object with script is in scene - first instance found
                    _instance = GameObject.FindObjectOfType<EndConfigurationManager>();
                return _instance;
            }
        }
        public static EndConfigurationManager _instance;

        /**/
        /*Core*/
        public EndConfigurationsScriptableObject endConfigurationsScriptableObject;
        /// <summary>
        /// The current reelstrip display configuration
        /// </summary>
        internal DisplayConfigurationContainer _displayConfigurationInUse
        {
            get
            {
                //if (endConfigurationsScriptableObject.currentConfigurationInUse.configuration.Length < 1)
                //    return popEndDisplayConfiguration;
                return endConfigurationsScriptableObject.currentConfigurationInUse;
            }
            set
            {
                endConfigurationsScriptableObject.currentConfigurationInUse = value;
            }
        }
        private DisplayConfigurationContainer nextConfiguration;
        /// <summary>
        /// Sets the currentConfigurationInUse to next configuration - generates configurations if none are present
        /// </summary>
        public DisplayConfigurationContainer popEndDisplayConfiguration
        {
            get
            {
                try
                {
                    StoreCurrentConfigurationAndSetNewConfiguration(0);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                    GenerateMultipleDisplayConfigurations(StaticStateManager.enCurrentMode, 20);
                    StoreCurrentConfigurationAndSetNewConfiguration(0);
                }
                return endConfigurationsScriptableObject.currentConfigurationInUse;
            }
        }

        public void Start()
        {
            _instance = this;
        }

        private void StoreCurrentConfigurationAndSetNewConfiguration(int v)
        {
            if (v > endConfigurationsScriptableObject.configurationsByState[StaticStateManager.enCurrentMode].data.Count)
            {
                GenerateMultipleDisplayConfigurations(StaticStateManager.enCurrentMode, v);
            }
            //Save the strip used into the backlog
            if (_displayConfigurationInUse.configuration?.Length > 0)
                SaveReelstripUsed(_displayConfigurationInUse);
            //TODO Validate Data in Reel Strip then Generate if no valid data found
            SetCurrentConfigurationTo(endConfigurationsScriptableObject.configurationsByState[StaticStateManager.enCurrentMode].data[v].data);
            endConfigurationsScriptableObject.configurationsByState[StaticStateManager.enCurrentMode].data.RemoveAt(v);
        }

        private void SaveReelstripUsed(DisplayConfigurationContainer currentReelstripConfiguration)
        {
            endConfigurationsScriptableObject.AddReelstripToUsedList(currentReelstripConfiguration);
        }

        private void SetCurrentConfigurationTo(DisplayConfigurationContainer newConfiguration)
        {
            _displayConfigurationInUse = newConfiguration;
        }

        private void SetNextConfigurationToDisplay(DisplayConfigurationContainer nextConfigurationToDisplay)
        {
            nextConfiguration = nextConfigurationToDisplay;
        }

        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateMultipleDisplayConfigurations(GameModes gameState, int amount)
        {
            if (!endConfigurationsScriptableObject.configurationsByState.ContainsKey(gameState))
                endConfigurationsScriptableObject.configurationsByState[gameState] = new GameStateConfigurationStorage();
            if (endConfigurationsScriptableObject.configurationsByState[gameState].data == null)
                endConfigurationsScriptableObject.configurationsByState[gameState].data = new List<SpinConfigurationStorage>();

            //To generate for stepper strips need to feed strip back to make step increment by # of steps per spin
            for (int i = 0; i < amount; i++)
            {
                endConfigurationsScriptableObject.configurationsByState[gameState].data.Add(new SpinConfigurationStorage(GenerateStrips(gameState, configurationObject.configurationSettings.displayZones).Result));
            }
        }
        /// <summary>
        /// Generates Strips with random configuration - TODO - add logic to add symbols based on spin type - stepper, constant, etc...
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="stripsDisplayZones"></param>
        /// <returns></returns>
        internal async Task<DisplayConfigurationContainer> GenerateStrips(GameModes gameState, ConfigurationDisplayZonesStruct[] stripsDisplayZones)
        {
            //Strips need to be generated
            DisplayConfigurationContainer output = new DisplayConfigurationContainer();
            output.configuration = new GroupSpinInformationStruct[stripsDisplayZones.Length];
            for (int strip = 0; strip < stripsDisplayZones.Length; strip++)
            {
                output.configuration[strip] = new GroupSpinInformationStruct(await GenerateStripConfiguration(gameState, stripsDisplayZones[strip]));
            }
            return output;
        }

        /// <summary>
        /// Used to set the display symbols for a group based on spin parameter
        /// </summary>
        /// <param name="objectGroupManager"></param>
        internal void SetDisplaySymbolsForGroup(ref BaseObjectGroupManager objectGroupManager)
        {
            Debug.Log($"Building display symbol sequence for {objectGroupManager.gameObject.name}");
            List<NodeDisplaySymbolContainer> symbolSequence = new List<NodeDisplaySymbolContainer>();
            //Symbol Sequence needs to account for index in path

            //Get how many symbols on strip will clear from group manager spin parameters
            int symbolsToReplaceReel = objectGroupManager.GetSymbolsToBeReplacedPerSpin();
            Debug.Log($"Replacing {symbolsToReplaceReel} symbol(s) on strip {objectGroupManager.gameObject.name}");
            
            string symbolSequenceDebugMessage = "";

            //Add symbols from strip if not full strip replacement - need to account for padding slots. does not account for active and inactive display zones
            if (symbolsToReplaceReel < objectGroupManager.objectsInGroup.Length - objectGroupManager.configurationGroupDisplayZones.paddingBefore)
            {//Symbols to replace less then strip length - remove end and add Start
             //- if index on path != 0 then Padding slot is next symbol at index and using the next debug symbol at path position for x spins.
             //- while set symbols at path position (x) > 0 Symbols to replace per spin then set symbol in last position to index on path untilspin end then last symbol in end position will go to start of path to be adjusted again and align with symbol sequence.
             //Spin at index in path needs to be accounted for

                //Debug.Log($"symbolSequence.Count before raw add slots decending = {symbolSequence.Count}");
                //Add initial range of current symbols in decending order; then adjust for symbols to replace per spin
                //*Remove X Symbols to Replace from End
                //Insert X symbols @ index at path symbol debug_symbol if available (The debug symbol will be poped later by the slot changing to the debug symbol)
                symbolSequence.AddRange(objectGroupManager.GetCurrentDisplaySymbolsFromDecending());
                //Debug.Log($"symbolSequence.Count from raw add slots decending = {symbolSequence.Count}");
                //Build Debug Message
                for (int i = 0; i < symbolSequence.Count; i++)
                {
                    symbolSequenceDebugMessage += $"|{symbolSequence[i].primarySymbol}";
                }
                Debug.Log($"raw current sequence from decending slots = {symbolSequenceDebugMessage}");
                //Spin at index needs to be < positions along path end index - this may not work with partial reel clears above 1 symbol replace (Stepper reel step per spin) needs more testing
                //Only built for 1 step (1 symbol replacement) per spin use-case - If spin at path is > last index on path 
                symbolSequence.RemoveRange(symbolSequence.Count - symbolsToReplaceReel, symbolsToReplaceReel);
                symbolSequenceDebugMessage = "";
                //Build Debug Message
                for (int i = 0; i < symbolSequence.Count; i++)
                {
                    symbolSequenceDebugMessage += $"|{symbolSequence[i].primarySymbol}";
                }
                Debug.Log($"Symol sequence after symbolSequence.RemoveRange(symbolSequence.Count{symbolSequence.Count} - symbolsToReplaceReel {symbolsToReplaceReel}, symbolsToReplaceReel);= {symbolSequenceDebugMessage}");
            }
            //Starts our printout for symbol sequence in unity console.

            //Debug.Log($"{objectGroupManager.gameObject.name}.GetIndexInGroup() = {objectGroupManager.GetIndexInGroup()}");
            //Build the data container required to add symbols to display sequence
            BuildSymbolSequenceDataContainer buildSymbolSequenceDataContainer = new BuildSymbolSequenceDataContainer(symbolsToReplaceReel, objectGroupManager.spinAtIndexInPath, displayConfigurationInUse.configuration[objectGroupManager.GetIndexInGroup()], objectGroupManager);
            //Use container data to build symbolSequence
            AddSymbolsToDisplaySequence(buildSymbolSequenceDataContainer, ref symbolSequence);
            
            symbolSequenceDebugMessage = "";
            for (int i = 0; i < symbolSequence.Count; i++)
            {
                symbolSequenceDebugMessage += $"|{symbolSequence[i].primarySymbol}";
            }
            Debug.Log($"Display sequence after add = {symbolSequenceDebugMessage}");
            objectGroupManager.symbolsDisplaySequence = symbolSequence.ToArray();
        }

        public struct BuildSymbolSequenceDataContainer
        {
            public int symbolsToReplaceReel;
            public int spinAtIndexInPath;
            public GroupSpinInformationStruct groupSpinInformationStruct;
            public BaseObjectGroupManager objectGroupManager;

            public BuildSymbolSequenceDataContainer(int symbolsToReplaceReel, int spinAtIndexInPath, GroupSpinInformationStruct groupSpinInformationStruct, BaseObjectGroupManager objectGroupManager)
            {
                this.symbolsToReplaceReel = symbolsToReplaceReel;
                this.spinAtIndexInPath = spinAtIndexInPath;
                this.groupSpinInformationStruct = groupSpinInformationStruct;
                this.objectGroupManager = objectGroupManager;
            }
        }
        /// <summary>
        /// Add's symbols to the start of a display sequence
        /// </summary>
        /// <param name="symbolsToAdd"></param>
        /// <param name="groupSpinInformationStruct"></param>
        /// <param name="symbolsSequenceList"></param>
        private void AddSymbolsToDisplaySequence(BuildSymbolSequenceDataContainer dataContainer, ref List<NodeDisplaySymbolContainer> symbolsSequenceList)
        {
            Debug.Log($"groupSpinInformationStruct.displaySymbolSequence.Length = {dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length}");
            int symbol = -1;
            //Insert symbols at spin at index until 1 left to insert at top
            //Works for Stepper reels and constant directional reels use-case only - more testing required
            for (int displaySymbolSequenceIndex = 0; displaySymbolSequenceIndex < dataContainer.symbolsToReplaceReel; displaySymbolSequenceIndex++)
            {
                //TODO change random range to pull list of symbols valid for column - Testing features only Cash Crossing
                //Initialize symbol ID to set
                symbol = UnityEngine.Random.Range(0, 3);
                Debug.Log($"Symbol generated on add symbol to sequence init = {symbol}");
                //Check Object Manager for debug symbols and use until debug list is empty or displaySymbolSequenceIndex > debugSymbols.Length
                if (dataContainer.objectGroupManager.debugNextSymbolsToLoad != null)
                {
                    Debug.Log($"dataContainer.objectGroupManager.debugNextSymbolsToLoad.Count = {dataContainer.objectGroupManager.debugNextSymbolsToLoad.Count}");
                    if (displaySymbolSequenceIndex < dataContainer.objectGroupManager.debugNextSymbolsToLoad.Count)
                    {
                        symbol = dataContainer.objectGroupManager.debugNextSymbolsToLoad[displaySymbolSequenceIndex];
                        Debug.Log($"Symbol Set to debugNextSymbolsToLoad[{displaySymbolSequenceIndex}] = {symbol}");
                    }
                }
                //Load symbol into index on path and re

                Debug.Log($"displaySymbolSequenceIndex {displaySymbolSequenceIndex} >= groupSpinInformationStruct.displaySymbolSequence.Length {dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length} = {displaySymbolSequenceIndex >= dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length}");
                
                //Bug Resolve full reel replacement - For full reel replacement you will replace objects in group - sometimes you will have more padding slots then in display sequence - evaluation manager needs all slots in row filled including padding and inactive payline evaluations since using suffix tree data structure and logic to identify nodes with columsn and rows.
                if (displaySymbolSequenceIndex >= dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length)
                {
                    Debug.Log($"displaySymbolSequenceIndex {displaySymbolSequenceIndex} >= dataContainer.groupSpinInformationStruct.displaySymbolsToLoad{dataContainer.groupSpinInformationStruct.displaySymbolsToLoad} = {displaySymbolSequenceIndex >= dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length}");
                    //Refactored to include index at path
                    if (displaySymbolSequenceIndex == dataContainer.symbolsToReplaceReel - 1)//If we are on the last symbol then insert at start of strip - will break if padding is > 1
                    {//TODO include in data container group display struct for reel and account for padding before reel to replace symbols at top
                        Debug.Log($"Inserting symbolID {symbol} into symbolsSequenceList at {dataContainer.spinAtIndexInPath}");
                        symbolsSequenceList.Insert(0, new NodeDisplaySymbolContainer(symbol)); //Insert at start of array
                    }
                    else
                    {
                        Debug.Log($"Inserting symbolID {symbol} into symbolsSequenceList at {dataContainer.spinAtIndexInPath}");
                        symbolsSequenceList.Insert(dataContainer.spinAtIndexInPath, new NodeDisplaySymbolContainer(symbol)); //Insert at index in path
                    }
                }
                else //Stepper only replaces 1 slot per spin
                {
                    Debug.Log($"groupSpinInformationStruct.displaySymbolSequence.Length = {dataContainer.groupSpinInformationStruct.displaySymbolsToLoad.Length} getting index {displaySymbolSequenceIndex}");
                    Debug.Log($"Adding symbol {symbol} to sequence");
                    //symbolsSequenceList.Insert(0, groupSpinInformationStruct.displaySymbolSequence[displaySymbolSequenceIndex]);
                    symbolsSequenceList.Insert(dataContainer.spinAtIndexInPath, new NodeDisplaySymbolContainer(symbol));
                    Debug.Log($"symbolsSequenceList[dataContainer.spinAtIndexInPath] = {symbolsSequenceList[dataContainer.spinAtIndexInPath].primarySymbol}");
                }
            }  
        }

        //public List<NodeDisplaySymbolContainer> enmd
        //Todo Refactor and combine function
        internal async Task<DisplayConfigurationContainer> GenerateStrips(GameModes gameState, ConfigurationDisplayZonesStruct[] displayZones, Features featureToGenerate)
        {
            DisplayConfigurationContainer output = new DisplayConfigurationContainer();
            output.configuration = new GroupSpinInformationStruct[displayZones.Length];
            for (int strip = 0; strip < displayZones.Length; strip++)
            {
                output.configuration[strip] = new GroupSpinInformationStruct(await GenerateEndingStrip(gameState, displayZones[strip], featureToGenerate, strip, displayZones.Length));
            }
            return output;
        }

        private async Task<NodeDisplaySymbolContainer[]> GenerateStripConfiguration(GameModes mode, ConfigurationDisplayZonesStruct configurationDisplayZonesStruct)
        {
            List<NodeDisplaySymbolContainer> output = new List<NodeDisplaySymbolContainer>();
            //Debug.LogWarning($"reelStripManager.configurationGroupDisplayZones.totalPositions = {configurationDisplayZonesStruct.totalPositions}");
            //Generate a symbol for each display zone slot
            for (int i = 0; i < configurationDisplayZonesStruct.displayZonesPositionsTotal; i++)
            {
                output.Add(await GetRandomWeightedSymbol(mode));
            }
            return output.ToArray();
        }

        private async Task<NodeDisplaySymbolContainer[]> GenerateEndingStrip(GameModes mode, ConfigurationDisplayZonesStruct configurationDisplayZonesStruct,Features featureToGenerate, int strip, int lengthGroupManagers)
        {
            List<NodeDisplaySymbolContainer> output = new List<NodeDisplaySymbolContainer>();
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
        public async Task<NodeDisplaySymbolContainer> GetRandomWeightedSymbol(GameModes currentMode)
        {
            int symbol = await configurationObject.DrawRandomSymbol(currentMode);
            return await GetNodeDisplaySymbol(symbol);
        }
        /// <summary>
        /// Generate a random symbol based on weights defined
        /// </summary>
        /// <returns></returns>
        public async Task<NodeDisplaySymbolContainer> GetNodeDisplaySymbol(int symbol)
        {
            NodeDisplaySymbolContainer output = new NodeDisplaySymbolContainer();
            ////Debug.LogWarning($"End Configuration Draw Random Symbol returned {symbol}");
            //if (configurationObject.isSymbolOverlay(symbol))
            //{
            //    output.SetOverlaySymbolTo(symbol);
            //    output.AddFeaturesTo(configurationObject.GetSymbolFeatures(symbol));
            //    while (configurationObject.isSymbolOverlay(symbol))
            //    {
            //        symbol = await configurationObject.DrawRandomSymbol();//symbol_weights_per_state[StateManager.enCurrentMode].intDistribution.Draw();
            //        //Set Overlay feature in list and freespin
            //    }
            //}
            //if (configurationObject.isFeatureSymbol(symbol))
            //{
            //    output.AddFeaturesTo(configurationObject.GetSymbolFeatures(symbol));
            //}
            //if (configurationObject.isWildSymbol(symbol))
            //{
            //    output.SetWildTo(symbol);
            //}
            output.primarySymbol = symbol;
            //Debug.Log(String.Format("Symbol Generated form Weighted Distribution is {0}", ((Symbol)output).ToString()));
            return output;
        }

        internal DisplayConfigurationContainer UseNextConfigurationInList()
        {
            return popEndDisplayConfiguration;
        }
        /// <summary>
        /// Sets the matrix to the display current reelstrip configuration
        /// </summary>
        internal void SetMatrixToReelConfiguration()
        {
            configurationObject.SetSymbolsToDisplayOnConfigurationObjectTo(_displayConfigurationInUse);
        }

        internal void AddConfigurationToSequence(GameModes gameState, DisplayConfigurationContainer configuration)
        {
            if (endConfigurationsScriptableObject.configurationsByState[gameState] == null)
                endConfigurationsScriptableObject.configurationsByState[gameState] = new GameStateConfigurationStorage();
            //if valid configuration then add and move on
            endConfigurationsScriptableObject.configurationsByState[gameState].data.Insert(0, new SpinConfigurationStorage(configuration));
        }

        internal void AddConfigurationToSequence(Features feature)
        {
            GroupSpinInformationStruct[] configuration = new GroupSpinInformationStruct[0];
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
            foreach (KeyValuePair<GameModes, GameStateConfigurationStorage> weight in endConfigurationsScriptableObject.configurationsByState)
            {
                weight.Value.data.Clear();
            }
        }

        internal DisplayConfigurationContainer GenerateFeatureConfigurationAndAddToStateNextSpin(GameModes gameState, Features featureToTest)
        {
            DisplayConfigurationContainer output = GenerateStrips(gameState, configurationObject.configurationSettings.displayZones,featureToTest).Result;
            endConfigurationsScriptableObject.configurationsByState[gameState].data.Add(new SpinConfigurationStorage(output));
            return output;
        }
    }
}