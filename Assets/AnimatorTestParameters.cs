using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BoomSports.Prototype.Managers;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
namespace BoomSports.Prototype
{

#if UNITY_EDITOR
    [CustomEditor(typeof(AnimatorTestParameters))]
    class AnimatorTestParametersEditor : BoomSportsEditor
    {
        private ReorderableList list;
        supportedAnimatorTriggers triggerToMod;
        supportedAnimatorBools boolToMod;
        bool value;

        AnimatorTestParameters myTarget;
        public void OnEnable()
        {
            myTarget = (AnimatorTestParameters)target;
        }
        public override async void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Animator Debug Utility");
            EditorGUILayout.EnumPopup(StaticStateManager.enCurrentState);
            BoomEditorUtilities.DrawUILine(Color.white);
            DrawTriggerUtilityWindow();
            BoomEditorUtilities.DrawUILine(Color.white);
            DrawBoolUtilityWindow();
            BoomEditorUtilities.DrawUILine(Color.white);
            //Draw Set Trigger Selection based on 
            base.OnInspectorGUI();
        }

        private void DrawBoolUtilityWindow()
        {
            EditorGUILayout.LabelField("Animator Bool Debug Utility");
            BoomEditorUtilities.DrawUILine(Color.red);
            GUILayout.BeginHorizontal();
            // Set Trigger to Mod
            boolToMod = (supportedAnimatorBools)EditorGUILayout.EnumPopup("Bool to Set", boolToMod);
            value = EditorGUILayout.Toggle("Bool Value", value);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Set Bool To "))
            {
                myTarget.SetBool(boolToMod,value);
            }
            BoomEditorUtilities.DrawUILine(Color.red);
        }

        private void DrawTriggerUtilityWindow()
        {
            EditorGUILayout.LabelField("Animator Trigger Debug Utility");
            BoomEditorUtilities.DrawUILine(Color.red);
            // Set Trigger to Mod
            triggerToMod = (supportedAnimatorTriggers)EditorGUILayout.EnumPopup("Trigger to Set", triggerToMod);
            if (GUILayout.Button("Set Trigger"))
            {
                myTarget.SetTrigger(triggerToMod);
            }
            if (GUILayout.Button("Reset Trigger"))
            {
                myTarget.ResetTrigger(triggerToMod);
            }
        }
    }
#endif
    [RequireComponent(typeof(Animator))]
    public class AnimatorTestParameters : StateMachineManagerBase
    {
        internal Animator _instance;

        public Animator instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetComponent<Animator>();
                return _instance;
            }
        }

        internal void ResetTrigger(supportedAnimatorTriggers triggerToMod)
        {
            Animator controller = instance;
            ResetTrigger(ref controller,triggerToMod);
        }

        internal void SetBool(supportedAnimatorBools boolToMod, bool value)
        {
            Animator controller = instance;
            SetBool(ref controller, boolToMod,value);
        }

        internal void SetTrigger(supportedAnimatorTriggers triggerToMod)
        {
            Animator controller = instance;
            SetTrigger(ref controller,triggerToMod);
        }
    }
}
