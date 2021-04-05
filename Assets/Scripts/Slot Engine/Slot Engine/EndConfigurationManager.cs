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
        SerializedProperty end_reelstrips_to_display_sequence;
        SerializedProperty current_reelstrip_configuration;
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
            list = new ReorderableList(serializedObject,serializedObject.FindProperty("end_reelstrips_to_display_sequence"),true,true,true,true);
            current_reelstrip_configuration = serializedObject.FindProperty("current_reelstrip_configuration");
        }
        public override async void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("EndConfigurationManager Properties");
            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            
            BoomEditorUtilities.DrawUILine(Color.white);
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
            if (current_reelstrip_configuration.type != null)
            {
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
    public class EndConfigurationManager : MonoBehaviour
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

        internal WeightedDistribution.IntDistribution symbol_weights
        {
            get
            {
                if(_symbol_weights.Items.Count < 1)
                {
                    for (int symbol = 0; symbol < matrix.symbols_in_matrix.symbols.Length; symbol++)
                    {
                        WeightedDistribution.IntDistributionItem item = matrix.symbols_in_matrix.symbols[symbol].symbol_weight_info;
                        _symbol_weights.Add(item.Value,item.Weight);
                    }
                }
                return _symbol_weights;
            }
        }
        internal WeightedDistribution.IntDistribution _symbol_weights;

        public ReelStripsStruct current_reelstrip_configuration;
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
                return current_reelstrip_configuration;
            }
        }

        private void SetAndRemoveConfiguration(int v)
        {
            if(v > end_reelstrips_to_display_sequence.Count)
            {
                GenerateMultipleEndReelStripsConfiguration(v);
            }
            //TODO Validate Data in Reel Strip then Generate if no valid data found
            SetCurrentConfigurationTo(end_reelstrips_to_display_sequence[v]);
            end_reelstrips_to_display_sequence.RemoveAt(v);
        }

        private void SetCurrentConfigurationTo(ReelStripsStruct reelstrips)
        {
            current_reelstrip_configuration = reelstrips;
        }

        public List<ReelStripsStruct> end_reelstrips_to_display_sequence;
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

        private int[] GenerateEndingReelStrip(ref ReelStripManager reelStripManager)
        {
            List<int> output = new List<int>();
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
        public int GetRandomWeightedSymbol()
        {
#if UNITY_EDITOR
            int output = matrix.symbol_weights.Draw();
#else
            int output = UnityEngine.Random.Range(0,((int)(Symbol.End))-2);
#endif
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
                        configuration.reelstrips[i].spin_info.display_symbols = new int[3] { i % 2 == 0 ? (int)Symbol.SA01 : (int)Symbol.RO03, (int)Symbol.RO01, (int)Symbol.RO02 };
                    }
                    break;
                default:
                    break;
            }

            AddConfigurationToSequence(configuration);
        }
    }
}