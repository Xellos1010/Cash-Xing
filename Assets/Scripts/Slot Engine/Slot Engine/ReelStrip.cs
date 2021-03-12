//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using System;

[System.Serializable]
public class ReelStrip
{
    [UnityEngine.SerializeField]
    public ReelStripStruct reelStrip;
    public ReelStrip(int[] display_symbols)
    {
        reelStrip.display_symbols = display_symbols;
    }

    internal void GenerateReelStrip(int slots_per_strip_onSpinLoop, WeightedDistribution.IntDistribution intDistribution)
    {
        //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
        reelStrip.reel_spin_symbols = GenerateReelStripStatic(slots_per_strip_onSpinLoop, intDistribution);
    }

    internal static int[] GenerateReelStripStatic(int slots_per_strip_onSpinLoop, WeightedDistribution.IntDistribution intDistribution)
    {
        //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
        int[] reel_spin_symbols = new int[slots_per_strip_onSpinLoop];
        for (int i = 0; i < slots_per_strip_onSpinLoop; i++)
        {
            reel_spin_symbols[i] = intDistribution.Draw();
        }
        return reel_spin_symbols;
    }

    internal string ReturnDisplaySymbolsPrint()
    {
        return String.Join(" ", reelStrip.display_symbols);
    }
}
[Serializable]
public struct ReelStripsStruct
{
    [UnityEngine.SerializeField]
    public ReelStripStruct[] reelstrips;
}
[Serializable]
public struct ReelStripStruct
{
    /// <summary>
    /// end reel configuration symbols for this spin
    /// </summary>
    [UnityEngine.SerializeField]
    public int[] display_symbols;
    /// <summary>
    /// The display symbols reel will cycle thru on loop
    /// </summary>
    [UnityEngine.SerializeField]
    public int[] reel_spin_symbols;
    /// <summary>
    /// Holds the lower and upper range of array where ending symbols were placed in spin symbols
    /// </summary>
    [UnityEngine.SerializeField]
    public int[] display_symbol_range;
}