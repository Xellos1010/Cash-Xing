//For Parsing Purposes
using BoomSports.Prototype;
using System;
using UnityEngine;
[System.Serializable]
public partial class WinningPayline : WinningObject
{
    [SerializeField]
    public Payline payline;

    public WinningPayline(Payline payline, WinningEvaluatedNodeContainer[] winning_symbols)
    {
        this.payline = payline;
        this.winningNodes = winning_symbols;
    }
    /// <summary>
    /// Calculates total win of payline then returns final value
    /// </summary>
    /// <returns></returns>
    internal float GetTotalWin(StripConfigurationObject matrix)
    {
        float output = 0;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            output += CalculateTotalWin(matrix.symbolDataScriptableObject.symbols[winningNodes[i].symbol].winValue,ref matrix);
        }
        return output;
    }

    private float CalculateTotalWin(int win_value, ref StripConfigurationObject matrix)
    {
        return (win_value * matrix.managers.machineInfoManager.machineInfoScriptableObject.bet_amount);
    }

    internal bool IsSymbolOnWinningPayline(int reel, int slot, int reel_start_padding, WinningEvaluatedNodeContainer symbol_to_check)
    {
        //Check Winning slot at reel 
        if (payline.configuration.payline[reel]+reel_start_padding==slot && IsSymbolWinningSymbol(symbol_to_check))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsSymbolWinningSymbol(WinningEvaluatedNodeContainer symbol_to_check)
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

    internal WinningEvaluatedNodeContainer GetWinningWymbol()
    {
        //Default to the first - need to add check if wild and provide override logic
        return winningNodes[0];
    }

    internal bool ContainsAllNodes(WinningEvaluatedNodeContainer[] winningNodes)
    {
        Debug.Log($"Checking if {PrintWinningNodesAndSymbols()} contains {PrintWinningNodesAndSymbols(winningNodes)}");
        bool output = false;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            Debug.Log($"{PrintWinningNodesAndSymbols()} ContainsNode(winningNodes[{i}].nodeInfo{winningNodes[i].nodeInfo.Print()}) = {ContainsNode(winningNodes[i].nodeInfo)}");
            if(!ContainsNode(winningNodes[i].nodeInfo))
            {
                break;
            }
            if(i == winningNodes.Length-1)
            {
                output = true;
            }
        }
        return output;
    }
}