//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Slot.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Slot_Engine.Matrix.EndConfigurationManager;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SlotManager))]
    class SlotEditor : BoomSportsEditor
    {
        SlotManager myTarget;
        public void OnEnable()
        {
            myTarget = (SlotManager)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if(GUILayout.Button("Set Sub Animators State Machine"))
            {
                myTarget.SetSubStateMachineAnimators();
            }
            if (GUILayout.Button("Set Animators To Sync State Machine"))
            {
                myTarget.SetStateMachineAnimators();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            base.OnInspectorGUI();

        }
    }
#endif
        public class SlotManager : MonoBehaviour
        {
        /// <summary>
        /// The symbol presenting after the reel stops
        /// </summary>
        public string presentation_symbol_name;
        public int presentation_symbol;
        
        public ReelStripManager reel_parent;

        public bool movement_enabled = false;
        /// <summary>
        /// the end position for the reels to calculate and land on
        /// </summary>
        public Vector3 end_position;
        /// <summary>
        /// While slot is moving will go to end position and stop
        /// </summary>
        public bool set_to_display_end_symbol = false;
        /// <summary>
        /// Reference if slot is in end position
        /// </summary>
        public bool slot_in_end_position = false;
        /// <summary>
        /// Reference if slot has end graphics assigned
        /// </summary>
        public bool graphics_set_to_end = false;

        public MeshRenderer _meshRenderer;

        public AnimatorStateMachineManager state_machine
        {
            get
            {
                if (_state_machine == null)
                    _state_machine = GetComponent<AnimatorStateMachineManager>();
                return _state_machine;
            }
        }
        [SerializeField]
        internal AnimatorStateMachineManager _state_machine;

        public class SymbolPrefab
        {
            //Path to prefab
            //Generated Prefab Reference
            //Generate Prefab Call
        }
        public SymbolPrefab symbol_prefab;

        public Transform symbol_prefabs_container;
        public Transform[] symbol_prefabs;

        public MeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponentInChildren<MeshRenderer>();
                return _meshRenderer;
            }
        }

        public void StartSpin()
        {
            ResetAllVars();
            SetSlotMovementEnabledTo(true);
            SetTriggerTo(supported_triggers.SpinStart);
            SetTriggerSubStatesTo(supported_triggers.SpinStart);
        }

        Vector3 GeneratePositionUpdateSpeed(Vector3 amount_to_add) //Needs to be positive to move forwards and negative to move backwards
        {
            return transform.localPosition + amount_to_add; //new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        void Update()
        {
            if (movement_enabled)
            {
                Vector3 toPosition;
               
                toPosition = GeneratePositionUpdateSpeed(reel_parent.reelstrip_info.spin_parameters.reel_spin_direction * reel_parent.reel_spin_speed_current);
                //Check X Y and Z and move slot to opposite

                //Check if to far left or right and move

                //Check if to far down or up and move
                if (reel_parent.reelstrip_info.spin_parameters.reel_spin_direction.y < 0)
                {
                    if (toPosition.y <= reel_parent.positions_in_path_v3_local[reel_parent.positions_in_path_v3_local.Length - 1].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3_local[reel_parent.positions_in_path_v3_local.Length - 1], true);
                }
                else if (reel_parent.reelstrip_info.spin_parameters.reel_spin_direction.y > 0)
                {
                    if (toPosition.y >= reel_parent.positions_in_path_v3_local[0].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3_local[reel_parent.positions_in_path_v3_local.Length - 1], false);
                }
                if(set_to_display_end_symbol && graphics_set_to_end)
                    if (toPosition.y <= end_position.y) //TODO refactor for Omni Spin
                    {
                        toPosition = end_position;
                        slot_in_end_position = true;
                        ResetAllVars();
                    }
                transform.localPosition = toPosition;
            }
        }

        private void ResetAllVars()
        {
            SetSlotMovementEnabledTo(false);
            set_to_display_end_symbol = false;
            //Set state of reel to end
        }


        private void ShiftToPositionBy(ref Vector3 toPosition, Vector3 lastPosition, bool upDown)
        {
            if(upDown)
                toPosition = new Vector3(toPosition.x,toPosition.y - lastPosition.y, toPosition.z);
            else
                toPosition = new Vector3(toPosition.x, toPosition.y + lastPosition.y, toPosition.z);

            if(set_to_display_end_symbol)
            {
                //Set Graphics and end position
                graphics_set_to_end = true;
                end_position = reel_parent.positions_in_path_v3_local[(reel_parent.positions_in_path_v3_local.Length - 2) - reel_parent.end_symbols_set_from_config];

                if (reel_parent.end_symbols_set_from_config < reel_parent.ending_symbols.Length)
                {
                    SetDisplaySymbolTo(reel_parent.ending_symbols[reel_parent.ending_symbols.Length - 1 - reel_parent.end_symbols_set_from_config]);
                    reel_parent.end_symbols_set_from_config += 1;
                }
                else
                {
                    SetPresentationSymbolTo(-1); //TODO Define whether to set the top slot graphic
                }
                //Debug.Log("Slot " + transform.name + " symbol presentation = " + presentation_symbol + " end position = " + end_position);
            }
            else
            {
                bool symbol_set = false;
                if (reel_parent.change_symbol_graphic_on_spin_idle)
                {
                    //If Symbol Generated = opverlay - Generate Sub Symbol and attach 2 materials
                    if (reel_parent.reelstrip_info.spin_info.reel_spin_symbols != null)
                    {
                        if (reel_parent.reelstrip_info.spin_info.reel_spin_symbols.Length > 0)
                        {
                            SlotDisplaySymbol symbol = reel_parent.ReturnNextSymbolInStrip();
                            SetDisplaySymbolTo(symbol);
                            symbol_set = true;
                        }
                    }
                    if (!symbol_set)
                    {
                        //Determines an overlay symbol
                        SlotDisplaySymbol symbol = reel_parent.matrix.slot_machine_managers.end_configuration_manager.GetRandomWeightedSymbol(StateManager.enCurrentMode);
                        SetDisplaySymbolTo(symbol);
                    }
                }
            }
        }

        private string ReturnSymbolNameFromInt(int symbol)
        {
            return ((Symbol)symbol).ToString();
        }

        internal async void SetSymbolResolveWin()
        {
            //Set the sub symbol Animator
            Debug.Log(String.Format("Setting {0} to symbol win for {1}",String.Join("_",transform.gameObject.name,transform.parent.gameObject.name),presentation_symbol));
            Animator sub_state_animator = state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine[0].sub_state_animators[presentation_symbol]; //may display wrong animator is out of order
            Debug.Log(String.Format("Symbol Set to win = {0}", sub_state_animator.transform.name));
            SetBoolTo(ref sub_state_animator, supported_bools.SymbolResolve, true);
            SetBoolTo(ref sub_state_animator, supported_bools.LoopPaylineWins, true);
            //PingPong float
            //StartCoroutine(PingPongAnimation());
            //SetPingPong(true);
            //SetFloatMotionTimeTo(0);
            sub_state_animator.Play("Resolve_Win_Idle", -1, 0);
        }


        internal bool isSymbolAnimationFinished(string animation_to_check)
        {
            if (presentation_symbol > 0)
            {
                AnimatorStateInfo state_info = state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine[0].sub_state_animators[presentation_symbol].GetCurrentAnimatorStateInfo(0);
                Debug.Log(String.Format("Current State Normalized Time = {0} State Name = {1}", state_info.normalizedTime, state_info.IsName(animation_to_check) ? animation_to_check : "Something Else"));
                
                if (state_info.IsName(animation_to_check))
                {
                    return true;
                }
                else
                {
                    Debug.Log(String.Format("Not {0}", animation_to_check));
                    return false;
                }
            }
            //Default return true if animator is not on matrix
            return true;
        }

        private void SetBoolTo(ref Animator animator, supported_bools symbolResolve, bool value)
        {
            state_machine.SetBool(ref animator, symbolResolve,value);
        }

        public bool ping_pong = false;
        public bool up_down = true;
        public float new_percentage = 0;

        public float completePercent = 0;

        private void SetFloatMotionTimeTo(float v)
        {
            Debug.Log("Setting Motion Time nOt implemented");
            //state_machine.SetFloatTo(supported_floats.MotionTime,0.0f);
        }

        private void SetBoolTo(supported_bools bool_name, bool v)
        {
            Debug.Log("Not Implemented SetBoolTo");
            //state_machine.SetAllBoolStateMachinesTo(bool_name,v);
        }

        internal void SetSymbolResolveToLose()
        {
            //Debug.Log(String.Format("Symbol Resolve Lose - presentation_symbol = {0} state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine[0].sub_state_animators = {1}", presentation_symbol, state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine[0].sub_state_animators.Length));
            Animator sub_state_animator = state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine[0].sub_state_animators[presentation_symbol];
            SetBoolTo(ref sub_state_animator, supported_bools.SymbolResolve, false);
            if (!sub_state_animator.GetCurrentAnimatorStateInfo(0).IsName("Resolve_Intro"))
            {
                Debug.Log(String.Format("current state name != Resolve Intro"));
                sub_state_animator.PlayInFixedTime("Resolve_Intro", -1, 0);
            }
        }

        internal void SetOverrideControllerTo(AnimatorOverrideController animatorOverrideController)
        {
            state_machine.SetRuntimeControllerTo(animatorOverrideController);
        }

        internal void SetDisplaySymbolTo(SlotDisplaySymbol symbol_to_display)
        {
            //Debug.Log(string.Format("Set Display symbol to {0}", v));
            SetPresentationSymbolTo(symbol_to_display.primary_symbol);
            ShowSymbolRenderer(symbol_to_display.primary_symbol);
            if(symbol_to_display.is_overlay)
            {
                //ShowSymbolOverlay();
                ShowSymbolRenderer(symbol_to_display.overlay_symbol, false);
            }
        }

        private void ShowSymbolOverlay()
        {
            //ShowSymbolRenderer(symbol_to_display.primary_symbol);
        }

        private void SetPresentationSymbolTo(Symbol to_symbol)
        {
            SetPresentationSymbolTo((int)to_symbol);
        }
        private void SetPresentationSymbolTo(int to_symbol)
        {
            if (to_symbol < 0)
                presentation_symbol_name = "Not on Matrix";
            else
                presentation_symbol_name = ReturnSymbolNameFromInt(to_symbol);
            presentation_symbol = to_symbol;
        }
        /// <summary>
        /// Sets the sub-state machine references to the symbol animators
        /// </summary>
        internal void SetSubStateMachineAnimators()
        {
            Animator[] sub_states = transform.GetComponentsInChildren<Animator>(true).RemoveAt<Animator>(0);
            //Remove at 1st index because self reference
            state_machine.SetSubStateMachinesTo(ref sub_states);
        }

        internal void SetBoolStateMachines(supported_bools bool_name, bool v)
        {
            state_machine.SetBoolAllStateMachines(bool_name, v);
        }

        /// <summary>
        /// Used to set the slot to go to end position
        /// </summary>
        internal void SetToStopSpin()
        {
            set_to_display_end_symbol = true;
            slot_in_end_position = false;
            graphics_set_to_end = false;
        }

        internal void SetSlotMovementEnabledTo(bool enable_disable)
        {
            movement_enabled = enable_disable;
            if (enable_disable)
            {
                slot_in_end_position = false;
                graphics_set_to_end = false;
            }
        }

        internal void ResetAnimator()
        {
            state_machine.InitializeAnimator();
        }

        internal void InitializeAnimatorToPresentWin()
        {
            state_machine.InitializeAnimator();
            state_machine.SetBoolAllStateMachines(supported_bools.WinRacking, true);
            
        }

        internal void SetTriggerTo(supported_triggers to_trigger)
        {
            state_machine.SetAllTriggersTo(to_trigger);
        }

        internal void ResetTrigger(supported_triggers slot_to_trigger)
        {
            state_machine.ResetAllTrigger(slot_to_trigger);
        }

        internal void ShowRandomSymbol()
        {
            ShowSymbolRenderer(reel_parent.matrix.symbol_weights_per_state[StateManager.enCurrentMode].intDistribution.Draw());
        }
        /// <summary>
        /// Shows a symbols renderer
        /// </summary>
        /// <param name="symbol_to_show">which symbol</param>
        /// <param name="force_hide_others">will force hide other symbol renderers defaulttrue</param>
        private void ShowSymbolRenderer(int symbol_to_show, bool force_hide_others = true)
        {
            //Ensure Symbol Prefab Objects are instantiated
            if (symbol_prefabs?.Length != reel_parent.matrix.symbols_data_for_matrix.symbols.Length)
            {
                InstantiateSymbolPrefabs();
            }
            MeshRenderer renderer;
            for (int symbol_prefab = 0; symbol_prefab < symbol_prefabs.Length; symbol_prefab++)
            {
                if (symbol_prefabs[symbol_prefab].gameObject.activeSelf == false)
                    symbol_prefabs[symbol_prefab].gameObject.SetActive(true);
                if (force_hide_others)
                {
                    renderer = symbol_prefabs[symbol_prefab].GetChild(0).GetComponent<MeshRenderer>();
                    if (renderer.enabled && symbol_prefab != symbol_to_show)
                        renderer.enabled = false;
                }
            }
            symbol_prefabs[symbol_to_show].GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        }

        internal void SetAllSubSymbolsGameobjectActive()
        {
            for (int symbol_prefab = 0; symbol_prefab < symbol_prefabs.Length; symbol_prefab++)
            {
                if (symbol_prefabs[symbol_prefab].gameObject.activeSelf == false)
                    symbol_prefabs[symbol_prefab].gameObject.SetActive(true);
                symbol_prefabs[symbol_prefab].GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void InstantiateSymbolPrefabs()
        {
#if UNITY_EDITOR
            symbol_prefabs = new Transform[reel_parent.matrix.symbols_data_for_matrix.symbols.Length];
            for (int symbol = 0; symbol < symbol_prefabs.Length; symbol++)
            {
                symbol_prefabs[symbol] = PrefabUtility.InstantiatePrefab(reel_parent.matrix.symbols_data_for_matrix.symbols[symbol].symbol_prefab) as Transform;
                symbol_prefabs[symbol].gameObject.name = String.Format("Symbol_{0}", reel_parent.matrix.symbols_data_for_matrix.symbols[symbol].symbol_name);
                symbol_prefabs[symbol].parent = transform;
                symbol_prefabs[symbol].localPosition = Vector3.zero;
                symbol_prefabs[symbol].localRotation = Quaternion.LookRotation(Vector3.back);
                symbol_prefabs[symbol].localScale = reel_parent.matrix.slot_size;
                symbol_prefabs[symbol].gameObject.SetActive(false);
        }
#endif
        }

        internal void SetTriggerSubStatesTo(supported_triggers toTrigger)
        {
            //Debug.Log(String.Format("Setting sub states to trigger {0}",toTrigger.ToString()));
            state_machine.SetStateMachinesTriggerTo(toTrigger);
        }

        internal void ResetTriggerSubStates(supported_triggers triggerToReset)
        {
            state_machine.ResetTriggerStateMachines(triggerToReset);
        }

        internal void SetAllSubStateAnimators()
        {
            state_machine.SetStateMachinesBySubAnimators();
        }

        internal void ClearAllSubStateAnimators()
        {
            state_machine.ClearStateMachinesBySubAnimators();
        }

        internal void SetStateMachineAnimators()
        {
            state_machine.SetStateMachineSyncAnimators();
        }
    }
}