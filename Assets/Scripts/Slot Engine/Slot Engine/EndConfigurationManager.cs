//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(EndConfigurationManager))]
    class EndConfigurationManagerEditor : BoomSportsEditor
    {
        EndConfigurationManager myTarget;
        SerializedProperty state;
        public void OnEnable()
        {
            myTarget = (EndConfigurationManager)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("EndConfigurationManager Properties");
            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("End Configuration Manager Controls");
            if(GUILayout.Button("Generate Reelstrips"))
            {
                myTarget.GenerateMultipleEndReelStripsConfiguration(5);
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

        //Ending Reelstrips current
        public ReelStrip[] end_reelstrips_to_display
        {
            get
            {
                if (end_reelstrips_to_display_sequence == null)
                    GenerateMultipleEndReelStripsConfiguration(5);
                if(end_reelstrips_to_display_sequence.Length == 0)
                    GenerateMultipleEndReelStripsConfiguration(5);
                //TODO Validate Data in Reel Strip then Generate if no valid data found
                return end_reelstrips_to_display_sequence[0];
            }
        }
        [SerializeField]
        public ReelStrip[][] end_reelstrips_to_display_sequence;
        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateEndReelStripsConfiguration()
        {
            SetEndingReelStripToDisplay(GenerateReelStrips(matrix.reel_strip_managers).Result);
        }

        private void SetEndingReelStripToDisplay(ReelStrip[] reelstrips_to_display)
        {
            List<ReelStrip[]> reelStrips= new List<ReelStrip[]>();
            reelStrips.Add(reelstrips_to_display);
            reelStrips.AddRange(end_reelstrips_to_display_sequence);
            end_reelstrips_to_display_sequence = reelStrips.ToArray();
        }

        /// <summary>
        /// Generates the Display matrix then runs payline evaluation
        /// </summary>
        /// <returns>Task.Completed</returns>
        internal async Task GenerateMultipleEndReelStripsConfiguration(int amount)
        {
            end_reelstrips_to_display_sequence = new ReelStrip[amount][];
            for (int i = 0; i < amount; i++)
            {
                end_reelstrips_to_display_sequence[i] = GenerateReelStrips(matrix.reel_strip_managers).Result;
            }
        }
        internal Task<ReelStrip[]> GenerateReelStrips(ReelStripManager[] reel_strip_managers)
        {
            List<ReelStrip> output = new List<ReelStrip>();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                output.Add(new ReelStrip(GenerateEndingReelStrip(3))); // TODO change to scale slot size
            }
            return Task.FromResult<ReelStrip[]>(output.ToArray());
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
            return matrix.intWeightedDistributionSymbols.Draw();
        }

        internal void RemoveCurrentDisplayReelConfiguration()
        {
            end_reelstrips_to_display_sequence.RemoveAt<ReelStrip[]>(0);
        }

        internal ReelStrip[] GetConfigurationToDisplay()
        {
            return end_reelstrips_to_display;
        }
    }
}