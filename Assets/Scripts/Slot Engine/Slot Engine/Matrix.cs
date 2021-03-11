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
                    myTarget.spin_manager.InterruptSpin();
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

        internal void ReturnPositionsBasedOnPayline(ref int[] payline, out List<Vector3> out_positions)
        {
            out_positions = new List<Vector3>();
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                Vector3 position_cache = reel_strip_managers[i].positions_in_path_v3[payline[i] + reel_strip_managers[i].padding_slots_top];
                out_positions.Add(position_cache);
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
        internal int reel_start_padding = 1;
        internal int reel_ending_padding = 1;
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

        internal void SetSymbolsForWinConfigurationDisplay(WinningPayline winning_payline)
        {
            Debug.Log(String.Format("Showing Winning Payline {0} with winning symbols {1}",
                String.Join(" ", winning_payline.payline.payline.ToString()), String.Join(" ",winning_payline.winning_symbols)));
            //Get Winning Slots and loosing slots
            List<SlotManager> winning_slots,losing_slots;
            ReturnWinLoseSlots(winning_payline, out winning_slots, out losing_slots, ref reel_strip_managers);
            SetWinningSlotsToResolveWinLose(ref winning_slots,true);
            SetWinningSlotsToResolveWinLose(ref losing_slots,false);
        }

        private void SetWinningSlotsToResolveWinLose(ref List<SlotManager> winning_slots, bool v)
        {
            for (int slot = 0; slot < winning_slots.Count; slot++)
            {
                if (v)
                    winning_slots[slot].SetSymbolResolveWin();
                else
                    winning_slots[slot].SetSymbolResolveToLose();
            }
        }

        private void ReturnWinLoseSlots(WinningPayline winning_payline, out List<SlotManager> winning_slots, out List<SlotManager> losing_slots, ref ReelStripManager[] reel_managers)
        {
            winning_slots = new List<SlotManager>();
            losing_slots = new List<SlotManager>();
            //Implement check left right
            int winning_symbols_added = 0;
            for (int reel = winning_payline.left_right ? 0 : reel_managers.Length-1;
                winning_payline.left_right ? reel < reel_managers.Length: reel >= 0; 
                reel += winning_payline.left_right ? 1 : -1)
            {
                List<SlotManager> slots_decending_in_reel = reel_managers[reel].GetSlotsDecending();
                if (winning_symbols_added >= winning_payline.winning_symbols.Length)
                {
                    //to_controller = symbol_lose_resolve;
                    losing_slots.AddRange(slots_decending_in_reel);
                }
                else
                {
                    for (int slot = reel_managers[reel].padding_slots_top; slot < slots_decending_in_reel.Count; slot++)
                    {
                        if (slot == (winning_payline.payline.payline[reel] + reel_managers[reel].padding_slots_top))
                        {
                            winning_slots.Add(slots_decending_in_reel[slot]);
                            winning_symbols_added += 1;
                        }
                        else
                        {
                            losing_slots.Add(slots_decending_in_reel[slot]);
                        }
                }
            }
            }
        }
        internal void SlamLoopingPaylines()
        {
            StateManager.SetStateTo(States.Resolve_Outro);

        }

        internal void SetSystemToPresentWin()
        {

            animator_state_machine.SetTrigger(supported_triggers.SpinResolve);
            animator_state_machine.SetBool(supported_bools.WinRacking, true);
            SetSlotsAnimatorTrigger(supported_triggers.SpinResolve);
            SetSlotsAnimatorBoolTo(supported_bools.WinRacking, true);
        }

        internal void CycleWinningPaylinesMode()
        {
            SetSlotsAnimatorBoolTo(supported_bools.LoopPaylineWins,true);
            paylines_manager.PlayCycleWins();
        }

        internal async void SetTriggersByState(States state)
        {
            switch (state)
            {
                case States.None:
                    break;
                case States.preloading:
                    break;
                case States.Coin_In:
                    break;
                case States.Coin_Out:
                    break;
                case States.Idle_Intro:
                    //Reset all Triggers and bools and set state for slots to idle idle
                    SetSlotsAnimatorTrigger(supported_triggers.SpinResolve);
                    animator_state_machine.SetTrigger(supported_triggers.SpinResolve);
                    break;
                case States.Idle_Idle:
                    PrepareSlotMachineToSpin();
                    break;
                case States.Idle_Outro:
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_Interrupt:
                    break;
                case States.Spin_Outro:
                    break;
                case States.Spin_End:
                    break;
                case States.Resolve_Intro:
                    break;
                
                case States.Resolve_Win_Idle:
                    break;
                case States.Resolve_Lose_Idle:
                    break;
                case States.Resolve_Lose_Outro:
                    break;
                case States.Resolve_Win_Outro:
                    break;
                case States.win_presentation:
                    break;
                case States.racking_start:
                    break;
                case States.racking_loop:
                    break;
                case States.racking_end:
                    break;
                case States.feature_transition_out:
                    break;
                case States.feature_transition_in:
                    break;
                case States.total_win_presentation:
                    break;
                default:
                    break;
            }
        }

        private void PrepareSlotMachineToSpin()
        {
            //for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            //{
            //    for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
            //    {

            //    }
            //}
        }

        private void SetSlotsAnimatorBoolTo(supported_bools bool_name, bool v)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].state_machine.SetBool(bool_name,v);
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
                i = add_subtract_reels ? i+1: i-1)
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

        internal void ResetAnimatorsSlots()
        {
            animator_state_machine.InitializeAnimator();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].ResetAnimator();
                }
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
            //Return List Slots In Order 0 -> -
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                //Get current slot order based on slot transform compared to positions in path.
                List<SlotManager> slots_decending_in_reel = reel_strip_managers[reel].GetSlotsDecending();
                //Cache the position of the slot that we need from this reel
                linePositions.Add(ReturnSlotPositionOnPayline(payline.payline[reel], ref slots_decending_in_reel, ref reel_strip_managers[reel]));
            }
        }

        private Vector3 ReturnSlotPositionOnPayline(int payline_slot, ref List<SlotManager> slots_decending_in_reel, ref ReelStripManager reelStripManager)
        {
            //Calculate the reel display area - Take Display Slots and start is
            SlotManager[] display_slots = ReturnDisplaySlots(ref slots_decending_in_reel, ref reelStripManager);
            return display_slots[payline_slot].transform.position;
        }

        private SlotManager[] ReturnDisplaySlots(ref List<SlotManager> slots_decending_in_reel, ref ReelStripManager reelStripManager)
        {
            return slots_decending_in_reel.GetRange(reelStripManager.padding_slots_top, reelStripManager.display_slots).ToArray();
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
            //This is temporary - we need to initialize the slot engine in a different scene then when preloading is done swithc to demo_attract.
            StateManager.SetStateTo(States.Idle_Intro);
        }

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
        }
        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }
        /// <summary>
        /// Matrix State Machine
        /// </summary>
        /// <param name="State"></param>
        private async void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.None:
                    break;
                case States.preloading:
                    break;
                case States.Coin_In:
                    break;
                case States.Coin_Out:
                    break;
                case States.Idle_Intro:
                    //Reset the state of all slots to 
                    ResetAnimatorsSlots();
                    //Fall thru to Idle_Idle State - ATM the animator falls thru Idle_Intro
                    StateManager.SetStateTo(States.Idle_Idle);
                    break;
                case States.Idle_Idle:
                    break;
                case States.Idle_Outro:
                    //Decrease Bet Amount
                    PlayerHasBet(machine_information_manager.bet_amount);
                    animator_state_machine.SetTrigger(supported_triggers.SpinStart);
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_Interrupt:
                    //Set the matrix and slots to spin interrupt
                    animator_state_machine.SetTrigger(supported_triggers.SpinSlam);
                    SetSlotsAnimatorTrigger(supported_triggers.SpinSlam);
                    StateManager.SetStateTo(States.Spin_Outro);
                    break;
                case States.Spin_Outro:
                    break;
                case States.Spin_End:
                    break;
                case States.Resolve_Intro:
                    racking_manager.GetRackingInformation();
                    CycleWinningPaylinesMode();
                    break;
                case States.Resolve_Outro:
                    paylines_manager.CancelCycleWins();
                    SetSlotsAnimatorBoolTo(supported_bools.WinRacking, false);
                    animator_state_machine.SetBool(supported_bools.WinRacking, false);
                    await Task.Delay(1000);
                    StateManager.SetStateTo(States.Idle_Intro);
                    break;
                case States.Resolve_Win_Idle:
                    break;
                case States.Resolve_Lose_Idle:
                    break;
                case States.Resolve_Lose_Outro:
                    break;
                case States.Resolve_Win_Outro:
                    break;
                case States.win_presentation:
                    break;
                case States.racking_start:
                    break;
                case States.racking_loop:
                    break;
                case States.racking_end:
                    break;
                case States.feature_transition_out:
                    break;
                case States.feature_transition_in:
                    break;
                case States.total_win_presentation:
                    break;
                default:
                    break;
            }
        }

        private async Task WaitAllReelsStopSpinning()
        {
            bool task_lock = true;
            while (task_lock)
            {
                for (int reel = 0; reel < reel_strip_managers.Length; reel++)
                {
                    if(reel_strip_managers[reel].are_slots_spinning)
                    {
                        await Task.Delay(100);
                    }
                    else
                    {
                        task_lock = false;
                    }
                }
            }
        }

        private void SetSlotsAnimatorTrigger(supported_triggers slot_to_trigger)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].SetTriggerTo(slot_to_trigger);
                }
            }
        }

        void OnApplicationQuit()
        {
            StateManager.SetStateTo(States.None);
        }
    }
}