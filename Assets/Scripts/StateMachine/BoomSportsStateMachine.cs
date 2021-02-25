using System;
using System.Collections;
using UnityEngine;

class BoomSportsStateMachine : DTStateMachineBehaviour<StateMachineBehaviour>
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
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
    }
}