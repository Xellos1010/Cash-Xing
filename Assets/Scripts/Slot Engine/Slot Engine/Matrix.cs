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
            //if (Application.isPlaying)
            //{
            //    if (GUILayout.Button("Start Test Spin"))
            //    {
            //        myTarget.StartCoroutine(myTarget.spin_manager.StartSpin());
            //    }
            //    if (GUILayout.Button("End Test Spin"))
            //    {
            //        myTarget.spin_manager.InterruptSpin();
            //    }
            //}
            //else
            //{
            //    if (GUILayout.Button("Generate Ending Symbols"))
            //    {
            //        myTarget.slot_machine_managers.end_configuration_manager.GenerateEndReelStripsConfiguration();
            //    }
            //    if (GUILayout.Button("Set Symbols to End Symbols"))
            //    {
            //        myTarget.SetReelStripsEndConfiguration();
            //    }
            //}
            base.OnInspectorGUI();
        }
    }
#endif

    public class Matrix : MonoBehaviour
    {
        public ManagersReferenceScript slot_machine_managers
        {
            get
            {
                if (_slot_machine_managers == null)
                    _slot_machine_managers = transform.parent.GetComponentInChildren<ManagersReferenceScript>();
                return _slot_machine_managers;
            }
        }
        [SerializeField]
        internal ManagersReferenceScript _slot_machine_managers;

        private ReelStripStructDisplayZones[] display_zones_per_reelstrip;
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

        /// <summary>
        /// Controls how many slots to include in reel strip
        /// </summary>
        [SerializeField]
        internal int slots_per_strip_onSpinLoop = 50;

        public AnimatorOverrideController symbol_win_resolve;
        public AnimatorOverrideController symbol_lose_resolve;
        /// <summary>
        /// Reel Strip Managers that make up the matrix - Generated with MatrixGenerator Script - Order determines reel strip spin delay and Symbol Evaluation Logic
        /// </summary>
        [SerializeField]
        internal ReelStripManager[] reel_strip_managers; //Each reel strip has a manager 
        /// <summary>
        /// Used for logical gate to activate bonus mode in animator
        /// </summary>
        internal bool bonus_game_triggered = false;

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
        
        public string[] supported_symbols
        {
            get
            {
                Debug.Log("Getting supported symbols");
                List<string> output = new List<string>();
                List<WeightedDistribution.IntDistributionItem> distributionList = slot_machine_managers.symbols_weights.Items;
                for (int i = 0; i < distributionList.Count; i++)
                {
                    output.Add(distributionList[i].name);
                }
                return output.ToArray();
            }
        }
        void Start()
        {
            //Initialize Machine and Player  Information
            slot_machine_managers.machine_info_manager.InitializeTestMachineValues(10000.0f, 0.0f, slot_machine_managers.machine_info_manager.supported_bet_amounts.Length - 1, 1, 0);
            //slot_machine_managers.end_configuration_manager.GenerateMultipleEndReelStripsConfiguration(20);
            ReelStripsStruct stripInitial = slot_machine_managers.end_configuration_manager.pop_end_reelstrips_to_display_sequence;
            string print_string = PrintSpinSymbols(ref stripInitial);
            Debug.Log(String.Format("Initial pop of end_configiuration_manager = {0}", print_string));
            //This is temporary - we need to initialize the slot engine in a different scene then when preloading is done swithc to demo_attract.
            StateManager.SetStateTo(States.Idle_Intro);
        }

        internal void ReturnPositionsBasedOnPayline(ref Payline payline, out List<Vector3> out_positions)
        {
            out_positions = new List<Vector3>();
            int payline_positions_set = 0;
            for (int reel = payline.left_right ? 0 : reel_strip_managers.Length-1; 
                payline.left_right? reel < reel_strip_managers.Length : reel >= 0; 
                reel += payline.left_right? 1:-1)
            {
                Vector3 payline_posiiton_on_reel = Vector3.zero;
                try
                {
                    if (payline_positions_set < payline.payline_configuration.payline.Length)
                    { 
                        payline_posiiton_on_reel = ReturnPositionOnReel(ref reel_strip_managers[reel], payline.payline_configuration.payline[payline_positions_set]);
                        payline_positions_set += 1;
                        out_positions.Add(payline_posiiton_on_reel);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        private Vector3 ReturnPositionOnReel(ref ReelStripManager reel, int slot_in_reel)
        {
            return reel.positions_in_path_v3[reel.reelstrip_info.before_display_zone_slot_objects+slot_in_reel] + (Vector3.back * 10);
        }

        internal IEnumerator InitializeSymbolsForWinConfigurationDisplay()
        {
            SetSlotsAnimatorBoolTo(supported_bools.LoopPaylineWins,false);
            yield return 0;
        }
        internal Task SetSymbolsForWinConfigurationDisplay(WinningPayline winning_payline)
        {
            Debug.Log(String.Format("Showing Winning Payline {0} with winning symbols {1}",
                String.Join(" ", winning_payline.payline.payline_configuration.ToString()), String.Join(" ",winning_payline.winning_symbols)));
            //Get Winning Slots and loosing slots
            List<SlotManager> winning_slots,losing_slots;
            ReturnWinLoseSlots(winning_payline, out winning_slots, out losing_slots, ref reel_strip_managers);
            SetWinningSlotsToResolveWinLose(ref winning_slots,true);
            SetWinningSlotsToResolveWinLose(ref losing_slots,false);
            return Task.CompletedTask;
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
            //Iterate over each reel and get the winning slot

            int winning_symbols_added = 0;
            bool winning_slot_set = false;
            for (int reel = winning_payline.payline.left_right ? 0 : reel_managers.Length - 1;
                winning_payline.payline.left_right ? reel < reel_managers.Length : reel >= 0;
                reel += winning_payline.payline.left_right ? 1 : -1)
            {
                List<SlotManager> slots_decending_in_reel = reel_managers[reel].GetSlotsDecending();
                int first_display_slot = reel_managers[reel].reelstrip_info.before_display_zone_slot_objects;
                Debug.Log(String.Format("first_display_slot for reel {0} = {1}", reel, first_display_slot));
                for (int slot = first_display_slot; slot < slots_decending_in_reel.Count; slot++)
                {
                    if (winning_symbols_added < winning_payline.payline.payline_configuration.payline.Length && !winning_slot_set)
                    {
                        int winning_slot = winning_payline.payline.payline_configuration.payline[winning_symbols_added] + reel_managers[reel].reelstrip_info.before_display_zone_slot_objects;
                        if (slot == winning_slot)
                        {
                            Debug.Log(String.Format("Adding Winning Symbol on reel {0} slot {1}",reel, slot));
                            winning_slots.Add(slots_decending_in_reel[slot]);
                            winning_symbols_added += 1;
                            winning_slot_set = true;
                        }
                        else
                        {
                            losing_slots.Add(slots_decending_in_reel[slot]);
                        }
                    }
                    else
                    {
                        losing_slots.Add(slots_decending_in_reel[slot]);
                    }
                }
                winning_slot_set = false;
            }
        }
        
        internal void SlamLoopingPaylines()
        {
            StateManager.SetStateTo(States.Resolve_Outro);
        }

        internal void SetSystemToPresentWin()
        {
            slot_machine_managers.animator_statemachine_master.SetTrigger(supported_triggers.SpinResolve);
            slot_machine_managers.animator_statemachine_master.SetBool(supported_bools.ResolveSpin,true);
            slot_machine_managers.animator_statemachine_master.SetBool(supported_bools.WinRacking, true);
            SetSlotsAnimatorTrigger(supported_triggers.SpinResolve);
            SetSlotsAnimatorBoolTo(supported_bools.WinRacking, true);
        }

        internal void CycleWinningPaylinesMode()
        {
            slot_machine_managers.paylines_manager.PlayCycleWins();
            slot_machine_managers.animator_statemachine_master.SetTrigger(supported_triggers.SpinResolve);
        }

        internal async void SetTriggersByState(States state)
        {

        }

        internal void ActivateFreeSpins()
        {
            bonus_game_triggered = true;
            SetAllAnimatorsBoolTo(supported_bools.BonusActive, true);
        }

        private void SetAllAnimatorsBoolTo(supported_bools bool_to_set, bool value)
        {
            slot_machine_managers.animator_statemachine_master.SetBool(bool_to_set, value);
            SetSlotsAnimatorBoolTo(bool_to_set,value);
        }

        private void PrepareSlotMachineToSpin()
        {
            Debug.Log("Preparing Slot Machine for Spin");
        }

        internal void SetSymbolsToDisplayOnMatrixTo(ReelStripsStruct current_reelstrip_configuration)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].SetSymbolCurrentDisplayTo(current_reelstrip_configuration.reelstrips[reel]);
            }
        }

        private void SetSlotsAnimatorBoolTo(supported_bools bool_name, bool v)
        {
            Debug.Log(String.Format("Setting Slot Animator {0} to {1}",bool_name.ToString(),v));
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].state_machine.SetBool(bool_name,v);
                }
            }
        }

        /// <summary>
        /// Sets the reelstrips info for the matrix
        /// </summary>
        /// <param name="before_display_zone_objects_per_reel">number of display slot objects to generate before the display zone</param>
        /// <param name="display_zones_per_reel">The display zone breakdown per reel</param>
        /// <param name="after_display_zone_empty_positions_per_reel">The empty positions after each display zone to allow spin off</param>
        /// <param name="slot_size">Size of the slot prefab instantiated in each reel_strip</param>
        /// <returns></returns>
        public Task SetMatrixReelStripsInfo(int[] before_display_zone_objects_per_reel, ReelStripStructDisplayZones[] display_zones_per_reel, int[] after_display_zone_empty_positions_per_reel, Vector3 slot_size)
        {
            //Build reelstrip info 
            ReelStripsStruct reelstrips_configuration = new ReelStripsStruct(before_display_zone_objects_per_reel, display_zones_per_reel, after_display_zone_empty_positions_per_reel);

            SetSpinDirectionForReelStrip(ref reelstrips_configuration, Vector3.down);

            //Set Matrix Settings first then update the reel configuration 
            SetMatrixSettings(slot_size);
            UpdateReelsAndReelStripsSlotsPerReel(reelstrips_configuration);
            //TODO Update Slot Size
            return Task.CompletedTask;
        }
        /// <summary>
        /// Sets the spin direction for the reelstrip
        /// </summary>
        /// <param name="reelstrips_configuration">reference reelstrip configuration to set direction</param>
        /// <param name="direction">Direction to spin in</param>
        private void SetSpinDirectionForReelStrip(ref ReelStripsStruct reelstrips_configuration, Vector3 direction)
        {
            for (int reelstrip = 0; reelstrip < reelstrips_configuration.reelstrips.Length; reelstrip++)
            {
                reelstrips_configuration.reelstrips[reelstrip].SetSpinDirectionTo(direction);
            }
        }

        private void SetMatrixSettings(Vector3 slot_size)
        {
            SetSlotSize(slot_size);
            //TODO padding offset
        }

        private void SetSlotSize(Vector3 slot_size)
        {
            this.slot_size = slot_size;
        }

        /// <summary>
        /// Anytime this is called - the end_configuration, paylines managers need to update.
        /// </summary>
        /// <param name="slots_per_reelstrip"></param>
        private void UpdateReelsAndReelStripsSlotsPerReel(ReelStripsStruct reelstrips_configuration)
        {
            List<ReelStripManager> reelstrip_managers = FindReelstripManagers();
            SetReelStripManagersTo(reelstrips_configuration.reelstrips.Length, ref reelstrip_managers);

            //Ensure reels have slots
            for (int i = 0; i < reelstrips_configuration.reelstrips.Length; i++)
            {
                reelstrip_managers[i].SetReelConfigurationTo(reelstrips_configuration.reelstrips[i]);
            }
            reel_strip_managers = reelstrip_managers.ToArray();
        }
        /// <summary>
        /// Generate or remove reelstrip objects based on number of reels set
        /// </summary>
        /// <param name="number_of_reels">Reels in Configuration</param>
        /// <param name="reelstrip_managers">reference var to cached reelstrip_managers</param>
        private void SetReelStripManagersTo(int number_of_reels, ref List<ReelStripManager> reelstrip_managers)
        {
            //See whether we need to make more or subtract some 
            bool add_subtract_reels = reelstrip_managers.Count < number_of_reels ? true : false;
            //First we are going to ensure the amount of reels are the correct amount - then we are going to initialize the amount of slots per reel

            //If current reels generated are > or < matrix.length then need to adjust accordingly
            //Ensure enough reels are on the board then ensure all reels have slots
            for (int reel = add_subtract_reels ? reelstrip_managers.Count : reelstrip_managers.Count - 1;
                add_subtract_reels ? reel < number_of_reels : reel >= number_of_reels;
                reel = add_subtract_reels ? reel + 1 : reel - 1)
            {
                if (add_subtract_reels)
                {
                    reelstrip_managers.Add(GenerateReel(reel));
                }
                else
                {
                    if (reelstrip_managers[reel] != null)
                        Destroy(reelstrip_managers[reel].gameObject);
                    reelstrip_managers.RemoveAt(reel);
                }
            }
        }

        private List<ReelStripManager> FindReelstripManagers()
        {
            //Initialize reelstrips managers
            List<ReelStripManager> reelstrip_managers = new List<ReelStripManager>();
            if (reel_strip_managers == null)
            {
                ReelStripManager[] reelstrip_managers_intiialzied = transform.GetComponentsInChildren<ReelStripManager>(true);
                if (reelstrip_managers_intiialzied.Length < 1)
                    reel_strip_managers = new ReelStripManager[0];
                else
                    reel_strip_managers = reelstrip_managers_intiialzied;
            }
            //Load any reelstrip managers that are already initialized
            reelstrip_managers.AddRange(reel_strip_managers);
            return reelstrip_managers;
        }

        internal void GenerateReelStripsToSpinThru(ref ReelStripsStruct reel_configuration)
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(ref reel_strip_managers, ref reel_configuration, slots_per_strip_onSpinLoop);
        }

        private void GenerateReelStripsFor(ref ReelStripManager[] reel_strip_managers, ref ReelStripsStruct spin_configuration_reelstrip, int slots_per_strip_onSpinLoop)
        {
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                if (spin_configuration_reelstrip.reelstrips[i].spin_info.reel_spin_symbols?.Length != slots_per_strip_onSpinLoop)
                {
                    //Generates reelstrip based on weights
                    spin_configuration_reelstrip.reelstrips[i].spin_info.reel_spin_symbols = ReelStrip.GenerateReelStripStatic(slots_per_strip_onSpinLoop, slot_machine_managers.symbols_weights);
                }
                //Assign reelstrip to reel
                reel_strip_managers[i].reelstrip_info.SetSpinConfigurationTo(spin_configuration_reelstrip.reelstrips[i]);
            }
        }

        ReelStripManager GenerateReel(int reel_number)
        {
            //Create Reel Game Object
            
            Type[] gameobject_components = new Type[1];
            gameobject_components[0] = typeof(ReelStripManager);
            ReelStripManager output_reelstrip_manager = StaticUtilities.CreateGameobject<ReelStripManager>(gameobject_components,"Reel_" + reel_number, transform);
            return output_reelstrip_manager;
        }
        /// <summary>
        /// generate pre-slot objects or remove pre slot objects - generate display zone per reel - set after display zone positions and calculate the longest empty position path and apply to all reels. A reel may have slots turned on and off
        /// </summary>
        /// <param name="slots_per_reel"></param>
        /// <param name="display_zones_per_reel"></param>
        /// <param name="after_display_zone_empty_positions_per_reel"></param>
        internal void UpdateSlotObjectsPerReel(int[] slots_per_reel, ReelStripStructDisplayZones[] display_zones_per_reel, int[] after_display_zone_empty_positions_per_reel)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                //reel_strip_managers[reel].InitializeSlotsInReel(slots_per_reel[reel], this, reel_ending_padding);
            }
        }

        internal void ResetAnimatorsSlots()
        {
            slot_machine_managers.animator_statemachine_master.InitializeAnimator();
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
                reel_strip_managers[i].SetEndingDisplaySymbolsTo(_slot_machine_managers.end_configuration_manager.pop_end_reelstrips_to_display_sequence.reelstrips[i]);
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
                linePositions.Add(ReturnSlotPositionOnPayline(payline.payline_configuration.payline[reel], ref slots_decending_in_reel, ref reel_strip_managers[reel]));
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
            throw new Exception("Todo refactor");
            //return slots_decending_in_reel.GetRange(reelStripManager.reelstrip_info.display_zones, reelStripManager.reelstrip_info.display_zones.Length).ToArray();
        }

        internal void PlayerHasBet(float amount)
        {
            //Set the UI to remove player wallet amount and update the player information to remove amount
            OffetPlayerWalletBy(-amount);
        }

        internal void OffetPlayerWalletBy(float amount)
        {
            slot_machine_managers.machine_info_manager.OffsetPlayerAmountBy(amount);
        }

        private string PrintSpinSymbols(ref ReelStripsStruct stripInitial)
        {
            string output = "";
            for (int strip = 0; strip < stripInitial.reelstrips.Length; strip++)
            {
                output += ReturnDisplaySymbolsPrint(stripInitial.reelstrips[strip]);
            }
            return output;
        }

        private string ReturnDisplaySymbolsPrint(ReelStripStruct reelstrip_info)
        {
            return String.Join("|",reelstrip_info.spin_info.display_symbols);
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
                    PlayerHasBet(slot_machine_managers.machine_info_manager.bet_amount);
                    SetAllAnimatorsTriggerTo(supported_triggers.SpinStart, true);
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_Interrupt:
                    //Set the matrix and slots to spin interrupt
                    slot_machine_managers.animator_statemachine_master.SetTrigger(supported_triggers.SpinSlam);
                    SetSlotsAnimatorTrigger(supported_triggers.SpinSlam);
                    StateManager.SetStateTo(States.Spin_Outro);
                    break;
                case States.Spin_Outro:
                    break;
                case States.Spin_End:
                    break;
                case States.Resolve_Intro:
                    slot_machine_managers.racking_manager.StartRacking();
                    CycleWinningPaylinesMode();
                    break;
                case States.Resolve_Outro:
                    slot_machine_managers.paylines_manager.CancelCycleWins();
                    SetAllAnimatorsBoolTo(supported_bools.WinRacking, false);
                    break;
                case States.bonus_idle_intro:
                    SetAllAnimatorsTriggerTo(supported_triggers.SpinResolve,false);
                    SetAllAnimatorsBoolTo(supported_bools.ResolveSpin,false);
                    SetAllAnimatorsTriggerTo(supported_triggers.SpinSlam, false);
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

        private void SetAllAnimatorsTriggerTo(supported_triggers trigger, bool v)
        {
            if(v)
            {
                slot_machine_managers.animator_statemachine_master.SetTrigger(trigger);
                SetSlotsAnimatorTrigger(trigger);
            }
            else
            {
                slot_machine_managers.animator_statemachine_master.ResetTrigger(trigger);
                ResetSlotsAnimatorTrigger(trigger);
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

        private void ResetSlotsAnimatorTrigger(supported_triggers slot_to_trigger)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].ResetTrigger(slot_to_trigger);
                }
            }
        }

        void OnApplicationQuit()
        {
            StateManager.SetStateTo(States.None);
        }

        internal void SetPlayerWalletTo(float to_value)
        {
            slot_machine_managers.machine_info_manager.SetPlayerWalletTo(to_value);
        }
    }
}
public struct Matrix_Settings
{
    public int reels;

    //can be individual or overall set
    public int[] slots_per_reel;

    public Matrix_Settings(int reels, int slots_per_reel) : this()
    {
        this.reels = reels;
        this.slots_per_reel = SetSlotsPerReelTo(reels, slots_per_reel);
    }

    private int[] SetSlotsPerReelTo(int reels, int slots_per_reel)
    {
        int[] output = new int[reels];
        for (int reel = 0; reel < output.Length; reel++)
        {
            output[reel] = slots_per_reel;
        }
        return output;
    }
}