//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(Matrix))]
    class MatrixEditor : BoomSportsEditor
    {
        Matrix myTarget;
        SerializedProperty state;
        SerializedProperty reel_spin_delay_ms;
        SerializedProperty supported_symbols;
        SerializedProperty ending_symbols;
        public void OnEnable()
        {
            myTarget = (Matrix)target;
            reel_spin_delay_ms = serializedObject.FindProperty("reel_spin_delay_ms");
            supported_symbols = serializedObject.FindProperty("supported_symbols");
            ending_symbols = serializedObject.FindProperty("ending_symbols");
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Matrix Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            EditorGUILayout.PropertyField(supported_symbols);
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Matrix Controls");
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start Test Spin"))
                {
                    myTarget.spin_manager.StartSpin();
                }
                if (GUILayout.Button("End Test Spin"))
                {
                    myTarget.spin_manager.EndSpin();
                }
            }
            else
            {
                if (GUILayout.Button("Generate Ending Symbols"))
                {
                    myTarget.end_configuration_manager.GenerateEndReelStripsConfiguration();
                }
                if (GUILayout.Button("Set Symbols to End Symbols"))
                {
                    myTarget.SetReelStripsEndConfiguration();
                }
                if (GUILayout.Button("Display End Reels and Reset"))
                {
                    myTarget.DisplayEndingReelStrips();
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif

    [RequireComponent(typeof(SpinManager))]
    [RequireComponent(typeof(PaylinesManager))]
    public class Matrix : MonoBehaviour
    {
        /**Managers needed by matrix**/
        internal EndConfigurationManager end_configuration_manager
        {
            get
            {
                if (_end_configuration_manager == null)
                    _end_configuration_manager = GetComponent<EndConfigurationManager>();
                return _end_configuration_manager;
            }
        }
        [SerializeField]
        private EndConfigurationManager _end_configuration_manager;

        /// <summary>
        /// symbols_material_manager internal get accessor - Sets symbols_material_manager reference to GetComponent<SymbolMaterialsManager>()
        /// </summary>
        internal SymbolMaterialsManager symbols_material_manager
        {
            get
            {
                if (_symbols_material_manager == null)
                    _symbols_material_manager = GameObject.FindObjectOfType<SymbolMaterialsManager>();
                return _symbols_material_manager;
            }
        }
        /// <summary>
        /// symbols_material_manager reference
        /// </summary>
        [SerializeField]
        internal SymbolMaterialsManager _symbols_material_manager;
        /// <summary>
        /// paylines_manager internal get accessor - Sets paylines_manager reference to GetComponent<PaylinesManager>()
        /// </summary>
        internal PaylinesManager paylines_manager
        {
            get
            {
                if (_paylines_manager == null)
                    _paylines_manager = GetComponent<PaylinesManager>();
                return _paylines_manager;
            }
        }
        /// <summary>
        /// paylines_manager reference
        /// </summary>
        [SerializeField]
        private PaylinesManager _paylines_manager;
        /// <summary>
        /// Reel Strip Managers that make up the matrix - Generated with MatrixGenerator Script - Order determines reel strip spin delay and Symbol Evaluation Logic
        /// </summary>
        [SerializeField]
        internal ReelStripManager[] reel_strip_managers; //Each reel strip has a manager 
        internal ReelStripManager[] reel_strips_forward_to_back
        {
            get
            {
                return reel_strip_managers;
            }
        }
        internal ReelStripManager[] reel_strips_back_to_forward
        {
            get
            {
                return Enumerable.Reverse(reel_strip_managers).ToArray();
            }
        }
        internal SpinManager spin_manager
        {
            get
            {
                if (_spin_manager == null)
                    _spin_manager = GetComponent<SpinManager>();
                return _spin_manager;
            }
        }
        [SerializeField]
        private SpinManager _spin_manager;
        
        public string[] supported_symbols = new string[10] { "MA01", "MI01", "MI02", "MI03", "RO01", "RO02", "RO03", "SA_01", "SA_02", "BW01" }; //when a matrix is generated - only 1 configuration can be loaded at a time

        public Vector3[] matrix; //elements are reels value is slot per reel
        public Vector3 slot_size;
        public Vector3 padding;
        public Vector2 reel_slot_padding = new Vector2(0, 1); //TODO set from Matrix Generator

        internal void ReturnSymbolPositionsOnPayline(ref Payline payline, out List<Vector3> linePositions)
        {
            linePositions = new List<Vector3>();
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                //Top slot cushion slot - use 1 as first element - If more than 1 cushion slot change logic to pull display slots from -2 end of array. Slots move off screen
                Vector3 position_cache = reel_strip_managers[i].slots_in_reel[payline.payline[i] + 1].transform.position;                 
                position_cache = new Vector3(position_cache.x, position_cache.y, -10); //TODO Change Hardcoded Value
                                                                                       //TOOD change to get slot at position at path to return x and y
                linePositions.Add(position_cache);
            }
        }

        /// <summary>
        /// Controls the wegihted probability of symbols appearing on the reel
        /// </summary>
        internal WeightedDistribution.IntDistribution intWeightedDistributionSymbols
        {
            get
            {
                if(_intWeightedDistributionSymbols == null)
                {
                    _intWeightedDistributionSymbols = GetComponent<WeightedDistribution.IntDistribution>();
                }
                return _intWeightedDistributionSymbols;
            }
        }
        [SerializeField]
        private WeightedDistribution.IntDistribution _intWeightedDistributionSymbols;
        /// <summary>
        /// uses predefeined reelstrips to loop thru on spin loop
        /// </summary>
        [SerializeField]
        internal bool use_reelstrips_for_spin_loop;
        [SerializeField]
        internal int slots_per_strip_onSpinLoop = 50;

        //TODO - Enable offset from 0,0,0
        public Task GenerateMatrix(Vector3[] matrix, Vector3 slot_size, Vector3 padding)
        {
            this.matrix = matrix;
            this.slot_size = slot_size;
            this.padding = padding;
            if (transform.childCount > 0) // Destroy old matrix if you have one
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
            GenerateReels(matrix);
            return Task.CompletedTask;
        }

        private void GenerateReels(Vector3[] matrix)
        {
            List<ReelStripManager> newReelList = new List<ReelStripManager>();
            if (reel_strip_managers == null)
                reel_strip_managers = new ReelStripManager[0];
            newReelList.AddRange(reel_strip_managers);
            if(newReelList.Count < matrix.Length)
            {
                for (int i = 0; i < matrix.Length; i++)
                {
                    if(i == newReelList.Count)
                    {
                        newReelList.Add(GenerateReel(i));
                    }
                    newReelList[i].InitializeSlotsInReel(matrix[i], this);
                }
            }
            reel_strip_managers = newReelList.ToArray();
        }

        internal void GenerateReelStripsToSpinThru()
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(ref reel_strip_managers,end_configuration_manager.end_reelstrips_to_display, slots_per_strip_onSpinLoop);
        }

        private void GenerateReelStripsFor(ref ReelStripManager[] reel_strip_managers, ReelStrip[] spin_configuration_reelstrip, int slots_per_strip_onSpinLoop)
        {
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                //Generates reelstrip based on weights
                spin_configuration_reelstrip[i].GenerateReelStrip(slots_per_strip_onSpinLoop, intWeightedDistributionSymbols);
                //Assign reelstrip to reel
                reel_strip_managers[i].reel_strip_to_use_for_spin = spin_configuration_reelstrip[i];
            }
        }

        private void GenerateReelStripsFor(ref ReelStripManager[] reel_strip_managers)
        {
            throw new NotImplementedException();
        }

        ReelStripManager GenerateReel(int reel_number)
        {
            //Create Reel Game Object
            Type[] ReelType = new Type[1];
            ReelType[0] = typeof(ReelStripManager);
            //Debug.Log("Creating Reel Number " + ReelNumber);
            ReelStripManager ReturnValue = new GameObject("Reel_" + reel_number, ReelType).GetComponent<ReelStripManager>();
            //Debug.Log("Setting Transform and Parent Reel-" + ReelNumber);
            //Position object in Game Space based on Reel Number Then set the parent
            ReturnValue.transform.parent = transform;
            ReturnValue.reel_number = reel_number;
            return ReturnValue;
        }

        internal void UpdateNumberOfSlotsInReel()
        {
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                reel_strip_managers[i].InitializeSlotsInReel(matrix[i],this);
            }
        }

        internal void SetReelStripsEndConfiguration()
        {
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                reel_strip_managers[i].SetEndingDisplaySymbolsTo(_end_configuration_manager.end_reelstrips_to_display[i]);
            }
        }

        internal void DisplayEndingReelStrips()
        {
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                reel_strip_managers[i].TestDisplayEndSymbols();
            }
        }

    }
}