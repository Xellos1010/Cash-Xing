﻿//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Slot.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace BoomSports.Prototype.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseObjectManager : MonoBehaviour
    {
        public delegate void ObjectPointOnPathEvent(BaseObjectManager objectManagerInGroup);
        public event ObjectPointOnPathEvent startSpinCheckNextPointInPath;
        public event ObjectPointOnPathEvent nextPointInPathChanged;
        /// <summary>
        /// Holds the display sequence for symbols. Stepper strips require length == steps allowed per spin. Directional Constant requires length == active display zone positions in group
        /// </summary>
        [SerializeField]
        public List<int> symbolDisplaySequence;
        [SerializeField]
        internal BaseObjectGroupManager baseObjectGroupParent;
        /// <summary>
        /// Cached reference what position started spin in
        /// </summary>
        [SerializeField]
        internal Vector3 startPosition;
        /// <summary>
        /// the end position for the strip to calculate and land on
        /// </summary>
        public Vector3 stopSpinEndPosition;
        /// <summary>
        /// current position of slot in Path
        /// </summary>
        public int indexOnPath;
        /// <summary>
        /// Cached reference to calculate toPosition for the object within spin cycle
        /// </summary>
        [SerializeField]
        internal Vector3 toPosition;
        /// <summary>
        /// Cached reference to hold calculated offset amount for toPosition when object in spin cycle
        /// </summary>
        [SerializeField]
        internal Vector3 offsetAmount;
        /// <summary>
        /// Animator State machine that controls the Object
        /// </summary>
        public AnimatorStateMachineManager animatorStateMachine
        {
            get
            {
                if (_stateMachine == null)
                    _stateMachine = GetComponent<AnimatorStateMachineManager>();
                return _stateMachine;
            }
        }
        [SerializeField]
        internal AnimatorStateMachineManager _stateMachine;
        /// <summary>
        /// Symbol prefabs container
        /// </summary>
        [SerializeField]
        public Transform symbolPrefabsContainer;
        /// <summary>
        /// Reference Symbol Prefabs in order of Symbols in SymbolData Scriptable Object
        /// </summary>
        [SerializeField]
        public Transform[] symbolPrefabs;
        /// <summary>
        /// Is movement enabled
        /// </summary>
        public bool spinMovementEnabled = false;
        /// <summary>
        /// Reference if object is in end position
        /// </summary>
        public bool objectInEndPosition = false;
        /// <summary>
        /// Reference if slot has end graphics assigned then this will be true. Resets on spin start
        /// </summary>
        public bool presentationSymbolSetToEnd = false;
        /// <summary>
        /// The symbol presenting after the reel stops
        /// </summary>
        public string currentPresentingSymbolName;
        /// <summary>
        /// SymbolID being presented
        /// </summary>
        public int currentPresentingSymbolID;
        /// <summary>
        /// When object reachs end of spin cycle new symbol assigned is either random or pre-defined for spin loop or spin end
        /// </summary>
        public bool setToPresentationSymbolNextSpinCycle = false;

        /// <summary>
        /// Local reference for spin manager timer - used to debug Object Position Along Spin Path - 
        /// Ex: Strip Directional Constant Position Along Path: is determined from constantSpeed/fps * spinCurrentTimer
        /// Ex: Strip Directional Stepper: Position Along Path is determined from timeToTraverseUnit (a Unit is 1 ObjectPosition -> x / time + delay till next step) - feed spinCurrentTimer and see where along timeline return position would be - account for reaching end of ConfigurationGroup and MoveToTop was perfromed
        /// </summary>
        [SerializeField]
        internal float spinCurrentTimer = 0.0f;
        /// <summary>
        /// While debuging we are locking the speed to fps
        /// </summary>
        [SerializeField]
        internal double lockFramesSpeedTo = 30.0;
        /// <summary>
        /// Starts a Spin - Resets previous vars - Sets object movement enabled to true - sends an evaluation check for the parent if symbol activates anything on slot enter for next slot in path
        /// </summary>
        public virtual void StartSpin(bool test = false)
        {
            ResetAllVarsFromPreviousSpin();
            SetObjectMovementEnabledTo(true);
            Debug.Log($"Index on path = {indexOnPath}");
            //Cash Crossing Specific Feature - Stepper Strips will trigger Bonus Visuals thru Animator Trigger - may be repurposed for other things
            SignalParentToEvaluateConditionsForNextSlotInPathViaSymbol();
        }
        internal virtual void SetStartPosition() { }
        /// <summary>
        /// Check if next position in path activates a conditional event based on symbol
        /// </summary>
        internal void SignalParentToEvaluateConditionsForNextSlotInPathViaSymbol()
        {
            //This is a work around to get Cash-Xing implemented and can be refined and integrated into a signaling system when an object changes index in path
            //Debug.Log($"{gameObject.name} is signaling {baseObjectGroupParent.gameObject.name} to check for evaluation conditions for next slot");
            startSpinCheckNextPointInPath?.Invoke(this);
        }

        /// <summary>
        /// Sets the objects movement enabled - OnTrue uses current local position as start position
        /// </summary>
        /// <param name="enableDisable"></param>
        internal void SetObjectMovementEnabledTo(bool enableDisable)
        {
            spinMovementEnabled = enableDisable;
            if (enableDisable)
            {
                //This is a spin starting
                objectInEndPosition = false;
                presentationSymbolSetToEnd = false;
                startPosition = transform.localPosition;
                setToPresentationSymbolNextSpinCycle = false;
                objectInEndPosition = false;
                presentationSymbolSetToEnd = false;
                stopSpinEndPosition = Vector3.zero;
                spinCurrentTimer = 0.0f;
            }
        }


        internal void SetTriggerSubStatesTo(supportedAnimatorTriggers toTrigger)
        {
            //Debug.Log(String.Format("Setting sub states to trigger {0}",toTrigger.ToString()));
            animatorStateMachine.SetSubStateMachinesTriggerTo(0, toTrigger);
        }

        internal void ResetTriggerSubStates(supportedAnimatorTriggers triggerToReset)
        {
            animatorStateMachine.ResetTriggerStateMachines(triggerToReset);
        }

        internal void SetAllSubStateAnimators()
        {
            animatorStateMachine.SetStateMachinesBySubAnimators();
        }

        internal void ClearAllSubStateAnimators()
        {
            animatorStateMachine.ClearStateMachinesBySubAnimators();
        }

        internal void SetStateMachineAnimators()
        {
            animatorStateMachine.SetStateMachineSyncAnimators();
        }

        internal bool isAllAnimatorsFinished(string animation_to_check)
        {
            bool output = false;
            for (int subStateMachine = 0; subStateMachine < animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines.Length; subStateMachine++)
            {
                for (int animator = 0; animator < animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[subStateMachine].sub_state_animators.Length; animator++)
                {
                    AnimatorStateInfo state_info = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[subStateMachine].sub_state_animators[animator].GetCurrentAnimatorStateInfo(0);
                    //Debug.Log(String.Format("Current State Normalized Time = {0} State Name = {1}", state_info.normalizedTime, state_info.IsName(animation_to_check) ? animation_to_check : "Something Else"));

                    if (state_info.IsName(animation_to_check) && state_info.normalizedTime >= 1 && (subStateMachine == animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines.Length - 1) && (animator == animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[subStateMachine].sub_state_animators.Length))
                    {
                        output = true;
                    }
                    else
                    {
                        //Debug.Log(String.Format("Not {0}", animation_to_check));
                        break;
                    }
                }
            }
            return output;
        }

        internal void AddAnimatorsToList(ref List<Animator> output)
        {
            for (int subStateMachine = 0; subStateMachine < animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines.Length; subStateMachine++)
            {
                for (int animator = 0; animator < animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[subStateMachine].sub_state_animators.Length; animator++)
                {
                    output.Add(animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[subStateMachine].sub_state_animators[animator]);
                }
            }
        }
        internal bool isSymbolAnimatorFinishedAndAtPauseState(string stateToCheck)
        {
            if (currentPresentingSymbolID > 0)
            {
                AnimatorStateInfo state_info = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[currentPresentingSymbolID].GetCurrentAnimatorStateInfo(0);
                if (state_info.IsName(stateToCheck) && state_info.normalizedTime >= 1.0)
                {
                    return true;
                }
                else
                {
                    //No way to check if its passed the state or what without making a hashtable with all the state names codes
                    return false;
                }
            }
            return true;
        }

        internal void SetAllSubSymbolsGameobjectActive()
        {
            for (int symbol_prefab = 0; symbol_prefab < symbolPrefabs.Length; symbol_prefab++)
            {
                if (symbolPrefabs[symbol_prefab].gameObject.activeSelf == false)
                    symbolPrefabs[symbol_prefab].gameObject.SetActive(true);
                symbolPrefabs[symbol_prefab].GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void InstantiateSymbolPrefabs()
        {
#if UNITY_EDITOR
            //Clear all sub objects
            for (int i = transform.childCount-1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            symbolPrefabs = new Transform[baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length];
            for (int symbol = 0; symbol < symbolPrefabs.Length; symbol++)
            {
                symbolPrefabs[symbol] = PrefabUtility.InstantiatePrefab(baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols[symbol].symbolPrefab) as Transform;
                symbolPrefabs[symbol].gameObject.name = String.Format("{0}", baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols[symbol].symbolName);
                symbolPrefabs[symbol].parent = transform;
                symbolPrefabs[symbol].localPosition = Vector3.zero;
                symbolPrefabs[symbol].localRotation = Quaternion.LookRotation(Vector3.back);
                symbolPrefabs[symbol].localScale = Vector3.one;
                symbolPrefabs[symbol].gameObject.SetActive(false);
            }
#endif
        }


        /// <summary>
        /// Sets the sub-state machine references to the symbol animators
        /// </summary>
        internal void SetSubStateMachineAnimators()
        {
            Animator[] sub_states = transform.GetComponentsInChildren<Animator>(true).RemoveAt<Animator>(0);
            //Remove at 1st index because self reference
            animatorStateMachine.SetSubStateMachinesTo(ref sub_states);
        }

        internal void SetBoolStateMachines(supportedAnimatorBools bool_name, bool v)
        {
            animatorStateMachine.SetBoolAllStateMachines(bool_name, v);
        }

        internal void ResetAnimator()
        {
            animatorStateMachine.InitializeAnimator();
        }

        internal void InitializeAnimatorToPresentWin()
        {
            animatorStateMachine.InitializeAnimator();

        }

        internal void SetTriggerTo(supportedAnimatorTriggers to_trigger)
        {
            animatorStateMachine.SetAllTriggersTo(to_trigger);
        }

        internal void ResetTrigger(supportedAnimatorTriggers slot_to_trigger)
        {
            animatorStateMachine.ResetAllTrigger(slot_to_trigger);
        }

        internal void ShowRandomSymbol()
        {
            //Debug.Log($"baseObjectGroupParent = {baseObjectGroupParent.gameObject.name}");
            //Debug.Log($"baseObjectGroupParent.configurationObjectParent = {baseObjectGroupParent.configurationObjectParent.gameObject.name}");
            ShowSymbolRenderer(baseObjectGroupParent.configurationObjectParent.DrawRandomSymbolFromCurrentMode());
        }
        /// <summary>
        /// Shows a symbols renderer
        /// </summary>
        /// <param name="symbol_to_show">which symbol</param>
        /// <param name="force_hide_others">will force hide other symbol renderers defaulttrue</param>
        private void ShowSymbolRenderer(int symbol_to_show, bool force_hide_others = true)
        {
            //Debug.Log(String.Format("Enabling symbol_prefabs[{0}] = {1}", symbol_to_show, symbol_prefabs[symbol_to_show].gameObject.ToString()));
            //Ensure Symbol Prefab Objects are instantiated
            if (symbolPrefabs?.Length != baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length)
            {
                if (baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length == transform.childCount)
                {
                    //Set array to children
                    SetPrefabArrayToChildren();
                }
                else
                {
                    InstantiateSymbolPrefabs();
                }
            }
            //null check
            for (int i = 0; i < symbolPrefabs.Length; i++)
            {
                if(symbolPrefabs[i] == null && transform.childCount != baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length)
                {
                    InstantiateSymbolPrefabs();
                    break;
                }
                else
                {
                    if (symbolPrefabs.Length != baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length)
                        symbolPrefabs = new Transform[baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length];
                    for (int j = 0; j < transform.childCount; j++)
                    {
                        //Debug.Log($"transform.childCount = {transform.childCount} child getting = {j}");
                        symbolPrefabs[j] = transform.GetChild(j);
                    }
                    break;
                }
            }
            MeshRenderer[] renderers;
            for (int symbol_prefab = 0; symbol_prefab < symbolPrefabs.Length; symbol_prefab++)
            {
                if (symbolPrefabs[symbol_prefab].gameObject.activeSelf == false)
                    symbolPrefabs[symbol_prefab].gameObject.SetActive(true);
                if (force_hide_others)
                {
                    renderers = symbolPrefabs[symbol_prefab].GetChild(0).GetComponentsInChildren<MeshRenderer>();
                    for (int renderer = 0; renderer < renderers.Length; renderer++)
                    {
                        if (renderers[renderer].enabled && symbol_prefab != symbol_to_show)
                            renderers[renderer].enabled = false;
                    }
                }
            }
            renderers = symbolPrefabs[symbol_to_show].GetChild(0).GetComponentsInChildren<MeshRenderer>();
            for (int renderer = 0; renderer < renderers.Length; renderer++)
            {
                renderers[renderer].enabled = true;
            }
        }

        internal void SetIndexOnPathfromCurrentPosition(ref Vector3[] localPositionsInStrip)
        {
            for (int position = 0; position < localPositionsInStrip.Length; position++)
            {
                //If the sqr magnitude of local position is 
                if(Mathf.Abs(transform.localPosition.sqrMagnitude) <= Mathf.Abs(localPositionsInStrip[position].sqrMagnitude))
                {
                    indexOnPath = position;
                    break;
                }
            }
        }

        private void SetPrefabArrayToChildren()
        {
            if (symbolPrefabs.Length != baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length)
                symbolPrefabs = new Transform[baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols.Length];
            for (int j = 0; j < transform.childCount; j++)
            {
                Debug.Log($"transform.childCount = {transform.childCount} child getting = {j}");
                symbolPrefabs[j] = transform.GetChild(j);
            }
        }

        private string ReturnSymbolNameFromInt(int symbol)
        {
            return baseObjectGroupParent.configurationObjectParent.symbolDataScriptableObject.symbols[symbol].symbolName;
        }

        internal async void SetSymbolResolveWin()
        {
            //Set the sub symbol Animator
            //Debug.Log(String.Format("Setting {0} to symbol win for {1}",String.Join("_",transform.gameObject.name,transform.parent.gameObject.name),presentation_symbol));
            Animator sub_state_animator = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[currentPresentingSymbolID]; //may display wrong animator is out of order
            //Debug.Log(String.Format("Symbol Set to win = {0}", sub_state_animator.transform.name));
            SetBoolTo(ref sub_state_animator, supportedAnimatorBools.SymbolResolve, true);
            SetBoolTo(ref sub_state_animator, supportedAnimatorBools.LoopPaylineWins, true);
            //PingPong float
            //StartCoroutine(PingPongAnimation());
            //SetPingPong(true);
            //SetFloatMotionTimeTo(0);
            //sub_state_animator.Play("Resolve_Win_Idle", -1, 0);
        }

        internal async void PlayAnimationOnPresentationSymbol(string animation)
        {
            Animator sub_state_animator = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[currentPresentingSymbolID]; //may display wrong animator is out of order
            sub_state_animator.Play(animation, -1, 0);
        }


        internal bool isSymbolAnimationFinished(string animation_to_check)
        {
            if (currentPresentingSymbolID > 0)
            {
                AnimatorStateInfo state_info = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[currentPresentingSymbolID].GetCurrentAnimatorStateInfo(0);
                //Debug.Log(String.Format("Current State Normalized Time = {0} State Name = {1}", state_info.normalizedTime, state_info.IsName(animation_to_check) ? animation_to_check : "Something Else"));

                if (state_info.IsName(animation_to_check))
                {
                    return true;
                }
                else
                {
                    //Debug.Log(String.Format("Not {0}", animation_to_check));
                    return false;
                }
            }
            //Default return true if animator is not on matrix
            return true;
        }

        internal void SyncSymbolInfoToCurrentDisplay()
        {
            MeshRenderer activeMeshrenderer = null;
            //Get sub symbol active
            for (int i = 0; i < transform.childCount; i++)
            {
                activeMeshrenderer = transform.GetChild(i).GetComponentInChildren<MeshRenderer>();
                if (activeMeshrenderer.enabled)
                {
                    break;
                }
            }
            //Debug.Log($"activeMeshrenderer.transform.parent.name = ");
            currentPresentingSymbolID = activeMeshrenderer.transform.parent.GetSiblingIndex();
            currentPresentingSymbolName = activeMeshrenderer.transform.parent.name;
        }

        internal void SyncCurrentDisplaySymbolInfo()
        {
            ShowSymbolRenderer(currentPresentingSymbolID);
        }

        internal void SetBoolTo(ref Animator animator, supportedAnimatorBools supportedBool, bool value)
        {
            //Debug.Log(String.Format("{0} bool {1} is {2}", animator.gameObject.name, supportedBool.ToString(), value));
            animatorStateMachine.SetBool(ref animator, supportedBool, value);
        }

        internal void SetSymbolResolveToLose()
        {
            if (Application.isPlaying)
            {
                Animator subAnimatorStateMachines = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[currentPresentingSymbolID];
                SetBoolTo(ref subAnimatorStateMachines, supportedAnimatorBools.SymbolResolve, false);
            }
        }

        internal void SetOverrideControllerTo(AnimatorOverrideController animatorOverrideController)
        {
            animatorStateMachine.SetRuntimeControllerTo(animatorOverrideController);
        }

        internal void SetDisplaySymbolTo(NodeDisplaySymbolContainer symbol_to_display)
        {
            //Debug.Log($"Setting Display symbol for {gameObject.name} to {symbol_to_display.primary_symbol}");
            SetPresentationSymbolTo(symbol_to_display.primarySymbol);
            ShowSymbolRenderer(symbol_to_display.primarySymbol);
            //if (symbol_to_display.is_overlay)
            //{
            //    ShowSymbolRenderer(symbol_to_display.overlay_symbol, false);
            //}
        }

        internal void SetPresentationSymbolTo(int to_symbol)
        {
            if (to_symbol < 0)
                currentPresentingSymbolName = "Not on Matrix";
            else
                currentPresentingSymbolName = ReturnSymbolNameFromInt(to_symbol);
            currentPresentingSymbolID = to_symbol; 
        }
        internal void UpdateSpinTimerFromSpinManager()
        {
            SetLocalCurrentSpinTimerTo(baseObjectGroupParent.configurationObjectParent.managers.spinManager.timeCounter);
        }

        private void SetLocalCurrentSpinTimerTo(float toValue)
        {
            spinCurrentTimer = toValue;
        }

        internal virtual void ResetAllVarsFromPreviousSpin()
        {
            SetObjectMovementEnabledTo(false);
        }

        /// <summary>
        /// Used to set the slot to go to end position
        /// </summary>
        internal virtual void SetToStopSpin()
        {
            setToPresentationSymbolNextSpinCycle = true;
            objectInEndPosition = false;
            presentationSymbolSetToEnd = false;
        }
        internal virtual void Update() { }
    }
}