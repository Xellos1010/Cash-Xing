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

            if (GUILayout.Button("Display Current Animator Layer Name"))
            {
                myTarget.DisplayAnimatorInfo();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Interaction Controller Controls");
            base.OnInspectorGUI();
        }


    }
#endif
    
    public class InteractionController : MonoBehaviour
    {
        public enum supported_triggers
        {
            SpinStart,
            SpinSlam,
            SpinResolve,
            End
        }
        public int idle_idle_hash = Animator.StringToHash("Idle_Idle");
        public Animator StateMachineController
        {
            get
            {
                if (_StateMachineController == null)
                    _StateMachineController = GameObject.FindGameObjectWithTag("StateMachine").GetComponent<Animator>();
                return _StateMachineController;
            }
        }
        public Animator _StateMachineController;

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
            else
            {
                if (StateManager.enCurrentState == States.idle_intro)
                {
                    canTriggerSet = true;
                    SetTrigger(supported_triggers.End);
                }
            }
        }

        private void SetTrigger(supported_triggers trigger_to_set)
        {
            for (int trigger_to_check = 0; trigger_to_check < (int)supported_triggers.End; trigger_to_check++)
            {
                if((supported_triggers)trigger_to_check != trigger_to_set)
                {
                    Debug.Log(String.Format("Resetting trigger {0}", ((supported_triggers)trigger_to_check).ToString()));
                    StateMachineController.ResetTrigger(((supported_triggers)trigger_to_check).ToString());
                }
                else
                {
                    Debug.Log(String.Format("Setting trigger to {0}", ((supported_triggers)trigger_to_check).ToString()));
                    StateMachineController.SetTrigger(((supported_triggers)trigger_to_check).ToString());
                }
            }
        }

        private void SetStateTo(States to_state)
        {
            canTriggerSet = false;
            StateManager.SetStateTo(to_state);
        }

        internal void DisplayAnimatorInfo()
        {
            idle_idle_hash = Animator.StringToHash("Idle_Idle");
            AnimatorStateInfo animatorStateInfo = StateMachineController.GetCurrentAnimatorStateInfo(0);
            print(String.Format("Animator Hash Pre is {0} Animator Hask Get {1}", idle_idle_hash,animatorStateInfo.fullPathHash));

        }
    }
}