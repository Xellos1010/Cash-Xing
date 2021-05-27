//For Parsing Purposes
using BoomSports.Prototype;
using System;
using UnityEngine;

[Serializable]
public struct WinningEvaluatedNodeContainer
{
    [SerializeField]
    public SuffixTreeNodeInfo nodeInfo;
    [SerializeField]
    public int symbol;

    public WinningEvaluatedNodeContainer(SuffixTreeNodeInfo nodeInfo, int symbol) : this()
    {
        this.nodeInfo = nodeInfo;
        this.symbol = symbol;
        //Debug.Log($"Evaluation Node Container created for {symbol} {nodeInfo.Print()}");
        //Debug.Log($"Evaluation Node Container information = {this.symbol} {this.nodeInfo.Print()}");
    }

    internal string Print()
    {
        string output = "";
        output += $"|{symbol.ToString()}|{nodeInfo.Print()}|";
        return output;
    }
}
