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
using WeightedDistribution;

namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StripConfigurationObject))]
    class MatrixEditor : BoomSportsEditor
    {
        StripConfigurationObject myTarget;
        SerializedProperty state;
        SerializedProperty reel_spin_delay_ms;
        SerializedProperty ending_symbols;
        public void OnEnable()
        {
            myTarget = (StripConfigurationObject)target;
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
            if (GUILayout.Button("Ensure Slots Display PresentationID for symbol currently active"))
            {
                myTarget.SyncCurrentSymbolDisplayed();
            }
            if (GUILayout.Button("Create Empty Animation Container"))
            {
                myTarget.CreateEmptyAnimationContainer();
            }
            if (GUILayout.Button("Set Slot Container Animator sub states"))
            {
                myTarget.SetSubStatesAllSlotAnimatorStateMachines();
            }
            if (GUILayout.Button("Set Slot Container Animator Sync States"))
            {
                myTarget.SetAllSlotAnimatorSyncStates();
            }
            if (GUILayout.Button("Clear Slot Container Animator sub states"))
            {
                myTarget.ClearSubStatesAllSlotAnimatorStateMachines();
            }
            if (GUILayout.Button("Set Managers State Machine Sub States"))
            {
                myTarget.SetManagerStateMachineSubStates();
            }
            if (GUILayout.Button("Set Reel Numbers"))
            {
                myTarget.SetStripInfoStruct();
            }
            if(GUILayout.Button("Set Weights from symbols data"))
            {
                myTarget.SetSymbolWeightsByState();
                //myTarget.EnsureWeightsAreCorrect();
            }
            base.OnInspectorGUI();
        }
    }

