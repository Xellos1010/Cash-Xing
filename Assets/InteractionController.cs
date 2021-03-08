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
    
    [RequireComponent(typeof(Animator))]
    public class InteractionController : MonoBehaviour
    {
        public int idle_idle_hash = Animator.StringToHash("Idle_Idle");
        public Animator StateMachineController
        {
            get
            {
                if (_StateMachineController == null)
                    _StateMachineController = GetComponent<Animator>();
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

#endif
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (StateManager.enCurrentState == States.idle_idle)
                    {
                        StateMachineController.SetTrigger("SpinStart");
                    }
                    else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_loop)
                    {
                        StateMachineController.ResetTrigger("SpinStart");
                        Debug.Log("Slam was triggered");
                        StateMachineController.SetTrigger("SpinSlam");
                    }
                }
            }
            else
            {
                if (StateManager.enCurrentState == States.idle_intro)
                {
                    canTriggerSet = true;
                    StateMachineController.ResetTrigger("SpinStart");
                    StateMachineController.ResetTrigger("SpinSlam");
                    StateMachineController.ResetTrigger("SpinResolve");
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