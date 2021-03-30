﻿using System;
using System.Collections.Generic;
//For Parsing Purposes
using UnityEngine;
//************
#if UNITY_EDITOR
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace Slot_Engine.Matrix
{
    [Serializable]
    public struct suffix_tree_node
    {
        [SerializeField]
        internal bool left_right;
        [SerializeField]
        internal suffix_tree_node_info node_info;

        [SerializeField]
        internal suffix_tree_node_info[] parent_nodes;

        [SerializeField]
        internal int[] connected_nodes;

        [SerializeField]
        internal suffix_tree_node[] connected_nodes_struct;

        public suffix_tree_node(int primary_node, suffix_tree_node_info[] parent_nodes, suffix_tree_node_info parent_node, int column) : this()
        {
            this.node_info.row = primary_node;

            if (this.parent_nodes == null && parent_nodes == null)
            {
                this.parent_nodes = new suffix_tree_node_info[0];
            }
            else
            {
                this.parent_nodes = parent_nodes;
            }
            this.parent_nodes = this.parent_nodes.AddAt<suffix_tree_node_info>(0, parent_node);
            this.node_info.column = column;
        }

        public suffix_tree_node(int primary_node, suffix_tree_node_info[] parent_nodes, suffix_tree_node_info parent_node, int column, bool left_right) : this()
        {
            this.node_info.row = primary_node;

            if (this.parent_nodes == null && parent_nodes == null)
            {
                this.parent_nodes = new suffix_tree_node_info[0];
            }
            else
            {
                this.parent_nodes = parent_nodes;
            }
            this.parent_nodes = this.parent_nodes.AddAt<suffix_tree_node_info>(0, parent_node);
            this.node_info.column = column;
            this.left_right = left_right;
        }

        /// <summary>
        /// Initialize the winning symbol list and check dynamic paylines for wins
        /// </summary>
        /// <param name="symbols_configuration">symbols on matrix</param>
        internal WinningPayline[] InitializeAndCheckForWinningPaylines(ref int[][] symbols_configuration, ref Dictionary<Symbol, FeaturesStructSymbolEvaluation> special_symbols)
        {
            //Debug.Log("Initialize check for winning paylines");
            //Initialize Winning Symbol List
            List<SymbolWinStruct> winning_symbols = new List<SymbolWinStruct>();
            //Debug.Log(String.Format("Checking for winning node {0}", node_info.Print()));
            int primary_linewin_symbol = symbols_configuration[node_info.column][node_info.row];
            //Check for features to activate with the first symbol and add the first symbol to the line win
            CheckFeatureConditions(primary_linewin_symbol, ref special_symbols, ref node_info);
            AddWinningSymbol(primary_linewin_symbol, ref winning_symbols, ref node_info);

            //Initialize Winning Paylines
            List<WinningPayline> winning_paylines = new List<WinningPayline>();

            //Debug.Log(String.Format("Starting check for winning paylines from node {0}", node_info.Print()));
            //Check all connected nodes for a win using dfs (depth first search) search
            CheckConnectedNodes(ref node_info, ref connected_nodes_struct, ref symbols_configuration, ref winning_symbols, ref winning_paylines, primary_linewin_symbol, ref special_symbols);

            return winning_paylines.ToArray();
        }
        /// <summary>
        /// Checks a symbol for any win conditions within special symbols
        /// </summary>
        /// <param name="primary_linewin_symbol">the symbol that won</param>
        /// <param name="special_symbols">The special symbols conditions</param>
        private void CheckFeatureConditions(int primary_linewin_symbol, ref Dictionary<Symbol, FeaturesStructSymbolEvaluation> special_symbols, ref suffix_tree_node_info node_info)
        {
            if (special_symbols.ContainsKey((Symbol)primary_linewin_symbol))
            {
                FeaturesStructSymbolEvaluation featuresStructSymbolEvaluation = special_symbols[(Symbol)primary_linewin_symbol];
                switch (special_symbols[(Symbol)primary_linewin_symbol].feature)
                {
                    case Features.None:
                        break;
                    case Features.freespin:
                        //Activate Freespin Mode if there are 3
                        featuresStructSymbolEvaluation.AddNodeIfNotExist(ref node_info);
                        special_symbols[(Symbol)primary_linewin_symbol] = featuresStructSymbolEvaluation;
                        break;
                    case Features.Count:
                        break;
                    default:
                        break;
                }
            }
        }

        private void CheckConnectedNodes(ref suffix_tree_node_info current_node, ref suffix_tree_node[] connected_nodes_struct, ref int[][] symbols_configuration, ref List<SymbolWinStruct> winning_symbols, ref List<WinningPayline> winning_paylines, int symbol_to_check_for, ref Dictionary<Symbol, FeaturesStructSymbolEvaluation> special_symbols)
        {
            //if primary_linewin_symbol is a wild then use the next symbol in sequence - if next symbol is a wild then continue
            //Cycle thru each connected node for a winning payline
            for (int connected_node = 0; connected_node < connected_nodes_struct.Length; connected_node++)
            {
                //Debug.Log(String.Format("Checking Connected node {0} from {1}", connected_nodes_struct[connected_node].node_info.Print(),current_node.Print()));
                //reference list
                CheckForDynamicWinningPaylinesOnNode(ref connected_nodes_struct[connected_node], ref symbols_configuration, ref winning_symbols, symbol_to_check_for, ref winning_paylines, ref special_symbols);
                //if connected nodes are the same leading up to the end winning symbol use the largest list length
            }
        }

        /// <summary>
        /// Used for recursive check of suffix tree to evaluate winning paylines
        /// </summary>
        /// <param name="suffix_tree_node">Node being checked</param>
        /// <param name="symbols_configuration">symbols configuration to check against</param>
        /// <param name="winning_symbols">winning symbols list</param>
        private void CheckForDynamicWinningPaylinesOnNode(ref suffix_tree_node suffix_tree_node, ref int[][] symbols_configuration, ref List<SymbolWinStruct> winning_symbols, int symbol_to_check_for, ref List<WinningPayline> winning_paylines, ref Dictionary<Symbol, FeaturesStructSymbolEvaluation> special_symbols)
        {
            //Debug.Log(String.Format("Checking node {0}", suffix_tree_node.node_info.Print()));
            //Get current node symbol
            int current_symbol_to_check = symbols_configuration[suffix_tree_node.node_info.column][suffix_tree_node.node_info.row];
            //get the feature condition if any for the node
            FeaturesStructSymbolEvaluation feature_condition_current = new FeaturesStructSymbolEvaluation(Features.None);
            if (special_symbols.ContainsKey((Symbol)current_symbol_to_check))
            {
                CheckFeatureConditions(current_symbol_to_check, ref special_symbols, ref suffix_tree_node.node_info);
                feature_condition_current = special_symbols[(Symbol)current_symbol_to_check];
            }
            //Get the previous node feature condition if any
            FeaturesStructSymbolEvaluation feature_condition_previous = new FeaturesStructSymbolEvaluation(Features.None);
            if (special_symbols.ContainsKey((Symbol)symbol_to_check_for))
            {
                feature_condition_previous = special_symbols[(Symbol)symbol_to_check_for];
            }
            //Check if that symbol is a wild
            if (current_symbol_to_check == symbol_to_check_for ||
                feature_condition_current.feature == Features.wild ||
                feature_condition_previous.feature == Features.wild)
            {
                //If the previous symbol was a wild then use the current symbol as primary symbol to evaluate
                if (feature_condition_previous.feature == Features.wild)
                {
                    symbol_to_check_for = current_symbol_to_check;
                }

                AddWinningSymbol(current_symbol_to_check, ref winning_symbols, ref suffix_tree_node.node_info);

                //Current payline index
                int column_index = winning_symbols.Count - 1;
                //There is a match - move to the next node if the winning symbols don't equal total columns
                if (winning_symbols.Count < symbols_configuration.Length)
                {
                    //Check each connected node
                    CheckConnectedNodes(ref suffix_tree_node.node_info, ref suffix_tree_node.connected_nodes_struct, ref symbols_configuration, ref winning_symbols, ref winning_paylines, symbol_to_check_for, ref special_symbols);
                }
                else
                {
                    //Reached the end of the payline - add this payline and override others - remove symbol and start down next tree
                    InitializeAndAddDynamicWinningPayline(suffix_tree_node, ref winning_symbols, ref winning_paylines);
                }
                ////Debug.Log(winning_symbols.PrintElements<int>());
                RemoveWinningSymbol(ref winning_symbols, column_index);
            }
            else
            {
                if (winning_symbols.Count >= 3)
                {
                    InitializeAndAddDynamicWinningPayline(suffix_tree_node, ref winning_symbols, ref winning_paylines);
                }
            }
        }

        private void InitializeAndAddDynamicWinningPayline(suffix_tree_node suffix_tree_node, ref List<SymbolWinStruct> winning_symbols, ref List<WinningPayline> winning_paylines)
        {
            //Debug.Log(String.Format("Payline {0} won!", PrintDynamicPayline(ref winning_symbols)));
            int[] payline = new int[winning_symbols.Count];
            List<int> winning_symbol_row = new List<int>();
            for (int symbol = 0; symbol < winning_symbols.Count; symbol++)
            {
                payline[symbol] = winning_symbols[symbol].suffix_tree_node_info.row;
                winning_symbol_row.Add(winning_symbols[symbol].symbol);
            }
            AddDynamicWinningPayline(payline, winning_symbol_row, suffix_tree_node.left_right, ref winning_paylines);
        }

        internal void AddDynamicWinningPayline(int[] payline, List<int> matching_symbols_list, bool left_right, ref List<WinningPayline> winning_paylines)
        {
            List<string> symbol_names = new List<string>();
            for (int i = 0; i < matching_symbols_list.Count; i++)
            {
                symbol_names.Add(((Symbol)matching_symbols_list[i]).ToString());
            }
            //Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match {2}", payline, left_right ? "left" : "right", String.Join(" ", symbol_names)));
            Payline payline_won = new Payline(payline, left_right);
            WinningPayline new_winning_payline = new WinningPayline(payline_won, matching_symbols_list.ToArray());

            //If we have a payline that is similiar enough to our current payline to submit then we need to keep highest value payline
            WinningPayline duplicate_payline;
            //Check if Payline symbol configuration are already the list - keep highest winning payline
            if (IsWinningPaylineInList(new_winning_payline, ref winning_paylines, out duplicate_payline))
            {
                if (duplicate_payline != new_winning_payline)
                {
                    //Debug.Log(String.Format("New winning payline {0} is higher value than a payline already in the list {1}",string.Join("|", new_winning_payline.payline.payline_configuration.payline), string.Join("|", duplicate_payline.payline.payline_configuration.payline)));
                    winning_paylines.Remove(duplicate_payline);
                    winning_paylines.Add(new_winning_payline);
                }
                else
                {
                    //Debug.Log(String.Format("New winning payline {0} is lower value or already in the list. Not adding to list", string.Join("|", new_winning_payline.payline.payline_configuration.payline)));
                }
            }
            else
            {
                //Debug.Log(String.Format("adding winning payline {0}", string.Join("|", new_winning_payline.payline.payline_configuration.payline)));
                winning_paylines.Add(new_winning_payline);
            }
        }


        /// <summary>
        /// This ensures there are no winning paylines that share the same payline already. Keep highest value winning_payline 
        /// </summary>
        /// <param name="new_winning_payline"></param>
        /// <param name="winning_paylines"></param>
        private bool IsWinningPaylineInList(WinningPayline new_winning_payline, ref List<WinningPayline> winning_paylines, out WinningPayline duplicate_payline_reference)
        {
            //Initialize vars for payline checking
            int[] new_winning_payline_configuration = new_winning_payline.payline.payline_configuration.payline;
            int[] shortest_payline_configuration;
            int[] list_entry_winning_payline_configuration;

            //Iterate thru each winning payline to compare to new payline
            for (int winning_payline = 0; winning_payline < winning_paylines.Count; winning_payline++)
            {
                list_entry_winning_payline_configuration = winning_paylines[winning_payline].payline.payline_configuration.payline;
                //if the paylines are the same up to the third symbol - and the new winning payline is a 4 symbol payline - keep the 4 symbol

                //Compare both paylines until the shortest length. then keep the highest winning payling
                shortest_payline_configuration = CompareReturnShortestPayline(new_winning_payline.payline, winning_paylines[winning_payline].payline);
                for (int column = 0; column < shortest_payline_configuration.Length; column++)
                {
                    //Compare Both Paylines for duplicate payline entry
                    if (new_winning_payline_configuration[column] == list_entry_winning_payline_configuration[column])
                    {
                        //if column
                        if (column == shortest_payline_configuration.Length - 1)
                        {
                            //Check for largest payline configuration and keep highest
                            if (new_winning_payline_configuration.Length > list_entry_winning_payline_configuration.Length)
                            {
                                //Debug.Log("Duplicate reference = winning_paylines[winning_payline]");
                                duplicate_payline_reference = winning_paylines[winning_payline];
                                //We have a similar payline - keep the one with highest value
                                return true;
                            }
                            else
                            {
                                //Debug.Log("Duplicate reference = new_winning_payline");
                                duplicate_payline_reference = new_winning_payline;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //Debug.Log("No Duplicate found");
            duplicate_payline_reference = null;
            return false;
        }
        /// <summary>
        /// Compares 2 paylines and returns shortest one
        /// </summary>
        /// <param name="payline1"></param>
        /// <param name="payline2"></param>
        /// <returns></returns>
        private int[] CompareReturnShortestPayline(Payline payline1, Payline payline2)
        {
            return payline1.payline_configuration.payline.Length > payline2.payline_configuration.payline.Length ? payline2.payline_configuration.payline : payline1.payline_configuration.payline;
        }

        private string PrintDynamicPayline(ref List<SymbolWinStruct> winning_symbols)
        {
            int[] payline = new int[winning_symbols.Count];
            int[] winning_symbol_row = new int[winning_symbols.Count];
            for (int symbol = 0; symbol < winning_symbols.Count; symbol++)
            {
                payline[symbol] = winning_symbols[symbol].suffix_tree_node_info.row;
                winning_symbol_row[symbol] = winning_symbols[symbol].symbol;
            }
            return String.Format(
                "Payline = {0} Symbol Win Configuration = {1}",
                String.Join("|", payline),
                String.Join("|", winning_symbol_row)
                );
        }

        private void RemoveWinningSymbol(ref List<SymbolWinStruct> winning_symbols, int index)
        {
            //Debug.Log(String.Format("Removing winning symbol {0}", winning_symbols[index]));
            winning_symbols.RemoveAt(index);
        }

        /// <summary>
        /// Adds a winning symbol to track for dynamic payline evaluation
        /// </summary>
        /// <param name="symbol">symbol to add</param>
        /// <param name="winning_symbols">winning symbols reference list</param>
        private void AddWinningSymbol(int symbol, ref List<SymbolWinStruct> winning_symbols, ref suffix_tree_node_info suffix_tree_node_info)
        {
            ////Debug.Log(String.Format("Adding winning symbol {0} from node {1}", symbol, suffix_tree_node_info.Print()));
            winning_symbols.Add(new SymbolWinStruct(suffix_tree_node_info, symbol));
        }

        internal void InitializeNextNodes(int current_column, int rows_in_column, ref suffix_tree_node parent_node, bool left_right)
        {
            //Start in column 1

            List<suffix_tree_node> children_nodes = new List<suffix_tree_node>();
            List<int> child_nodes = new List<int>();
            //Check if within range of primary node
            if (parent_node.node_info.row == -1)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (parent_node.node_info.row - 1 >= 0)
                {
                    child_nodes.Add(parent_node.node_info.row - 1);
                    children_nodes.Add(new suffix_tree_node(parent_node.node_info.row - 1, parent_node.parent_nodes, parent_node.node_info, current_column, left_right));
                }
                if (parent_node.node_info.row < rows_in_column)
                {
                    child_nodes.Add(parent_node.node_info.row);
                    children_nodes.Add(new suffix_tree_node(parent_node.node_info.row, parent_node.parent_nodes, parent_node.node_info, current_column, left_right));
                }
                if (parent_node.node_info.row + 1 < rows_in_column)
                {
                    child_nodes.Add(parent_node.node_info.row + 1);
                    children_nodes.Add(new suffix_tree_node(parent_node.node_info.row + 1, parent_node.parent_nodes, parent_node.node_info, current_column, left_right));
                }
            }
            connected_nodes = child_nodes.ToArray();
            connected_nodes_struct = children_nodes.ToArray();
        }

        internal string PrintPayline()
        {
            //This is called when we have no more columns to enable - join all primary node from parents into | seperated string
            List<int> payline = GetPrimaryNodeOfNodeAndParents(ref this);
            return String.Join("|", payline);
        }

        private List<int> GetPrimaryNodeOfNodeAndParents(ref suffix_tree_node node)
        {
            List<int> output = new List<int>();
            output.Add(node.node_info.row);
            for (int parent_node = 0; parent_node < parent_nodes.Length; parent_node++)
            {
                output.Add(parent_nodes[parent_node].row);
            }
            return output;
        }
    }

    [Serializable]
    public struct suffix_tree_node_info
    {
        [SerializeField]
        internal int column;
        [SerializeField]
        internal int row;

        public suffix_tree_node_info(int column, int row) : this()
        {
            this.column = column;
            this.row = row;
        }

        internal string Print()
        {
            return String.Format("Node: Column {0} Row {1}", column, row);
        }
    }

    [Serializable]
    public struct suffix_tree_root_nodes
    {
        [SerializeField]
        internal suffix_tree_node[] root_nodes;
        [SerializeField]
        public Payline[] paylines_supported;

        internal Payline ReturnPayline(int payline_to_show)
        {
            return paylines_supported[payline_to_show];
        }

        internal void AddPaylineSupported(int[] vs, bool left_right)
        {
            if (paylines_supported == null)
                paylines_supported = new Payline[0];
            paylines_supported = paylines_supported.AddAt<Payline>(paylines_supported.Length, new Payline(vs, left_right));
        }
    }
    [Serializable]
    public struct SymbolWinStruct
    {
        [SerializeField]
        internal suffix_tree_node_info suffix_tree_node_info;
        [SerializeField]
        internal int symbol;

        public SymbolWinStruct(suffix_tree_node_info suffix_tree_node_info, int symbol) : this()
        {
            this.suffix_tree_node_info = suffix_tree_node_info;
            this.symbol = symbol;
        }
    }
}