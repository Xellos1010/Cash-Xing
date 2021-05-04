using System;
using UnityEngine;
public enum supportedAnimatorTriggers
{
    SpinSlam,
    SpinResolve,
    ResolveEnd,
    StartSpin,
    TransitionToFromBonus,
    End
}
public enum supportedAnimatorBools
{
    SpinStart,
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

    internal void ResetTrigger(ref Animator animator, supportedAnimatorTriggers trigger)
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

    internal void SetBool(ref Animator animator,supportedAnimatorBools bool_name, bool value)
    {
        AnimatorStaticUtilites.SetBoolTo(ref animator, bool_name, value);
    }
    internal void SetAllBoolStateMachinesTo(ref Animator[] animators, supportedAnimatorBools bool_name, bool value)
    {
        //Debug.Log(String.Format("Setting Bool {0} to {1}",bool_name.ToString(),value));
        for (int i = 0; i < animators.Length; i++)
        {
            AnimatorStaticUtilites.SetBoolTo(ref animators[i], bool_name, value); 
        }
    }

    internal void SetTrigger(ref Animator animator, supportedAnimatorTriggers trigger_to_set)
    {
        AnimatorStaticUtilites.SetTriggerTo(ref animator, trigger_to_set);
    }
    internal void SetFloatTo(ref Animator animator, supported_floats float_to_set, float value)
    {
        AnimatorStaticUtilites.SetFloatTo(ref animator, float_to_set, value);
    }

    internal void SetAllAnimatorTriggers(ref Animator[] animators, supportedAnimatorTriggers to_trigger)
    {
        for (int animator = 0; animator < animators.Length; animator++)
        {
            SetTrigger(ref animators[animator],to_trigger);
        }
    }

    internal void SetAllAnimatorBoolTo(ref Animator[] animators, supportedAnimatorBools toBool, bool value)
    {
        for (int animator = 0; animator < animators.Length; animator++)
        {
            SetBool(ref animators[animator], toBool, value);
        }
    }

    internal void ResetAllTrigger(ref Animator[] animators, supportedAnimatorTriggers trigger)
    {
        for (int animator = 0; animator < animators.Length; animator++)
        {
            ResetTrigger(ref animators[animator],trigger);
        }
    }
}