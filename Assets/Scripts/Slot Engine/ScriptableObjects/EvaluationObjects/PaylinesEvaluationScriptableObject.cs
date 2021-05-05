﻿//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : SlotEngine.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
using Slot_Engine.Matrix.Managers;
using System.Collections.Generic;
using System;

namespace Slot_Engine.Matrix.ScriptableObjects
{
    public enum payline_direction
    {
        left,
        right,
        both,
        count
    }
    /// <summary>
    /// Paylines Evaluation Scriptable Object - Holds nodes and conditions to build nodes and store information
    /// </summary>
    [CreateAssetMenu(fileName = "PaylinesEvaluationObject", menuName = "BoomSportsScriptableObjects/PaylinesEvaluationScriptableObject", order = 4)]
    public class PaylinesEvaluationScriptableObject : EvaluationScriptableObject
    {
        /// <summary>
        /// Which way should the evaluation take place?
        /// </summary>
        public payline_direction evaluation_direction;
        /// <summary>
        /// number of paylines supported - pre-generated in editor mode
        /// </summary>
        public int number_of_paylines = 0;
        /// <summary>
        /// The root nodes for dynamic paylines using a suffix tree
        /// </summary>
        public SuffixTreeRootNodes dynamic_paylines;
        public EvaluationObjectStruct evaluationUsed;
        /// <summary>
        /// MainEntry to evaluate for WinningPaylines
        /// </summary>
        /// <param name="evaluationObject">The Configuration & Feature slot evaluators</param>
        /// <returns>WinningPayline[]</returns>
        public override object EvaluatePaylines(ref EvaluationObjectStruct evaluationObject)
        {
            evaluationUsed = evaluationObject;
            List<WinningPayline> output_raw = new List<WinningPayline>();
            List<WinningPayline> output_filtered = new List<WinningPayline>();
            Debug.Log($"dynamic_paylines.rootNodes.Length = {dynamic_paylines.rootNodes.Length}");
            //Filter thru each node and check the active feature conditions for activating a feature
            for (int rootNode = 0; rootNode < dynamic_paylines.rootNodes.Length; rootNode++)
            {
                Debug.Log($"Checking Root Node {dynamic_paylines.rootNodes[rootNode].node_info.Print()}");
                output_raw.AddRange(dynamic_paylines.rootNodes[rootNode].InitializeAndCheckForWinningPaylines(ref evaluationObject));
                //Don't add the same full line win both ways
                FilterRawOutputForDuplicateRootNodeEntries(ref output_filtered, ref output_raw,evaluationObject.maxLength);
                output_filtered.AddRange(output_raw);
                output_raw.Clear();
                //Debug.Log(String.Format("winning paylines Count = {0} for root_node {1} info = {2}", output_filtered.Count,rootNode, dynamic_paylines.rootNodes[rootNode].node_info.Print()));
            }
            if (evaluationObject.featureEvaluationActiveCount != null)
            {
                //Debug.Log(String.Format("Looking for features that activated"));
                foreach (KeyValuePair<Features, List<SuffixTreeNodeInfo>> item in evaluationObject.featureEvaluationActiveCount)
                {
                    //Multiplier calculated first then mode is applied
                    Debug.Log(String.Format("Feature name = {0}, counter = {1} mode - {2}", item.Key.ToString(), item.Value.Count, StateManager.enCurrentMode));
                    if ((item.Key == Features.overlay || item.Key == Features.multiplier))
                    {
                        OverlayEvaluationScriptableObject overlayLogic = EvaluationManager.GetFirstInstanceFeatureEvaluationObject<OverlayEvaluationScriptableObject>(ref evaluationObject.slotEvaluationObjects);
                        //Feature Evaluated Slot items are in raw format waiting for winningObjects to be generated to run evaluation logic- run check if items are valid
                        for (int node = item.Value.Count - 1; node >= 0; node--)
                        {
                            if (!overlayLogic.EvaluateNodeForConditionsMet(item.Value[node], output_filtered.ToArray()))
                            {
                                item.Value.RemoveAt(node);
                            }
                        }
                        if (item.Value.Count > 0)
                        {
                            if (StateManager.enCurrentMode != GameModes.freeSpin)
                            {
                                if (StateManager.enCurrentMode == GameModes.baseGame) //Can Only apply overlay feature in base-game
                                    StateManager.SetFeatureActiveTo(Features.multiplier, true);
                                EvaluationManager.GetFirstInstanceFeatureEvaluationObject<OverlayEvaluationScriptableObject>(ref evaluationObject.slotEvaluationObjects).nodesActivatingEvaluationConditions = item.Value;
                            }
                            else
                            {
                                StateManager.AddToMultiplier(item.Value.Count);
                            }
                        }
                    }
                    if (item.Key == Features.freespin)
                    {
                        FreeSpinEvaluationScriptableObject freespinsObject = EvaluationManager.GetFirstInstanceFeatureEvaluationObject<FreeSpinEvaluationScriptableObject>(ref evaluationObject.slotEvaluationObjects);
                        bool activateFeature = freespinsObject.EvaluateConditionsMet(item.Value, output_filtered.ToArray());
                        if (activateFeature)
                        {
                            StateManager.SetFeatureActiveTo(Features.freespin, true);
                        }
                        else
                        {
                            freespinsObject.nodesActivatingEvaluationConditions.Clear();
                            break;
                        }
                    }
                }
            }
            evaluated = true;
            if (winningObjects == null)
                winningObjects = new List<WinningObject>();
            winningObjects.AddRange(output_filtered.ToArray());
            BuildWinningSymbolNodes(ref winningObjects); 
            return output_filtered.ToArray();
        }

