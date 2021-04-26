using Slot_Engine.Matrix.ScriptableObjects;
using System;
using System.Collections.Generic;
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
    public struct SuffixTreeNodes
    {
        [SerializeField]
        internal bool left_right;
        [SerializeField]
        internal SuffixTreeNodeInfo node_info;

        [SerializeField]
        internal SuffixTreeNodeInfo[] parent_nodes;

        [SerializeField]
        internal int[] connected_nodes;

        [SerializeField]
        internal SuffixTreeNodes[] connected_nodes_struct;

        public SuffixTreeNodes(int primary_node, SuffixTreeNodeInfo[] parent_nodes, SuffixTreeNodeInfo parent_node, int column) : this()
        {
            this.node_info.row = primary_node;

            if (this.parent_nodes == null && parent_nodes == null)
            {
                this.parent_nodes = new SuffixTreeNodeInfo[0];
            }
            else
            {
                this.parent_nodes = parent_nodes;
            }
            this.parent_nodes = this.parent_nodes.AddAt<SuffixTreeNodeInfo>(0, parent_node);
            this.node_info.column = column;
        }

        public SuffixTreeNodes(int column, int row, SuffixTreeNodeInfo[] parent_nodes, SuffixTreeNodeInfo parent_node, bool left_right) : this()
        {
            SuffixTreeNodeInfo node_Info = new SuffixTreeNodeInfo(column, row);
            this.node_info = node_Info;
            if (this.parent_nodes == null && parent_nodes == null)
            {
                this.parent_nodes = new SuffixTreeNodeInfo[0];
            }
            else
            {
                this.parent_nodes = parent_nodes;
            }
            this.parent_nodes = this.parent_nodes.AddAt<SuffixTreeNodeInfo>(0, parent_node);
            this.left_right = left_right;
        }

        /// <summary>
        /// Initialize the winning symbol list and check dynamic paylines for wins
        /// </summary>
        /// <param name="symbols_configuration">symbols on matrix</param>
        internal WinningPayline[] InitializeAndCheckForWinningPaylines(ref EvaluationObjectStruct evaluationObject)
        {
            //Initialize the node with the grid configuration lookup information
            evaluationObject.InitializeWinningSymbolsFeaturesActiveCollections();
            //This could be a wild or overlay - evaluate the feature and add to list
            NodeDisplaySymbol linewin_symbol = evaluationObject.gridConfiguration[node_info.column].displaySymbols[node_info.row];
            //Checks the first symbol for a feature condition
            CheckSlotNeedsFeatureEvaluated(linewin_symbol, ref evaluationObject, ref node_info);
            //Adds the first symbol as a lineWin and makes the primary symbol to track for
            AddWinningSymbol(linewin_symbol.primary_symbol, ref evaluationObject, ref node_info);
            //Initialize Winning Paylines
            List<WinningPayline> winning_paylines = new List<WinningPayline>();
            Debug.Log(String.Format("Starting check for winning paylines from node {0}", node_info.Print()));
            //Check all connected nodes for a win using dfs (depth first search) search
            CheckConnectedNodesForWin(ref node_info, ref connected_nodes_struct, ref evaluationObject, ref winning_paylines, linewin_symbol);
            evaluationObject.winning_symbols.Clear();
            return winning_paylines.ToArray();
        }
        private string PrintIntIntArray(int[][] symbols_configuration)
        {
            string output = "";
            for (int column = 0; column < symbols_configuration.Length; column++)
            {
                output += (String.Format("Column {0} Symbols = {1}", column, String.Join("|", symbols_configuration[column])));
            }
            return output;
        }

        /// <summary>
        /// Checks if a sllot evaluates with a feature condition
        /// </summary>
        /// <param name="evaluationDisplaySymbol">the symbol that won</param>
        /// <param name="evaluationObject">The special symbols conditions</param>
        private void CheckSlotNeedsFeatureEvaluated(NodeDisplaySymbol evaluationDisplaySymbol, ref EvaluationObjectStruct evaluationObject, ref SuffixTreeNodeInfo nodeInfo)
        {
            //Debug.Log(String.Format("linewin_symbol.is_feature = {0}", linewin_symbol.is_feature));
            if (evaluationDisplaySymbol.features != null) //Check if symbol has features
            {
                //Debug.Log($"linewin_symbol.features.Count {evaluationDisplaySymbol.features?.Count} for node {nodeInfo.Print()}");
                if (evaluationDisplaySymbol.features?.Count > 0)
                {
                    SlotEvaluationScriptableObject slotEvaluationActivated = null;
                    bool? evaluationObjectContainsItemWithFeature;
                    for (int feature = 0; feature < evaluationDisplaySymbol.features.Count; feature++)
                    {
                        evaluationObjectContainsItemWithFeature = evaluationObject.ContainsItemWithFeature<SlotEvaluationScriptableObject>(evaluationDisplaySymbol.features[feature], ref slotEvaluationActivated);
                        //TODO house reference for feature activated nodes within scriptable object
                        if (evaluationObjectContainsItemWithFeature != null)
                        {
                            if (evaluationObjectContainsItemWithFeature == true)
                            {
                                if (evaluationObject.featureEvaluationActiveCount == null)
                                {
                                    evaluationObject.featureEvaluationActiveCount = new Dictionary<Features, List<SuffixTreeNodeInfo>>();
                                }
                                //Store that the node has a feature. After Paylines are evaluated we take the nodes in evaluationObject.featureEvaluationActiveCount[feature] and ensure the winning 
                                Debug.Log($"slotEvaluationActivated = {slotEvaluationActivated} Slot Evaluating Feature Condition");
                                if (!evaluationObject.featureEvaluationActiveCount.ContainsKey(evaluationDisplaySymbol.features[feature]))
                                    evaluationObject.featureEvaluationActiveCount[evaluationDisplaySymbol.features[feature]] = new List<SuffixTreeNodeInfo>();
                                if (!evaluationObject.featureEvaluationActiveCount[evaluationDisplaySymbol.features[feature]].Contains(nodeInfo))
                                {
                                    Debug.Log($"node {nodeInfo.Print()} has feature and needs to evaluate if feature activates");
                                    evaluationObject.featureEvaluationActiveCount[evaluationDisplaySymbol.features[feature]].Add(nodeInfo);
                                }
                                else
                                {
                                    Debug.Log($"node is already in list");
                                }
                                Debug.Log($"evaluationObject.featureEvaluationActiveCount.Count = {evaluationObject.featureEvaluationActiveCount.Count} node info = {nodeInfo.Print()}");
                            }
                        }
                    }
                }
            }
        }

        private void CheckConnectedNodesForWin(ref SuffixTreeNodeInfo nodeChecked, ref SuffixTreeNodes[] connectedNodes, ref EvaluationObjectStruct evaluationObject, ref List<WinningPayline> winning_paylines, NodeDisplaySymbol rootWinSymbol)
        {
            //Cycle thru each connected node for a winning payline
            for (int connected_node = 0; connected_node < connectedNodes.Length; connected_node++)
            {
                //Debug.Log(String.Format("Checking Connected node {0} from {1}", connected_nodes_struct[connected_node].node_info.Print(),current_node.Print()));
                CheckForDynamicWinningPaylinesOnNode(ref connectedNodes[connected_node], ref evaluationObject, rootWinSymbol, ref winning_paylines);
            }
        }

        /// <summary>
        /// Used for recursive check of suffix tree to evaluate winning paylines
        /// </summary>
        /// <param name="nodeToCheck">Node being checked</param>
        /// <param name="evaluationObject">symbols configuration to check against</param>
        /// <param name="winning_symbols">winning symbols list</param>
        private void CheckForDynamicWinningPaylinesOnNode(ref SuffixTreeNodes nodeToCheck, ref EvaluationObjectStruct evaluationObject, NodeDisplaySymbol nextDisplaySymbol, ref List<WinningPayline> winning_paylines)
        {
            //Get current node symbol display struct
            NodeDisplaySymbol currentDisplaySymbol = evaluationObject.gridConfiguration[nodeToCheck.node_info.column].displaySymbols[nodeToCheck.node_info.row];

            //Checks the node for a feature condition
            //Wild makes current node any symbol
            CheckSlotNeedsFeatureEvaluated(currentDisplaySymbol, ref evaluationObject, ref nodeToCheck.node_info);

            //First Level Check - Wilds - TODO Refactor to abstract logic - Need to build in wild support
            if (SymbolsMatch(currentDisplaySymbol, nextDisplaySymbol))
            {
                //Add the winning symbol to the payline
                AddWinningSymbol(currentDisplaySymbol.primary_symbol, ref evaluationObject, ref nodeToCheck.node_info);

                //Current payline index - to remove symbol from payline after checking later nodes
                int winningSymbolIndex = evaluationObject.winning_symbols.Count - 1;

                //There is a match - move to the next node if the winning symbols don't equal total columns
                if (evaluationObject.winning_symbols.Count < evaluationObject.gridConfiguration.Length)
                {
                    //Check each connected node
                    CheckConnectedNodesForWin(ref nodeToCheck.node_info, ref nodeToCheck.connected_nodes_struct, ref evaluationObject, ref winning_paylines, nextDisplaySymbol);
                }
                else
                {
                    //Reached the end of the payline - add this payline and override others - remove symbol and start down next tree
                    InitializeAndAddDynamicWinningPayline(nodeToCheck, ref evaluationObject.winning_symbols, ref winning_paylines);
                }
                //Remove the winning Symbol when done evaluating all possible configuration paths after this node so you can evaluate next node in supported configuration sequence
                RemoveWinningSymbol(ref evaluationObject.winning_symbols, winningSymbolIndex);
            }
            else
            {
                if (evaluationObject.winning_symbols.Count >= 3)
                {
                    InitializeAndAddDynamicWinningPayline(nodeToCheck, ref evaluationObject.winning_symbols, ref winning_paylines);
                }
            }
        }

        private bool SymbolsMatch(NodeDisplaySymbol currentDisplaySymbol, NodeDisplaySymbol nextDisplaySymbol)
        {
            //Checks if either symbol is a wild symbol or an overlay symbol applying wild or if symbols match
            if (PrimarySymbolCheck(currentDisplaySymbol, nextDisplaySymbol) || WildSymbolCheck(ref currentDisplaySymbol, ref nextDisplaySymbol))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitializeAndAddDynamicWinningPayline(SuffixTreeNodes suffix_tree_node, ref List<WinningNode> winning_symbols, ref List<WinningPayline> winning_paylines)
        {
            Debug.Log(String.Format("Payline {0} won!", PrintDynamicPayline(ref winning_symbols)));
            int[] payline = new int[winning_symbols.Count];
            List<WinningNode> winning_symbol_row = new List<WinningNode>();
            for (int symbol = 0; symbol < winning_symbols.Count; symbol++)
            {
                payline[symbol] = winning_symbols[symbol].nodeInfo.row;
                winning_symbol_row.Add(winning_symbols[symbol]);
            }
            AddDynamicWinningPayline(payline, winning_symbol_row, suffix_tree_node.left_right, ref winning_paylines);
        }

        internal void AddDynamicWinningPayline(int[] payline, List<WinningNode> matching_symbols_list, bool left_right, ref List<WinningPayline> winning_paylines)
        {
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

        private string PrintDynamicPayline(ref List<WinningNode> winning_symbols)
        {
            int[] payline = new int[winning_symbols.Count];
            int[] winning_symbol_row = new int[winning_symbols.Count];
            for (int symbol = 0; symbol < winning_symbols.Count; symbol++)
            {
                payline[symbol] = winning_symbols[symbol].nodeInfo.row;
                winning_symbol_row[symbol] = winning_symbols[symbol].symbol;
            }
            return String.Format(
                "Payline = {0} Symbol Win Configuration = {1}",
                String.Join("|", payline),
                String.Join("|", winning_symbol_row)
                );
        }

        private void RemoveWinningSymbol(ref List<WinningNode> winning_symbols, int index)
        {
            //Debug.Log(String.Format("Removing winning symbol {0}", winning_symbols[index]));
            winning_symbols.RemoveAt(index);
        }

        /// <summary>
        /// Adds a winning symbol to track for dynamic payline evaluation
        /// </summary>
        /// <param name="symbol">symbol to add</param>
        /// <param name="evaluationObject">winning symbols reference list</param>
        private void AddWinningSymbol(int symbol, ref EvaluationObjectStruct evaluationObject, ref SuffixTreeNodeInfo suffix_tree_node_info)
        {
            ////Debug.Log(String.Format("Adding winning symbol {0} from node {1}", symbol, suffix_tree_node_info.Print()));
            evaluationObject.winning_symbols.Add(new WinningNode(suffix_tree_node_info, symbol));
        }

        internal void InitializeNextNodes(int current_column, ref ReelStripStructDisplayZone[] display_zones, ref SuffixTreeNodes parent_node, bool left_right)
        {
            //Start in column 1

            List<SuffixTreeNodes> children_nodes = new List<SuffixTreeNodes>();
            List<int> child_nodes = new List<int>();
            //Check if within range of primary node
            if (parent_node.node_info.row == -1)
            {
                throw new NotImplementedException();
            }
            else
            {

                if (IsInActiveDisplayZone(parent_node.node_info.row - 1, ref display_zones))
                {
                    child_nodes.Add(parent_node.node_info.row - 1);
                    children_nodes.Add(new SuffixTreeNodes(current_column, parent_node.node_info.row - 1, parent_node.parent_nodes, parent_node.node_info, left_right));
                }
                if (IsInActiveDisplayZone(parent_node.node_info.row, ref display_zones))
                {
                    child_nodes.Add(parent_node.node_info.row);
                    children_nodes.Add(new SuffixTreeNodes(current_column, parent_node.node_info.row, parent_node.parent_nodes, parent_node.node_info, left_right));
                }

                if (IsInActiveDisplayZone(parent_node.node_info.row + 1, ref display_zones))
                {
                    child_nodes.Add(parent_node.node_info.row + 1);
                    children_nodes.Add(new SuffixTreeNodes(current_column, parent_node.node_info.row + 1, parent_node.parent_nodes, parent_node.node_info, left_right));
                }
            }
            connected_nodes = child_nodes.ToArray();
            connected_nodes_struct = children_nodes.ToArray();
        }
        private bool IsInActiveDisplayZone(int v, ref ReelStripStructDisplayZone[] display_zones)
        {
            int active_slot = 0;
            for (int i = 0; i < display_zones.Length; i++)
            {
                if (active_slot > v)
                    return false;
                if (display_zones[i].active_payline_evaluations)
                {
                    for (int slot = 0; slot < display_zones[i].slots_in_reelstrip_zone; slot++)
                    {
                        if (v == active_slot)
                        {
                            return true;
                        }
                        active_slot += 1;
                    }
                }
                else
                {
                    active_slot += display_zones[i].slots_in_reelstrip_zone;
                }
            }
            return false;
        }

        internal string PrintPayline()
        {
            //This is called when we have no more columns to enable - join all primary node from parents into | seperated string
            List<int> payline = GetPrimaryNodeOfNodeAndParents(ref this);
            return String.Join("|", payline);
        }

        private List<int> GetPrimaryNodeOfNodeAndParents(ref SuffixTreeNodes node)
        {
            List<int> output = new List<int>();
            output.Add(node.node_info.row);
            for (int parent_node = 0; parent_node < parent_nodes.Length; parent_node++)
            {
                output.Add(parent_nodes[parent_node].row);
            }
            return output;
        }

        private bool WildSymbolCheck(ref NodeDisplaySymbol currentDisplaySymbol, ref NodeDisplaySymbol nextDisplaySymbol)
        {
            //TODO this won't evauate if an overlay symbol triggers wild feature
            return currentDisplaySymbol.is_wild || nextDisplaySymbol.is_wild;
        }

        private bool PrimarySymbolCheck(NodeDisplaySymbol currentDisplaySymbol, NodeDisplaySymbol nextDisplaySymbol)
        {
            return currentDisplaySymbol.primary_symbol == nextDisplaySymbol.primary_symbol;
        }
    }
}
