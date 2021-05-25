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
        internal StripConfigurationObject configurationObject
        {
            get
            {
                if (_configurationObject == null)
                    //TODO hardcoded - will change
                    _configurationObject = transform.parent.parent.GetComponentInChildren<StripConfigurationObject>();
                return _configurationObject;
            }
        }
        [SerializeField]
        private StripConfigurationObject _configurationObject;
        public EndConfigurationsScriptableObject endConfigurationsScriptableObject;
        /// <summary>
        /// The current reelstrip display configuration
        /// </summary>
        internal DisplayConfigurationContainer _displayConfigurationInUse
        {
            get
            {
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
            Debug.Log($"Setting Display Symbols for {objectGroupManager.gameObject.name}");
            List<NodeDisplaySymbolContainer> symbolSequence = new List<NodeDisplaySymbolContainer>();
            //Get how many symbols on strip will clear from group manager spin parameters
            int symbolsToReplaceReel = objectGroupManager.GetSymbolsToBeReplacedPerSpin();
            //if symbolsToReplaceReel < total slot objects. (remove x from end of strip x=symbols to leave strip) and build end display symbols
            //if (symbolsToReplaceReel < objectGroupManager.objectsInGroup.Length)
            //{
            //Get symbols currently on reel in priority order
            //extra call but hack for now - if need performance really add the if
            List<BaseObjectManager> objectsLastInFirstOut = objectGroupManager.GetSlotsDecending();
            Debug.Log($"current display symbols = {objectGroupManager.PrintCurrentDisplaySymbols()}");
            objectsLastInFirstOut.RemoveRange(objectsLastInFirstOut.Count-1-symbolsToReplaceReel, symbolsToReplaceReel);
            for (int i = objectsLastInFirstOut.Count - 1; i > 0; i--)
            {
                Debug.Log($"{objectsLastInFirstOut[i].gameObject.name} currentPresentingSymbolID = {objectsLastInFirstOut[i].currentPresentingSymbolID}");
                symbolSequence.Add(new NodeDisplaySymbolContainer(objectsLastInFirstOut[i].currentPresentingSymbolID));
            }
            string debugmessage = "";
            for (int i = 0; i < symbolSequence.Count; i++)
            {
                debugmessage += $"|{symbolSequence[i].primarySymbol}";
            }
            Debug.Log($"Display sequence before add= {debugmessage}");
            //Symbols on reel in last of symbol sequence - add next symbols ontop and return from strip sequence
            //May need to refactor in future
            AddSymbolsToDisplaySequence(symbolsToReplaceReel, _displayConfigurationInUse.configuration[objectGroupManager.GetIndexInGroup()], ref symbolSequence);
            debugmessage = "";
            for (int i = 0; i < symbolSequence.Count; i++)
            {
                debugmessage += $"|{symbolSequence[i].primarySymbol}";
            }
            Debug.Log($"Display sequence after add= {debugmessage}");
            objectGroupManager.symbolsDisplaySymbolsSequence = symbolSequence.ToArray();
        //}
            //Get symbols from strip sequence based on object group manager index in groups. (This should be controled by objectGroupManager parent)
            //Set object group managers sequence display symbols (should contain symbols on strip and x symbol off strip. x = objectGroupManager.displayzone.paddingbefore)
        }

        private void AddSymbolsToDisplaySequence(int symbolsToAdd, GroupSpinInformationStruct groupSpinInformationStruct, ref List<NodeDisplaySymbolContainer> symbolsSequenceList)
        {
            for (int i = 0; i < symbolsToAdd; i++)
            {
                symbolsSequenceList.Add(groupSpinInformationStruct.displaySymbolSequence[i]);
            }  
        }

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