        private void BuildWinningSymbolNodes(ref List<WinningObject> winningPaylines)
        {
            HashSet<EvaluationNode> winningNodes = new HashSet<EvaluationNode>();
            for (int payline = 0; payline < winningPaylines.Count; payline++)
            {
                for (int node = 0; node < winningPaylines[payline].winningNodes.Length; node++)
                {
                    winningNodes.Add(winningPaylines[payline].winningNodes[node]);
                }
            }
            if (evaluationUsed.winningEvaluationNodes == null)
                evaluationUsed.winningEvaluationNodes = new List<EvaluationNode>();
            evaluationUsed.winningEvaluationNodes.AddRange(winningNodes);
        }

        public override int? ReturnEvaluationObjectSupportedRootCount()
        {
            return number_of_paylines;
        }

        //Generating Paylines supported froma matrix
        internal void GenerateDynamicPaylinesFromMatrix(ref ReelStripManager[] matrixReels)
        {
            //        Initialize:
            //          Create a root node at column 0(off - screen, non - column part of all solutions)
            //          root node.length = 0
            //          root node.terminal = false
            //          Add all paylines(in the form of length w arrays of integers ranging from 1 to h) to the root nodes' "toDistribute set"
            //          Create a toWork queue, add the root node to it

            //        Iterate: while toWork not empty:
            //          let node n = toWork.pop()
            //          if n.length < w
            //          create children of n with length n.length + 1 and terminal = (n.length + 1 == w).
            //          for payline p in n.toDistribute
            //          remove p from n.toDistribute
            //          if (p.length > 1)
            //              add p.subArray(1, end) to child of n as applicable.
            //              add children of n to toWork
            //Node 0 is connected to 0 / 1
            //Initializing the first reel root nodes
            List<SuffixTreeNodes> paylines = new List<SuffixTreeNodes>();
            List<SuffixTreeNodes> finished_list = new List<SuffixTreeNodes>();

            number_of_paylines = 0;
            dynamic_paylines.paylinesSupported = new Payline[0];

            dynamic_paylines.rootNodes = InitializeRootNodes(ref matrixReels).ToArray();
            List<SuffixTreeNodes> to_finish_list = new List<SuffixTreeNodes>();

            for (int root_node = 0; root_node < dynamic_paylines.rootNodes.Length; root_node++)
            {
                //Start a new payline that is going to be printed per root node
                List<int> payline = new List<int>();
                //Build all paylines
                BuildPayline(ref payline, ref dynamic_paylines.rootNodes[root_node], ref matrixReels);
            }
        }
        internal void BuildPayline(ref List<int> payline, ref SuffixTreeNodes node, ref ReelStripManager[] reel_strip_managers)
        {
            //Add current node to payline
            payline.Add(node.node_info.row);
            int next_column = node.left_right ? node.node_info.column + 1 : node.node_info.column - 1;
            //Check the column is the last column and continue if it is
            if (node.left_right ?
                next_column >= reel_strip_managers.Length :
                next_column < 0)
            {
                Debug.Log("Reached end of payline");
                dynamic_paylines.AddPaylineSupported(payline.ToArray(), node.left_right);
                number_of_paylines += 1;
            }
            else
            {
                SuffixTreeNodes parent_node = node; //First pass thru this will be nothing
                ReelStripStructDisplayZone[] rows_in_next_column = reel_strip_managers[next_column].reelstrip_info.display_zones;
                //First in is parent_node = 0 | Children Column = 1 | slots_per_reel = 5
                node.InitializeNextNodes(next_column, ref rows_in_next_column, ref parent_node, node.left_right);
                for (int child_nodes = 0; child_nodes < node.connected_nodes_struct.Length; child_nodes++)
                {
                    //Now build out the child refs
                    BuildPayline(ref payline, ref node.connected_nodes_struct[child_nodes], ref reel_strip_managers);
                    //Remove payline buildup
                    payline.RemoveRange(node.parent_nodes.Length, payline.Count - node.parent_nodes.Length);
                }
            }
        }

