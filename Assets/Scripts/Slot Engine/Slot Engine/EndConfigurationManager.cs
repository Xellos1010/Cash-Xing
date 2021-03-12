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
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
            list = new ReorderableList(serializedObject,serializedObject.FindProperty("end_reelstrips_to_display_sequence"),true,true,true,true);
        }
        public override async void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("EndConfigurationManager Properties");
            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("End Configuration Manager Controls");
            if(GUILayout.Button("Generate Reelstrips"))
            {
                await myTarget.GenerateMultipleEndReelStripsConfiguration(500);
                serializedObject.ApplyModifiedProperties();
            }
            if(GUILayout.Button("Clear Reels Display"))
            {
                //myTarget.end_reelstrips_to_display_sequence = null;
            }
            if(GUILayout.Button("Pop Reel Configuration Test"))
            {
                Debug.Log(myTarget.pop_end_reelstrips_to_display_sequence.reelstrips.Length);
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
                    _matrix = GetComponent<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        private Matrix _matrix;

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
                output.reelstrips[reel] = (new ReelStrip(GenerateEndingReelStrip(3))).reelStrip; // TODO change to scale slot size
            }
            return Task.FromResult<ReelStripsStruct>(output);
        }

        private int[] GenerateEndingReelStrip(int v)
        {
            List<int> output = new List<int>();
            for (int i = 0; i < v; i++)
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
            int output = matrix.weighted_distribution_symbols.Draw();
#else
            int output = UnityEngine.Random.Range(0,((int)(Symbol.End))-2);
#endif
            Debug.Log(String.Format("Symbol Generated form Weighted Distribution is {0}", ((Symbol)output).ToString()));
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
    }
}