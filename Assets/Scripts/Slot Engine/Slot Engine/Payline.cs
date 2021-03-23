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

    public Payline(int[] vs)
    {
        payline_configuration.payline = vs;
    }

    internal string PrintConfiguration()
    {
        return String.Join("|", payline_configuration.payline);
    }
}

[System.Serializable]
public struct PaylineConfiguration
{
    [UnityEngine.SerializeField]
    public int[] payline;
}