        private List<SuffixTreeNodes> InitializeRootNodes(ref ReelStripManager[] reel_strip_managers)
        {
            List<SuffixTreeNodes> root_nodes = new List<SuffixTreeNodes>();
            SuffixTreeNodes node;
            //Initialize all the rows and the next elements
            switch (evaluation_direction)
            {
                case payline_direction.left:
                    root_nodes.AddRange(BuildRootNodes(0, ref reel_strip_managers[0], true));
                    break;
                case payline_direction.right:
                    root_nodes.AddRange(BuildRootNodes(reel_strip_managers.Length - 1, ref reel_strip_managers[reel_strip_managers.Length - 1], false));
                    break;
                case payline_direction.both:
                    root_nodes.AddRange(BuildRootNodes(0, ref reel_strip_managers[0], true));
                    root_nodes.AddRange(BuildRootNodes(reel_strip_managers.Length - 1, ref reel_strip_managers[reel_strip_managers.Length - 1], false));
                    break;
                default:
                    Debug.Log("Please set the evaluation direciton to left, right or both");
                    break;
            }
            return root_nodes;
        }

        private List<SuffixTreeNodes> BuildRootNodes(int column, ref ReelStripManager reel_strip_manager, bool left_right)
        {
            List<SuffixTreeNodes> root_nodes = new List<SuffixTreeNodes>();
            SuffixTreeNodes node;
            //Used to assign each row in the column - active or inactive payline evaluation
            int row = 0;
            for (int display_zone = 0; display_zone < reel_strip_manager.reelstrip_info.display_zones.Length; display_zone++)
            {
                ReelStripStructDisplayZone reel_display_zone = reel_strip_manager.reelstrip_info.display_zones[display_zone];
                if (reel_display_zone.active_payline_evaluations)
                {
                    for (int slot = 0; slot < reel_display_zone.positionsInZone; slot++)
                    {
                        //Build my node
                        node = new SuffixTreeNodes(column, row, null, new SuffixTreeNodeInfo(-1, -1), left_right);
                        root_nodes.Add(node);
                        //Debug.Log(String.Format("Registering Root Node {0}", node.node_info.Print()));
                        row += 1;
                    }
                }
                else
                {
                    Debug.Log(String.Format("Non-active pay zone- skipping {0} rows ", reel_display_zone.positionsInZone));
                    for (int slot = 0; slot < reel_display_zone.positionsInZone; slot++)
                    {
                        //Register blank slot
                        Debug.Log(String.Format("Root Node {0} {1} not in active payzone", column, row));
                        row += 1;
                    }
                }
            }
            return root_nodes;
        }

