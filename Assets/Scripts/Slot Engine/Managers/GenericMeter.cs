using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

    public class GenericMeter : MonoBehaviour
    {
        //Variables needed
        //Get Meter data based on name of the gameobject
        private float fMeterCounter = 0;
        public float fDefaultRackingTime = 5.0f;
        public bool bIsRacking = false;

        public eMeters MeterType = eMeters.None;

        void OnStart()
        {
            //Find MeterInformationManager and annouce that you are a meter
            ResetVars();
        }

        void ResetVars()
        {
            fMeterCounter = 0;
            fDefaultRackingTime = 5.0f;
            bIsRacking = false;
            GetComponent<UnityEngine.UI.Text>().text = "0";
        }

        //Racking function and Hooks.
        public void SwitchState(States State)
        {
            
        }

        void Update()
        {
#if UNITY_ANDROID || UNITY_IPHONE
       if (Input.touchCount > 1)
        {
            if (fMeterCounter == 0)
                    RackUp(5000);
                else
                    RackDown(5000);
        }
#else

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (fMeterCounter == 0)
                    RackUp(5000);
                else
                    RackDown(5000);
            }
#endif
        }

        public void RackUp(int iAmountToAdd)
        {
            RackUp(iAmountToAdd, fDefaultRackingTime);
        }

        public void RackUp(int iAmountToAdd, float fTime)
        {
            if(!bIsRacking)
                iTween.ValueTo(gameObject, TweenSettings(fMeterCounter, (fMeterCounter + iAmountToAdd), fTime));
        }

        public void RackDown(int iAmountToRemove)
        {
            RackDown(iAmountToRemove, fDefaultRackingTime);
        }

        public void RackDown(int iAmountToRemove, float fTime)
        {
            if (!bIsRacking)
                iTween.ValueTo(gameObject, TweenSettings(fMeterCounter, (fMeterCounter - iAmountToRemove), fTime));
        }

        //Useful for setting a number of credits won to rack down from
        public void RackDown(int iStartingNumber, int iAmountToRemove, float fTime)
        {
            if (!bIsRacking)
            {
                fMeterCounter = iStartingNumber;
                iTween.ValueTo(gameObject, TweenSettings(fMeterCounter, (fMeterCounter - iAmountToRemove), fTime));
            }
        }

        private Hashtable TweenSettings(float fAmountFrom,float fAmountTo, float fTime)
        {
            Hashtable hSettings = new Hashtable();
            hSettings.Add("name", transform.name + "_racking");
            hSettings.Add("time", fTime);
            hSettings.Add("from", fAmountFrom);
            hSettings.Add("to", fAmountTo);
            hSettings.Add("onstarttarget", gameObject);
            hSettings.Add("onstart", "RackingStart");
            hSettings.Add("onupdatetarget", gameObject);
            hSettings.Add("onupdate", "UpdateMeter");
            hSettings.Add("oncompletetarget", gameObject);
            hSettings.Add("oncomplete", "RackingComplete");
            return hSettings;
        }

        public void RackingStart()
        {
            bIsRacking = true;
        }

        public void RackingComplete()
        {
            bIsRacking = false;
        }

        public void UpdateMeter(float fUpdatedValue)
        {
            fMeterCounter = fUpdatedValue;
            GetComponent<UnityEngine.UI.Text>().text = ((int)fMeterCounter).ToString();
        }

        void CheckGUIText()
        {
            if (!GetComponent<UnityEngine.UI.Text>())
                gameObject.AddComponent<UnityEngine.UI.Text>();
        }
    }

