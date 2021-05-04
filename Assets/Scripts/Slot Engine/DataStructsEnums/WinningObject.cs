//For Parsing Purposes
using Slot_Engine.Matrix;
using System;
using UnityEngine;
/// <summary>
/// Can be a Payline a Way line - A Shape - etc...
/// </summary>
[Serializable]
public partial class WinningObject
{
    [SerializeField]
    public EvaluationNode[] winningNodes;

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
