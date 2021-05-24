//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using BoomSports.Prototype;
using System;
using System.Collections.Generic;
/// <summary>
/// The structure or the reel strip spin loop strip and the end configuration placed within the reelstrip
/// </summary>
[Serializable]
public struct GroupSpinInformationStruct
{
    /// <summary>
    /// Display Symbol Sequence for this spin - First in First Out
    /// </summary>
    [UnityEngine.SerializeField]
    public NodeDisplaySymbolContainer[] displaySymbolSequence;
    /// <summary>
    /// The spin symbols that will sequence on Spin Idle
    /// </summary>
    [UnityEngine.SerializeField]
    public NodeDisplaySymbolContainer[] spinIdleSymbolSequence;
    /// <summary>
    /// Holds the lower and upper range for spinIdleSymbolSequence where the ending display symbols were placed.
    /// </summary>
    [UnityEngine.SerializeField]
    public int[] endSymbolDisplayRangeOnSpinIdleSequence;

    public GroupSpinInformationStruct(NodeDisplaySymbolContainer[] slotDisplaySymbols) : this()
    {
        displaySymbolSequence = slotDisplaySymbols;
    }
    /// <summary>
    /// Used to Get all display symbols index in SymbolData Scriptable Object
    /// </summary>
    /// <returns>List of symbol int index</returns>
    internal List<int> GetAllDisplaySymbolsIndex()
    {
        List<int> output = new List<int>();
        for (int i = 0; i < displaySymbolSequence.Length; i++)
        {
            output.Add(displaySymbolSequence[i].primarySymbol);
        }
        return output;
    }

    internal string PrintDisplaySymbols()
    {
        string output = "";
        for (int i = 0; i < displaySymbolSequence.Length; i++)
        {
            output += "|" + displaySymbolSequence[i].primarySymbol.ToString();
        }
        return output;
    }
}