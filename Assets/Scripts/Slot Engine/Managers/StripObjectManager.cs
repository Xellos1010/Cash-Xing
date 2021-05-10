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
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StripObjectManager))]
    class SlotEditor : BoomSportsEditor
    {
        StripObjectManager myTarget;
        public void OnEnable()
        {
            myTarget = (StripObjectManager)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Calculate MoveObjectToSpinPosition()"))
            {
                Debug.Log($"MoveObjectToSpinPosition() output toPosition = {myTarget.MoveObjectToSpinPosition()}");
            }
            if (GUILayout.Button("Set Sub Animators State Machine"))
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
    public class StripObjectManager : BaseObjectManager
    {
        /// <summary>
        /// Reference to the start position - always set on instantiate and spinEnd
        /// </summary>
        [SerializeField]
        internal int startPositionIndex;
        /// <summary>
        /// times object has reached end of path - set to 0 on spin start and instantiate
        /// </summary>
        [SerializeField]
        internal int timesReachedEndOfPath;

        /// <summary>
        /// StripGroupManager is a group of StripObjects
        /// </summary>
        public StripObjectGroupManager stripManager
        {
            get
            {
                return baseObjectGroupParent as StripObjectGroupManager;
            }
        }
        /// <summary>
        /// Starts a Spin
        /// </summary>
        public override void StartSpin()
        {
            ResetAllVars();
            timesReachedEndOfPath = 0;
            SetObjectMovementEnabledTo(true);
        }

        Vector3 OffsetPositionBy(Vector3 amountToAdd) //Needs to be positive to move forwards and negative to move backwards
        {
            return transform.localPosition + amountToAdd; //new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        internal override void Update()
        {
            if (spinMovementEnabled)
            {
                MoveObjectToSpinPosition();
            }
        }
        /// <summary>
        /// Moves an objects along SpinCycle (Spin Sequence/Path) and returns the calculated to position based on spinCurrentTimer;
        /// </summary>
        /// <returns></returns>
        internal Vector3 MoveObjectToSpinPosition()
        {
            toPosition = Vector3.zero;
            BasePathTransformSpinEvaluatorScriptableObject temp = stripManager.stripInfo.GetSpinParameters();
            if (Application.isPlaying)
            {
                UpdateSpinTimerFromSpinManager();
            }
            StripObjectGroupManager temp2 = baseObjectGroupParent as StripObjectGroupManager;
            SpinPath pathToEvaluate = new SpinPath(temp2.localPositionsInStrip, startPositionIndex);
            temp.EvaluateSpin(spinCurrentTimer, ref pathToEvaluate);
            toPosition = pathToEvaluate.toPositionEvaluated;
            Debug.Log($"Offsetting Position by {toPosition.ToString()}");
            Debug.Log($"Times reached end of path =  {pathToEvaluate.timesReachedEndOfPath} timesReachedEndOfPath supplied = {timesReachedEndOfPath}");
            if (timesReachedEndOfPath != pathToEvaluate.timesReachedEndOfPath)
            {
                timesReachedEndOfPath = pathToEvaluate.timesReachedEndOfPath;
                if (Application.isPlaying)
                    SetSymbolGraphics();
            }
            if (Application.isPlaying)
                toPosition = OffsetPositionBy(toPosition);
            return toPosition;
        }

        private void SetSymbolGraphics()
        {
            if (setToPresentationSymbolNextSpinCycle)
            {
                //Set Graphics and end position
                presentationSymbolSetToEnd = true;
                stopSpinEndPosition = stripManager.localPositionsInStrip[(stripManager.localPositionsInStrip.Length - 2) - stripManager.endSymbolsSetFromConfiguration];

                if (stripManager.endSymbolsSetFromConfiguration < stripManager.ending_symbols.Length)
                {
                    SetDisplaySymbolTo(stripManager.ending_symbols[stripManager.ending_symbols.Length - 1 - stripManager.endSymbolsSetFromConfiguration]);
                    stripManager.endSymbolsSetFromConfiguration += 1;
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
                if (stripManager.randomSetSymbolOnEndOfSequence)
                {
                    //If Symbol Generated = opverlay - Generate Sub Symbol and attach 2 materials
                    if (stripManager.stripInfo.spin_info.stripSpinSymbols != null)
                    {
                        if (stripManager.stripInfo.spin_info.stripSpinSymbols.Length > 0)
                        {
                            NodeDisplaySymbol symbol = stripManager.ReturnNextSymbolInStrip();
                            SetDisplaySymbolTo(symbol);
                            symbol_set = true;
                        }
                    }
                    if (!symbol_set)
                    {
                        //Determines an overlay symbol
                        NodeDisplaySymbol symbol = stripManager.configurationObjectParent.managers.endConfigurationManager.GetRandomWeightedSymbol(StateManager.enCurrentMode).Result;
                        SetDisplaySymbolTo(symbol);
                    }
                }
            }
        }

        internal Animator SetOverlayAnimatorToFeatureAndGet()
        {
            Animator output;
            //Compare to Symbols
            for (int i = 0; i < stripManager.configurationObjectParent.symbolDataScriptableObject.symbols.Length; i++)
            {
                if (stripManager.configurationObjectParent.isSymbolOverlay(i))
                {
                    output = stateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[i];
                    SetBoolTo(ref output, supportedAnimatorBools.FeatureTrigger, true);
                    return output;
                }
            }
            return null;
        }

    }
}