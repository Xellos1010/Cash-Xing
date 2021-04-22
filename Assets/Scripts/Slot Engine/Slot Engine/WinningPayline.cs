//For Parsing Purposes
using Slot_Engine.Matrix;
using System;
using UnityEngine;
//[Serializable]
//public struct PaylineNode
//{
//    [SerializeField]
//    public suffix_tree_node_info nodeInfo;
//    [SerializeField]
//    public int symbol;
//}
[System.Serializable]
public class WinningPayline
{
    public Payline payline;
    public PaylineNode[] winning_symbols;

    public WinningPayline(Payline payline, PaylineNode[] winning_symbols)
    {
        this.payline = payline;
        this.winning_symbols = winning_symbols;
    }
    /// <summary>
    /// Calculates total win of payline then returns final value
    /// </summary>
    /// <returns></returns>
    internal float GetTotalWin(Matrix matrix)
    {
        float output = 0;
        for (int i = 0; i < winning_symbols.Length; i++)
        {
            output += CalculateTotalWin(matrix.symbols_data_for_matrix.symbols[winning_symbols[i].symbol].winValue,ref matrix);
        }
        return output;
    }

    private float CalculateTotalWin(int win_value, ref Slot_Engine.Matrix.Matrix matrix)
    {
        //Total win = (Bet Amount * win_value) * multiplier
        return (win_value * matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount) * matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier;
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot, int reel_start_padding, PaylineNode symbol_to_check)
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

    private bool IsSymbolWinningSymbol(PaylineNode symbol_to_check)
    {
        bool output = false;
        for (int i = 0; i < winning_symbols.Length; i++)
        {
            if(winning_symbols[i].symbol == symbol_to_check.symbol)
            {
                output = true;
                break;
            }
        }
        return output;
    }

    internal PaylineNode GetWinningWymbol()
    {
        //Default to the first - need to add check if wild and provide override logic
        return winning_symbols[0];
    }
}