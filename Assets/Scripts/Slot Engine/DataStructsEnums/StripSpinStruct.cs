//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using Slot_Engine.Matrix;
using System;
using System.Collections.Generic;
/// <summary>
/// The structure or the reel strip spin loop strip and the end configuration placed within the reelstrip
/// </summary>
[Serializable]
public struct StripSpinStruct
{
    /// <summary>
    /// end reel configuration symbols for this spin
    /// </summary>
    [UnityEngine.SerializeField]
    public NodeDisplaySymbol[] displaySymbols;
    /// <summary>
    /// The display symbols reel will cycle thru on loop
    /// </summary>
    [UnityEngine.SerializeField]
    public NodeDisplaySymbol[] stripSpinSymbols;
    /// <summary>
    /// Holds the lower and upper range of array where ending symbols were placed in spin symbols
    /// </summary>
    [UnityEngine.SerializeField]
    public int[] endSymbolDisplayRangeOnStrip;

    public StripSpinStruct(NodeDisplaySymbol[] slotDisplaySymbols) : this()
    {
        displaySymbols = slotDisplaySymbols;
    }

    internal List<int> GetAllDisplaySymbols()
    {
        List<int> output = new List<int>();
        for (int i = 0; i < displaySymbols.Length; i++)
        {
            output.Add(displaySymbols[i].primary_symbol);
        }
        return output;
    }
}