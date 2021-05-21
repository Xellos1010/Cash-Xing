using Slot_Engine.Matrix;
using System;
using UnityEngine;
/// <summary>
/// Defines Display Configuration of Symbols on matrix
/// </summary>
[Serializable]
public struct DisplayConfigurationSymbolsGroup
{
    /// <summary>
    /// Display Symbols in sequence first in first out
    /// </summary>
    [SerializeField]
    public NodeDisplaySymbol[] displaySymbolSequence;

    internal void SetColumnSymbolsTo(NodeDisplaySymbol[] displaySymbolSequence)
    {
        this.displaySymbolSequence = displaySymbolSequence;
    }

    internal string PrintDisplaySymbols()
    {
        //Debug.Log($"displaySymbols.Length = {displaySymbols.Length}");
        string output = "";
        for (int i = 0; i < displaySymbolSequence.Length; i++)
        {
            output += "|" + displaySymbolSequence[i].primarySymbol;
        }
        return output;
    }
}