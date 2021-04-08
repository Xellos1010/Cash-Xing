//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
        SerializedProperty current_reelstrip_configuration;
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
            list = new ReorderableList(serializedObject,serializedObject.FindProperty("end_reelstrips_to_display_sequence"),true,true,true,true);
            current_reelstrip_configuration = serializedObject.FindProperty("current_reelstrip_configuration");
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
                if (GUILayout.Button("Generate Reelstrips"))
                {
                    await myTarget.GenerateMultipleEndReelStripsConfiguration(20);
                    serializedObject.ApplyModifiedProperties();
                }
                if(GUILayout.Button("Clear end_reelstrips_to_display_sequence"))
                {
                    myTarget.end_reelstrips_to_display_sequence = null;
                }
            
                if (GUILayout.Button("Pop Reel Configuration Test"))
                {
                    Debug.Log(String.Format("current configuration was set with reelstrip length of {0}",myTarget.pop_end_reelstrips_to_display_sequence.reelstrips.Length));
                    serializedObject.Update();
                    current_reelstrip_configuration = serializedObject.FindProperty("current_reelstrip_configuration");
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
        internal Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    //TODO hardcoded - will change
                    _matrix = transform.parent.parent.GetComponentInChildren<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        private Matrix _matrix;
        public EndConfigurationsScriptableObject endConfigurationsScriptableObject;
        internal List<ReelStripsStruct> end_reelstrips_to_display_sequence
        {
            get
            {
                return endConfigurationsScriptableObject.end_reelstrips_to_display_sequence;
            }
            set
            {
                endConfigurationsScriptableObject.end_reelstrips_to_display_sequence = value;
            }
        }

        internal ReelStripsStruct current_reelstrip_configuration
        {
            get
            {
                return endConfigurationsScriptableObject.current_reelstrip_configuration;
            }
            set
            {
                endConfigurationsScriptableObject.current_reelstrip_configuration = value;
            }
        }

        //Ending Reelstrips current
        public ReelStripsStruct pop_end_reelstrips_to_display_sequence
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
                    GenerateMultipleEndReelStripsConfiguration(20);
                    SetAndRemoveConfiguration(0);
                }
                return endConfigurationsScriptableObject.current_reelstrip_configuration;
            }
        }

        private void SetAndRemoveConfiguration(int v)
        {
            if(v > end_reelstrips_to_display_sequence.Count)
            {
                GenerateMultipleEndReelStripsConfiguration(v);
            }
            //Save the strip used into the backlog
            if (current_reelstrip_configuration.reelstrips != null && current_reelstrip_configuration.reelstrips.Length > 0)
                SaveReelstripUsed(current_reelstrip_configuration);
            //TODO Validate Data in Reel Strip then Generate if no valid data found
            SetCurrentConfigurationTo(end_reelstrips_to_display_sequence[v]);
            end_reelstrips_to_display_sequence.RemoveAt(v);
        }

        private void SaveReelstripUsed(ReelStripsStruct current_reelstrip_configuration)
        {
            endConfigurationsScriptableObject.AddReelstripToUsedList(current_reelstrip_configuration);
        }

        private void SetCurrentConfigurationTo(ReelStripsStruct reelstrips)
        {
            current_reelstrip_configuration = reelstrips;
        }

        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateEndReelStripsConfiguration()
        {
            SetEndingReelStripToDisplay(GenerateReelStrips(matrix.reel_strip_managers).Result);
        }

        private void SetEndingReelStripToDisplay(ReelStripsStruct reelstrips_to_display)
        {
            end_reelstrips_to_display_sequence.Add(reelstrips_to_display);
        }

        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateMultipleEndReelStripsConfiguration(int amount)
        {
            end_reelstrips_to_display_sequence = new List<ReelStripsStruct>();
            for (int i = 0; i < amount; i++)
            {
                end_reelstrips_to_display_sequence.Add(GenerateReelStrips(matrix.reel_strip_managers).Result);
            }
         }
        internal Task<ReelStripsStruct> GenerateReelStrips(ReelStripManager[] reel_strip_managers)
        {
            ReelStripsStruct output = new ReelStripsStruct();
            output.reelstrips = new ReelStripStruct[reel_strip_managers.Length];
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                output.reelstrips[reel] = (new ReelStrip(GenerateEndingReelStrip(ref reel_strip_managers[reel]))).reelStrip;
            }
            return Task.FromResult<ReelStripsStruct>(output);
        }

        private SlotDisplaySymbol[] GenerateEndingReelStrip(ref ReelStripManager reelStripManager)
        {
            List<SlotDisplaySymbol> output = new List<SlotDisplaySymbol>();
            //Generate a symbol for each display zone slot
            for (int i = 0; i < reelStripManager.reelstrip_info.total_display_slots; i++)
            {
                output.Add(GetRandomWeightedSymbol());
            }
            return output.ToArray();
        }
        /// <summary>
        /// Generate a random symbol based on weights defined
        /// </summary>
        /// <returns></returns>
        public SlotDisplaySymbol GetRandomWeightedSymbol()
        {
            SlotDisplaySymbol output = new SlotDisplaySymbol();
            int symbol = matrix.symbol_weights.Draw();
            if(matrix.isSymbolOverlay(symbol))
            {
                output.SetOverlaySymbolTo(symbol);
                output.AddFeaturesTo(matrix.GetSymbolFeatures(symbol));
                while (matrix.isSymbolOverlay(symbol))
                {
                    symbol = matrix.symbol_weights.Draw();
                    //Set Overlay feature in list and freespin
                }
            }
            if (matrix.isFeatureSymbol(symbol))
            {
                output.AddFeaturesTo(matrix.GetSymbolFeatures(symbol));
            }
            if (matrix.isWildSymbol(symbol))
            {
                output.SetWildTo(symbol);
            }
            output.primary_symbol = symbol;
            //Debug.Log(String.Format("Symbol Generated form Weighted Distribution is {0}", ((Symbol)output).ToString()));
            return output;
        }

        internal ReelStripsStruct UseNextConfigurationInList()
        {
            return pop_end_reelstrips_to_display_sequence;
        }

        internal ReelStripsStruct GetCurrentConfiguration()
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
            return current_reelstrip_configuration;
        }

        internal void SetMatrixToReelConfiguration()
        {
            matrix.SetSymbolsToDisplayOnMatrixTo(current_reelstrip_configuration);
        }

        internal void AddConfigurationToSequence(ReelStripsStruct configuration)
        {
            if (end_reelstrips_to_display_sequence == null)
                end_reelstrips_to_display_sequence = new List<ReelStripsStruct>();
            //if valid configuration then add and move on
            end_reelstrips_to_display_sequence.Insert(0, configuration);
        }

        internal void AddConfigurationToSequence(Features feature)
        {
            ReelStripsStruct configuration = new ReelStripsStruct();
            switch (feature)
            {
                case Features.freespin:
                    configuration.reelstrips = new ReelStripStruct[matrix.reel_strip_managers.Length];
                    for (int i = 0; i < configuration.reelstrips.Length; i++)
                    {
                        configuration.reelstrips[i].spin_info.display_symbols = new SlotDisplaySymbol[3] 
                        { 
                            new SlotDisplaySymbol(i % 2 == 0 
                        ? (int)Symbol.SA01 : (int)Symbol.RO03),
                            new SlotDisplaySymbol((int)Symbol.RO01),
                            new SlotDisplaySymbol((int)Symbol.RO02)
                        };
                        configuration.reelstrips[i].spin_info.display_symbols[0].AddFeature(Features.freespin);
                    }
                    break;
                default:
                    break;
            }

            AddConfigurationToSequence(configuration);
        }
    }
}