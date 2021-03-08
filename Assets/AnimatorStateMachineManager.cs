using System;
using System.Collections;
using System.Collections.Generic;
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
    SpinResolveEnsure,
    End
}

//if (GUILayout.Button("Display Current Animator Layer Name"))
//{
//    myTarget.DisplayAnimatorInfo();
//}

[RequireComponent(typeof(Animator))]
public class AnimatorStateMachineManager : MonoBehaviour
{
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

    public int idle_idle_hash = Animator.StringToHash("Idle_Idle");
    public void SetStateToIdle_Idle()
    {
        SetStateTo(States.idle_idle);
    }
    public void SetStateTo(States to_state)
    {
        StateManager.SetStateTo(to_state);
    }


    internal void SetTrigger(supported_triggers trigger_to_set)
    {
        for (int trigger_to_check = 0; trigger_to_check < (int)supported_triggers.End; trigger_to_check++) //Don't change spin resolve yet. will need to reset on spin idleidle
        {
            if ((supported_triggers)trigger_to_check != trigger_to_set)
            {
                //If I am setting up my spin resolve logic to apply before spin start then do not reset the spin resolve trigger while slamming
                if (trigger_to_check != (int)supported_triggers.SpinResolve)
                {
                    Debug.Log(String.Format("Resetting trigger {0}", ((supported_triggers)trigger_to_check).ToString()));
                    state_machine.ResetTrigger(((supported_triggers)trigger_to_check).ToString());
                }
            }
            else
            {
                Debug.Log(String.Format("Setting trigger to {0}", ((supported_triggers)trigger_to_check).ToString()));
                state_machine.SetTrigger(((supported_triggers)trigger_to_check).ToString());
            }
        }
    }

    public void ResetAllTriggers()
    {
        for (int trigger_to_check = 0; trigger_to_check < (int)supported_triggers.End; trigger_to_check++) //Don't change spin resolve yet. will need to reset on spin idleidle
        {
                Debug.Log(String.Format("Resetting trigger {0}", ((supported_triggers)trigger_to_check).ToString()));
                state_machine.ResetTrigger(((supported_triggers)trigger_to_check).ToString());
        }
    }
            
    internal void DisplayAnimatorInfo()
    {
        idle_idle_hash = Animator.StringToHash("Idle_Idle");
        AnimatorStateInfo animatorStateInfo = state_machine.GetCurrentAnimatorStateInfo(0);
        print(String.Format("Animator Hash Pre is {0} Animator Hask Get {1}", idle_idle_hash, animatorStateInfo.fullPathHash));
    }

    internal void SetBool(supported_bools bool_name, bool value)
    {
        state_machine.SetBool(bool_name.ToString(),value);
    }

    internal void ResetAllBools()
    {
        for (int bool_to_check = 0; bool_to_check < (int)supported_bools.End; bool_to_check++) //Don't change spin resolve yet. will need to reset on spin idleidle
        {
            Debug.Log(String.Format("Resetting trigger {0}", ((supported_bools)bool_to_check).ToString()));
            state_machine.ResetTrigger(((supported_bools)bool_to_check).ToString());
        }
    }
}
