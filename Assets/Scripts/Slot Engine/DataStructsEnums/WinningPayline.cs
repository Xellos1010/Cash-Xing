//For Parsing Purposes
using Slot_Engine.Matrix;
using System;
using UnityEngine;
/// <summary>
/// A representation of a payline win object
/// </summary>
[Serializable]
public partial class WinningPayline : WinningObject
{
    /// <summary>
    /// Payline configuration that you won - will be partial filled for partial paylines
    /// </summary>
    [SerializeField]
    public Payline payline;
    public WinningPayline(Payline payline, WinningNode[] winning_symbols)
    {
        this.payline = payline;
        this.winningNodes = winning_symbols;
    }
    /// <summary>
    /// Calculates total win of payline then returns final value
    /// </summary>
    /// <returns></returns>
    internal float GetTotalWin(Matrix matrix)
    {
        float output = 0;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            output += CalculateTotalWin(matrix.symbols_data_for_matrix.symbols[winningNodes[i].symbol].winValue,ref matrix);
        }
        return output;
    }
//TODO Base this on payline table
    private float CalculateTotalWin(int win_value, ref Slot_Engine.Matrix.Matrix matrix)
    {
        if (matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
        {
            Debug.Log($"Calculating Total win = {(win_value * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.bet_amount) * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.multiplier}");
            return (win_value * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.bet_amount) * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.multiplier;
        }
        else
        {
            Debug.Log($"Calculating Total win = {win_value * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.bet_amount}");
            return (win_value * matrix.slotMachineManagers.machine_info_manager.machineInfoScriptableObject.bet_amount);
        }
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot, int reel_start_padding, WinningNode symbol_to_check)
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

    private bool IsSymbolWinningSymbol(WinningNode symbol_to_check)
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

    internal WinningNode GetWinningWymbol()
    {
        //Default to the first - need to add check if wild and provide override logic
        return winningNodes[0];
    }
}