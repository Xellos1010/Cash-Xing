//For Parsing Purposes
using System;

[System.Serializable]
public class WinningPayline
{
    public Payline payline;
    public int[] winning_symbols;
    public bool left_right; //true for left to right false for right to left

    public WinningPayline(Payline payline, int[] winning_symbols, bool left_right)
    {
        this.payline = payline;
        this.winning_symbols = winning_symbols;
        this.left_right = left_right;
    }
    /// <summary>
    /// Calculates total win of payline then returns final value
    /// </summary>
    /// <returns></returns>
    internal int GetTotalWin(WeightedDistribution.IntDistribution intWeightedDistributionSymbols)
    {
        int output = 0;
        for (int i = 0; i < winning_symbols.Length; i++)
        {
            output += intWeightedDistributionSymbols.Items[winning_symbols[i]].win_value;
        }
        return output;
    }
}