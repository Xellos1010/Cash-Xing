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
    internal float GetTotalWin(WeightedDistribution.IntDistribution intWeightedDistributionSymbols, Slot_Engine.Matrix.Matrix matrix)
    {
        float output = 0;
        for (int i = 0; i < winning_symbols.Length; i++)
        {
            output += CalculateTotalWin(intWeightedDistributionSymbols.Items[winning_symbols[i]].win_value,ref matrix);
        }
        return output;
    }

    private float CalculateTotalWin(int win_value, ref Slot_Engine.Matrix.Matrix matrix)
    {
        //Total win = (Bet Amount * win_value) * multiplier
        return (win_value * matrix.machine_information_manager.bet_amount) * matrix.machine_information_manager.multiplier;
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot)
    {
        //Check Winning slot at reel 
        if (payline.payline[reel]==slot)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}