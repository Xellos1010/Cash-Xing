using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InteractionController : MonoBehaviour
{
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (StateManager.enCurrentState == States.idle)
                {
                    StateMachineController.SetTrigger("SpinStart");
                }
                else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_idle)
                {
                    StateMachineController.ResetTrigger("SpinStart");
                    StateMachineController.SetTrigger("SpinSlam");
                }
            }
        }
        else
        {
            if(StateManager.enCurrentState == States.idle)
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
}
