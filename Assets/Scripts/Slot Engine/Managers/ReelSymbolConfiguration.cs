using Slot_Engine.Matrix;
using System;
using UnityEngine;
/// <summary>
/// Defines Reeel Display Symbol to evaluate
/// </summary>
[Serializable]
public struct ReelSymbolConfiguration
{
    [SerializeField]
    public NodeDisplaySymbol[] displaySymbols;

    internal void SetColumnSymbolsTo(NodeDisplaySymbol[] displaySymbols)
    {
        this.displaySymbols = displaySymbols;
    }

    internal string PrintDisplaySymbols()
    {
        //Debug.Log($"displaySymbols.Length = {displaySymbols.Length}");
        string output = "";
        for (int i = 0; i < displaySymbols.Length; i++)
        {
            output += "|" + displaySymbols[i].primarySymbol;
        }
        return output;
    }
}