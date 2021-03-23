//For Parsing Purposes
using System;

[System.Serializable]
public class WinningPayline
{
    public Payline payline;
    public int[] winning_symbols;

    public WinningPayline(Payline payline, int[] winning_symbols)
    {
        this.payline = payline;
        this.winning_symbols = winning_symbols;
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
        return (win_value * matrix.slot_machine_managers.machine_info_manager.bet_amount) * matrix.slot_machine_managers.machine_info_manager.multiplier;
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot, int reel_start_padding, int symbol_to_check)
    {
        //Check Winning slot at reel 
        if (payline.payline_configuration.payline[reel]+reel_start_padding==slot && IsSymbolWinningSymbol(symbol_to_check))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsSymbolWinningSymbol(int symbol_to_check)
    {
        bool output = false;
        for (int i = 0; i < winning_symbols.Length; i++)
        {
            if(winning_symbols[i] == symbol_to_check)
            {
                output = true;
                break;
            }
        }
        return output;
    }
}