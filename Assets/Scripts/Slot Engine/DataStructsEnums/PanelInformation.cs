//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : PanelInformation.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PanelInformation 
{
	public float fCredits ;

	public Texture[] tDenominations ;

	public Font fntCreditsFont ;
	public Font fntBonusCreditsFont ;
	public bool bIsRacking ;

    //MeterInformationTracking
    private Dictionary<string, int> _MeterInformation;
    public Dictionary<string, int> MeterInformation
    {
        get
        {
            if (_MeterInformation.Count < (int)eMeters.Last)
            {
                _MeterInformation = new Dictionary<string, int>((int)eMeters.Last);
                for (int i = 0; i < (int)eMeters.Last; i++)
                {
                    _MeterInformation.Add(((eMeters)i).ToString(), 0);
                }
            }
            return _MeterInformation;
        }
    }
    //**********

    /*
     * Unity Default functions
     */


    /*
     * Class Functions
     * */

	public void StartRacking()
    {

	}

	public void StopRacking()
    {

	}

	public void InterruptRacking()
    {

	}

	public void CycleLineWinBanners()
    {

	}
	public void InterruptLineWinBanners()
    {

	}


}