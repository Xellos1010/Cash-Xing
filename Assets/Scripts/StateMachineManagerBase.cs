using System;
using UnityEngine;
public enum supported_triggers
{
    SpinStart,
    SpinSlam,
    SpinResolve,
    ResolveEnd,
    FeatureTransition,
    End
}
public enum supported_bools
{
    WinRacking,
    BonusActive,
    FeatureTrigger,
    SymbolResolve,
    LoopPaylineWins,
    Compact,
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

    internal void ResetTrigger(ref Animator animator, supported_triggers trigger)
    {
        AnimatorStaticUtilites.ResetTrigger(ref animator, trigger);
    }

    /// <summary>
    /// Initialize Animators
    /// </summary>
    /// <param name="animators"></param>
    internal void InitializeAnimator(ref Animator[] animators)
    {
        for (int i = 0; i < animators.Length; i++)
        {
            AnimatorStaticUtilites.InitializeAnimator(ref animators[i]);
        }
    }
    /// <summary>
    /// Resets all bools supported in all animators passed
    /// </summary>
    /// <param name="animators"></param>
    internal void ResetAllBools(ref Animator[] animators)
    {
        for (int i = 0; i < animators.Length; i++)
        {
            AnimatorStaticUtilites.ResetAllBools(ref animators[i]);
        }
    }

    internal void SetBool(ref Animator animator,supported_bools bool_name, bool value)
    {
        AnimatorStaticUtilites.SetBoolTo(ref animator, bool_name, value);
    }
    internal void SetAllBoolStateMachinesTo(ref Animator[] animators, supported_bools bool_name, bool value)
    {
        //Debug.Log(String.Format("Setting Bool {0} to {1}",bool_name.ToString(),value));
        for (int i = 0; i < animators.Length; i++)
        {
            AnimatorStaticUtilites.SetBoolTo(ref animators[i], bool_name, value); 
        }
    }

    internal void SetTrigger(ref Animator animator, supported_triggers trigger_to_set)
    {
        AnimatorStaticUtilites.SetTriggerTo(ref animator, trigger_to_set);
    }
    internal void SetFloatTo(ref Animator animator, supported_floats float_to_set, float value)
    {
        AnimatorStaticUtilites.SetFloatTo(ref animator, float_to_set, value);
    }

    internal void SetAllAnimatorTriggers(ref Animator[] animators, supported_triggers to_trigger)
    {
        for (int animator = 0; animator < animators.Length; animator++)
        {
            SetTrigger(ref animators[animator],to_trigger);
        }
    }

    internal void ResetAllTrigger(ref Animator[] animators, supported_triggers trigger)
    {
        for (int animator = 0; animator < animators.Length; animator++)
        {
            ResetTrigger(ref animators[animator],trigger);
        }
    }
}