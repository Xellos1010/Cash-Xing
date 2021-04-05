using System;
using UnityEngine;
public enum supported_triggers
{
    SpinStart,
    SpinSlam,
    SpinResolve,
    End
}
public enum supported_bools
{
    WinRacking,
    BonusActive,
    FeatureTrigger,
    SymbolResolve,
    LoopPaylineWins,
    End
}

public enum supported_floats
{
    MotionTime,
    End
}

[RequireComponent(typeof(Animator))]
public class StateMachineManagerBase : MonoBehaviour
{
    public Animator[] sub_state_machines
    {
        get
        {
            if(_sub_state_machines?.Length < 1)
            {
                _sub_state_machines = transform.GetComponentsInChildren<Animator>().RemoveAt<Animator>(0);
            }
            else if(_sub_state_machines == null)
            {
                _sub_state_machines = transform.GetComponentsInChildren<Animator>().RemoveAt<Animator>(0);
            }
            return _sub_state_machines;
        }
    }
    public Animator[] _sub_state_machines;

    [SerializeField]
    private Animator _state_machine;

    internal Animator state_machine
    {
        get
        {
            if (_state_machine == null)
                _state_machine = GetComponent<Animator>();
            return _state_machine;
        }
    }

    public void ResetAllTriggers()
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.ResetAllTriggers(ref animator);
    }

    internal void ResetTrigger(supported_triggers trigger)
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.ResetTrigger(ref animator, trigger);
    }

    public void SetStateTo(States to_state)
    {
        StateManager.SetStateTo(to_state);
    }

    internal void InitializeAnimator()
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.InitializeAnimator(ref animator);
        InitializeAnimatorSubStates();
    }

    private void InitializeAnimatorSubStates()
    {
        if (_sub_state_machines?.Length > 0)
        {
            for (int state_machine = 0; state_machine < sub_state_machines.Length; state_machine++)
            {
                AnimatorStaticUtilites.InitializeAnimator(ref sub_state_machines[state_machine]);
            }
        }
    }

    internal void ResetAllBools()
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.ResetAllBools(ref animator);
        ResetAllBoolsSubStates();
    }

    private void ResetAllBoolsSubStates()
    {
        if(sub_state_machines?.Length > 0)
        {
            for (int state_machine = 0; state_machine < sub_state_machines.Length; state_machine++)
            {
                AnimatorStaticUtilites.ResetAllBools(ref sub_state_machines[state_machine]);
            }
        }
    }

    internal void SetBool(supported_bools bool_name, bool value)
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.SetBoolTo(ref animator, bool_name, value);
    }
    internal void SetBoolSubStateMachineTo(ref Animator sub_state_machine, supported_bools bool_name, bool value)
    {
        AnimatorStaticUtilites.SetBoolTo(ref sub_state_machine, bool_name, value);
    }
    internal void SetBoolSubStateMachinesTo(supported_bools bool_name, bool value)
    {
        if(sub_state_machines?.Length > 0)
        {
            for (int state_machine = 0; state_machine < sub_state_machines.Length; state_machine++)
            {
                AnimatorStaticUtilites.SetBoolTo(ref sub_state_machines[state_machine], bool_name, value); 
            }
        }
    }

    internal void SetTrigger(supported_triggers trigger_to_set)
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.SetTriggerTo(ref animator, trigger_to_set);
    }
    internal void SetFloatTo(supported_floats float_to_set, float value)
    {
        Animator animator = state_machine;
        AnimatorStaticUtilites.SetFloatTo(ref animator, float_to_set, value);
    }
}