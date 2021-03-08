using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractionController))]
    class InteractionControllerEditor : BoomSportsEditor
    {
        InteractionController myTarget;
        public void OnEnable()
        {
            myTarget = (InteractionController)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Interaction Controller Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Interaction Controller Controls");
            base.OnInspectorGUI();
        }


    }
#endif
    
    public class InteractionController : MonoBehaviour
    { 
        public AnimatorStateMachineManager StateMachineController
        {
            get
            {
                if (_StateMachineController == null)
                    _StateMachineController = GameObject.FindGameObjectWithTag("StateMachine").GetComponent<AnimatorStateMachineManager>();
                return _StateMachineController;
            }
        }
        public AnimatorStateMachineManager _StateMachineController;

        public bool canTriggerSet = false;
        // Update is called once per frame
        void Update()
        {
            if (canTriggerSet)
            {
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                Touch temp = Input.touches[0];
                if (temp.phase == TouchPhase.Began)
                {
                    Debug.Log("Trigger for spin/slam pressed");
                    if (StateManager.enCurrentState == States.idle_idle)
                    {
                        SetTrigger(supported_triggers.SpinStart);
                    }
                    else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_loop)
                    {
                        SetTrigger(supported_triggers.SpinSlam);
                    }
                }
            }
#endif
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Trigger for spin/slam pressed");
                    if (StateManager.enCurrentState == States.Idle_Idle)
                    {
                        SetTriggerTo(supported_triggers.SpinStart);
                    }
                    else if (StateManager.enCurrentState == States.Spin_Intro || StateManager.enCurrentState == States.Spin_Idle)
                    {
                        SlamSpin();
                    }
                }
            }
            else
            {
                if (StateManager.enCurrentState == States.Idle_Intro)
                {
                    canTriggerSet = true;
                    SetTriggerTo(supported_triggers.End);
                }
            }
        }

        private void SetTriggerTo(supported_triggers to_trigger)
        {
            StateMachineController.SetTrigger(to_trigger);
        }

        public void SlamSpin()
        {
            SetTriggerTo(supported_triggers.SpinSlam);
        }


        private void SetStateTo(States to_state)
        {
            canTriggerSet = false;
            StateManager.SetStateTo(to_state);
        }

    }
}