        private static List<int> GenerateConnectedNodes(ref ReelStripStructDisplayZone[] rows_in_next_column, int primary_node)
        {
            List<int> connected_nodes = new List<int>();
            if (IsInActiveDisplayZone(primary_node - 1, ref rows_in_next_column))
            {
                connected_nodes.Add(primary_node - 1);
            }
            //if you have a 5x4x3x4x5 then we want to be able to calculate 5 -> 4 but not 4 -> 5
            if (IsInActiveDisplayZone(primary_node, ref rows_in_next_column))
            {
                connected_nodes.Add(primary_node);
            }
            //if you have a 5x4x3x4x5 then we want to be able to calculate 5 -> 4 but not 4 -> 5
            if (IsInActiveDisplayZone(primary_node + 1, ref rows_in_next_column))
            {
                connected_nodes.Add(primary_node + 1);
            }

            return connected_nodes;
        }
        private static bool IsInActiveDisplayZone(int node, ref ReelStripStructDisplayZone[] display_zones)
        {
            int active_slot = 0;
            for (int i = 0; i < display_zones.Length; i++)
            {
                if (active_slot > node)
                    return false;
                if (display_zones[i].active_payline_evaluations)
                {
                    for (int slot = 0; slot < display_zones[i].positionsInZone; slot++)
                    {
                        if (node == active_slot)
                        {
                            return true;
                        }
                        active_slot += 1;
                    }
                }
                else
                {
                    active_slot += display_zones[i].positionsInZone;
                }
            }
            return false;
        }
        private void FilterRawOutputForDuplicateRootNodeEntries(ref List<WinningPayline> output_filtered, ref List<WinningPayline> output_raw, int? maxLength)
        {
            List<WinningPayline> duplicate_paylines = new List<WinningPayline>();
            WinningPayline raw_payline;
            for (int payline = 0; payline < output_raw.Count; payline++)
            {
                raw_payline = output_raw[payline];
                //Compare both ends of a line win spanning the reels.length
                if (raw_payline.winningNodes.Length == maxLength)
                {
                    //Check for a duplicate entry already in output filter
                    if (IsFullLineWinInList(raw_payline, ref output_filtered))
                    {
                        duplicate_paylines.Add(raw_payline);
                    }
                }
            }
            for (int duplicate_payline = 0; duplicate_payline < duplicate_paylines.Count; duplicate_payline++)
            {
                output_raw.Remove(duplicate_paylines[duplicate_payline]);
            }
        }
        /// <summary>
        /// This ensures there are no winning paylines that share the same payline already. Keep highest value winning_payline 
        /// </summary>
        /// <param name="new_winning_payline"></param>
        /// <param name="winning_paylines"></param>
        private bool IsFullLineWinInList(WinningPayline new_winning_payline, ref List<WinningPayline> winning_paylines)
        {
            //Check which 
            int left_root_node_new_winning_payline = new_winning_payline.payline.ReturnLeftRootNodeFromFullLineWin();
            int right_root_node_new_winning_payline = new_winning_payline.payline.ReturnRightRootNodeFromFullLineWin();
            int left_root_node_winning_payline = 0;
            int right_root_node_winning_payline = 0;
            for (int winning_payline = 0; winning_payline < winning_paylines.Count; winning_payline++)
            {
                left_root_node_winning_payline = winning_paylines[winning_payline].payline.ReturnLeftRootNodeFromFullLineWin();
                right_root_node_winning_payline = winning_paylines[winning_payline].payline.ReturnRightRootNodeFromFullLineWin();
                //Debug.Log(String.Format("left_root_node_winning_payline = {0} | left_root_node_new_winning_payline = {1} | right_root_node_winning_payline = {2} | right_root_node_new_winning_payline = {3}", left_root_node_winning_payline,left_root_node_new_winning_payline,right_root_node_winning_payline,right_root_node_new_winning_payline));
                if (left_root_node_winning_payline == left_root_node_new_winning_payline && right_root_node_new_winning_payline == right_root_node_winning_payline)
                {
                    return true;
                }
            }
            return false;
        }


        private int GetPossiblePaylineCombinations(ref SuffixTreeNodes suffix_tree_node)
        {
            int paylines_supported = 0;
            if (suffix_tree_node.connected_nodes_struct != null)
            {
                paylines_supported = suffix_tree_node.connected_nodes_struct.Length;
                if (suffix_tree_node.connected_nodes.Length > 0)
                {
                    for (int sub_node = 0; sub_node < suffix_tree_node.connected_nodes_struct.Length; sub_node++)
                    {
                        paylines_supported += GetPossiblePaylineCombinations(ref suffix_tree_node.connected_nodes_struct[sub_node]);
                    }
                }
            }
            return paylines_supported;
        }

        public override void ClearWinningObjects()
        {
            winningObjects = new List<WinningObject>();
        }
    }
}