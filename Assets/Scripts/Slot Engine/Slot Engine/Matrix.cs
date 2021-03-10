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
        SerializedProperty ending_symbols;
        public void OnEnable()
        {
            myTarget = (Matrix)target;
            reel_spin_delay_ms = serializedObject.FindProperty("reel_spin_delay_ms");
            ending_symbols = serializedObject.FindProperty("ending_symbols");
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Matrix Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
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

    public class Matrix : MonoBehaviour
    {
        /**Managers needed by Slot Engine**/
        /// <summary>
        /// Manages the reference for the machine information manager
        /// </summary>
        [SerializeField]
        internal MachineInfoManager machine_information_manager;
        /// <summary>
        /// handles setting the reference to end configuration manager if one is not set.
        /// </summary>
        internal EndConfigurationManager end_configuration_manager
        {
            get
            {
                if (_end_configuration_manager == null)
                    _end_configuration_manager = GameObject.FindObjectOfType<EndConfigurationManager>();
                return _end_configuration_manager;
            }
        }
        /// <summary>
        /// Manages Reference to End Configuration Manager
        /// </summary>
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
                    _paylines_manager = GameObject.FindObjectOfType<PaylinesManager>();
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
        public string[] supported_symbols
        {
            get
            {
                Debug.Log("Getting supported symbols");
                List<string> output = new List<string>();
                List<WeightedDistribution.IntDistributionItem> distributionList = weighted_distribution_symbols.Items;
                for (int i = 0; i < distributionList.Count; i++)
                {
                    output.Add(distributionList[i].name);
                }
                return output.ToArray();
            }
        }
        public Vector3[] matrix; //elements are reels value is slot per reel
        /// <summary>
        /// What is the size of the slot in pixels
        /// </summary>
        public Vector3 slot_size;
        /// <summary>
        /// The padding between each slot
        /// </summary>
        public Vector3 padding;
        /// <summary>
        /// The padding between each reel
        /// </summary>
        public Vector2 reel_slot_padding = new Vector2(0, 1); //TODO set from Matrix Generator
        [SerializeField]
        private int reel_start_padding = 1;
        private int reel_ending_padding = 1;
        public AnimatorStateMachineManager animator_state_machine;
        
        public AnimatorOverrideController symbol_win_resolve;
        public AnimatorOverrideController symbol_lose_resolve;

        /// <summary>
        /// Controls the wegihted probability of symbols appearing on the reel
        /// </summary>
        internal WeightedDistribution.IntDistribution weighted_distribution_symbols
        {
            get
            {
                if(_weighted_distribution_symbols == null)
                {
                    _weighted_distribution_symbols = FindObjectOfType<WeightedDistribution.IntDistribution>();
                }
                return _weighted_distribution_symbols;
            }
        }

        internal void SetSymbolsForWinConfigurationDisplay(WinningPayline payline_to_show)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    AnimatorOverrideController to_controller = payline_to_show.IsSymbolOnWinningPayline(reel, slot) ? symbol_win_resolve : symbol_lose_resolve;
                    Debug.Log(String.Format("In Reel {0} Setting Symbol {1} with symbol of {2} Animation controller to {3}", reel_strip_managers[reel].name, reel_strip_managers[reel].slots_in_reel[slot].transform.name, reel_strip_managers[reel].slots_in_reel[slot].presentation_symbol, to_controller.name));
                    reel_strip_managers[reel].slots_in_reel[slot].SetOverrideControllerTo(to_controller);
                }
            }
        }

        [SerializeField]
        private WeightedDistribution.IntDistribution _weighted_distribution_symbols;

        /// <summary>
        /// Controls how many slots to include in reel strip
        /// </summary>
        [SerializeField]
        internal int slots_per_strip_onSpinLoop = 50;
        [SerializeField]
        internal RackingManager racking_manager;

        //TODO - Enable offset from 0,0,0
        public Task GenerateMatrix(Vector3[] matrix, Vector3 slot_size, Vector3 padding)
        {
            this.matrix = matrix;
            this.slot_size = slot_size;
            this.padding = padding;
            GenerateReels(matrix);
            return Task.CompletedTask;
        }

        private void GenerateReels(Vector3[] matrix)
        {
            List<ReelStripManager> reels_generated = new List<ReelStripManager>();
            if (reel_strip_managers == null)
                reel_strip_managers = new ReelStripManager[0];
            reels_generated.AddRange(reel_strip_managers);
            bool add_subtract_reels = reels_generated.Count < matrix.Length ? true:false;
            //If current reels generated are > or < matrix.length then need to adjust accordingly
            //Ensure enough reels are on the board then ensure all reels have slots
            for (int i = add_subtract_reels ? reels_generated.Count : reels_generated.Count - 1;
                add_subtract_reels ? i < matrix.Length : i >= matrix.Length;
                i = add_subtract_reels ? i++: i--)
            {
                if(add_subtract_reels)
                {
                    reels_generated.Add(GenerateReel(i));
                    reels_generated[i].InitializeSlotsInReel(matrix[i], this, reel_ending_padding);
                }
                else
                {
                    if(reels_generated[i] != null)
                        Destroy(reels_generated[i].gameObject);
                    reels_generated.RemoveAt(i);
                }
            }
            //Ensure reels have slots
            for (int i = 0; i < reels_generated.Count; i++)
            {
                reels_generated[i].InitializeSlotsInReel(matrix[i], this, reel_ending_padding);
            }
            reel_strip_managers = reels_generated.ToArray();
        }

        internal void GenerateReelStripsToSpinThru(ref ReelStrip[] reel_configuration)
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(ref reel_strip_managers, ref reel_configuration, slots_per_strip_onSpinLoop);
        }

        private void GenerateReelStripsFor(ref ReelStripManager[] reel_strip_managers, ref ReelStrip[] spin_configuration_reelstrip, int slots_per_strip_onSpinLoop)
        {
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                if (spin_configuration_reelstrip[i].reel_spin_symbols?.Length != slots_per_strip_onSpinLoop)
                {
                    //Generates reelstrip based on weights
                    spin_configuration_reelstrip[i].GenerateReelStrip(slots_per_strip_onSpinLoop, weighted_distribution_symbols);
                }
                //Assign reelstrip to reel
                reel_strip_managers[i].reel_strip_to_use_for_spin = spin_configuration_reelstrip[i];
            }
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
                reel_strip_managers[i].InitializeSlotsInReel(matrix[i],this, reel_ending_padding);
            }
        }

        internal void SetReelStripsEndConfiguration()
        {
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                reel_strip_managers[i].SetEndingDisplaySymbolsTo(_end_configuration_manager.pop_end_reelstrips_to_display_sequence[i]);
            }
        }

        internal void DisplayEndingReelStrips()
        {
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                reel_strip_managers[i].TestDisplayEndSymbols();
            }
        }

        internal void ReturnSymbolPositionsOnPayline(ref Payline payline, out List<Vector3> linePositions)
        {
            linePositions = new List<Vector3>();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                //Top slot cushion slot - use 1 as first element - If more than 1 cushion slot change logic to pull display slots from -2 end of array. Slots move off screen
                Vector3 position_cache = reel_strip_managers[reel].slots_in_reel[payline.ReturnSlotNumberFromReel(reel, reel_strip_managers.Length, reel_start_padding)].transform.position;
                position_cache = new Vector3(position_cache.x, position_cache.y, -10); //TODO Change Hardcoded Value
                                                                                       //TOOD change to get slot at position at path to return x and y
                linePositions.Add(position_cache);
            }
        }

        internal void PlayerHasBet(float amount)
        {
            //Set the UI to remove player wallet amount and update the player information to remove amount
            OffetPlayerWalletBy(-amount);
        }

        internal void OffetPlayerWalletBy(float amount)
        {
            machine_information_manager.OffsetPlayerAmountBy(amount);
        }

        void Start()
        {
            //Initialize Machine and Player  Information
            machine_information_manager.InitializeTestMachineValues(10000.0f, 0.0f, machine_information_manager.supported_bet_amounts.Length - 1, 1, 0);
        }
    }
}