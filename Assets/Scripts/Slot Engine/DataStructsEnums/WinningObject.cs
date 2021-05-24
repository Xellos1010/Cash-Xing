﻿//For Parsing Purposes
using BoomSports.Prototype;
using System;
using UnityEngine;
/// <summary>
/// Can be a Payline a Way line - A Shape - etc...
/// </summary>
[Serializable]
public partial class WinningObject
{
    [SerializeField]
    public WinningEvaluatedNodeContainer[] winningNodes;

    internal bool ContainsNode(SuffixTreeNodeInfo nodeInfo)
    {
        for (int node = 0; node < winningNodes.Length; node++)
        {
            if (winningNodes[node].nodeInfo.ColumnRow() == nodeInfo.ColumnRow())
                return true;
        }
        return false;
    }
}
