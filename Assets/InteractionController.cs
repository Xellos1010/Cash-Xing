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
    
    void Start()
    {
        //TODO Change to include pre-loader
        StateManager.SwitchState(States.idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (canTriggerSet)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (StateManager.enCurrentState == States.idle)
                {
                    SetStateTo(States.spin_start);
                    StateMachineController.SetTrigger("Spin Start");
                }
                else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_idle)
                {
                    StateMachineController.ResetTrigger("SpinStart");
                    StateMachineController.SetTrigger("SpinSlam");
                }
            }
        }
    }

    private void SetStateTo(States to_state)
    {
        canTriggerSet = false;
        StateManager.SwitchState(to_state);
    }
}
