﻿using System;
using System.Collections;
using System.Collections.Generic;
//For Parsing Purposes
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
//************
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PaylinesManager))]
    class PayLinesEditor : BoomSportsEditor
    {
        PaylinesManager myTarget;
        SerializedProperty paylines_supported_from_file;
        SerializedProperty winning_paylines;
        SerializedProperty paylines_evaluated;
        SerializedProperty root_payline_nodes;
        private int payline_to_show;
        private int winning_payline_to_show;
        public void OnEnable()
        {
            myTarget = (PaylinesManager)target;
            paylines_supported_from_file = serializedObject.FindProperty("paylines_supported_from_file");
            winning_paylines = serializedObject.FindProperty("winning_paylines");
            paylines_evaluated = serializedObject.FindProperty("paylines_evaluated");
            root_payline_nodes = serializedObject.FindProperty("root_payline_nodes");
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Set Paylines From file"))
            {
                myTarget.SetPaylines();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Generate Paylines from Matrix"))
            {
                //todo get matrix from script
                myTarget.GeneratePaylinesFromMatrix(new Matrix_Settings(5,3));
                serializedObject.ApplyModifiedProperties();
            }
            
            if (paylines_supported_from_file.arraySize > 0)
            {
                payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, paylines_supported_from_file.arraySize - 1);
                if (GUILayout.Button("Show Payline"))
                {
                    myTarget.ShowPaylineRaw(payline_to_show);
                }
                if (GUILayout.Button("Evaluate Payline"))
                {
                    myTarget.EvaluateWinningSymbols();
                    serializedObject.ApplyModifiedProperties();
                }
                if (paylines_evaluated.boolValue)
                {
                    if (winning_paylines.arraySize > 0)
                    {
                        winning_payline_to_show = EditorGUILayout.IntSlider(winning_payline_to_show, 0, winning_paylines.arraySize - 1);
                        if (GUILayout.Button("Show Winning Payline"))
                        {
                            myTarget.ShowWinningPayline(myTarget.GetWinningPayline(winning_payline_to_show));
                        }
                        if (GUILayout.Button("Clear Winning Paylines"))
                        {
                            myTarget.ClearWinningPaylines();
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            if(root_payline_nodes.arraySize > 0)
            {
                payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, root_payline_nodes.arraySize - 1);
                if (GUILayout.Button("Show Payline"))
                {
                    myTarget.ShowPaylineRaw(payline_to_show);
                }
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            EditorGUILayout.PropertyField(winning_paylines);
            base.OnInspectorGUI();
        }
    }
#endif


    public class PaylinesManager : MonoBehaviour
    {
        [SerializeField]
        public Payline[] paylines_supported_from_file;
        [SerializeField]
        internal WinningPayline[] winning_paylines;
        public int current_winning_payline_shown = -1;
        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal float GetTotalWinAmount()
        {
            float output = 0;
            for (int i = 0; i < winning_paylines.Length; i++)
            {
                output += winning_paylines[i].GetTotalWin(matrix.weighted_distribution_symbols, matrix);
            }
            return output;
        }

        //The range for active paylines to use when evaluating paylines
        public int active_payline_range_lower = 0;
        public int active_payline_range_upper = 98;
        //**
        public int paylines_active;
        public bool paylines_evaluated = false;
        public bool cycle_paylines = true;
        //TODO Change this to access animator length of state
        public float delay_between_wininng_payline = .5f;
        public float wininng_payline_highlight_time = 2;
        [SerializeField]
        private int padding_end_reel = 1;
        public PaylineRendererManager payline_renderer_manager
        {
            get
            {
                if (_payline_renderer_manager == null)
                {
                    _payline_renderer_manager = GameObject.FindObjectOfType<PaylineRendererManager>();
                }
                return _payline_renderer_manager;
            }
        }
        public PaylineRendererManager _payline_renderer_manager;
        internal Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        private Matrix _matrix;

        public suffix_tree_node[] root_payline_nodes;

        public Task cycle_paylines_task;
        internal void SetPaylines()
        {
            //Find File - Parse File - Fill Array of int[]
            TextAsset paylines = Resources.Load<TextAsset>("Data/99paylines_m3x5");
            Debug.Log(paylines.text);
            List<int> paylineListRaw = new List<int>();
            List<Payline> paylineListOutput = new List<Payline>();

            for (int i = 0; i < paylines.text.Length; i++)
            {
                if (Char.IsDigit(paylines.text[i]))
                {
                    Debug.Log(string.Format("Char {0} is {1}", i, Char.GetNumericValue(paylines.text[i])));
                    paylineListRaw.Add((int)Char.GetNumericValue(paylines.text[i]));
                    if (paylineListRaw.Count == 5)
                    {

                        paylineListOutput.Add(new Payline(paylineListRaw.ToArray()));
                        Debug.Log(paylineListRaw.ToArray().ToString());
                        paylineListRaw.Clear();
                    }
                }
            }
            Debug.Log(paylineListOutput.ToArray().ToString());
            paylines_supported_from_file = paylineListOutput.ToArray();
        }

        int ReturnLengthStreamReader(StreamReader Reader)
        {
            int i = 0;
            while (Reader.ReadLine() != null) { i++; }
            return i;
        }

        public void ShowPaylineRaw(int payline_to_show)
        {
            if (paylines_supported_from_file.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < paylines_supported_from_file.Length)
                    payline_renderer_manager.ShowPayline(paylines_supported_from_file[payline_to_show]);
            }
            else if(root_payline_nodes.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < GetSupportedGeneratedPaylines()) // TODO have a number of valid paylines printed
                {
                    
                }
            }
        }

        private int GetSupportedGeneratedPaylines()
        {
            int number_of_paylines = 0;
            for (int i = 0; i < root_payline_nodes.Length; i++)
            {
                number_of_paylines += GetPossiblePaylineCombinations(ref root_payline_nodes[i]);
            }
            return number_of_paylines;
        }

        private int GetPossiblePaylineCombinations(ref suffix_tree_node suffix_tree_node)
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

        internal void CancelCycleWins()
        {
            Debug.Log("Canceling Cycle Wins");
            cycle_paylines = false;
            StopAllCoroutines();
        }

        public IEnumerator ShowWinningPayline(WinningPayline payline_to_show)
        {
            payline_renderer_manager.ShowPayline(payline_to_show.payline);
            yield return matrix.SetSymbolsForWinConfigurationDisplay(payline_to_show);
        }

        public void SetReelConfiguration()
        {
            
        }
        public void EvaluateWinningSymbols()
        {
            EvaluateWinningSymbols(matrix.end_configuration_manager.current_reelstrip_configuration);
        }

        internal void EvaluateWinningSymbols(ReelStripsStruct ending_reelstrips)
        {
            int[][] symbols_configuration = new int[ending_reelstrips.reelstrips.Length][];
            for (int reel = 0; reel < ending_reelstrips.reelstrips.Length; reel++)
            {
                symbols_configuration[reel] = ending_reelstrips.reelstrips[reel].display_symbols;
            }
            EvaluateWinningSymbols(symbols_configuration); //TODO Determine if Bonus or Special symbols were triggered
        }

        public void EvaluateWinningSymbols(int[][] symbols_configuration)
        {
            //Initialize variabled needed for caching
            List<WinningPayline> payline_won = new List<WinningPayline>();
            List<int> symbols_in_row = new List<int>();
            List<int> matching_symbols_list;
            int primary_symbol_index;//index for machine_symbols_list with the symbol to check for in the payline.

            //Iterate through each payline and check for a win 
            for (int payline = active_payline_range_lower; payline < active_payline_range_upper; payline++)
            {
                //Gather raw symbols in row
                GetSymbolsOnPayline(payline, ref symbols_configuration, out symbols_in_row);

                //Initialize variabled needed for checking symbol matches and direction
                InitializeMachingSymbolsVars(0, symbols_in_row[0], out matching_symbols_list, out primary_symbol_index);
                CheckSymbolsMatchLeftRight(true, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index, ref payline, ref payline_won);
                //Time to check right to left
                InitializeMachingSymbolsVars(symbols_in_row.Count - 1, symbols_in_row[symbols_in_row.Count - 1], out matching_symbols_list, out primary_symbol_index);
                CheckSymbolsMatchLeftRight(false, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index, ref payline, ref payline_won);
            }
            if (payline_won.Count > 0)
            {
                winning_paylines = payline_won.ToArray();
                matrix.SetSystemToPresentWin();
            }
            paylines_evaluated = true;
        }
        internal void PlayCycleWins()
        {
            cycle_paylines = true;
            current_winning_payline_shown = -1;
            payline_renderer_manager.ToggleRenderer(true);
            StartCoroutine(ShowWinningPayline());

        }

        private IEnumerator ShowWinningPayline()
        {
            current_winning_payline_shown = -1;
            while (cycle_paylines)
            {
                yield return CyclePaylines();
                current_winning_payline_shown += 1;
            }
        }

        private IEnumerator CyclePaylines()
        {
            //matrix.InitializeSymbolsForWinConfigurationDisplay();
            //yield return new WaitForSeconds(delay_between_wininng_payline/2);
            Debug.Log("Showing Payline");
            yield return ShowWinningPayline(current_winning_payline_shown + 1 < winning_paylines.Length ? current_winning_payline_shown + 1 : 0);
            Debug.Log(String.Format("Waiting for {0} seconds", wininng_payline_highlight_time));
            yield return new WaitForSeconds(wininng_payline_highlight_time);
            Debug.Log("Hiding Payline");
            yield return HideWinningPayline();
            Debug.Log(String.Format("Delaying for {0} seconds", delay_between_wininng_payline));
            yield return new WaitForSeconds(delay_between_wininng_payline);
        }

        private IEnumerator HideWinningPayline()
        {
            yield return matrix.InitializeSymbolsForWinConfigurationDisplay();
        }

        private IEnumerator ShowWinningPayline(int v)
        {
            current_winning_payline_shown = v;
            Debug.Log(String.Format("Current wining payline shown = {0}",v));
            yield return ShowWinningPayline(winning_paylines[current_winning_payline_shown]);
        }

        private void CheckSymbolsMatchLeftRight(bool left_right, ref List<int> symbols_in_row, ref List<int> symbols_list, ref int primary_symbol_index, ref int payline, ref List<WinningPayline> payline_won)
        {
            for (int symbol = left_right ? 1 : symbols_in_row.Count - 2;
                left_right ? symbol < symbols_in_row.Count : symbol >= 0;
                symbol += left_right ? 1 : -1)
            {
                if (!CheckSymbolsMatch(symbols_in_row[primary_symbol_index], symbols_in_row[symbol]))
                {
                    break;
                }
                else
                {

                    if((Symbol)symbols_in_row[primary_symbol_index] == Symbol.SA01)
                    {
                        if((Symbol)symbols_in_row[symbol] != Symbol.SA01)
                        {
                            primary_symbol_index = symbol;
                        }
                    }
                    symbols_list.Add(symbols_in_row[symbol]);
                }
            }
            if (symbols_list.Count > 2)
            {
                AddWinningPayline(payline, symbols_list, left_right, ref payline_won);
            }
        }

        private void GetSymbolsOnPayline(int payline, ref int[][] symbols_configuration, out List<int> symbols_in_row)
        {
            //TODO Check Symbol Configuration Reels are length of payline
            symbols_in_row = new List<int>();
            Payline currentPayline = paylines_supported_from_file[payline];
            if (currentPayline.payline_configuration.payline.Length != symbols_configuration.Length)
                Debug.LogWarning(String.Format("currentPayline.payline.Length = {0} symbols_configuration.Length = {1}", currentPayline.payline_configuration.payline.Length, symbols_configuration.Length));

            for (int reel = 0; reel < symbols_configuration.Length; reel++)
            {
                try
                {
                    //Get Symbol on payline 
                    symbols_in_row.Add(symbols_configuration[reel][currentPayline.payline_configuration.payline[reel]]);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }

        private void AddWinningPayline(int payline, List<int> matching_symbols_list, bool left_right, ref List<WinningPayline> payline_won)
        {
            List<string> symbol_names = new List<string>();
            for (int i = 0; i < matching_symbols_list.Count; i++)
            {
                symbol_names.Add(((Symbol)matching_symbols_list[i]).ToString());
            }
            Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match {2}", payline, left_right ? "left":"right", String.Join(" ", symbol_names)));

            payline_won.Add(new WinningPayline(paylines_supported_from_file[payline], matching_symbols_list.ToArray(), left_right));
        }
        private bool CheckSymbolsMatch(int primary_symbol, int symbol_to_check)
        {
            //Now see if the next symbol is a wild or the same as the primary symbol. check false condition first
            if (symbol_to_check == (int)Symbol.SA01 || primary_symbol == (int)Symbol.SA01 || primary_symbol == symbol_to_check) // Wild symbol - look to match next symbol to wild or set symbol 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckNextSymbolWild(int v1, int v2)
        {
            if (v1 == (int)Symbol.SA01)
            {
                if (v2 != (int)Symbol.SA01)
                {
                    return true;
                }
            }
            return false;
        }

        private void InitializeMachingSymbolsVars(int primary_symbol_index, int symbol_to_match, out List<int> matching_symbols_list, out int primary_symbol_index_out)
        {
            matching_symbols_list = new List<int>();
            matching_symbols_list.Add(symbol_to_match);
            primary_symbol_index_out = primary_symbol_index;
        }

        internal void ClearWinningPaylines()
        {
            winning_paylines = new WinningPayline[0];
            paylines_evaluated = false;
        }

        /// <summary>
        /// This will animate the Payline. (May need to cache slots into Event and fire event off. Find out in Optimization Phase)
        /// </summary>
        /// <param name="iPayline"> Defines the Payline. Length of input should be reel length</param>
        public void AnimateSymboldOnPayLine(int[] iPayline)
        {
            for (int i = 0; i < iPayline.Length; i++)
            {
                matrix.reel_strip_managers[i].slots_in_reel[iPayline[i]].PlayAnimation();
            }
        }

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
        }

        private void StateManager_StateChangedTo(States state)
        {
            switch (state)
            {
                case States.None:
                    break;
                case States.preloading:
                    break;
                case States.Coin_In:
                    break;
                case States.Coin_Out:
                    break;
                case States.Idle_Intro:
                    break;
                case States.Idle_Idle:
                    break;
                case States.Idle_Outro:
                    payline_renderer_manager.ToggleRenderer(false);
                    cycle_paylines = false;
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_End:
                    break;
                case States.Resolve_Intro:
                    break;
                case States.win_presentation:
                    break;
                case States.racking_start:
                    break;
                case States.racking_loop:
                    break;
                case States.racking_end:
                    break;
                case States.feature_transition_out:
                    break;
                case States.feature_transition_in:
                    break;
                case States.total_win_presentation:
                    break;
                default:
                    break;
            }
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }

        void OnApplicationQuit()
        {
            //Reset All Tasks
            cycle_paylines = false;
            //TODO Task Managment system
            //cycle_paylines_task?.Dispose();
        }

        internal WinningPayline GetWinningPayline(int winning_payline_to_show)
        {
            return winning_paylines[winning_payline_to_show];
        }

        internal void GeneratePaylines()
        {
            GeneratePaylinesFromMatrix(new Matrix_Settings(5,3));
        }

        internal void GeneratePaylinesFromMatrix(Matrix_Settings matrix)
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
            List<suffix_tree_node> paylines = new List<suffix_tree_node>();
            List<suffix_tree_node> finished_list = new List<suffix_tree_node>();
            
            //Initialize and Generate Paylines Left to Right
           int column = 0;
            int rows_in_column = matrix.slots_per_reel[0];

            number_of_paylines = 0;

            root_payline_nodes = InitializeRootNodes(column, rows_in_column).ToArray();
            List<suffix_tree_node> to_finish_list = new List<suffix_tree_node>();

            for (int root_node = 0; root_node < root_payline_nodes.Length; root_node++)
            {
                //Start a new payline that is going to be printed per root node
                List<int> payline = new List<int>();
                //Build all paylines
                BuildPayline(ref payline,ref root_payline_nodes[root_node],ref matrix.reels,ref matrix.slots_per_reel);
                //Remove any payline information added in build payline

            }
        }
        public int number_of_paylines = 0;
        internal void BuildPayline(ref List<int> payline, ref suffix_tree_node node, ref int reels, ref int[] slots_per_reel)
        {
            //Add current node to payline
            payline.Add(node.node_info.primary_node);
            //Check the column is the last column and continue if it is
            if (node.node_info.column + 1 >= reels)
            {
                Debug.Log("Reached end of payline");
                number_of_paylines += 1;
                Debug.Log(string.Join("|", payline));
            }
            else
            {
                //Initialize each childs tasks and move out
                int next_column = node.node_info.column + 1;
                suffix_tree_node parent_node = node; //First pass thru this will be nothing
                int rows_in_next_column = slots_per_reel[next_column];
                //First in is parent_node = 0 | Children Column = 1 | slots_per_reel = 5
                node.InitializeNextNodes(next_column, rows_in_next_column, ref parent_node);
                for (int child_nodes = 0; child_nodes < node.connected_nodes_struct.Length; child_nodes++)
                {
                    //Now build out the child refs
                    BuildPayline(ref payline, ref node.connected_nodes_struct[child_nodes],ref reels, ref slots_per_reel);
                    //Remove paylien buildup
                    payline.RemoveRange(node.parent_nodes.Length, payline.Count - node.parent_nodes.Length);
                }
            }
        }

        private List<suffix_tree_node> InitializeRootNodes(int column,int rows_in_root_column)
        {
            List<suffix_tree_node> root_nodes = new List<suffix_tree_node>();
            suffix_tree_node node;
            //Initialize all the rows and the next elements
            for (int primary_node = 0; primary_node < rows_in_root_column; primary_node++)
            {
                //Build my node
                node = new suffix_tree_node(primary_node, null,new suffix_tree_node_info(-1,-1), column);
                root_nodes.Add(node);
            }
            return root_nodes;
        }

        private static List<int> GenerateConnectedNodes(int rows_in_next_column, int primary_node)
        {
            List<int> connected_nodes = new List<int>();
            if (primary_node - 1 >= 0)
            {
                connected_nodes.Add(primary_node - 1);
            }
            //if you have a 5x4x3x4x5 then we want to be able to calculate 5 -> 4 but not 4 -> 5
            if (primary_node < rows_in_next_column)
            {
                connected_nodes.Add(primary_node);
            }
            //if you have a 5x4x3x4x5 then we want to be able to calculate 5 -> 4 but not 4 -> 5
            if (primary_node + 1 < rows_in_next_column)
            {
                connected_nodes.Add(primary_node + 1);
            }

            return connected_nodes;
        }

        [Serializable]
        public struct suffix_tree_node_info
        {
            [SerializeField]
            internal int column;
            [SerializeField]
            internal int primary_node;

            public suffix_tree_node_info(int column,int primary_node) : this()
            {
                this.column = column;
                this.primary_node = primary_node;
            }
        }

        [Serializable]
        public struct suffix_tree_node
        {
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
                this.node_info.primary_node = primary_node;

                if(this.parent_nodes == null && parent_nodes == null)
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
            
            internal void InitializeNextNodes(int current_column, int rows_in_column, ref suffix_tree_node parent_node)
            {
                //Start in column 1

                List<suffix_tree_node> children_nodes = new List<suffix_tree_node>();
                List<int> child_nodes = new List<int>();
                //Check if within range of primary node
                if (parent_node.node_info.primary_node == -1)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (parent_node.node_info.primary_node - 1 >= 0)
                    {
                        child_nodes.Add(parent_node.node_info.primary_node - 1);
                        children_nodes.Add(new suffix_tree_node(parent_node.node_info.primary_node - 1, parent_node.parent_nodes,parent_node.node_info, current_column));
                    }
                    if (parent_node.node_info.primary_node < rows_in_column)
                    {
                        child_nodes.Add(parent_node.node_info.primary_node);
                        children_nodes.Add(new suffix_tree_node(parent_node.node_info.primary_node, parent_node.parent_nodes, parent_node.node_info, current_column));
                    }
                    if (parent_node.node_info.primary_node + 1 < rows_in_column)
                    {
                        child_nodes.Add(parent_node.node_info.primary_node + 1);
                        children_nodes.Add(new suffix_tree_node(parent_node.node_info.primary_node + 1, parent_node.parent_nodes, parent_node.node_info, current_column));
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
                output.Add(node.node_info.primary_node);
                for (int parent_node = 0; parent_node < parent_nodes.Length; parent_node++)
                {
                    output.Add(parent_nodes[parent_node].primary_node);
                }
                return output;
            }
        }
    }
}

public struct Matrix_Settings
{
    public int reels;
    //can be individual or overall set
    public int[] slots_per_reel;

    public Matrix_Settings(int reels, int slots_per_reel) : this()
    {
        this.reels = reels;
        this.slots_per_reel = SetSlotsPerReelTo(reels,slots_per_reel);
    }

    private int[] SetSlotsPerReelTo(int reels,int slots_per_reel)
    {
        int[] output = new int[reels];
        for (int reel = 0; reel < output.Length; reel++)
        {
            output[reel] = slots_per_reel;
        }
        return output;
    }
}