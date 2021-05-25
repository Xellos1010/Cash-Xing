//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using BoomSports.Prototype;
using System;
using UnityEngine;

[System.Serializable]
public class Payline
{
    [UnityEngine.SerializeField]
    public PaylineConfiguration configuration;
    [UnityEngine.SerializeField]
    public bool left_right;
    /// <summary>
    /// Root node connected to this payline
    /// </summary>
    public SuffixTreeNodeInfo rootNode;
    public Payline(Payline payline)
    {
        left_right = payline.left_right;
        configuration = payline.configuration;
    }

    public Payline(int[] vs, bool left_right, SuffixTreeNodeInfo rootNode)
    {
        configuration.payline = vs;
        this.left_right = left_right;
        this.rootNode = rootNode;
    }

    internal string PrintConfiguration()
    {
        return String.Join("|", configuration.payline);
    }

    internal int ReturnLeftRootNodeFromLineWin()
    {
        return left_right ? configuration.payline[0]:configuration.payline[configuration.payline.Length - 1];
    }

    internal int ReturnRightRootNodeFromLineWin()
    {
        return left_right ? configuration.payline[configuration.payline.Length - 1] : configuration.payline[0];
    }
}

[System.Serializable]
public struct PaylineConfiguration
{
    [UnityEngine.SerializeField]
    public int[] payline;
}