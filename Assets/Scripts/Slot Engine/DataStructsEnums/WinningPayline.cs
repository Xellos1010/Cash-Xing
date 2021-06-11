//For Parsing Purposes
using BoomSports.Prototype;
using BoomSports.Prototype.Managers;
using System;
using System.Collections.Generic;
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
        int multiplier = 1;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            //Cash Crossing Specific but can be extracted
            if(StripConfigurationObject.instance.isFeatureSymbol(winningNodes[i].symbol,Features.multiplier))
            {
                //Does not account for multiple multipliers
                //Point to symbol container data from matrix and get multiplier value
                List<BaseObjectManager> temp = matrix.groupObjectManagers[winningNodes[i].nodeInfo.column].GetSlotsDecending();
                //Should account for padding slot already - If you refactor without understanding may break.
                Debug.Log($"Found multiplier win on node {winningNodes[i].nodeInfo.Print()} - symbol {winningNodes[i].symbol} - gameobject name = {matrix.groupObjectManagers[winningNodes[i].nodeInfo.column].gameObject.name} {temp[winningNodes[i].nodeInfo.row].gameObject.name}");
                //Add all multipliers in the winning array
                multiplier += temp[winningNodes[i].nodeInfo.row].baseSymbolData.winMultiplier;
            }
            //Take symbol win amount and add together - if symbol is a multiplier then take multiplier amount from row in column symbol data is set to
            output += CalculateTotalWin(matrix.symbolDataScriptableObject.symbols[winningNodes[i].symbol].winValue,ref matrix);
        }
        output *= multiplier;
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
        //Debug.Log($"Checking if {PrintWinningNodesAndSymbols()} contains {PrintWinningNodesAndSymbols(winningNodes)}");
        bool output = false;
        for (int i = 0; i < winningNodes.Length; i++)
        {
            //Debug.Log($"{PrintWinningNodesAndSymbols()} ContainsNode(winningNodes[{i}].nodeInfo{winningNodes[i].nodeInfo.Print()}) = {ContainsNode(winningNodes[i].nodeInfo)}");
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="columnsToCheckForNode"></param>
    /// <returns>int arraw of colunns to get row</returns>
    internal SuffixTreeNodeInfo[] ContainsNodeFromColumns(int[] columnsToCheckForNode)
    {
        List<SuffixTreeNodeInfo> output = new List<SuffixTreeNodeInfo>();
        int index = 0;
        for (int winningNode = 0; winningNode < winningNodes.Length; winningNode++)
        {
            Debug.Log($"Checking if {String.Join("|", columnsToCheckForNode)} contains {winningNodes[winningNode].nodeInfo.column}");
            if(columnsToCheckForNode.Contains<int>(winningNodes[winningNode].nodeInfo.column,out index))
            {
                Debug.Log($"{String.Join("|", columnsToCheckForNode)} Contains {winningNodes[winningNode].nodeInfo.column} adding {winningNodes[winningNode].nodeInfo.Print()} to output");
                output.Add(winningNodes[winningNode].nodeInfo);
            }
        }
        return output.ToArray();
    }
}