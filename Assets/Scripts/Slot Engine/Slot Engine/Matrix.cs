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
    [CanEditMultipleObjects]
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
            if(GUILayout.Button("Create Empty Animation Container"))
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
                myTarget.SetReelInfoNumbers();
            }
            if(GUILayout.Button("Set Weights from symbols data"))
            {
                Debug.Log(myTarget.symbol_weights_per_state_dictionary == null ? "No object returned" : "Ensuring weights");
                myTarget.AddSymbolStateWeightToDict();
                //myTarget.EnsureWeightsAreCorrect();
            }
            base.OnInspectorGUI();
        }
    }
#endif

    public class Matrix : MonoBehaviour
    {
        /// <summary>
        /// Holds the reference to all managers
        /// </summary>
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
        /// <summary>
        /// Symbol information for the matrix
        /// </summary>
        public SymbolScriptableObject symbols_data_for_matrix;
        /// <summary>
        /// Holds reference for the symbol weights prefab and other utility prefabs
        /// </summary>
        public CorePrefabsReferencesScriptableObject core_prefabs;
        [SerializeField]
        public Dictionary<GameStates, WeightsDistributionScriptableObject> symbol_weights_per_state_dictionary;
        [SerializeField]
        public GameStateDistributionDictionary symbol_weights_per_state
        {
            get
            {
                if (_symbol_weights_per_state == null)
                {
                    _symbol_weights_per_state = new GameStateDistributionDictionary();
                }
                return _symbol_weights_per_state;
            }
        }

        internal async Task AddSymbolStateWeightToDict()
        {
            symbol_weight_state temp;
            Dictionary<GameStates, List<float>> symboWeightsByState = new Dictionary<GameStates, List<float>>();
            for (int symbol = 0; symbol < symbols_data_for_matrix.symbols.Length; symbol++)
            {
                for (int weight_state = 0; weight_state < symbols_data_for_matrix.symbols[symbol].symbolWeights.Length; weight_state++)
                {
                    temp = symbols_data_for_matrix.symbols[symbol].symbolWeights[weight_state];
                    if (!symboWeightsByState.ContainsKey(temp.gameState))
                    {
                        symboWeightsByState[temp.gameState] = new List<float>();
                    }
                    symboWeightsByState[temp.gameState].Add(temp.symbolWeightInfo);
                }
            }
            await AddSymbolStateWeightToDict(symboWeightsByState);
        }

        public WeightsDistributionScriptableObject[] weightObjects;

        private async Task AddSymbolStateWeightToDict(Dictionary<GameStates, List<float>> symbol_weight_state)
        {
            int counter = -1;
            foreach (KeyValuePair<GameStates, List<float>> item in symbol_weight_state)
            {
                if (symbol_weights_per_state_dictionary == null)
                    symbol_weights_per_state_dictionary = new Dictionary<GameStates, WeightsDistributionScriptableObject>();
                counter++;
                symbol_weights_per_state_dictionary[item.Key] = weightObjects[counter];
                await Task.Delay(20);
                if (symbol_weights_per_state_dictionary[item.Key]?.intDistribution != null)
                {
                    //Clear and re-add
                    for (int i = symbol_weights_per_state_dictionary[item.Key].intDistribution.Items.Count - 1; i >= 0; i--)
                    {
                        symbol_weights_per_state_dictionary[item.Key].intDistribution.RemoveAt(i);
                        await Task.Delay(10);
                    }
                }

                for (int i = 0; i < item.Value.Count; i++)
                {
                    //Debug.Log(String.Format("{0} value added = {1} iterator = {2}", item.Key.ToString(), item.Value[i],i));
                    //Setting the value to the idex of the symbol so to support reorderable lists 2020.3.3
                    await Task.Delay(20);
                    symbol_weights_per_state_dictionary[item.Key].intDistribution.Add(i, item.Value[i]);
                    await Task.Delay(20);
                    symbol_weights_per_state_dictionary[item.Key].intDistribution.Items[i].Weight = item.Value[i];
                }
            }
//#if UNITY_EDITOR
//            EditorUtility.SetDirty(this);
//            foreach (KeyValuePair<GameStates, List<float>> item in symbol_weight_state)
//            {
//                symbol_weights_per_state[item.Key] = FindDistributionFromResources(item.Key);
//                await Task.Delay(20);
//                if (symbol_weights_per_state[item.Key]?.intDistribution != null)
//                {
//                    //Clear and re-add
//                    for (int i = symbol_weights_per_state[item.Key].intDistribution.Items.Count - 1; i >= 0; i--)
//                    {
//                        symbol_weights_per_state[item.Key].intDistribution.RemoveAt(i);
//                        await Task.Delay(10);
//                    }
//                }

//                for (int i = 0; i < item.Value.Count; i++)
//                {
//                    //Debug.Log(String.Format("{0} value added = {1} iterator = {2}", item.Key.ToString(), item.Value[i],i));
//                    //Setting the value to the idex of the symbol so to support reorderable lists 2020.3.3
//                    await Task.Delay(20);
//                    symbol_weights_per_state[item.Key].intDistribution.Add(i, item.Value[i]);
//                    await Task.Delay(20);
//                    symbol_weights_per_state[item.Key].intDistribution.Items[i].Weight = item.Value[i];
//                }
//                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
//            }
//#else
//            foreach (KeyValuePair<GameStates, List<float>> item in symbol_weight_state)
//            {
//                symbol_weights_per_state_dictionary[item.Key] = FindDistributionFromResources(item.Key);
//                await Task.Delay(20);
//                if (symbol_weights_per_state_dictionary[item.Key]?.intDistribution != null)
//                {
//                    //Clear and re-add
//                    for (int i = symbol_weights_per_state_dictionary[item.Key].intDistribution.Items.Count - 1; i >= 0; i--)
//                    {
//                        symbol_weights_per_state_dictionary[item.Key].intDistribution.RemoveAt(i);
//                        await Task.Delay(10);
//                    }
//                }

//                for (int i = 0; i < item.Value.Count; i++)
//                {
//                    //Debug.Log(String.Format("{0} value added = {1} iterator = {2}", item.Key.ToString(), item.Value[i],i));
//                    //Setting the value to the idex of the symbol so to support reorderable lists 2020.3.3
//                    await Task.Delay(20);
//                    symbol_weights_per_state_dictionary[item.Key].intDistribution.Add(i, item.Value[i]);
//                    await Task.Delay(20);
//                    symbol_weights_per_state_dictionary[item.Key].intDistribution.Items[i].Weight = item.Value[i];
//                }
//            }
//#endif


        }

        private WeightsDistributionScriptableObject FindDistributionFromResources(GameStates key)
        {
            Debug.Log(String.Format("Loading Resources/Core/ScriptableObjects/Weights/{0}", key.ToString()));
            return Resources.Load(String.Format("Core/ScriptableObjects/Weights/{0}", key.ToString())) as WeightsDistributionScriptableObject;
        }

        [SerializeField]
        public GameStateDistributionDictionary _symbol_weights_per_state;
      
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

        /// <summary>
        /// Controls how many slots to include in reel strip
        /// </summary>
        [SerializeField]
        internal int slots_per_strip_onSpinLoop = 50;
        /// <summary>
        /// Symbol Win Resolve Animator Override Controller
        /// </summary>
        public AnimatorOverrideController symbol_win_resolve;
        /// <summary>
        /// Symbol Lose Resolve Animator Override Controller
        /// </summary>
        public AnimatorOverrideController symbol_lose_resolve;
        /// <summary>
        /// Reel Strip Managers that make up the matrix - Generated with MatrixGenerator Script - Order determines reel strip spin delay and Symbol Evaluation Logic
        /// </summary>
        [SerializeField]
        internal ReelStripManager[] reel_strip_managers; //Each reel strip has a manager 
        /// <summary>
        /// Used for logical gate to activate bonus mode in animator
        /// </summary>
        [SerializeField]
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
                string[] names = new string[symbols_data_for_matrix.symbols.Length];
                for (int i = 0; i < symbols_data_for_matrix.symbols.Length; i++)
                {
                    names[i] = symbols_data_for_matrix.symbols[i].symbolName;
                }
                return names;
            }
        }
        async void Start()
        {
            StateManager.SetGameModeActiveTo(GameStates.baseGame);
            int symbol_weight_pass_check = -1;
            try
            {
                symbol_weight_pass_check = symbol_weights_per_state_dictionary[GameStates.baseGame].intDistribution.Draw();//symbol_weights_per_state[GameStates.baseGame].intDistribution.Draw();
            }
            catch
            {
                await AddSymbolStateWeightToDict();
                symbol_weight_pass_check = symbol_weights_per_state_dictionary[GameStates.baseGame].intDistribution.Draw();//symbol_weights_per_state[GameStates.baseGame].intDistribution.Draw();
                Debug.Log("Weights are in");
            }
            //On Play editor referenced state machines loos reference. Temp Solution to build on game start. TODO find way to store info between play and edit mode - Has to do with prefabs
            SetAllSlotAnimatorSyncStates();
            SetSubStatesAllSlotAnimatorStateMachines();
            SetManagerStateMachineSubStates();
            //Initialize Machine and Player  Information
            slot_machine_managers.machine_info_manager.InitializeTestMachineValues(10000.0f, 0.0f, slot_machine_managers.machine_info_manager.machineInfoScriptableObject.supported_bet_amounts.Length / 2 -1, 0, 0);
            //Debug.Log(String.Format("Initial pop of end_configiuration_manager = {0}", print_string));
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
                        payline_posiiton_on_reel = ReturnPositionOnReelForPayline(ref reel_strip_managers[reel], payline.payline_configuration.payline[payline_positions_set]);
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

        internal async Task StopReels()
        {
            //Get the end display configuration and set per reel
            ReelStripSpinStruct[] configuration_to_use = slot_machine_managers.end_configuration_manager.GetCurrentConfiguration();
            //Determine whether to stop reels forwards or backwards.
            for (int i = slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? 0 : reel_strip_managers.Length - 1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? i < reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? i + 1 : i - 1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                //If reel strip delays are enabled wait between strips to stop
                if (slot_machine_managers.spin_manager.spinSettingsScriptableObject.reel_spin_delay_end_enabled)
                {
                    await reel_strip_managers[i].StopReel(configuration_to_use[i]);//Only use for specific reel stop features
                }
                else
                {
                    reel_strip_managers[i].StopReel(configuration_to_use[i]);//Only use for specific reel stop features
                }
            }
            //Wait for all reels to be in spin.end state before continuing
            await WaitForAllReelsToStop(reel_strip_managers);
        }

        private async Task WaitForAllReelsToStop(ReelStripManager[] reel_strip_managers)
        {
            bool lock_task = true;
            while (lock_task)
            {
                for (int i = 0; i < reel_strip_managers.Length; i++)
                {
                    if (reel_strip_managers[i].current_spin_state == SpinStates.spin_end)
                    {
                        if (i == reel_strip_managers.Length - 1)
                        {
                            lock_task = false;
                            break;
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                        break;
                    }
                }
            }
        }

        internal async Task SpinReels()
        {
            //The end reel configuration is set when spin starts to the next item in the list
            ReelStripSpinStruct[] end_reel_configuration = slot_machine_managers.end_configuration_manager.UseNextConfigurationInList();
            //Evaluation is ran over those symbols and if there is a bonus trigger the matrix will be set into display bonus state
            slot_machine_managers.paylines_manager.EvaluateWinningSymbolsFromCurrentConfiguration();

            await SpinReels(end_reel_configuration);
        }
        /// <summary>
        /// Used to start spinning the reels
        /// </summary>
        internal async Task SpinReels(ReelStripSpinStruct[] end_reel_configuration)
        {
            //If we want to use ReelStrips for the spin loop we need to stitch the end_reel_configuration and display symbols together
            if (slot_machine_managers.spin_manager.spinSettingsScriptableObject.use_reelstrips_for_spin_loop)
            {
                //Generate Reel strips if none are present
                GenerateReelStripsToLoop(ref end_reel_configuration);
                //TODO Set each reelstripmanager to spin thru the strip
                //TODO Insert end_reelstrips_to_display into generated reelstrips
            }
            //Spin the reels - if there is a delay between reels then wait delay amount
            for (int i = slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? 0 : reel_strip_managers.Length - 1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? i < reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = slot_machine_managers.spin_manager.spinSettingsScriptableObject.spin_reels_starting_forward_back ? i + 1 : i - 1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                await reel_strip_managers[i].StartSpin();
            }
        }

        internal async Task PlayFeatureAnimation(List<suffix_tree_node_info> overlaySymbols)
        {
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
            //Lerp to Multiplier Bank
            for (int i = 0; i < overlaySymbols.Count; i++)
            {
                await LerpToMeFinished(symbolAnimators[i].transform);
                _slot_machine_managers.machine_info_manager.SetMultiplierTo(_slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier + 1);
            }
            
            for (int i = 0; i < symbolAnimators.Count; i++)
            {
                symbolAnimators[i].SetBool(supported_bools.FeatureTrigger.ToString(), false);
            }
        }

        public bool isLerping = false;
        private async Task LerpToMeFinished(Transform transform)
        {
            isLerping = true;
            LerpToMe.lerpComplete += LerpToMe_lerpComplete; 
            slot_machine_managers.lerpToMe.SetLerpToMe(transform);
            while(isLerping)
            {
                await Task.Delay(100);
            }
        }
        private void LerpToMe_lerpComplete()
        {
            isLerping = false;
            LerpToMe.lerpComplete -= LerpToMe_lerpComplete;
        }

        internal Animator SetAnimatorFeatureTriggerAndReturn(suffix_tree_node_info suffix_tree_node_info)
        {
            Animator output = SetOverlayFeatureAndReturnAnimatorFromNode(suffix_tree_node_info);
            return output;
        }

        private Animator SetOverlayFeatureAndReturnAnimatorFromNode(suffix_tree_node_info suffix_tree_node_info)
        {
            return reel_strip_managers[suffix_tree_node_info.column].GetSlotsDecending()[reel_strip_managers[suffix_tree_node_info.column].reelstrip_info.padding_before + suffix_tree_node_info.row].SetOverlayAnimatorToFeatureAndGet();
        }

        internal bool isSymbolOverlay(int symbol)
        {
            return symbols_data_for_matrix.symbols[symbol].isOverlaySymbol;
        }

        internal bool isWildSymbol(int symbol)
        {
            return symbols_data_for_matrix.symbols[symbol].isWildSymbol;
        }



        internal async Task WaitForSymbolWinResolveToIntro()
        {
            if (current_payline_displayed != null)
            {
                List<SlotManager> winning_slots, losing_slots;
                ReturnWinLoseSlots(current_payline_displayed, out winning_slots, out losing_slots, ref reel_strip_managers);
                bool is_all_animations_resolve_intro = false;
                while (!is_all_animations_resolve_intro)
                {
                    for (int slot = 0; slot < winning_slots.Count; slot++)
                    {
                        if (!winning_slots[slot].isSymbolAnimationFinished("Resolve_Intro"))
                        {
                            await Task.Delay(100);
                            break;
                        }
                        if (slot == winning_slots.Count - 1)
                            is_all_animations_resolve_intro = true;
                    }
                }
                current_payline_displayed = null;
                winning_slots = new List<SlotManager>();
                losing_slots = new List<SlotManager>();
            }
        }

        internal bool isFeatureSymbol(int symbol)
        {
            return symbols_data_for_matrix.symbols[symbol].isFeatureSymbol;
        }

        internal Features[] GetSymbolFeatures(int symbol)
        {
            return symbols_data_for_matrix.symbols[symbol].features;
        }

        private Vector3 ReturnPositionOnReelForPayline(ref ReelStripManager reel, int slot_in_reel)
        {
            return reel.transform.TransformPoint(reel.positions_in_path_v3_local[reel.reelstrip_info.padding_before+slot_in_reel] + (Vector3.back * 10));
        }

        internal IEnumerator InitializeSymbolsForWinConfigurationDisplay()
        {
            SetSlotsAnimatorBoolTo(supported_bools.LoopPaylineWins,false);
            yield return 0;
        }
        private WinningPayline current_payline_displayed;
        List<SlotManager> winning_slots, losing_slots;
        internal Task SetSymbolsForWinConfigurationDisplay(WinningPayline winning_payline)
        {
            //Debug.Log(String.Format("Showing Winning Payline {0} with winning symbols {1}",String.Join(" ", winning_payline.payline.payline_configuration.ToString()), String.Join(" ",winning_payline.winning_symbols)));
            //Get Winning Slots and loosing slots
            current_payline_displayed = winning_payline;
            ReturnWinLoseSlots(winning_payline, out winning_slots, out losing_slots, ref reel_strip_managers);
            SetSlotsToResolveWinLose(ref winning_slots,true);
            SetSlotsToResolveWinLose(ref losing_slots,false);
            return Task.CompletedTask;
        }

        private void SetSlotsToResolveWinLose(ref List<SlotManager> slots, bool v)
        {
            for (int slot = 0; slot < slots.Count; slot++)
            {
                if (v)
                    slots[slot].SetSymbolResolveWin();
                else
                    slots[slot].SetSymbolResolveToLose();
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
                int first_display_slot = reel_managers[reel].reelstrip_info.padding_before;
                //Debug.Log(String.Format("first_display_slot for reel {0} = {1}", reel, first_display_slot));
                for (int slot = first_display_slot; slot < slots_decending_in_reel.Count; slot++)
                {
                    if (winning_symbols_added < winning_payline.payline.payline_configuration.payline.Length && !winning_slot_set)
                    {
                        int winning_slot = winning_payline.payline.payline_configuration.payline[winning_symbols_added] + reel_managers[reel].reelstrip_info.padding_before;
                        if (slot == winning_slot)
                        {
                            //Debug.Log(String.Format("Adding Winning Symbol on reel {0} slot {1}",reel, slot));
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
            //Pause and interrupt racking before continue
            slot_machine_managers.racking_manager.PauseRackingOnInterrupt();
            StateManager.SetStateTo(States.Resolve_Outro);
        }

        internal void SetSystemToPresentWin()
        {
            SetAllAnimatorsBoolTo(supported_bools.WinRacking, true);
        }

        internal void CycleWinningPaylinesMode()
        {
            slot_machine_managers.paylines_manager.PlayCycleWins();
        }
        /// <summary>
        /// Set Free spin information for all Animators
        /// </summary>
        /// <param name="onOff"></param>
        internal void ToggleFreeSpinActive(bool onOff)
        {
            Debug.Log("Bonus Active = " + onOff);
            bonus_game_triggered = onOff;
            SetAllAnimatorsBoolTo(supported_bools.BonusActive, onOff);
            if (!onOff)
                slot_machine_managers.machine_info_manager.SetMultiplierTo(0);
        }

        private void SetAllAnimatorsBoolTo(supported_bools bool_to_set, bool value)
        {
            slot_machine_managers.animator_statemachine_master.SetBoolAllStateMachines(bool_to_set, value);
            SetSlotsAnimatorBoolTo(bool_to_set,value);
        }

        private void PrepareSlotMachineToSpin()
        {
            Debug.Log("Preparing Slot Machine for Spin");
        }

        internal void SetSymbolsToDisplayOnMatrixTo(ReelStripSpinStruct[] current_reelstrip_configuration)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].SetSymbolCurrentDisplayTo(current_reelstrip_configuration[reel]);
            }
        }

        private void SetSlotsAnimatorBoolTo(supported_bools bool_name, bool v)
        {
            //Debug.Log(String.Format("Setting Slot Animator {0} to {1}",bool_name.ToString(),v));
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].SetBoolStateMachines(bool_name,v);
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
        public Task SetMatrixReelStripsInfo(ReelStripStructDisplayZones[] display_zones_per_reel, Vector3 slot_size)
        {
            //Build reelstrip info 
            ReelStripsStruct reelstrips_configuration = new ReelStripsStruct(display_zones_per_reel);
            //Set Matrix Settings first then update the reel configuration 
            SetMatrixSettings(slot_size);
            SetReelsAndSlotsPerReel(reelstrips_configuration);
            return Task.CompletedTask;
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
        internal void SetReelsAndSlotsPerReel(ReelStripsStruct reelstrips_configuration)
        {
            SetReelsTo(reelstrips_configuration.reelstrips.Length, ref reel_strip_managers);

            //Ensure reels have slots
            for (int i = 0; i < reelstrips_configuration.reelstrips.Length; i++)
            {
                reel_strip_managers[i].SetReelConfigurationTo(reelstrips_configuration.reelstrips[i]);
            }
            //Update Payline Manager
            slot_machine_managers.paylines_manager.GenerateDynamicPaylinesFromMatrix();
        }

        /// <summary>
        /// Generate or remove reelstrip objects based on number of reels set
        /// </summary>
        /// <param name="number_of_reels">Reels in Configuration</param>
        internal void SetReelsTo(int number_of_reels)
        {
            SetReelsTo(number_of_reels, ref reel_strip_managers);
        }

        /// <summary>
        /// Generate or remove reelstrip objects based on number of reels set
        /// </summary>
        /// <param name="number_of_reels">Reels in Configuration</param>
        /// <param name="reelstrip_managers">reference var to cached reelstrip_managers</param>
        internal void SetReelsTo(int number_of_reels, ref ReelStripManager[] reelstrip_managers)
        {
            //See whether we need to make more or subtract some 
            bool add_subtract_reels = reelstrip_managers.Length < number_of_reels ? true : false;
            //First we are going to ensure the amount of reels are the correct amount - then we are going to initialize the amount of slots per reel

            //If current reels generated are > or < matrix.length then need to adjust accordingly
            //Ensure enough reels are on the board then ensure all reels have slots
            for (int reel = add_subtract_reels ? reelstrip_managers.Length : reelstrip_managers.Length - 1;
                add_subtract_reels ? reel < number_of_reels : reel >= number_of_reels;
                reel = add_subtract_reels ? reel + 1 : reel - 1)
            {
                if (add_subtract_reels)
                {
                    reelstrip_managers = reelstrip_managers.AddAt<ReelStripManager>(reel,GenerateReel(reel));
                }
                else
                {
                    if (reelstrip_managers[reel] != null)
                        DestroyImmediate(reelstrip_managers[reel].gameObject);
                    reelstrip_managers = reelstrip_managers.RemoveAt(reel);
                }
            }
            for (int reel = 0; reel < reelstrip_managers.Length; reel++)
            {
                if(reelstrip_managers[reel] == null)
                {
                    reelstrip_managers[reel] = GenerateReel(reel);
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

        internal void RegenerateSlotObjects()
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].RegenerateSlotObjects();
            }
        }

        internal void SetSpinParametersTo(ReelStripSpinParametersScriptableObject spin_parameters)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].SetSpinParametersTo(spin_parameters);
            }
        }

        internal void GenerateReelStripsToLoop(ref ReelStripSpinStruct[] reel_configuration)
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(ref reel_strip_managers, ref reel_configuration, slots_per_strip_onSpinLoop);
        }

        private void GenerateReelStripsFor(ref ReelStripManager[] reel_strip_managers, ref ReelStripSpinStruct[] spin_configuration_reelstrip, int slots_per_strip_onSpinLoop)
        {
            EndConfigurationManager temp;
            temp = slot_machine_managers.end_configuration_manager;
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reel_strip_managers.Length; i++)
            {
                if (spin_configuration_reelstrip[i].reel_spin_symbols?.Length != slots_per_strip_onSpinLoop)
                {
                    //Generates reelstrip based on weights
                    spin_configuration_reelstrip[i].reel_spin_symbols = ReelStrip.GenerateReelStripStatic(StateManager.enCurrentMode,slots_per_strip_onSpinLoop, ref temp);
                }
                //Assign reelstrip to reel
                reel_strip_managers[i].reelstrip_info.SetSpinConfigurationTo(spin_configuration_reelstrip[i]);
            }
        }

        ReelStripManager GenerateReel(int reel_number)
        {
            Type[] gameobject_components = new Type[1];
            gameobject_components[0] = typeof(ReelStripManager);
            ReelStripManager output_reelstrip_manager = StaticUtilities.CreateGameobject<ReelStripManager>(gameobject_components,"Reel_" + reel_number, transform);
            return output_reelstrip_manager;
        }

        internal void InitializeAllAnimators()
        {
            slot_machine_managers.animator_statemachine_master.InitializeAnimator();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].state_machine.InitializeAnimator();
                }
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

        internal void OffetPlayerWalletBy(double amount)
        {
            slot_machine_managers.machine_info_manager.OffsetPlayerAmountBy(amount);
        }

        private string PrintSpinSymbols(ref ReelStripSpinStruct[] stripInitial)
        {
            string output = "";
            for (int strip = 0; strip < stripInitial.Length; strip++)
            {
                output += ReturnDisplaySymbolsPrint(stripInitial[strip]);
            }
            return output;
        }

        private string ReturnDisplaySymbolsPrint(ReelStripSpinStruct reelstrip_info)
        {
            return String.Join("|",reelstrip_info.display_symbols);
        }

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
            StateManager.featureTransition += StateManager_FeatureTransition;
            //StateManager.add_to_multiplier += StateManager_add_to_multiplierAsync;
        }

        private async void StateManager_add_to_multiplierAsync(int multiplier)
        {
            
        }

        private async Task isAnimatorThruState(Animator multiplier, string state)
        {
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            bool wait = true;
            while (wait)
            {
                state_info = multiplier.GetCurrentAnimatorStateInfo(0);

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
                        background.SetBool(supported_bools.BonusActive.ToString(),true);
                    }
                    else
                    {
                        background.SetBool(supported_bools.BonusActive.ToString(), false);
                    }
                    break;
                case Features.multiplier:
                    ToggleFreeSpinActive(active_inactive);
                    background.runtimeAnimatorController = backgroundACO[1];
                    break;
                default:
                    bonus_game_triggered = false;
                    break;
            }
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
            StateManager.featureTransition -= StateManager_FeatureTransition;
            //StateManager.add_to_multiplier -= StateManager_add_to_multiplierAsync;
        }

        public AnimatorOverrideController[] characterTier;
        public Animator character;
        public AnimatorOverrideController[] multiplierTier;
        public Animator multiplier;

        public Animator background;
        public AnimatorOverrideController[] backgroundACO;
        public TMPro.TextMeshPro freespinText;

        public Animator rackingRollupAnimator;
        public TMPro.TextMeshPro rackingRollupText;
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
                    await isAllAnimatorsThruState("Idle_Intro");
                    //Fall thru to Idle_Idle State - ATM the animator falls thru Idle_Intro
                    StateManager.SetStateTo(States.Idle_Idle);
                    break;
                case States.Idle_Outro:
                    //Decrease Bet Amount
                    PlayerHasBet(slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount);
                    await StartAnimatorSpinAndSpinState();
                    break;
                case States.Spin_End:
                    bool resolve_intro = false;
                    if (slot_machine_managers.paylines_manager.overlaySymbols.Count > 0)
                    {
                        if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 ||
                slot_machine_managers.paylines_manager.GetTotalWinAmount() > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9)
                        {
                            Debug.Log("Present Big Win First");
                            SetAnimatorOverrideControllerTo(ref this.multiplier, ref multiplierTier, 2);
                        }
                        else if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5 ||
                            slot_machine_managers.paylines_manager.GetTotalWinAmount() > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5)
                        {
                            Debug.Log("Presenting Medium win");
                            SetAnimatorOverrideControllerTo(ref this.multiplier, ref multiplierTier, 1);

                        }
                        else
                        {
                            Debug.Log("Presenting Small win");
                            SetAnimatorOverrideControllerTo(ref this.multiplier, ref multiplierTier, 0);
                        }
                        this.multiplier.SetBool(supported_bools.FeatureTrigger.ToString(), true);
                        await PlayFeatureAnimation(slot_machine_managers.paylines_manager.overlaySymbols);
                        await isAnimatorThruStateAndAtPauseState(this.multiplier, "Feature_Outro");
                        this.multiplier.SetBool(supported_bools.FeatureTrigger.ToString(), false);
                        Debug.Log("All Overlay Animators are finished");
                    }
                    //If the spin has ended and there are no wining paylines or freespins left then disable freespin mode
                    if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins <= 0)
                    {
                        Debug.Log("Setting Freespins Inactive");
                        StateManager.SetFeatureActiveTo(Features.freespin, false);
                    }
                    //matrix bonus gmae not triggered but animator is
                    if (CheckForWin())
                    {
                        if (!bonus_game_triggered)
                        {
                            Debug.Log("Win in Bonus Game Triggered");
                            //Calculate Rack Amount and either skip resolve phase or 
                            //Proceed to next state and sync state machine
                            SetAllAnimatorsBoolTo(supported_bools.WinRacking, true);
                            if (StateManager.enCurrentMode != GameStates.baseGame)
                                StateManager.SetFeatureActiveTo(Features.freespin,false);
                            resolve_intro = true;
                        }
                        else
                        {
                            //Rack win to bank and continue to next spin
                            Debug.Log("There is a win and bonus game triggeres");
                            slot_machine_managers.machine_info_manager.OffsetBankBy(slot_machine_managers.paylines_manager.GetTotalWinAmount());
                            freespinText.text = String.Format("{0:C2} Total Win Amount", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank);
                            freespinText.enabled = true;
                            SetAllAnimatorsBoolTo(supported_bools.WinRacking, false); // dont rack wins
                            if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins > 0)
                            {
                                //StateManager.SetStateTo(States.bonus_idle_intro);
                            }
                            else
                            {
                                resolve_intro = true;
                            }
                        }
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                    }
                    else
                    {
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                        if (bonus_game_triggered)
                        {
                            Debug.Log(String.Format("Bonus Game no win", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank));
                            if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins > 0)
                            {
                                //StateManager.SetStateTo(States.bonus_idle_intro);
                            }
                        }
                        else if (!bonus_game_triggered && slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > 0)
                        {
                            Debug.Log(String.Format("Bonus game ended and bank has amount to rack = {0}", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank));
                            resolve_intro = true;                        
                        }
                        else
                        {
                            Debug.Log(String.Format("Base Game no win", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank));
                        }
                    }
                    await isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
                    await isAllSlotSubAnimatorsReady("Spin_Outro");
                    
                    if (resolve_intro)
                    {
                        SetOverridesBasedOnTiers();
                        await Task.Delay(20);
                        SetAllAnimatorsBoolTo(supported_bools.WinRacking, true);
                        SetAllAnimatorsTriggerTo(supported_triggers.SpinResolve, true);
                        await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                        await isAllSlotSubAnimatorsReady("Resolve_Intro");
                        StateManager.SetStateTo(States.Resolve_Intro);
                    }
                    else
                    {
                        SetAllAnimatorsTriggerTo(supported_triggers.SpinResolve, true);
                        await isAllSlotSubAnimatorsReady("Idle_Intro");
                        await isAllAnimatorsThruStateAndAtPauseState("Idle_Idle");
                        if (bonus_game_triggered)
                            StateManager.SetStateTo(States.bonus_idle_intro);
                        else
                        {
                            StateManager.SetStateTo(States.Idle_Idle);
                        }
                    }
                    break;
                case States.Resolve_Intro:
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                    //Debug.Log("Playing resolve Intro on Matrix");
                    //If the player activated bonus trigger then increase player bank amount 
                    if (!bonus_game_triggered)
                    {
                        //First offset bank by win then rack
                        //TODO have offset occur when winning payline is animated to bank
                        slot_machine_managers.machine_info_manager.OffsetBankBy(slot_machine_managers.paylines_manager.GetTotalWinAmount());
                        //TODO Refactor hack
                        freespinText.text = String.Format("{0:C2} Total Win Amount", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank);
                        freespinText.enabled = true;
                        if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9)
                        {
                            //Determine if free spins or overlay amount won surpasses 9x
                            //Will drop animator intro symbol win resolve state on resolve intro load
                            rackingRollupAnimator.SetBool(supported_bools.LoopPaylineWins.ToString(), true);
                            rackingRollupAnimator.SetBool(supported_bools.SymbolResolve.ToString(), true);
                            rackingRollupText.text = String.Format("{0:C2}",slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank);
                        }
                        slot_machine_managers.racking_manager.rackEnd += CloseBigWinDisplay;
                        slot_machine_managers.racking_manager.StartRacking(); //This is to resolve wins in resolve intro
                    }
                    if (slot_machine_managers.paylines_manager.winning_paylines.Length > 0)
                    {
                        if (StateManager.enCurrentMode != GameStates.freeSpin)
                            CycleWinningPaylinesMode();
                        else if (StateManager.enCurrentMode == GameStates.freeSpin && (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins == 0 || slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins == 10))
                            CycleWinningPaylinesMode();
                    }
                    break;
                case States.Resolve_Outro:
                    await slot_machine_managers.paylines_manager.CancelCycleWins();
                    //TODO Refactor hack
                    if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins < 1)
                    {
                        freespinText.text = "";
                        freespinText.enabled = false;
                    }
                    else
                    {
                        slot_machine_managers.machine_info_manager.SetFreeSpinsTo(slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins);
                    }
                    //Set animator to Resolve_Outro State
                    SetAllAnimatorsBoolTo(supported_bools.WinRacking, false);
                    SetAllAnimatorsBoolTo(supported_bools.LoopPaylineWins, false);
                    //If going back to base game initialize vars for next bonus trigger
                    if (StateManager.enCurrentMode == GameStates.baseGame)
                    {
                        SetAllAnimatorsBoolTo(supported_bools.BonusActive, false);
                        //ensure multiplier set to 0
                        slot_machine_managers.machine_info_manager.ResetMultiplier();
                    }
                    //Wait for animator to play all resolve outro animations
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Outro");
                    await isAllSlotAnimatorsReady("Resolve_Outro");
                    //Need to refactor to integrate
                    await isAnimatorThruStateAndAtPauseState(this.multiplier,"Resolve_Outro");
                    SetAllAnimatorsTriggerTo(supported_triggers.ResolveEnd,true);
                    this.multiplier.SetTrigger(supported_triggers.ResolveEnd.ToString());
                    if (!bonus_game_triggered)
                    {                        //TODO wait for all animators to go thru idle_intro state
                        StateManager.SetStateTo(States.Idle_Intro);
                    }
                    else
                    {
                        StateManager.SetStateTo(States.bonus_idle_intro);
                    }
                    break;
                case States.bonus_idle_intro:
                    await isAllAnimatorsThruState("Idle_Intro");
                    StateManager.SetStateTo(States.bonus_idle_idle);
                    break;
                case States.bonus_idle_outro:
                    ReduceFreeSpinBy(1);
                    await StartAnimatorSpinAndSpinState();
                    break;
            }
        }

        private void CloseBigWinDisplay()
        {
            rackingRollupAnimator.SetBool(supported_bools.LoopPaylineWins.ToString(), false);
            rackingRollupAnimator.SetBool(supported_bools.SymbolResolve.ToString(), false);
            slot_machine_managers.racking_manager.rackEnd -= CloseBigWinDisplay;
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
            Debug.Log(String.Format("slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank  = {0} slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 = {1} slot_machine_managers.paylines_manager.GetTotalWinAmount() = {2}", slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank, slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9, slot_machine_managers.paylines_manager.GetTotalWinAmount()));
            if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 ||
                slot_machine_managers.paylines_manager.GetTotalWinAmount() > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9)
            {
                Debug.Log("Present Big Win First");
                SetAnimatorOverrideControllerTo(ref character, ref characterTier,2);
            }
            else if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5 ||
                slot_machine_managers.paylines_manager.GetTotalWinAmount() > slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 5)
            {
                Debug.Log("Presenting Medium win");
                SetAnimatorOverrideControllerTo(ref character, ref characterTier, 1);

            }
            else
            {
                Debug.Log("Presenting Small win");
                SetAnimatorOverrideControllerTo(ref character, ref characterTier, 0);
            }
        }

        private void SetAnimatorOverrideControllerTo(ref Animator character, ref AnimatorOverrideController[] characterTier, int v)
        {
            character.runtimeAnimatorController = characterTier[v];
        }

        private void PlayAnimationOnAllSlots(string v)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    //Temporary fix because Animator decides to sometimes play an animation override and sometimes not
                    reel_strip_managers[reel].slots_in_reel[slot].PlayAnimationOnPresentationSymbol("Resolve_Outro");
                }
            }
        }

        private async Task StartAnimatorSpinAndSpinState()
        {
            SetAllAnimatorsTriggerTo(supported_triggers.SpinStart, true);
            await isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
            await isAllSlotSubAnimatorsReady("Idle_Outro");
            //Tell the spin manager to start spinning - animator is ready
            slot_machine_managers.spin_manager.SetSpinStateTo(SpinStates.spin_start);
        }

        private async Task isAllSlotSubAnimatorsReady(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < reel_strip_managers.Length; reel++)
                {
                    for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                    {
                        if (!reel_strip_managers[reel].slots_in_reel[slot].isAllAnimatorsFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == reel_strip_managers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal bool CheckForWin()
        {
            return slot_machine_managers.paylines_manager.winning_paylines.Length > 0 ? true : false;
        }
        internal async Task isAllAnimatorsThruState(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            AnimatorStateInfo next_state_info;
            bool wait = true;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync.Length; state_machine++)
                {
                    state_info = _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync[state_machine].GetCurrentAnimatorStateInfo(0);

                    while (wait)
                    {
                        state_info = _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync[state_machine].GetCurrentAnimatorStateInfo(0);
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
                    Debug.Log("All States Resolved");
                    if (state_machine == _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal async Task isAllAnimatorsThruStateAndAtPauseState(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync.Length; state_machine++)
                {
                    if(isAnimatorThruState(state, _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync[state_machine]))
                    {
                        if(state_machine == _slot_machine_managers.animator_statemachine_master.animator_state_machines.state_machines_to_sync.Length -1)
                        {
                            is_all_animators_resolved = true;
                        }
                    }
                    else
                    {
                        await Task.Delay(300);
                    }
                }
            }
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
                for (int reel = 0; reel < reel_strip_managers.Length; reel++)
                {
                    for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                    {
                        if (!reel_strip_managers[reel].slots_in_reel[slot].isSymbolAnimationFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == reel_strip_managers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        private void ReduceFreeSpinBy(int amount)
        {
            if(slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins > 0)
            {
                slot_machine_managers.machine_info_manager.SetFreeSpinsTo(slot_machine_managers.machine_info_manager.machineInfoScriptableObject.freespins - amount);
            }
        }

        internal void SetAllAnimatorsTriggerTo(supported_triggers trigger, bool v)
        {
            if(v)
            {
                slot_machine_managers.animator_statemachine_master.SetAllTriggersTo(trigger);
                SetSlotsAnimatorTriggerTo(trigger);
            }
            else
            {
                slot_machine_managers.animator_statemachine_master.ResetAllTrigger(trigger);
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

        private void SetSlotsAnimatorTriggerTo(supported_triggers slot_to_trigger)
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                for (int slot = 0; slot < reel_strip_managers[reel].slots_in_reel.Length; slot++)
                {
                    reel_strip_managers[reel].slots_in_reel[slot].SetTriggerTo(slot_to_trigger);
                    reel_strip_managers[reel].slots_in_reel[slot].SetTriggerSubStatesTo(slot_to_trigger);
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
                    reel_strip_managers[reel].slots_in_reel[slot].ResetTriggerSubStates(slot_to_trigger);
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
            slot_machine_managers.machine_info_manager.SetPlayerWalletTo(to_value);
        }

        internal void SetReelInfoNumbers()
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                ReelStripStruct temp = reel_strip_managers[reel].reelstrip_info;
                temp.reel_number = reel;
                reel_strip_managers[reel].reelstrip_info = temp;
            }
        }

        internal void SetSubStatesAllSlotAnimatorStateMachines()
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].SetAllSlotContainersSubAnimatorStates();
            }
        }

        internal void ClearSubStatesAllSlotAnimatorStateMachines()
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].ClearAllSlotContainersSubAnimatorStates();
            }
        }

        internal void SetManagerStateMachineSubStates()
        {
            string[] keys = GatherKeysFromSubStates();
            AnimatorSubStateMachine[] values = GatherValuesFromSubStates();
            _slot_machine_managers.animator_statemachine_master.SetSubStateMachinesTo(keys,values);
        }

        private AnimatorSubStateMachine[] GatherValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                values.AddRange(reel_strip_managers[reel].ReturnAllValuesFromSubStates());
            }
            return values.ToArray();
        }

        private string[] GatherKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                keys.AddRange(reel_strip_managers[reel].ReturnAllKeysFromSubStates());
            }
            return keys.ToArray();
        }

        internal void SetAllSlotAnimatorSyncStates()
        {
            for (int reel = 0; reel < reel_strip_managers.Length; reel++)
            {
                reel_strip_managers[reel].SetAllSlotContainersAnimatorSyncStates();
            }
        }

        internal void EnsureWeightsAreCorrect()
        {
            //TBD
        }

        internal int GetRollupIndexFromAmountToRack(double amountToRack)
        {
            float betAmount = slot_machine_managers.machine_info_manager.machineInfoScriptableObject.supported_bet_amounts[slot_machine_managers.machine_info_manager.machineInfoScriptableObject.current_bet_amount];
            int index = 0;
            while(amountToRack > betAmount)
            {
                index += 1;
                amountToRack -= betAmount;
                //Big win if we are over the total rollups present
                if (index == slot_machine_managers.soundManager.machineSoundsReference.rollups.Length -1)
                    break;
            }
            return index;
        }

        internal AudioClip ReturnSymbolSound(int winningSymbol)
        {
            return symbols_data_for_matrix.symbols[winningSymbol].winAudioClip;
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