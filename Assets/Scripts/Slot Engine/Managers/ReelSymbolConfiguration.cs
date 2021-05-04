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
}