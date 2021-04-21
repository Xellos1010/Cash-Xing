using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
#if UNITY_EDITOR

[CustomEditor(typeof(AnimatorStateMachineManager))]
class AnimatorStateMachineManagerEditor : BoomSportsEditor
{
    AnimatorStateMachineManager myTarget;
    SerializedProperty animator_state_machines;
    public static States enCurrentState;
    public void OnEnable()
    {
        myTarget = (AnimatorStateMachineManager)target;
        StateManager.StateChangedTo += StateManager_StateChangedTo;
        animator_state_machines = serializedObject.FindProperty("animator_state_machines");
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
        if (animator_state_machines?.type == null)
        {
            EditorGUILayout.LabelField("Please set State Machine Scriptable Object Reference");
        }
        else
        {
            EditorGUILayout.PropertyField(animator_state_machines);

            if (GUILayout.Button("Set State machines by sub-animators"))
            {
                myTarget.SetStateMachinesBySubAnimators();
                serializedObject.Update();
            }
        }
        EditorGUILayout.LabelField("Current State of the State Manager");
        EditorGUILayout.EnumPopup(StateManager.enCurrentState);
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Animator State Machine Manager properties");
            base.OnInspectorGUI();
        }
    }
#endif
    public class AnimatorStateMachineManager : StateMachineManagerBase
{
    public AnimatorStateMachines animator_state_machines;

    public void SetRuntimeControllerTo(AnimatorOverrideController to_controller)
    {
        throw new Exception("To Be Implemented - pass reference to aniamtor that needs override applied");
        //state_machine.runtimeAnimatorController = to_controller;
    }

    internal void SetSubStateMachinesTo(ref Animator[] animators)
    {
        animator_state_machines.AddAnimatorsToSubList(transform.gameObject.name.Contains("Slot")?String.Format("{0}_{1}",transform.parent.gameObject.name, transform.gameObject.name):transform.gameObject.name, ref animators);
    }

    internal void SetStateMachinesTriggerTo(supported_triggers trigger)
    {
        if (animator_state_machines.state_machines_to_sync.Length > 0)
        {
            for (int animator = 0; animator < animator_state_machines.state_machines_to_sync.Length; animator++)
            {
                animator_state_machines.state_machines_to_sync[animator].SetTrigger(trigger.ToString());
            }
        }
    }

    internal void ResetTriggerStateMachines(supported_triggers triggerToReset)
    {
        for (int animator = 0; animator < animator_state_machines.state_machines_to_sync.Length; animator++)
        {
            animator_state_machines.state_machines_to_sync[animator].ResetTrigger(triggerToReset.ToString());
        }
        
    }

    internal void SetStateMachinesBySubAnimators()
    {
        Animator[] sub_states = transform.GetComponentsInChildren<Animator>(true).RemoveAt<Animator>(0);
        SetSubStateMachinesTo(ref sub_states);
    }

    internal void SetBoolAllStateMachines(supported_bools bool_name, bool v)
    {
        Animator[] animators = animator_state_machines.state_machines_to_sync;
        base.SetAllBoolStateMachinesTo(ref animators, bool_name,v);
    }

    internal void InitializeAnimator()
    {
        Animator[] animators = animator_state_machines.state_machines_to_sync;
        base.InitializeAnimator(ref animators);
    }

    internal void SetAllTriggersTo(supported_triggers to_trigger)
    {
        Animator[] animators = animator_state_machines.state_machines_to_sync;
        base.SetAllAnimatorTriggers(ref animators, to_trigger);
    }

    internal void ResetAllTrigger(supported_triggers trigger)
    {
        Animator[] animators = animator_state_machines.state_machines_to_sync;
        base.ResetAllTrigger(ref animators, trigger);
    }

    internal void ClearStateMachinesBySubAnimators()
    {
        animator_state_machines.ClearValues();
    }

    internal void SetSubStateMachinesTo(string[] keys, AnimatorSubStateMachine[] values)
    {
        animator_state_machines.ClearValues();
        animator_state_machines.sub_state_machines_keys = keys;
        animator_state_machines.sub_state_machines_values.sub_state_machines = values;
    }

    internal void SetStateMachineSyncAnimators()
    {
        Animator[] states_to_sync = transform.GetComponentsInChildren<Animator>(true);
        SetStateMachinesTo(ref states_to_sync);
    }

    private void SetStateMachinesTo(ref Animator[] states_to_sync)
    {
        animator_state_machines.state_machines_to_sync = states_to_sync;
    }

    internal void SetSubStateMachinesTriggerTo(int subStateIndex,supported_triggers toTrigger)
    {
        if(animator_state_machines.sub_state_machines_values.sub_state_machines[subStateIndex].sub_state_animators.Length > 0)
        {
            for (int animator = 0; animator < animator_state_machines.sub_state_machines_values.sub_state_machines[subStateIndex].sub_state_animators.Length; animator++)
            {
                animator_state_machines.sub_state_machines_values.sub_state_machines[subStateIndex].sub_state_animators[animator].SetTrigger(toTrigger.ToString());
            }
        }
    }
}