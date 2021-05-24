using BoomSports.Prototype;
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
    public NodeDisplaySymbolContainer[] displaySymbolSequence;

    internal void SetColumnSymbolsTo(NodeDisplaySymbolContainer[] displaySymbolSequence)
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