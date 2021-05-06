//For Parsing Purposes
using Slot_Engine.Matrix;
using System;
using UnityEngine;
[System.Serializable]
public partial class WinningPayline : WinningObject
{
    [SerializeField]
    public Payline payline;

    public WinningPayline(Payline payline, EvaluationNode[] winning_symbols)
    {
        this.payline = payline;
        this.winningNodes = winning_symbols;
    }
    /// <summary>
    /// Calculates total win of payline then returns final value
    /// </summary>
    /// <returns></returns>
    internal float GetTotalWin(ReelStripConfigurationObject matrix)
    {
        float output = 0;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            output += CalculateTotalWin(matrix.symbolDataScriptableObject.symbols[winningNodes[i].symbol].winValue,ref matrix);
        }
        return output;
    }

    private float CalculateTotalWin(int win_value, ref ReelStripConfigurationObject matrix)
    {
        //if (matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
        //{
        //    return (win_value * matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount) * matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier;
        //}
        //else
        //{
            return (win_value * matrix.managers.machine_info_manager.machineInfoScriptableObject.bet_amount);
        //}
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot, int reel_start_padding, EvaluationNode symbol_to_check)
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

    private bool IsSymbolWinningSymbol(EvaluationNode symbol_to_check)
    {
        bool output = false;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            if(winningNodes[i].symbol == symbol_to_check.symbol)
            {
                output = true;
                break;
            }
        }
        return output;
    }

    internal EvaluationNode GetWinningWymbol()
    {
        //Default to the first - need to add check if wild and provide override logic
        return winningNodes[0];
    }
}