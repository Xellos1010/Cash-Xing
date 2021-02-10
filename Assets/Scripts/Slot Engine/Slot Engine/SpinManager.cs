using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot_Engine.Matrix
{
    public class SpinManager : MonoBehaviour
    {
        public float reel_spin_speed;
        public float fReelSpinDelay = .2f; // Delays the reels when starting to spin like waterfall effect
        public float fStartStopSpeed = 50; // distance to move per second

        public List<Symbols[]> rReelsMultiSpinConfiguration;
        void Update()
        {
#if UNITY_ANDROID || UNITY_IPHONE
        if(Input.touchCount > 0)
         {
             Touch temp = Input.touches[0];
             if (temp.phase == TouchPhase.Ended)
             {
                 if (StateManager.enCurrentState != States.BaseGameSpinLoop && StateManager.enCurrentState != States.BaseGameSpinStart)
                     StartCoroutine(SpinReelsTest());
                 else if (StateManager.enCurrentState == States.BaseGameSpinLoop)
                 {
                     //Put ending sequence coroutine here
                     Debug.Log("Ending Spin State");
                     StartCoroutine(SpinEnd());
                 }
             }
        }
#else
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //if (StateManager.enCurrentState != States.BaseGameSpinLoop && StateManager.enCurrentState != States.BaseGameSpinStart)
                //    StartCoroutine(SpinReelsTest());
                //else if (StateManager.enCurrentState == States.BaseGameSpinLoop)
                //{
                //    //Put ending sequence coroutine here
                //    Debug.Log("Ending Spin State");
                //    StartCoroutine(SpinEnd());
                //}
            }
#endif
        }

        //Engine Functions
        public void Spin()
        {
            //TODO input logic for spin
        }
    }
}
