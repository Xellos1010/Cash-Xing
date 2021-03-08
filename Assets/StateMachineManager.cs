using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineManager : MonoBehaviour
{
    public void SetStateToIdle_Idle()
    {
        SetStateTo(States.idle_idle);
    }
    public void SetStateTo(States to_state)
    {
        StateManager.SetStateTo(to_state);
    }
}
