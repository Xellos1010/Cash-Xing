using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class LinePaysManager : MonoBehaviour
    {
        LinePaysManager _instance;

        private float _fCycleDurationOn = 3.0f;
        private float _fCycleDurationOff = 0.25f;

        private Texture[] _tLinePays;
	    public Texture[] tLinePays
        {
            get
            {
                if (_tLinePays.Length < 1)
                {
                    _tLinePays = ReturnLinePays();
                }
                return _tLinePays;
            }
        }

        private bool bCycleEnabled = false;
        /// <summary>
        /// False is Off True is On
        /// </summary>
        private bool bOnOff = false;

        private int[] _iCyclingPays;
        private int[] iCyclingPays
        {
            get
            {
                if (_iCyclingPays == null)
                {
                    Debug.Log("_iCyclingPays has not been set");
                }
                return _iCyclingPays;
            }
            set
            {
                _iCyclingPays = value;
            }
        }
        private int iCurrentPay = 0;

        private Texture[] ReturnLinePays()
        {
                Texture[] ReturnValue;
                ReturnValue = Resources.LoadAll<Texture>("Default Graphics/MeterPanel/LinePays");
                return ReturnValue;
        }

        void ResetAllVars()
        {
            _fCycleDurationOn = 3.0f;
            _fCycleDurationOff = 0.25f;
            bCycleEnabled = false;
            iCyclingPays = new int[0];
            iCurrentPay = 0;
        }

        public void CycleThroughLinePays(int[] iPays)
        {
            bCycleEnabled = true;
            iCyclingPays = iPays;
            StartCoroutine(CycleThroughLinePays());
        }

        public void CycleThroughLinePays(int[] iPays, float fCycleDurationOn, float fCycleDurationOff)
        {
            bCycleEnabled = true;
            bOnOff = false;
            iCyclingPays = iPays;
            _fCycleDurationOff = fCycleDurationOff;
            _fCycleDurationOn = fCycleDurationOn;
            StartCoroutine(CycleThroughLinePays());
        }

        public void InterruptLinePaysCycle()
        {
            EndCycleThroughPays();
        }

        private void EndCycleThroughPays()
        {
            StopCoroutine("CycleThroughLinePays");
            ResetAllVars();
        }

        //Will blink through the LinePays
        IEnumerator CycleThroughLinePays()
        {
            CheckGuiTexture();
            while(bCycleEnabled)
            {
                if (!bOnOff)
                {
                    //Needed to turn on and then wait
                    bOnOff = true;
                    GetComponent<UnityEngine.UI.RawImage>().enabled = true;
                    yield return _fCycleDurationOn;
                }
                else
                {
                    //Needed to turn off and then wait
                    bOnOff = false;
                    GetComponent<UnityEngine.UI.RawImage>().enabled = false;
                    IncrementPayLineShown();
                    yield return _fCycleDurationOff;
                }
            }
            yield return 0;
        }

        void CheckGuiTexture()
        {
            if (!GetComponent<UnityEngine.UI.RawImage>())
                gameObject.AddComponent<UnityEngine.UI.RawImage>();
        }

        void IncrementPayLineShown()
        {
            iCurrentPay++;
            if (iCurrentPay >= iCyclingPays.Length)
            {
                iCurrentPay = 0;
            }
            SyncTexture();
        }

        void SyncTexture()
        {
            if (iCurrentPay <= tLinePays.Length)
                GetComponent<UnityEngine.UI.RawImage>().texture = tLinePays[iCyclingPays[iCurrentPay]];
        }

        //State Manager Hook
        void SwitchState(States State)
        {
            if (State == States.BaseGameWinPresentation || State == States.BonusGameWinPresentation)
            {
                //Get LinePay Wins from Slot Engine
                CycleThroughLinePays();
                //Cycle through those wins
            }
        }

        //Unity Default Function
        void OnEnable()
        {
            _instance = this;
            StateManager.StateSwitched += SwitchState;
        }

        void OnDisable()
        {
            _instance = null;
            StateManager.StateSwitched -= SwitchState;
        }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IPHONE
       if (Input.touchCount > 1)
        {
            Touch temp = Input.touches[0];
            if (temp.phase == TouchPhase.Ended)
            {
                Debug.Log("Testing LinePaysGeneration");
                _instance = this;
                CycleThroughLinePays();
            }
        }
#else
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Testing LinePaysGeneration");
            _instance = this;
            CycleThroughLinePays();
        }
#endif
    }
    //************************************
    }

