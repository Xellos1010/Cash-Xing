using BoomSports.Prototype.ScriptableObjects;
using BoomSports.Prototype.Containers;
using System;
using System.Collections.Generic;
using UnityEngine;
//************
#if UNITY_EDITOR
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace BoomSports.Prototype
{
    [Serializable]
    public struct SuffixTreeNode
    {
        [SerializeField]
        internal bool leftRight;
        [SerializeField]
        internal SuffixTreeNodeInfo nodeInfo;
        internal SuffixTreeNodeInfo rootNode;

        [SerializeField]
        internal SuffixTreeNodeInfo[] parentNodes;

        [SerializeField]
        internal int[] connectedRows;

        [SerializeField]
        internal SuffixTreeNode[] connectedNodes;

        public SuffixTreeNode(int primary_node, SuffixTreeNodeInfo[] parent_nodes, SuffixTreeNodeInfo parent_node, int column) : this()
        {
            this.nodeInfo.row = primary_node;

            if (this.parentNodes == null && parent_nodes == null)
            {
                this.parentNodes = new SuffixTreeNodeInfo[0];
            }
            else
            {
                this.parentNodes = parent_nodes;
            }
            this.parentNodes = this.parentNodes.AddAt<SuffixTreeNodeInfo>(0, parent_node);
            this.nodeInfo.column = column;
        }

        public SuffixTreeNode(int column, int row, SuffixTreeNodeInfo[] parent_nodes, SuffixTreeNodeInfo parent_node, bool left_right) : this()
        {
            SuffixTreeNodeInfo node_Info = new SuffixTreeNodeInfo(column, row);
            this.nodeInfo = node_Info;
            if (this.parentNodes == null && parent_nodes == null)
            {
                this.parentNodes = new SuffixTreeNodeInfo[0];
            }
            else
            {
                this.parentNodes = parent_nodes;
            }
            this.parentNodes = this.parentNodes.AddAt<SuffixTreeNodeInfo>(0, parent_node);
            this.leftRight = left_right;
        }
        /// <summary>
        /// Clones a Tree Node
        /// </summary>
        /// <param name="nodeToClone"></param>
        public SuffixTreeNode(SuffixTreeNode nodeToClone) : this()
        {
            leftRight = nodeToClone.leftRight;
            nodeInfo = nodeToClone.nodeInfo;
            rootNode = nodeToClone.rootNode;
            parentNodes = nodeToClone.parentNodes;
            connectedRows = nodeToClone.connectedRows;
            connectedNodes = nodeToClone.connectedNodes;
        }

        /// <summary>
        /// Initialize the winning symbol list and check for wins
        /// </summary>
        internal WinningPayline[] EvaluateRawWinningPaylines(ref EvaluationObjectStruct evaluationObject)
        {
            //Reset winning nodes and track new evaluation
            evaluationObject.ResetWinningEvaluationNodesList();

            //This could be a wild
            NodeDisplaySymbolContainer rootWinSymbol = evaluationObject.displayConfigurationContainerEvaluating.configuration[nodeInfo.column].displaySymbolSequence[nodeInfo.row];
            //Checks the first symbol for a feature condition - for features that don't require a winning payline
            CheckAndAddSlotsNeedFeaturesEvaluated(rootWinSymbol, ref evaluationObject, ref nodeInfo);
            //Adds the first symbol as a lineWin and makes the primary symbol to track for
            AddWinningNodeInRawList(rootWinSymbol, ref this, ref evaluationObject);
            //Debug.Log($"{evaluationObject.winningEvaluationNodes[0].Print()}");

            //Initialize Winning Paylines
            List<WinningPayline> winningPaylines = new List<WinningPayline>();

            //Debug.Log(String.Format($"Root Win Symbol = {rootWinSymbol.primarySymbol} - Starting check for winning paylines from node {nodeInfo.Print()}"));

            //Check all connected nodes for a win using dfs (depth first search) search
            EvaluateConnectedNodesForWin(this, this, ref evaluationObject, ref winningPaylines);

            //Clear winning evaluation nodes and wait till next time
            evaluationObject.ResetWinningEvaluationNodesList();
            return winningPaylines.ToArray();
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
        private void CheckAndAddSlotsNeedFeaturesEvaluated(NodeDisplaySymbolContainer evaluationDisplaySymbol, ref EvaluationObjectStruct evaluationObject, ref SuffixTreeNodeInfo nodeInfo)
        {
            //Debug.Log($"evaluationDisplaySymbol.primarySymbol {evaluationDisplaySymbol.primarySymbol} Checking for feature");
            SymbolSlotEvaluationsReturnContainer slotEvaluationActivated = Managers.EvaluationManager.CheckReturnSymbolHasFeature(evaluationDisplaySymbol);
            //Raw add of symbols activating features - use conditionals to parse for conditions that activate feature
            if (slotEvaluationActivated.connectedEvaluators?.Length > 0) //Check if symbol has features
            {
                //Store that the node has a feature. After Paylines are evaluated we take the nodes in evaluationObject.featureEvaluationActiveCount[feature] and ensure the winning 
                //Debug.Log($"Symbol {evaluationDisplaySymbol.primarySymbol} slot evaluators count = {slotEvaluationActivated.connectedEvaluators.Length}");
                for (int evaluator = 0; evaluator < slotEvaluationActivated.connectedEvaluators.Length; evaluator++)
                {
                    if (Managers.EvaluationManager.instance.configurationObject.symbolDataScriptableObject.symbols[evaluationDisplaySymbol.primarySymbol].symbolName.Contains(slotEvaluationActivated.connectedEvaluators[evaluator].symbolTargetName))
                    {
                        //Debug.Log($"Checking to add node {nodeInfo.Print()} to feature list {slotEvaluationActivated.connectedEvaluators[evaluator].featureName.ToString()}");
                        if (evaluationObject.featureEvaluationActiveCount == null)
                            evaluationObject.featureEvaluationActiveCount = new Dictionary<Features, List<SuffixTreeNodeInfo>>();
                        if (!evaluationObject.featureEvaluationActiveCount.ContainsKey(slotEvaluationActivated.connectedEvaluators[evaluator].featureName))
                        {
                            evaluationObject.featureEvaluationActiveCount[slotEvaluationActivated.connectedEvaluators[evaluator].featureName] = new List<SuffixTreeNodeInfo>();
                        }
                        if (!evaluationObject.featureEvaluationActiveCount[slotEvaluationActivated.connectedEvaluators[evaluator].featureName].Contains(nodeInfo))
                        {
                            //Debug.Log($"Adding node {nodeInfo.Print()} to feature list {slotEvaluationActivated.connectedEvaluators[evaluator].featureName.ToString()}");
                            evaluationObject.featureEvaluationActiveCount[slotEvaluationActivated.connectedEvaluators[evaluator].featureName].Add(nodeInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates the connected nodes for a match
        /// </summary>
        /// <param name="previousNodeChecked"></param>
        /// <param name="connectedNodes"></param>
        /// <param name="evaluationObject"></param>
        /// <param name="winning_paylines"></param>
        /// <param name="rootWinSymbol"></param>
        private void EvaluateConnectedNodesForWin(SuffixTreeNode rootWinSymbol, SuffixTreeNode previousNodeChecked, ref EvaluationObjectStruct evaluationObject, ref List<WinningPayline> winning_paylines)
        {
            //Cycle thru each connected node for a winning payline
            for (int connectedNode = 0; connectedNode < previousNodeChecked.connectedNodes.Length; connectedNode++)
            {
                //Debug.Log($"Checking Connected node {previousNodeChecked.connectedNodes[connectedNode].nodeInfo.Print()} from {previousNodeChecked.nodeInfo.Print()}");
                EvaluateNodeForWin(rootWinSymbol, previousNodeChecked.connectedNodes[connectedNode], ref evaluationObject, ref winning_paylines);
            }
        }

        /// <summary>
        /// Used for recursive check of suffix tree to evaluate winning paylines
        /// </summary>
        /// <param name="rootWinSymbol">Node being checked - If wild then in current depth change primary symbol to connected node evaluating and change back after rest of depth search completed</param>
        /// <param name="evaluationObject">symbols configuration to check against</param>
        /// <param name="winning_symbols">winning symbols list</param>
        private void EvaluateNodeForWin(SuffixTreeNode rootWinSymbol, SuffixTreeNode nextSymbol,ref EvaluationObjectStruct evaluationObject, ref List<WinningPayline> winning_paylines)
        {
            //Debug.Log($"Checking node {nextSymbol.nodeInfo.Print()} against root node {rootWinSymbol.nodeInfo.Print()}");
            //Get current node symbol display container
            NodeDisplaySymbolContainer currentDisplaySymbol = evaluationObject.displayConfigurationContainerEvaluating.configuration[rootWinSymbol.nodeInfo.column].displaySymbolSequence[rootWinSymbol.nodeInfo.row];
            NodeDisplaySymbolContainer nextDisplaySymbol = evaluationObject.displayConfigurationContainerEvaluating.configuration[nextSymbol.nodeInfo.column].displaySymbolSequence[nextSymbol.nodeInfo.row];

            //Debug.Log($"currentDisplaySymbol = {currentDisplaySymbol.primarySymbol} nextDisplaySymbol = {nextDisplaySymbol.primarySymbol}");

            //Checks the node for a feature condition
            CheckAndAddSlotsNeedFeaturesEvaluated(currentDisplaySymbol, ref evaluationObject, ref rootWinSymbol.nodeInfo);

            //If previous node is wild, makes current node any symbol primary symbol - repeat until at end
            //First Level Check - Wilds - TODO Refactor to abstract logic - Need to build in wild support 
            if (SymbolsMatch(currentDisplaySymbol, nextDisplaySymbol))
            {
                //Temp - cache primary node information and save for later in-case of wild
                SuffixTreeNode rootWinSymbolCache = new SuffixTreeNode(rootWinSymbol);
                bool rootChanged = false;
                //Debug.Log($"{nextDisplaySymbol.primarySymbol} Match's! Winning Symbol in Node {rootWinSymbol.nodeInfo.Print()}");
                //Change the primary symbol to current symbol if primary is wild
                if (Managers.EvaluationManager.CheckSymbolActivatesFeature(currentDisplaySymbol.primarySymbol, Features.wild))
                {
                    //Debug.Log($"{currentDisplaySymbol.primarySymbol} is a wild! Changing root symbol to next node");
                    rootWinSymbol = nextSymbol;
                    rootChanged = true;
                }
                //Add the winning symbol to the payline
                AddWinningNodeInRawList(nextDisplaySymbol, ref nextSymbol,ref evaluationObject);

                //Current payline index - to remove symbol from payline after checking later nodes
                int winningSymbolIndex = evaluationObject.winningEvaluationNodes.Count - 1;

                //There is a match - move to the next node if the winning symbols don't equal total columns in configuration
                if (evaluationObject.winningEvaluationNodes.Count < evaluationObject.displayConfigurationContainerEvaluating.configuration.Length)
                {
                    //Check each connected node
                    EvaluateConnectedNodesForWin(rootWinSymbol, nextSymbol, ref evaluationObject, ref winning_paylines);
                }
                else //Reached the end of the payline - add this payline and override others - remove symbol and start down next tree
                {
                    InitializeAndAddDynamicWinningPayline(rootWinSymbol, ref evaluationObject.winningEvaluationNodes, ref winning_paylines);
                }
                //Remove the winning Symbol when done evaluating all possible configuration paths. So you can evaluate next node in supported configuration sequence
                RemoveWinningSymbol(ref evaluationObject.winningEvaluationNodes, winningSymbolIndex);
                if(rootChanged)
                {
                    rootWinSymbol = rootWinSymbolCache;
                }
            }
            else
            {
                //Debug.Log($"Reached end of Payline - evaluationObject.winningEvaluationNodes.Count {evaluationObject.winningEvaluationNodes.Count} >= 3 == {evaluationObject.winningEvaluationNodes.Count >= 3}");
                PrintWinningEvaluationNodes(ref evaluationObject);
                if (evaluationObject.winningEvaluationNodes.Count >= 3)
                {
                    InitializeAndAddDynamicWinningPayline(rootWinSymbol, ref evaluationObject.winningEvaluationNodes, ref winning_paylines);
                }
            }
        }

        private bool SymbolsMatch(NodeDisplaySymbolContainer currentDisplaySymbol, NodeDisplaySymbolContainer nextDisplaySymbol)
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

        private void InitializeAndAddDynamicWinningPayline(SuffixTreeNode suffix_tree_node, ref List<WinningEvaluatedNodeContainer> winning_symbols, ref List<WinningPayline> winning_paylines)
        {
            //Debug.Log(String.Format("Payline {0} won!", PrintDynamicPayline(ref winning_symbols)));
            int[] payline = new int[winning_symbols.Count];
            List<WinningEvaluatedNodeContainer> winning_symbol_row = new List<WinningEvaluatedNodeContainer>();
            SuffixTreeNodeInfo rootNode = winning_symbols[0].nodeInfo;
            for (int symbol = 0; symbol < winning_symbols.Count; symbol++)
            {
                payline[symbol] = winning_symbols[symbol].nodeInfo.row;
                winning_symbol_row.Add(winning_symbols[symbol]);
            }
            AddDynamicWinningPayline(payline, winning_symbol_row, suffix_tree_node.leftRight, ref winning_paylines, rootNode);
        }

        internal void AddDynamicWinningPayline(int[] payline, List<WinningEvaluatedNodeContainer> matching_symbols_list, bool left_right, ref List<WinningPayline> winning_paylines, SuffixTreeNodeInfo rootNode)
        {
            Payline payline_won = new Payline(payline, left_right, matching_symbols_list[0].nodeInfo);
            payline_won.rootNode = rootNode;
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
            int[] new_winning_payline_configuration = new_winning_payline.payline.configuration.payline;
            int[] shortest_payline_configuration;
            int[] list_entry_winning_payline_configuration;

            //Iterate thru each winning payline to compare to new payline
            for (int winning_payline = 0; winning_payline < winning_paylines.Count; winning_payline++)
            {
                list_entry_winning_payline_configuration = winning_paylines[winning_payline].payline.configuration.payline;
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
            return payline1.configuration.payline.Length > payline2.configuration.payline.Length ? payline2.configuration.payline : payline1.configuration.payline;
        }

        private string PrintDynamicPayline(ref List<WinningEvaluatedNodeContainer> winning_symbols)
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

        private void RemoveWinningSymbol(ref List<WinningEvaluatedNodeContainer> winning_symbols, int index)
        {
            //Debug.Log(String.Format("Removing winning symbol {0}", winning_symbols[index]));
            winning_symbols.RemoveAt(index);
        }

        /// <summary>
        /// Adds a winning symbol to track for dynamic payline evaluation
        /// </summary>
        /// <param name="symbol">symbol to add</param>
        /// <param name="evaluationObject">winning symbols reference list</param>
        private void AddWinningNodeInRawList(NodeDisplaySymbolContainer winningSymbolContainer, ref SuffixTreeNode winningNode, ref EvaluationObjectStruct evaluationObject)
        {
            evaluationObject.winningEvaluationNodes.Add(new WinningEvaluatedNodeContainer(winningNode.nodeInfo, winningSymbolContainer.primarySymbol));
            //PrintWinningEvaluationNodes(ref evaluationObject);
        }

        private void PrintWinningEvaluationNodes(ref EvaluationObjectStruct evaluationObject)
        { 
            string debugString = "";
            for (int i = 0; i < evaluationObject.winningEvaluationNodes.Count; i++)
            {
                debugString += $"|{evaluationObject.winningEvaluationNodes[i].symbol}-{evaluationObject.winningEvaluationNodes[i].nodeInfo.Print()}";
            }
            Debug.Log($"evaluationObject.winningEvaluationNodes = {debugString}");
        }
        /// <summary>
        /// Ensures the connected nodes are valid nodes
        /// </summary>
        /// <param name="nextColumn"></param>
        /// <param name="displayZoneNextColumn"></param>
        /// <param name="parentNode"></param>
        /// <param name="leftRight"></param>
        /// <param name="evaluationDirection"></param>
        /// <param name="customColumnsDefine"></param>
        internal void InitializeConnectedNodes(int nextColumn, ref ConfigurationDisplayZonesStruct displayZoneNextColumn, ref SuffixTreeNode parentNode, bool leftRight, paylineDirection evaluationDirection, CustomColumns[] customColumnsDefine = null)
        {
            //Debug.Log($"Initializing connected nodes for {parentNode.nodeInfo.Print()} - nextColumn = {nextColumn}");

            List<SuffixTreeNode> connectedNodes = new List<SuffixTreeNode>();
            List<int> connectedNodesList = new List<int>();

            //validate previous node is non-negatice
            if (parentNode.nodeInfo.row < 0)
            {
                //Debug.Log($"parent_node.node_info.row < 0 - Not a valid row - Check your code");
                throw new NotImplementedException();
            }
            else
            {
                //Evaluation happens here - check if custom columns 
                if (evaluationDirection != paylineDirection.customcolumns)
                {
                    //Debug.Log($"Evaluation Direction not custom - using spider tree method {parentNode.nodeInfo.Print()}");
                    Spider1NodeBuildTree(nextColumn, displayZoneNextColumn, parentNode, leftRight, connectedNodes, connectedNodesList);
                }
                else
                {
                    //If parent node falls within a custom column for or the current then respect both
                    //Debug.Log($"Checking Node column {nextColumn} row {parentNode.nodeInfo.row} Checking for custom Column in parent node");
                    //Need to build nodes based on custom columns
                    bool useSpider = true;
                    //Check current Column for custom Column
                    //The first check is if the parent node is an adjacent or spider node
                    for (int i = 0; i < customColumnsDefine.Length; i++)
                    {
                        if (parentNode.nodeInfo.column == customColumnsDefine[i].rootColumn)
                        {
                            if (customColumnsDefine[i].evaluateAdjacentRightLeftOnly)
                            {
                                useSpider = false;
                                break;
                            }
                        }
                        else if (nextColumn == customColumnsDefine[i].rootColumn)
                        {
                            if (customColumnsDefine[i].evaluateAdjacentRightLeftOnly)
                            {
                                useSpider = false;
                                break;
                            }
                        }
                    }
                    if (useSpider)
                    {
                        //Debug.Log($"column {nextColumn} row {parentNode.nodeInfo.row} using Spider Node Build Tree");
                        Spider1NodeBuildTree(nextColumn, displayZoneNextColumn, parentNode, leftRight, connectedNodes, connectedNodesList);
                    }
                    else
                    {
                        //Debug.Log($"column {nextColumn} row {parentNode.nodeInfo.row} Adding Adjacent node only");
                        AddAdjacentNode(nextColumn, displayZoneNextColumn, parentNode, leftRight, connectedNodes, connectedNodesList);
                    }
                }
            }
            connectedRows = connectedNodesList.ToArray();
            this.connectedNodes = connectedNodes.ToArray();
        }

        private void Spider1NodeBuildTree(int current_column, ConfigurationDisplayZonesStruct displayZoneNextColumn, SuffixTreeNode parent_node, bool left_right, List<SuffixTreeNode> connectedNodes, List<int> connectedNodesList)
        {
            AddDiagonalTopNode(current_column, displayZoneNextColumn, parent_node, left_right, connectedNodes, connectedNodesList);
            AddAdjacentNode(current_column, displayZoneNextColumn, parent_node, left_right, connectedNodes, connectedNodesList);
            AddDiagonalBottomNode(current_column, displayZoneNextColumn, parent_node, left_right, connectedNodes, connectedNodesList);
        }

        private void AddDiagonalBottomNode(int current_column, ConfigurationDisplayZonesStruct displayZoneNextColumn, SuffixTreeNode parent_node, bool left_right, List<SuffixTreeNode> connectedNodes, List<int> connectedNodesList)
        {
            //Debug.Log($"Checking Bottom Diagonal Node Column:{current_column} parent_node.node_info.row + 1 = {parent_node.nodeInfo.row + 1} is in active display zone");
            if (IsInActiveDisplayZone(parent_node.nodeInfo.row + 1, ref displayZoneNextColumn))
            {
                //Debug.Log($"Adding Bottom Diagonal Node Column:{current_column} parent_node.node_info.row + 1 = {parent_node.nodeInfo.row + 1} is in active display zone");
                connectedNodesList.Add(parent_node.nodeInfo.row + 1);
                connectedNodes.Add(new SuffixTreeNode(current_column, parent_node.nodeInfo.row + 1, parent_node.parentNodes, parent_node.nodeInfo, left_right));
            }
        }

        private void AddDiagonalTopNode(int current_column, ConfigurationDisplayZonesStruct displayZoneNextColumn, SuffixTreeNode parent_node, bool left_right, List<SuffixTreeNode> connectedNodes, List<int> connectedNodesList)
        {
            //Debug.Log($"Checking Top Diagonal Node Column:{current_column} parent_node.node_info.row - 1 = {parent_node.nodeInfo.row - 1} is in active display zone");
            if (parent_node.nodeInfo.row - 1 > -1)
            {
                if (IsInActiveDisplayZone(parent_node.nodeInfo.row - 1, ref displayZoneNextColumn))
                {
                    //Debug.Log($"Adding Top Diagonal Node Column:{current_column} parent_node.node_info.row - 1 = {parent_node.nodeInfo.row - 1} is in active display zone");
                    connectedNodesList.Add(parent_node.nodeInfo.row - 1);
                    connectedNodes.Add(new SuffixTreeNode(current_column, parent_node.nodeInfo.row - 1, parent_node.parentNodes, parent_node.nodeInfo, left_right));
                }
            }
        }

        private void AddAdjacentNode(int current_column, ConfigurationDisplayZonesStruct displayZoneNextColumn, SuffixTreeNode parent_node, bool left_right, List<SuffixTreeNode> connectedNodes, List<int> connectedNodesList)
        {
            //Debug.Log($"Checking if adjacent row to parent is in an active display zone");
            if (IsInActiveDisplayZone(parent_node.nodeInfo.row, ref displayZoneNextColumn))
            {
                //Debug.Log($"Adjecent Node Column:{current_column} parent_node.node_info.row = {parent_node.nodeInfo.row} is in active display zone");
                connectedNodesList.Add(parent_node.nodeInfo.row);
                connectedNodes.Add(new SuffixTreeNode(current_column, parent_node.nodeInfo.row, parent_node.parentNodes, parent_node.nodeInfo, left_right));
            }
        }

        private bool CustomColumnsContainsRootColumn(int column, ref CustomColumns[] customColumnsDefine, out int columnIndex)
        {
            columnIndex = -1;
            if (customColumnsDefine == null || customColumnsDefine?.Length < 1)
                return false;
            for (int i = 0; i < customColumnsDefine.Length; i++)
            {
                if(customColumnsDefine[i].rootColumn == column)
                {
                    columnIndex = i;
                    //Debug.Log($"Column {column} has Custom Columns {i}");
                    return true;
                }
            }
            return false;
        }

        private bool IsInActiveDisplayZone(int row, ref ConfigurationDisplayZonesStruct displayZoneNextColumn)
        {
            //Below padding or above total display zones + padding before
            if (row < displayZoneNextColumn.paddingBefore || row > (displayZoneNextColumn.displayZonesPositionsTotal + displayZoneNextColumn.paddingBefore))
            {
                //Debug.LogWarning($"Position does not fall within display range default return false is positon Active Display Zone");
                return false;
            }
            else
            {
                //Debug.Log($"Checking if row {row} is in active display zone padding = {displayZoneNextColumn.paddingBefore} displayZoneNextColumn.displayZonesPositionsTotal = {displayZoneNextColumn.displayZonesPositionsTotal}");
                int positionsInZone = 0;
                //Check each position in zone - if slot to check falls within active payzone return tru
                for (int i = 0; i < displayZoneNextColumn.displayZones.Length; i++)
                {
                    positionsInZone += displayZoneNextColumn.displayZones[i].positionsInZone;
                    //Debug.Log($"displayZoneNextColumn.displayZones[i].activePaylineEvaluations = {displayZoneNextColumn.displayZones[i].activePaylineEvaluations}");
                    if (displayZoneNextColumn.displayZones[i].activePaylineEvaluations)
                    {
                        //Debug.Log($"Checking row {row} in displayZoneNextColumn.displayZones[i].positionsInZone = {displayZoneNextColumn.displayZones[i].positionsInZone}");
                        //Debug.Log($"{row} >= {displayZoneNextColumn.paddingBefore} = {row >= displayZoneNextColumn.paddingBefore} : {row} < {displayZoneNextColumn.paddingBefore + positionsInZone} {row < displayZoneNextColumn.paddingBefore + positionsInZone}");
                        if (row >= displayZoneNextColumn.paddingBefore && row < displayZoneNextColumn.paddingBefore + positionsInZone) // Is within that zone - return active or inactive
                        {
                            //Debug.Log("Returning Active Zone");
                            return displayZoneNextColumn.displayZones[i].activePaylineEvaluations;
                        }
                        else
                        {
                            //Debug.Log("not in active zone - check next zone");
                        }
                    }
                }
            }
            //Debug.LogWarning($"Position is not valid - default return false is positon Active Display Zone");
            return false;
        }

        internal string PrintPayline()
        {
            //This is called when we have no more columns to enable - join all primary node from parents into | seperated string
            List<int> payline = GetPrimaryNodeOfNodeAndParents(ref this);
            return String.Join("|", payline);
        }

        private List<int> GetPrimaryNodeOfNodeAndParents(ref SuffixTreeNode node)
        {
            List<int> output = new List<int>();
            output.Add(node.nodeInfo.row);
            for (int parent_node = 0; parent_node < parentNodes.Length; parent_node++)
            {
                output.Add(parentNodes[parent_node].row);
            }
            return output;
        }

        private bool WildSymbolCheck(ref NodeDisplaySymbolContainer currentDisplaySymbol, ref NodeDisplaySymbolContainer nextDisplaySymbol)
        {
            //Check with evaluation manager based on conditionals if symbol is wild
            return Managers.EvaluationManager.CheckSymbolActivatesFeature(currentDisplaySymbol.primarySymbol,Features.wild)|| Managers.EvaluationManager.CheckSymbolActivatesFeature(nextDisplaySymbol.primarySymbol, Features.wild);
        }

        private bool PrimarySymbolCheck(NodeDisplaySymbolContainer currentDisplaySymbol, NodeDisplaySymbolContainer nextDisplaySymbol)
        {
            return currentDisplaySymbol.primarySymbol == nextDisplaySymbol.primarySymbol;
        }
    }
}