#endif
    public class StripConfigurationObject : BaseConfigurationObject
    {
        //Hacks for InstaSpin
        //public AnimatorOverrideController[] characterTier;
        //public Animator character;
        //public AnimatorOverrideController[] multiplierTier;
        //public Animator multiplierChar;

        public Animator background;
        public AnimatorOverrideController[] backgroundACO;
        public TMPro.TextMeshPro freespinText;

        public Animator rackingRollupAnimator;
        public TMPro.TextMeshPro rackingRollupText;
        //Hacks for InstaSpin


        /// <summary>
        /// Controls how many slots to include in reel strip generated ob
        /// </summary>
        [SerializeField]
        internal int slotsPerStripLoop = 50;

        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[][] positions_in_path_v3_global
        {
            get
            {
                if (configurationGroupManagers != null)
                    if (configurationGroupManagers.Length > 0)
                    {
                        List<Vector3[]> temp = new List<Vector3[]>();
                        StripObjectGroupManager temp2;
                        for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                        {
                            temp2 = configurationGroupManagers[reel] as StripObjectGroupManager;
                            temp.Add(temp2.localPositionsInStrip);
                        }
                        return temp.ToArray();
                    }
                Debug.Log("No local positions available from reelStrip managers - Check ConfigurationObject");
                return null;
            }
        }
        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[][] positions_in_path_v3_local
        {
            get
            {
                if(configurationGroupManagers != null)
                    if(configurationGroupManagers.Length > 0)
                    {
                        StripObjectGroupManager temp2;
                        List<Vector3[]> temp = new List<Vector3[]>();
                        for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                        {
                            temp2 = configurationGroupManagers[reel] as StripObjectGroupManager;
                            temp.Add(temp2.localPositionsInStrip);
                        }
                        return temp.ToArray();
                    }
                Debug.Log("No local positions available from reelStrip managers - Check ConfigurationObject");
                return null;
            }
        }

        internal void ReturnPositionsBasedOnPayline(ref Payline payline, out List<Vector3> out_positions)
        {
            out_positions = new List<Vector3>();
            int payline_positions_set = 0;
            bool rootNodeHit = false;
            Vector3 payline_posiiton_on_reel;
            for (int strip = payline.left_right ? 0 : configurationGroupManagers.Length-1; 
                payline.left_right? strip < configurationGroupManagers.Length : strip >= 0; 
                strip += payline.left_right? 1:-1)
            {
                //need to see if root node is on reel - if not then next reel
                if (!rootNodeHit)
                {
                    if (strip == payline.rootNode.column)
                    {
                        Debug.Log($"reel = {strip} payline.rootNode.column = {payline.rootNode.column}");
                        rootNodeHit = true;
                    }
                }
                if (rootNodeHit)
                {
                    try
                    {
                        if (payline_positions_set < payline.payline_configuration.payline.Length)
                        {
                            payline_posiiton_on_reel = ReturnPositionOnStripForPayline(ref configurationGroupManagers[strip], payline.payline_configuration.payline[payline_positions_set]);
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
        }


        internal async Task PlayFeatureAnimation(List<SuffixTreeNodeInfo> overlaySymbols)
        {
            //this.multiplierChar.SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), true);
            Debug.Log("Playing Feature Animation");
            List<Animator> symbolAnimators = new List<Animator>(); 
            for (int i = 0; i < overlaySymbols.Count; i++)
            {
                symbolAnimators.Add(SetAnimatorFeatureTriggerAndReturn(overlaySymbols[i]));
            }
            //Play the Feature Animation thru
            bool lock_function = true;
            while (lock_function)
            {
                for (int i = 0; i < symbolAnimators.Count; i++)
                {
                    if (isAnimatorThruState("Feature_Outro", symbolAnimators[i]))
                    {
                        if (i == symbolAnimators.Count - 1)
                        {
                            lock_function = false;
                        }
                    }
                    else
                    {
                        await Task.Delay(300);
                    }
                }
            }
            ////Lerp to Multiplier Bank
            //for (int i = 0; i < overlaySymbols.Count; i++)
            //{
            //    await LerpToMeFinished(symbolAnimators[i].transform);
            //    _managers.machine_info_manager.SetMultiplierTo(_managers.machine_info_manager.machineInfoScriptableObject.multiplier + 1);
            //}

            //await isAnimatorThruStateAndAtPauseState(this.multiplierChar, "Feature_Outro");
            //this.multiplierChar.SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), false);
            for (int i = 0; i < symbolAnimators.Count; i++)
            {
                symbolAnimators[i].SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), false);
            }
        }

        public bool isLerping = false;
        private async Task LerpToMeFinished(Transform transform)
        {
            isLerping = true;
            managers.multiplierLerpToMe.lerpComplete += MultiplierLerpToMe_lerpComplete; 
            managers.multiplierLerpToMe.AddLerpToMeObject(transform);
            while(isLerping)
            {
                await Task.Delay(100);
            }
        }
        private void MultiplierLerpToMe_lerpComplete()
        {
            Debug.Log("isLerping = False");
            isLerping = false;
            managers.multiplierLerpToMe.lerpComplete -= MultiplierLerpToMe_lerpComplete;
        }
        internal override async Task WaitForSymbolToResolveState(string state)
        {
            if (current_payline_displayed != null)
            {
                List<BaseObjectManager> winning_slots, losing_slots;
                ReturnWinLoseSlots(current_payline_displayed, out winning_slots, out losing_slots, ref configurationGroupManagers);
                bool is_all_animations_resolve_intro = false;
                while (!is_all_animations_resolve_intro)
                {
                    for (int slot = 0; slot < winning_slots.Count; slot++)
                    {
                        if (!winning_slots[slot].isSymbolAnimationFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                        if (slot == winning_slots.Count - 1)
                            is_all_animations_resolve_intro = true;
                    }
                }
                current_payline_displayed = null;
                winning_slots = new List<BaseObjectManager>();
                losing_slots = new List<BaseObjectManager>();
            }
        }

        internal Features[] GetSymbolFeatures(int symbol)
        {
            return managers.evaluationManager.GetSymbolFeatures(symbolDataScriptableObject.symbols[symbol]);
        }

        private Vector3 ReturnPositionOnStripForPayline(ref BaseObjectGroupManager group, int indexInGroup)
        {
            StripObjectGroupManager temp = group as StripObjectGroupManager;
            //Debug.Log($"Transforming Local Position temp.localPositionsInStrip[{indexInGroup}]{temp.localPositionsInStrip[indexInGroup]} to world space {group.transform.TransformPoint(temp.localPositionsInStrip[indexInGroup] + (Vector3.back * 10))}");
            return group.transform.TransformPoint(temp.localPositionsInStrip[indexInGroup] + (Vector3.back * 10));
        }

        internal IEnumerator InitializeSymbolsForWinConfigurationDisplay()
        {
            SetSlotsAnimatorBoolTo(supportedAnimatorBools.LoopPaylineWins,false);
            yield return 0;
        }
        private WinningPayline current_payline_displayed;
        internal Task SetSymbolsForWinConfigurationDisplay(WinningPayline winning_payline)
        {
            //Debug.Log(String.Format("Showing Winning Payline {0} with winning symbols {1}",String.Join(" ", winning_payline.payline.payline_configuration.ToString()), String.Join(" ",winning_payline.winning_symbols)));
            //Get Winning Slots and loosing slots
            current_payline_displayed = winning_payline;
            ReturnWinLoseSlots(winning_payline, out winning_slots, out losing_slots, ref configurationGroupManagers);
            SetSlotsToResolveWinLose(ref winning_slots,true);
            SetSlotsToResolveWinLose(ref losing_slots,false);
            return Task.CompletedTask;
        }

        private void ReturnWinLoseSlots(WinningPayline winningPayline, out List<BaseObjectManager> winningSlots, out List<BaseObjectManager> losingSlots, ref BaseObjectGroupManager[] objectGroupManagers)
        {
            winningSlots = new List<BaseObjectManager>();
            losingSlots = new List<BaseObjectManager>();
            //Iterate over each reel and get the winning slot

            int winning_symbols_added = 0;
            bool winning_slot_set = false;
            //Left Rigth should be determined in order objectGroupManagers come in
            for (int reel = winningPayline.payline.left_right ? 0 : objectGroupManagers.Length - 1;
                winningPayline.payline.left_right ? reel < objectGroupManagers.Length : reel >= 0;
                reel += winningPayline.payline.left_right ? 1 : -1)
            {
                List<BaseObjectManager> slots_decending_in_reel = objectGroupManagers[reel].GetSlotsDecending();
                int first_display_slot = objectGroupManagers[reel].configurationGroupDisplayZones.paddingBefore;
                //Debug.Log(String.Format("first_display_slot for reel {0} = {1}", reel, first_display_slot));
                for (int slot = first_display_slot; slot < slots_decending_in_reel.Count; slot++)
                {
                    if (winning_symbols_added < winningPayline.payline.payline_configuration.payline.Length && !winning_slot_set)
                    {
                        int winning_slot = winningPayline.payline.payline_configuration.payline[winning_symbols_added] + objectGroupManagers[reel].configurationGroupDisplayZones.paddingBefore;
                        if (slot == winning_slot)
                        {
                            //Debug.Log(String.Format("Adding Winning Symbol on reel {0} slot {1}",reel, slot));
                            winningSlots.Add(slots_decending_in_reel[slot]);
                            winning_symbols_added += 1;
                            winning_slot_set = true;
                        }
                        else
                        {
                            losingSlots.Add(slots_decending_in_reel[slot]);
                        }
                    }
                    else
                    {
                        losingSlots.Add(slots_decending_in_reel[slot]);
                    }
                }
                winning_slot_set = false;
            }
        }

        internal void SlamLoopingPaylines()
        {
            //Pause and interrupt racking before continue
            managers.racking_manager.PauseRackingOnInterrupt();
            StateManager.SetStateTo(States.Resolve_Outro);
        }

        internal void SetSystemToPresentWin()
        {
            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true);
        }

        internal void CycleWinningPaylinesMode()
        {
            managers.paylines_manager.PlayCycleWins();
        }
        /// <summary>
        /// Set Free spin information for all Animators
        /// </summary>
        /// <param name="onOff"></param>
        internal void ToggleFreeSpinActive(bool onOff)
        {
            Debug.Log("Bonus Active = " + onOff);
            StateManager.bonusGameTriggered = onOff;
            SetAllAnimatorsBoolTo(supportedAnimatorBools.BonusActive, onOff);
            if (!onOff)
                managers.machine_info_manager.SetMultiplierTo(0);
        }

        internal void SetAllAnimatorsBoolTo(supportedAnimatorBools bool_to_set, bool value)
        {
            managers.animator_statemachine_master.SetBoolAllStateMachines(bool_to_set, value);
            SetSlotsAnimatorBoolTo(bool_to_set,value);
        }

        private void PrepareSlotMachineToSpin()
        {
            Debug.Log("Preparing Slot Machine for Spin");
        }

        internal void SetSymbolsToDisplayOnConfigurationObjectTo(StripSpinStruct[] current_reelstrip_configuration)
        {
            Debug.Log($"stripManagers.Length = {configurationGroupManagers.Length}");
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                Debug.Log($"stripManagers[{reel}].SetSymbolEndSymbolsAndDisplay({String.Join("|",current_reelstrip_configuration[reel].GetAllDisplaySymbols())})");
                configurationGroupManagers[reel].SetSymbolEndSymbolsAndDisplay(current_reelstrip_configuration[reel]);
            }
        }

        private void SetSlotsAnimatorBoolTo(supportedAnimatorBools bool_name, bool v)
        {
            //Debug.Log(String.Format("Setting Slot Animator {0} to {1}",bool_name.ToString(),v));
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                {
                    configurationGroupManagers[reel].objectsInGroup[slot].SetBoolStateMachines(bool_name,v);
                }
            }
        }

        private List<StripObjectGroupManager> FindReelstripManagers()
        {
            //Initialize reelstrips managers
            List<StripObjectGroupManager> reelstrip_managers = new List<StripObjectGroupManager>();
            if (configurationGroupManagers == null)
            {
                StripObjectGroupManager[] reelstrip_managers_intiialzied = transform.GetComponentsInChildren<StripObjectGroupManager>(true);
                if (reelstrip_managers_intiialzied.Length < 1)
                    configurationGroupManagers = new StripObjectGroupManager[0];
                else
                    configurationGroupManagers = reelstrip_managers_intiialzied;
            }
            //Load any reelstrip managers that are already initialized
            reelstrip_managers.AddRange(configurationGroupManagers.Cast<StripObjectGroupManager>());
            return reelstrip_managers;
        }

        internal void GenerateReelStripsToLoop(ref StripSpinStruct[] reelConfiguration)
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(configurationGroupManagers.Cast<StripObjectGroupManager>().ToArray(), ref reelConfiguration, slotsPerStripLoop);
        }

        private void GenerateReelStripsFor(StripObjectGroupManager[] reelStripManagers, ref StripSpinStruct[] spinConfiguration, int slots_per_strip_onSpinLoop)
        {
            EndConfigurationManager temp;
            temp = managers.endConfigurationManager;
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reelStripManagers.Length; i++)
            {
                if (spinConfiguration[i].stripSpinSymbols?.Length != slots_per_strip_onSpinLoop)
                {
                    //Generates reelstrip based on weights
                    spinConfiguration[i].stripSpinSymbols = Strip.GenerateReelStripStatic(StateManager.enCurrentMode,slots_per_strip_onSpinLoop, ref temp);
                }
                //Assign reelstrip to reel
                reelStripManagers[i].stripInfo.SetSpinConfigurationTo(spinConfiguration[i]);
            }
        }


        internal void InitializeAllAnimators()
        {
            managers.animator_statemachine_master.InitializeAnimator();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                {
                    configurationGroupManagers[reel].objectsInGroup[slot].stateMachine.InitializeAnimator();
                }
            }
        }

        internal void ReturnSymbolPositionsOnPayline(ref Payline payline, out List<Vector3> linePositions)
        {
            linePositions = new List<Vector3>();
            //Return List Slots In Order 0 -> -
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                //Get current slot order based on slot transform compared to positions in path.
                List<BaseObjectManager> slots_decending_in_reel = configurationGroupManagers[reel].GetSlotsDecending();
                //Cache the position of the slot that we need from this reel
                linePositions.Add(ReturnSlotPositionOnPayline(payline.payline_configuration.payline[reel], ref slots_decending_in_reel, ref configurationGroupManagers[reel]));
            }
        }

        private Vector3 ReturnSlotPositionOnPayline(int payline_slot, ref List<BaseObjectManager> slots_decending_in_reel, ref BaseObjectGroupManager reelStripManager)
        {
            //Calculate the reel display area - Take Display Slots and start is
            BaseObjectManager[] display_slots = ReturnDisplaySlots(ref slots_decending_in_reel, ref reelStripManager);
            return display_slots[payline_slot].transform.position;
        }

        private BaseObjectManager[] ReturnDisplaySlots(ref List<BaseObjectManager> slots_decending_in_reel, ref BaseObjectGroupManager baseGroupManager)
        {
            //throw new Exception("Todo refactor");
            return slots_decending_in_reel.GetRange(baseGroupManager.configurationGroupDisplayZones.paddingBefore, baseGroupManager.configurationGroupDisplayZones.displayZones.Length - baseGroupManager.configurationGroupDisplayZones.paddingBefore).ToArray();
        }

        internal void PlayerHasBet(float amount)
        {
            //Set the UI to remove player wallet amount and update the player information to remove amount
            OffetPlayerWalletBy(-amount);
        }

        internal void OffetPlayerWalletBy(double amount)
        {
            managers.machine_info_manager.OffsetPlayerAmountBy(amount);
        }

        private string PrintSpinSymbols(ref StripSpinStruct[] stripInitial)
        {
            string output = "";
            for (int strip = 0; strip < stripInitial.Length; strip++)
            {
                output += ReturnDisplaySymbolsPrint(stripInitial[strip]);
            }
            return output;
        }

        private string ReturnDisplaySymbolsPrint(StripSpinStruct reelstrip_info)
        {
            return String.Join("|",reelstrip_info.displaySymbols);
        }

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
            StateManager.featureTransition += StateManager_FeatureTransition;
            StateManager.gameModeSetTo += StateManager_gameModeSetTo;
        }

        private void StateManager_gameModeSetTo(GameModes modeActivated)
        {
            switch (modeActivated)
            {
                case GameModes.baseGame:
                    break;
                case GameModes.overlaySpin:
                    break;
                case GameModes.freeSpin:
                    break;
                default:
                    break;
            }
        }

        private async void StateManager_add_to_multiplierAsync(int multiplier)
        {
            
        }

        private async Task isAnimatorThruState(Animator animator, string state)
        {
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            bool wait = true;
            while (wait)
            {
                state_info = animator.GetCurrentAnimatorStateInfo(0);

            Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                //Check if time has gone thru
                if (!state_info.IsName(state))
                {
                    wait = false;
                }
                else
                {
                    await Task.Delay(300);
                }
            }
        }
        private async Task isAnimatorsThruState(Animator[] animators, string state)
        {
            //TODO Refactor to set each animator into its own cancelable task that can wait for all until continue - this is to support parallel task management and cancelable tokens
            for (int animator = 0; animator < animators.Length; animator++)
            {
                await isAnimatorThruState(animators[animator], state);
            }
        }
        /// <summary>
        /// Used to receive and execute functions based on feature active or inactive
        /// </summary>
        /// <param name="feature">feature reference</param>
        /// <param name="active_inactive">state</param>
        private void StateManager_FeatureTransition(Features feature, bool active_inactive)
        {
            //Debug.Log(String.Format("Feature Transition for matrix = ",feature.ToString()));
            switch (feature)
            {
                case Features.freespin:
                    ToggleFreeSpinActive(active_inactive);
                    if(active_inactive)
                    {
                        background.runtimeAnimatorController = backgroundACO[0];
                        background.SetBool(supportedAnimatorBools.BonusActive.ToString(),true);
                    }
                    else
                    {
                        background.SetBool(supportedAnimatorBools.BonusActive.ToString(), false);
                    }
                    break;
                case Features.multiplier:
                    ToggleFreeSpinActive(active_inactive);
                    background.runtimeAnimatorController = backgroundACO[1];
                    break;
                default:
                    StateManager.bonusGameTriggered = false;
                    break;
            }
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
            StateManager.featureTransition -= StateManager_FeatureTransition;
            StateManager.gameModeSetTo -= StateManager_gameModeSetTo;
            managers.racking_manager.rackEnd -= Racking_manager_rackEnd;
        }
        /// <summary>
        /// Matrix State Machine
        /// </summary>
        /// <param name="State"></param>
        private async void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Intro:
                    //Reset the state of all slots to
                    await isAllMainAnimatorsThruState("Idle_Intro");
                    //Fall thru to Idle_Idle State - ATM the animator falls thru Idle_Intro
                    StateManager.SetStateTo(States.Idle_Idle);
                    break;
                case States.Idle_Outro:
                    //Decrease Bet Amount
                    PlayerHasBet(managers.machine_info_manager.machineInfoScriptableObject.bet_amount);
                    await StartAnimatorSpinAndSpinState();
                    break;
                case States.Spin_End:
                    bool resolve_intro = false;
                    Debug.LogWarning($"mode = {StateManager.enCurrentMode} Bonus Game triggered = {StateManager.bonusGameTriggered} FreeSpins Remaining = {managers.machine_info_manager.machineInfoScriptableObject.freespins} Multiplier = {managers.machine_info_manager.machineInfoScriptableObject.multiplier}");
                    
                    //Can Trigger Free Spins without having a win.
                    if (CheckForWin()) //If overlay has won will have winning payline
                    {
                        await CycleWinningPaylinesOneShot();
                        //await CheckForOverlayAndPlay();
                        //Bonus Game needs to be set before entering freespins mode and after last spin is accounted for
                        if (!StateManager.bonusGameTriggered)
                        {
                            Debug.Log("Win in Base Game Triggered");
                            //Calculate Rack Amount and either skip resolve phase or 
                            //Proceed to next state and sync state machine
                            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true);
                            SetFreespinTextTo(String.Format("{0:C2} Total Won this Spin", GetTotalSpinAmountWon()));
                            if (StateManager.enCurrentMode != GameModes.baseGame)
                                StateManager.SetFeatureActiveTo(Features.freespin, false);
                            Debug.LogWarning("Setting Resolve Intro true");
                            resolve_intro = true;
                        }
                        else
                        {
                            //Rack win to bank and continue to next spin
                            Debug.Log("There is a win and bonus game triggeres");
                            //Bank Lerped Already
                            //slot_machine_managers.machine_info_manager.OffsetBankBy(slot_machine_managers.paylines_manager.GetTotalWinAmount());
                            SetFreespinTextTo(String.Format("{0:C2} Total Won this Spin", GetTotalSpinAmountWon()));
                            ToggleTMPFreespin(true);
                            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, false); // dont rack wins
                            if (managers.machine_info_manager.machineInfoScriptableObject.freespins > 0)
                            {
                                Debug.LogWarning($"Freespins Remaining = {managers.machine_info_manager.machineInfoScriptableObject.freespins}");
                            }
                            else
                            {
                                Debug.LogWarning("Setting Resolve Intro true");
                                resolve_intro = true;
                            }
                        }
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                    }
                    else
                    {
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                        if (StateManager.bonusGameTriggered)
                        {
                            Debug.Log(String.Format("Bonus Game no win", managers.machine_info_manager.machineInfoScriptableObject.bank));
                            if (managers.machine_info_manager.machineInfoScriptableObject.freespins <= 0)
                            {
                                Debug.Log("Bonus Game No Win No Freespins- Setting Resolve Intro to True");
                                resolve_intro = true;
                            }
                        }
                        else if (!StateManager.bonusGameTriggered && managers.machine_info_manager.machineInfoScriptableObject.bank > 0)
                        {
                            Debug.Log(String.Format("Bonus game ended and bank has amount to rack = {0}", managers.machine_info_manager.machineInfoScriptableObject.bank));
                            resolve_intro = true;                        
                        }
                        else
                        {
                            Debug.Log(String.Format("Base Game no win - return to idle", managers.machine_info_manager.machineInfoScriptableObject.bank));
                        }
                    }
                    await isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
                    await isAllSlotSubAnimatorsReady("Spin_Outro");
                    
                    if (resolve_intro)
                    {
                        Debug.Log("Playing Resolve Into in Spin End");
                        SetOverridesBasedOnTiers();
                        await Task.Delay(20);
                        SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true);
                        SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.SpinResolve, true);
                        await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                        await isAllSlotSubAnimatorsReady("Resolve_Intro");
                        StateManager.SetStateTo(States.Resolve_Intro);
                    }
                    else
                    {
                        Debug.Log("Not Playing Resolve Outro in Spin End");
                        //If the spin has ended and there are no wining paylines or freespins left then disable freespin mode

                        if (managers.machine_info_manager.machineInfoScriptableObject.freespins <= 0 && StateManager.bonusGameTriggered)
                        {
                            Debug.Log($"Freespins = {managers.machine_info_manager.machineInfoScriptableObject.freespins} Setting Freespins Inactive");
                            StateManager.SetFeatureActiveTo(Features.freespin, false);
                        }
                        SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.SpinResolve, true);
                        await isAllSlotSubAnimatorsReady("Idle_Intro");
                        await isAllAnimatorsThruStateAndAtPauseState("Idle_Idle");
                        if (StateManager.bonusGameTriggered)
                            StateManager.SetStateTo(States.bonus_idle_intro);
                        else
                        {
                            StateManager.SetStateTo(States.Idle_Idle);
                        }
                    }
                    break;
                case States.Resolve_Intro:
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                    Debug.Log($"Resolve Intro state entered - Bonus Game Triggered = {StateManager.bonusGameTriggered} multiplier = {managers.machine_info_manager.machineInfoScriptableObject.multiplier} Toal Win = {GetTotalSpinAmountWon()}");
                    //The amount total even with multiplier
                    double totalSpinAwardeded = GetTotalSpinAmountWon();
                    double totalAwardeded = totalSpinAwardeded + managers.machine_info_manager.machineInfoScriptableObject.bank + totalSpinAwardeded;
                    //If multiplier > 0 then combine to present total
                    await DisplayWinAmount(totalSpinAwardeded, managers.machine_info_manager.machineInfoScriptableObject.bank);
                    //Check to reset freespin state now that everything is calculated
                    if (managers.machine_info_manager.machineInfoScriptableObject.freespins <= 0)
                    {
                        Debug.Log($"Resolve Intro Freespins = {managers.machine_info_manager.machineInfoScriptableObject.freespins} Setting Freespins Inactive");
                        StateManager.SetFeatureActiveTo(Features.freespin, false);
                    }
                    WinningObject[] winningObjects = managers.evaluationManager.ReturnWinningObjects();
                    if (winningObjects.Length > 0)
                    {
                        if (StateManager.enCurrentMode != GameModes.freeSpin)
                            CycleWinningPaylinesMode();
                        else if (StateManager.enCurrentMode == GameModes.freeSpin && (managers.machine_info_manager.machineInfoScriptableObject.freespins == 0 || managers.machine_info_manager.machineInfoScriptableObject.freespins == 10))
                            CycleWinningPaylinesMode();
                    }
                    break;
                case States.Resolve_Outro:
                    await managers.paylines_manager.CancelCycleWins();
                    //TODO Refactor hack
                    if (managers.machine_info_manager.machineInfoScriptableObject.freespins < 1)
                    {
                        SetFreespinTextTo("");
                        freespinText.enabled = false;
                    }
                    else
                    {
                        managers.machine_info_manager.SetFreeSpinsTo(managers.machine_info_manager.machineInfoScriptableObject.freespins);
                    }
                    //Set animator to Resolve_Outro State
                    SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, false);
                    SetAllAnimatorsBoolTo(supportedAnimatorBools.LoopPaylineWins, false);
                    //If going back to base game initialize vars for next bonus trigger
                    if (StateManager.enCurrentMode == GameModes.baseGame)
                    {
                        SetAllAnimatorsBoolTo(supportedAnimatorBools.BonusActive, false);
                        //ensure multiplier set to 0
                        managers.machine_info_manager.ResetMultiplier();
                    }
                    //Wait for animator to play all resolve outro animations
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Outro");
                    await isAllSlotAnimatorsReady("Resolve_Outro");
                    //Need to refactor to integrate
                    //await isAnimatorThruStateAndAtPauseState(this.multiplierChar,"Resolve_Outro");
                    SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.ResolveEnd,true);
                    //this.multiplierChar.SetTrigger(supportedAnimatorTriggers.ResolveEnd.ToString());
                    if (!StateManager.bonusGameTriggered)
                    {                        //TODO wait for all animators to go thru idle_intro state
                        StateManager.SetStateTo(States.Idle_Intro);
                    }
                    else
                    {
                        StateManager.SetStateTo(States.bonus_idle_intro);
                    }
                    break;
                case States.bonus_idle_intro:
                    await isAllMainAnimatorsThruState("Idle_Intro");
                    StateManager.SetStateTo(States.bonus_idle_idle);
                    break;
                case States.bonus_idle_outro:
                    ReduceFreeSpinBy(1);
                    await StartAnimatorSpinAndSpinState();
                    break;
            }
        }

        private async Task CheckForOverlayAndPlay()
        {
            if (managers.evaluationManager.overlaySymbols.Count > 0)
            {
                Debug.Log("Overlay in winning Line - Setting to play feature");
                if (managers.machine_info_manager.machineInfoScriptableObject.bank > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 ||
        managers.paylines_manager.GetTotalWinAmount() > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9)
                {
                    Debug.Log("Present Character Big Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 2);
                }
                else if (managers.machine_info_manager.machineInfoScriptableObject.bank > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5 ||
                    managers.paylines_manager.GetTotalWinAmount() > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5)
                {
                    Debug.Log("Present Character Medium Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 1);

                }
                else
                {
                    Debug.Log("Present Character Small Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 0);
                }
                await PlayFeatureAnimation(managers.evaluationManager.overlaySymbols);
                Debug.Log("All Overlay Animators are finished");
            }
        }

        private async Task CycleWinningPaylinesOneShot()
        {
            Debug.LogWarning("Implement Cycle Winning Paylines Oneshot");
            //await slotMachineManagers.paylines_manager.CyclePaylinesOneShot();
        }

        private void ToggleTMPFreespin(bool v)
        {
            freespinText.enabled = v;
        }

        private double GetTotalSpinAmountWon()
        {
            double output = 0;
            output = managers.paylines_manager.GetTotalWinAmount();

            //if (!StateManager.bonusGameTriggered)
            //{
            //    //First offset bank by win then rack
            //    //TODO have offset occur when winning payline is animated to bank
            //}
            //else
            //{
            //    if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
            //    {
            //        output = slot_machine_managers.paylines_manager.GetTotalWinAmount() * slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier;
            //    }
            //    else
            //    {
            //        output = slot_machine_managers.paylines_manager.GetTotalWinAmount();
            //    }
            //}
            return output;
        }

        //public Animator plateGraphicAnimatorWinBank;
        private async Task DisplayWinAmount(double spinWinAmount, double bankAmount)
        {
            SetFreespinTextTo(String.Format("{0:C2} {1}", spinWinAmount, " Won This Spin!"));
            ToggleMeshRendererForGameObject(rackingRollupText.gameObject, false);
            //If you are 9X bet or you have a multiplier win - multiplier win will be phased out for own mechanism
            if (spinWinAmount + bankAmount > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 || managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
            {
                //Present and set callback event for bigwin display
                PresentBigWinDisplayAnimator();
                managers.racking_manager.rackEnd += CloseBigWinDisplay;
                //This is a special feature for instaspin - multiplier smash. Need to build in UI based event sequencer
                //if (managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
                //{
                //    //await Smash together the Multiplier and Bank text Field.
                //    Animator[] winbankMultiplier = new Animator[1] { plateGraphicAnimatorWinBank };
                //    SetAnimatorsToTriggerFeature(winbankMultiplier, true);
                //    await isAllAnimatorsThruStateAndAtPauseStateTriggerEventAt(winbankMultiplier, States.Resolve_Win_Idle.ToString(), 1f, PresentBigWinDisplayAnimator);
                //    SetAnimatorsToTriggerFeature(winbankMultiplier, false);
                //    spinWinAmount = (bankAmount + spinWinAmount) * managers.machine_info_manager.machineInfoScriptableObject.multiplier;
                //}
            }
            SetRackingtextTo(spinWinAmount);
            ToggleMeshRendererForGameObject(rackingRollupText.gameObject, true);
            managers.machine_info_manager.machineInfoScriptableObject.bank = spinWinAmount;
            //Waits to present amount then racks down
            await Task.Delay(2000);
            managers.racking_manager.rackEnd += Racking_manager_rackEnd;
            amountRacked = spinWinAmount;
            managers.racking_manager.StartRacking();//spinWinAmount); //This is to resolve wins in resolve intro
        }

        private void SetRackingtextTo(double spinWinAmount)
        {
            Debug.Log(String.Format("{0:C2}", spinWinAmount));
            rackingRollupText.text = String.Format("{0:C2}", spinWinAmount);
        }

        private void ToggleMeshRendererForGameObject(GameObject gameObject, bool v)
        {
            MeshRenderer textRenderer = gameObject.GetComponent<MeshRenderer>();
            textRenderer.enabled = v;
        }

        private void SetFreespinTextTo(string toText)
        {
            Debug.Log($"Setting Freespin Text To {toText}");
            freespinText.text = toText;
        }

        //Temporary placeholder to hold amount to rack so at end display won amount in freespin bar
        private double amountRacked;
        private void Racking_manager_rackEnd()
        {
            Debug.Log("Matrix Recieved rack end ");
            SetFreespinTextTo(String.Format("{0:C2} {1}", amountRacked, " Total Winnings"));
            ToggleTMPFreespin(true);
            amountRacked = 0;
            managers.racking_manager.rackEnd -= Racking_manager_rackEnd;
        }

        private void PresentBigWinDisplayAnimator()
        {
            rackingRollupAnimator.SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), true);
            rackingRollupAnimator.SetBool(supportedAnimatorBools.SymbolResolve.ToString(), true);
        }

        private void SetAnimatorsToTriggerFeature(Animator[] animators,bool onOff)
        {
            for (int animator = 0; animator < animators.Length; animator++)
            {
                animators[animator].SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), onOff);
                animators[animator].SetBool(supportedAnimatorBools.SymbolResolve.ToString(), onOff);
            }
        }

        private void CloseBigWinDisplay()
        {
            rackingRollupAnimator.SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), false);
            rackingRollupAnimator.SetBool(supportedAnimatorBools.SymbolResolve.ToString(), false);
            managers.racking_manager.rackEnd -= CloseBigWinDisplay;
        }

        internal async Task isAnimatorThruStateAndAtPauseState(Animator multiplier, string state)
        {
            AnimatorStateInfo state_info;
            bool wait = true;
            while (wait)
            {
                state_info = multiplier.GetCurrentAnimatorStateInfo(0);

                Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                //Check if time has gone thru
                if (state_info.IsName(state) && state_info.normalizedTime >= 1)
                {
                    wait = false;
                }
                else
                {
                    await Task.Delay(300);
                }
            }
        }

        private void SetOverridesBasedOnTiers()
        {
            Debug.Log(String.Format("slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank  = {0} slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 = {1} slot_machine_managers.paylines_manager.GetTotalWinAmount() = {2}", managers.machine_info_manager.machineInfoScriptableObject.bank, managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9, managers.paylines_manager.GetTotalWinAmount()));
            if (managers.machine_info_manager.machineInfoScriptableObject.bank > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 ||
                managers.paylines_manager.GetTotalWinAmount() > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9)
            {
                Debug.Log("Present Big Win First");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier,2);
            }
            else if (managers.machine_info_manager.machineInfoScriptableObject.bank > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5 ||
                managers.paylines_manager.GetTotalWinAmount() > managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5)
            {
                Debug.Log("Presenting Medium win");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier, 1);

            }
            else
            {
                Debug.Log("Presenting Small win");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier, 0);
            }
        }

        private void SetAnimatorOverrideControllerTo(ref Animator character, ref AnimatorOverrideController[] characterTier, int v)
        {
            Debug.Log("No Character Controllers in this project");
            //character.runtimeAnimatorController = characterTier[v];
        }

        private void PlayAnimationOnAllSlots(string v)
        {
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                {
                    //Temporary fix because Animator decides to sometimes play an animation override and sometimes not
                    configurationGroupManagers[reel].objectsInGroup[slot].PlayAnimationOnPresentationSymbol("Resolve_Outro");
                }
            }
        }

        private async Task StartAnimatorSpinAndSpinState()
        {
            SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.StartSpin, true);
            await isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
            await isAllSlotSubAnimatorsReady("Idle_Outro");
            //Tell the spin manager to start spinning - animator is ready
            managers.spin_manager.SetSpinStateTo(SpinStates.spin_start);
        }

        private async Task isAllSlotSubAnimatorsReady(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                {
                    for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!configurationGroupManagers[reel].objectsInGroup[slot].isAllAnimatorsFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == configurationGroupManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal bool CheckForWin()
        {
            return managers.evaluationManager.ReturnWinningObjects().Length > 0 ? true : false;
        }
        internal async Task isAllSlotAnimatorsThruState(string state)
        {
            await isAllAnimatorsThruState( GetAllSlotAnimators(), state);
        }

        private Animator[] GetAllSlotAnimators()
        {
            List<Animator> output = new List<Animator>();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                configurationGroupManagers[reel].AddSlotAnimatorsToList(ref output);
            }
            return output.ToArray();
        }

        internal async Task isAllMainAnimatorsThruState(string state)
        {
            await isAllAnimatorsThruState(_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync, state);
        }

        internal async Task isAllAnimatorsThruState(Animator[] animators, string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            AnimatorStateInfo next_state_info;
            bool wait = true;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);

                    while (wait)
                    {
                        state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);
                        //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                        //Check if time has gone thru
                        if (!state_info.IsName(state))
                        {
                            wait = false;
                        }
                        else
                        {
                            await Task.Delay(300);
                        }
                    }
                    //Debug.Log("All States Resolved");
                    if (state_machine == animators.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal async Task isAllAnimatorsThruStateAndAtPauseState(Animator[] animators,string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    if (Application.isPlaying)
                    {
                        if (isAnimatorThruState(state, animators[state_machine]))
                        {
                            if (state_machine == animators.Length - 1)
                            {
                                is_all_animators_resolved = true;
                            }
                        }
                        else
                        {
                            await Task.Delay(300);
                        }
                    }
                    else
                    {
                        is_all_animators_resolved = true;
                        break;
                    }
                }
            }
        }

        internal delegate void presentBigWin();

        internal async Task isAllAnimatorsThruStateAndAtPauseStateTriggerEventAt(Animator[] animators, string state,float triggerEventAt, presentBigWin presentBigWin)
        {
            //Check all animators are on given state before continuing
            bool isAllAnimatorResolved = false;
            bool isEventTriggered = false;
            AnimatorStateInfo state_info;
            while (!isAllAnimatorResolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    if(!isEventTriggered)
                    {
                        state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);
                        if ((state_info.normalizedTime >= triggerEventAt) && state_machine > animators.Length)
                        {
                            presentBigWin();
                            isEventTriggered = true;
                        }
                    }
                    if (isAnimatorThruState(state, animators[state_machine]))
                    {
                        if (state_machine == animators.Length - 1)
                        {
                            isAllAnimatorResolved = true;
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            }
        }
        internal async Task isAllSlotAnimatorsReadyAndAtPauseState(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                {
                    for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!configurationGroupManagers[reel].objectsInGroup[slot].isSymbolAnimatorFinishedAndAtPauseState(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == configurationGroupManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }
        internal async Task isAllAnimatorsThruStateAndAtPauseState(string state)
        {
            await isAllAnimatorsThruStateAndAtPauseState(_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync,state);
        }

        internal bool isAnimatorThruState(string state_to_pause_on, Animator state_machine)
        {
            AnimatorStateInfo state_info;
            state_info = state_machine.GetCurrentAnimatorStateInfo(0);
            //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
            //normalized time - int is amount of times looped - float is percent animation complete
            while (state_info.normalizedTime < 1 || !state_info.IsName(state_to_pause_on))
            {
                return false;
                //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2} state length = {3}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else", state_info.length));
            }
            return true;
        }

        internal async Task isAllSlotAnimatorsReady(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                {
                    for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!configurationGroupManagers[reel].objectsInGroup[slot].isSymbolAnimationFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == configurationGroupManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        private void ReduceFreeSpinBy(int amount)
        {
            if(managers.machine_info_manager.machineInfoScriptableObject.freespins > 0)
            {
                managers.machine_info_manager.SetFreeSpinsTo(managers.machine_info_manager.machineInfoScriptableObject.freespins - amount);
            }
        }

        internal void SetAllAnimatorsTriggerTo(supportedAnimatorTriggers trigger, bool v)
        {
            if(v)
            {
                managers.animator_statemachine_master.SetAllTriggersTo(trigger);
                SetSlotsAnimatorTriggerTo(trigger);
            }
            else
            {
                managers.animator_statemachine_master.ResetAllTrigger(trigger);
                ResetSlotsAnimatorTrigger(trigger);
            }
        }

        private async Task WaitAllReelsStopSpinning()
        {
            bool task_lock = true;
            while (task_lock)
            {
                for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
                {
                    if(configurationGroupManagers[reel].are_slots_spinning)
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

        private void SetSlotsAnimatorTriggerTo(supportedAnimatorTriggers slot_to_trigger)
        {
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                {
                    configurationGroupManagers[reel].objectsInGroup[slot].SetTriggerTo(slot_to_trigger);
                    configurationGroupManagers[reel].objectsInGroup[slot].SetTriggerSubStatesTo(slot_to_trigger);
                }
            }
        }

        private void ResetSlotsAnimatorTrigger(supportedAnimatorTriggers slot_to_trigger)
        {
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                for (int slot = 0; slot < configurationGroupManagers[reel].objectsInGroup.Length; slot++)
                {
                    configurationGroupManagers[reel].objectsInGroup[slot].ResetTrigger(slot_to_trigger);
                    configurationGroupManagers[reel].objectsInGroup[slot].ResetTriggerSubStates(slot_to_trigger);
                }
            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application Quit");
            StateManager.SetStateTo(States.None);
        }

        internal void SetPlayerWalletTo(float to_value)
        {
            managers.machine_info_manager.SetPlayerWalletTo(to_value);
        }
        internal void SetStripInfoStruct()
        {
            Debug.Log($"Base call for SetStripInfoStruct in {gameObject.name} Display Zones in Base Call = {managers.configurationObject.configurationSettings.PrintDisplayZones()}");
            SetStripInfoStruct(managers.configurationObject);
        }
        internal void SetStripInfoStruct(StripConfigurationObject configurationObject)
        {
            for (int strip = 0; strip < configurationGroupManagers.Length; strip++)
            {
                Debug.Log($"configurationObject.configurationSettings.displayZones[{strip}] = {configurationObject.configurationSettings.displayZones[strip]}");
                SetStripInfoStruct(strip, configurationObject.configurationSettings.displayZones[strip]);
            }
        }

        internal void SetStripInfoStruct(int strip, ConfigurationDisplayZonesStruct displayZone)
        {
            StripObjectGroupManager temp = configurationGroupManagers[strip] as StripObjectGroupManager;
            configurationGroupManagers[strip].indexInGroupManager = strip;
            StripStruct temp2 = temp.stripInfo;
            temp2.stripColumn = strip;
            ConfigurationDisplayZonesStruct temp3 = new ConfigurationDisplayZonesStruct(displayZone);
            //Display Zone settings gets set later
            temp2.stripDisplayZonesSetting = temp3;
            temp.stripInfo = temp2;
            Debug.Log($"temp3 as displayZone.totalPositions {temp3.totalPositions}");
            temp.InitializeLocalPositions(temp2);
        }
        internal void SetStripInfoStruct(int strip,StripStruct stripStruct)
        {
            StripObjectGroupManager temp = configurationGroupManagers[strip] as StripObjectGroupManager;
            configurationGroupManagers[strip].indexInGroupManager = strip;
            ConfigurationDisplayZonesStruct temp3 = new ConfigurationDisplayZonesStruct(stripStruct);
            StripStruct tempStripStruct = new StripStruct(stripStruct);
            tempStripStruct.stripColumn = strip;
            temp.stripInfo = tempStripStruct;
            Debug.Log($"temp3 as displayZone.totalPositions {temp3.totalPositions}");
            temp.InitializeLocalPositions(tempStripStruct);
        }
        internal void ClearSubStatesAllSlotAnimatorStateMachines()
        {
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                configurationGroupManagers[reel].ClearAllSlotContainersSubAnimatorStates();
            }
        }

        private AnimatorSubStateMachine[] GatherValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                values.AddRange(configurationGroupManagers[reel].ReturnAllValuesFromSubStates());
            }
            return values.ToArray();
        }

        private string[] GatherKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int reel = 0; reel < configurationGroupManagers.Length; reel++)
            {
                keys.AddRange(configurationGroupManagers[reel].ReturnAllKeysFromSubStates());
            }
            return keys.ToArray();
        }


        internal void EnsureWeightsAreCorrect()
        {
            //TBD
        }

        internal int GetRollupIndexFromAmountToRack(double amountToRack)
        {
            float betAmount = managers.machine_info_manager.machineInfoScriptableObject.supported_bet_amounts[managers.machine_info_manager.machineInfoScriptableObject.current_bet_amount];
            int index = 0;
            while(amountToRack > betAmount)
            {
                index += 1;
                amountToRack -= betAmount;
                //Big win if we are over the total rollups present
                if (index == managers.soundManager.machineSoundsReference.rollups.Length -1)
                    break;
            }
            return index;
        }

        internal AudioClip ReturnSymbolSound(int winningSymbol)
        {
            return symbolDataScriptableObject.symbols[winningSymbol].winAudioClip;
        }

        internal void CreateEmptyAnimationContainer()
        {
#if UNITY_EDITOR
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            AnimationCurve curve = AnimationCurve.Linear(0.0F, 1.0F, .0001F, 1.0F); // Unity won't let me use 0 length, so use a very small length instead
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(UnityEngine.Animator), "ThisIsAnEmptyAnimationClip"); // Just dummy data
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            AssetDatabase.CreateAsset(clip, "Assets/" + name + ".anim");
#endif
        }

        internal void SyncCurrentSymbolDisplayed()
        {
            for (int i = 0; i < configurationGroupManagers.Length; i++)
            {
                configurationGroupManagers[i].SyncDisplaySymbolInformation();
            }
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