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
        Debug.LogWarning("Not Implemented");
    }

    public void RackDown(int iAmountToRemove)
    {
        RackDown(iAmountToRemove, fDefaultRackingTime);
    }

    public void RackDown(int iAmountToRemove, float fTime)
    {
        Debug.LogWarning("Not Implemented");
    }

    //Useful for setting a number of credits won to rack down from
    public void RackDown(int iStartingNumber, int iAmountToRemove, float fTime)
    {
        Debug.LogWarning("Not Implemented");
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