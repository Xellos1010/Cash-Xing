//For Parsing Purposes
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
    internal string PrintWinningNodes()
    {
        string output = "";
        for (int node = 0; node < winningNodes.Length; node++)
        {
            output += winningNodes[node].nodeInfo.Print();
        }
        return output;
    }
    internal string PrintWinningNodesAndSymbols()
    {
        return PrintWinningNodesAndSymbols(winningNodes);
    }
    internal string PrintWinningNodesAndSymbols(WinningEvaluatedNodeContainer[] winningNodes)
    {
        string output = "";
        for (int node = 0; node < winningNodes.Length; node++)
        {
            output += $"|{winningNodes[node].nodeInfo.Print()}|{winningNodes[node].symbol}";
        }
        return output;
    }

    internal bool ContainsSymbol(int symbolID)
    {
        for (int i = 0; i < winningNodes.Length; i++)
        {
            if (winningNodes[i].symbol == symbolID)
                return true;
        }
        return false;
    }
}
