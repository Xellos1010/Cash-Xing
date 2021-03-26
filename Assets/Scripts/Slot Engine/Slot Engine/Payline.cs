//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using System;

[System.Serializable]
public class Payline
{
    [UnityEngine.SerializeField]
    public PaylineConfiguration payline_configuration;
    [UnityEngine.SerializeField]
    public bool left_right;
    public Payline(int[] vs, bool left_right)
    {
        payline_configuration.payline = vs;
        this.left_right = left_right;
    }

    internal string PrintConfiguration()
    {
        return String.Join("|", payline_configuration.payline);
    }

    internal int ReturnLeftRootNodeFromFullLineWin()
    {
        return left_right ? payline_configuration.payline[0]:payline_configuration.payline[payline_configuration.payline.Length - 1];
    }

    internal int ReturnRightRootNodeFromFullLineWin()
    {
        return left_right ? payline_configuration.payline[payline_configuration.payline.Length - 1] : payline_configuration.payline[0];
    }
}

[System.Serializable]
public struct PaylineConfiguration
{
    [UnityEngine.SerializeField]
    public int[] payline;
}