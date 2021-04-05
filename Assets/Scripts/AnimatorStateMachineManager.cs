using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_EDITOR

[CustomEditor(typeof(AnimatorStateMachineManager))]
class AnimatorStateMachineManagerEditor : BoomSportsEditor
{
    AnimatorStateMachineManager myTarget;
    public static States enCurrentState;
    public void OnEnable()
    {
        myTarget = (AnimatorStateMachineManager)target;
        StateManager.StateChangedTo += StateManager_StateChangedTo;
    }
        
    private void OnDisable()
    {
        StateManager.StateChangedTo -= StateManager_StateChangedTo;
    }
    private void StateManager_StateChangedTo(States State)
    {
        serializedObject.Update();
    }

    public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Animator State Machine Manager Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Animator State Machine Manager Controls");
            base.OnInspectorGUI();
        }
    }
#endif
    public class AnimatorStateMachineManager : StateMachineManagerBase
{
    public void SetRuntimeControllerTo(AnimatorOverrideController to_controller)
    {
        state_machine.runtimeAnimatorController = to_controller;
    }

    internal void SetSubStateMachinesTo(Animator[] animators)
    {
        _sub_state_machines = animators;
    }

    internal void SetTriggerSubStateMachinesTo(supported_triggers trigger)
    {
        if(sub_state_machines.Length > 0)
        {
            for (int animator = 0; animator < sub_state_machines.Length; animator++)
            {
                sub_state_machines[animator].SetTrigger(trigger.ToString());
            }
        }
    }

    internal void ResetTriggerSubStateMachines(supported_triggers triggerToReset)
    {
        if (sub_state_machines == null)
        {

        }
        for (int animator = 0; animator < sub_state_machines.Length; animator++)
        {
            sub_state_machines[animator].ResetTrigger(triggerToReset.ToString());
        }
        
    }
}