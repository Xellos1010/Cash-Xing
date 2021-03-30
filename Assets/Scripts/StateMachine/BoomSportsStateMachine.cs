using System;
using System.Collections;
using UnityEngine;

class BoomSportsStateMachine : DTStateMachineBehaviour<StateMachineBehaviour>
{
    public bool set_state_enter;
    public States state_to_invoke_on_enter;
    public bool set_state_exit;
    public States state_to_invoke_on_exit;
    public bool set_trigger_on_enter;
    public string trigger_to_set;

    public bool reset_animator_trigger_bools;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (set_state_enter)
        {
            //Debug.Log(String.Format("Is name Idle_Idle for animator? {0}",animator.GetCurrentAnimatorStateInfo(0).IsName("Idle_Idle")));
            StateManager.SetStateTo(state_to_invoke_on_enter);
        }
        if(set_trigger_on_enter)
        {
            animator.SetTrigger(trigger_to_set);
        }
        if(reset_animator_trigger_bools)
        {
            AnimatorStaticUtilites.ResetAllBools(ref animator);
            AnimatorStaticUtilites.ResetAllTriggers(ref animator);
        }
        Debug.Log(animator.gameObject.name);
    }

    protected override void OnStateEntered()
    {
        base.OnStateEntered();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

    protected override void OnStateExited()
    {
        base.OnStateExited();
        if(set_state_exit)
            StateManager.SetStateTo(state_to_invoke_on_exit);
    }
}