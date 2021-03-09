using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UITextManager : MonoBehaviour
{
    public TextMeshPro bank, multiplier, freespin_info, bank_roll, bet_amount;

    public void SetBankTo(int value)
    {
        SetTextMeshProTextTo(ref bank, String.Format("${0:n}", value));
    }
    public void SetMultiplierTo(int value)
    {
        SetTextMeshProTextTo(ref multiplier, String.Format("{0}x", value));
    }

    public void SetFreeSpinRemainingTo(int value)
    {
        SetTextMeshProTextTo(ref freespin_info, String.Format("{0} Free Spin{1} Remaining",value,value>1?"s":""));
    }

    public void SetBankRollTo(int value)
    {
        SetTextMeshProTextTo(ref bank_roll, String.Format("${0:n}", value));
    }
    public void SetBetAmountTo(int value)
    {
        SetTextMeshProTextTo(ref bank_roll, String.Format("${0:n}", value));
    }

    private void SetTextMeshProTextTo(ref TextMeshPro element, string value)
    {
        element.text = value;
    }
}
