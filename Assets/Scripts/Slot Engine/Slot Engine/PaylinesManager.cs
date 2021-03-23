using System;
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
            //EditorApplication.update += EditorUpdate;
        }
        //public void OnDisable()
        //{
        //    //EditorApplication.update -= EditorUpdate;
        //}
        //internal IEnumerator coroutine;
        
        //void EditorUpdate()
        //{
        //    coroutine?.MoveNext();
        //}

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Set Paylines From file"))
            {
                myTarget.SetPaylinesFromFile();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Generate Paylines from Matrix"))
            {
                //todo get matrix from script
                myTarget.GeneratePaylinesFromMatrix(new Matrix_Settings(5, 3));
                serializedObject.ApplyModifiedProperties();
            }

            //Phasing out payline support file
            if (paylines_supported_from_file.arraySize > 0)
            {
                EditorGUILayout.LabelField("Payliens From File Commands");
                payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, paylines_supported_from_file.arraySize - 1);
                if (GUILayout.Button("Show Payline"))
                {
                    myTarget.ShowPaylineFromFileRaw(payline_to_show);
                }
                if (GUILayout.Button("Evaluate Payline"))
                {
                    myTarget.EvaluateWinningSymbolsFromCurrentConfiguration();
                    serializedObject.ApplyModifiedProperties();
                }
                if (GUILayout.Button("Clear Payline supported from file"))
                {
                    paylines_supported_from_file.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (myTarget.dynamic_paylines.paylines_supported.Length > 0)
            {
                EditorGUILayout.LabelField("Dynamic Paylines Commands");
                EditorGUI.BeginChangeCheck();
                payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, myTarget.dynamic_paylines.paylines_supported.Length - 1);
                if (EditorGUI.EndChangeCheck())
                {
                    myTarget.ShowDynamicPaylineRaw(payline_to_show);
                }
                EditorGUILayout.LabelField(String.Format("Currently Showing Payline {0}", payline_to_show));
                if (paylines_evaluated.boolValue)
                {
                    if (winning_paylines.arraySize > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        winning_payline_to_show = EditorGUILayout.IntSlider(winning_payline_to_show, 0, winning_paylines.arraySize - 1);
                        if (EditorGUI.EndChangeCheck())
                        {
                            myTarget.ShowWinningPayline(winning_payline_to_show);
                        }
                        EditorGUILayout.LabelField(String.Format("Currently Showing winning Payline {0}", winning_payline_to_show));
                        if (GUILayout.Button("Clear Winning Paylines"))
                        {
                            myTarget.ClearWinningPaylines();
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Evaluate Paylines"))
                    {
                        myTarget.EvaluateWinningSymbolsFromCurrentConfiguration();
                    }
                }
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            EditorGUILayout.PropertyField(winning_paylines);
            base.OnInspectorGUI();
        }
    }
#endif

    public enum payline_direction
    {
        left,
        right,
        both,
        count
    }
    public class PaylinesManager : MonoBehaviour
    {
        [SerializeField]
        public Payline[] paylines_supported_from_file;
        [SerializeField]
        internal WinningPayline[] winning_paylines;
        public int current_winning_payline_shown = -1;
        public payline_direction evaluation_direction;

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

        public int number_of_paylines = 0;
        public suffix_tree_root_nodes dynamic_paylines;

        public Task cycle_paylines_task;
        internal void SetPaylinesFromFile()
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

        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal float GetTotalWinAmount()
        {
            float output = 0;
            for (int i = 0; i < winning_paylines.Length; i++)
            {
                output += winning_paylines[i].GetTotalWin(matrix.slot_machine_managers.symbols_weights, matrix);
            }
            return output;
        }

        int ReturnLengthStreamReader(StreamReader Reader)
        {
            int i = 0;
            while (Reader.ReadLine() != null) { i++; }
            return i;
        }

        public void ShowPaylineFromFileRaw(int payline_to_show)
        {
            if (paylines_supported_from_file.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < paylines_supported_from_file.Length)
                    payline_renderer_manager.ShowPayline(paylines_supported_from_file[payline_to_show]);
            }
        }

        private int GetSupportedGeneratedPaylines()
        {
            int number_of_paylines = 0;
            for (int i = 0; i < dynamic_paylines.root_nodes.Length; i++)
            {
                number_of_paylines += GetPossiblePaylineCombinations(ref dynamic_paylines.root_nodes[i]);
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
        [ExecuteInEditMode]
        public Task ShowWinningPayline(WinningPayline payline_to_show)
        {
            payline_renderer_manager.ShowWinningPayline(payline_to_show);
            matrix.SetSymbolsForWinConfigurationDisplay(payline_to_show);
            return Task.CompletedTask;
        }

        public void SetReelConfiguration()
        {

        }
        public async void EvaluateWinningSymbolsFromCurrentConfiguration()
        {
            await EvaluateWinningSymbols(matrix.slot_machine_managers.end_configuration_manager.current_reelstrip_configuration);
            paylines_evaluated = true;
        }

        internal Task EvaluateWinningSymbols(ReelStripsStruct ending_reelstrips)
        {
            int[][] symbols_configuration = new int[ending_reelstrips.reelstrips.Length][];
            for (int reel = 0; reel < ending_reelstrips.reelstrips.Length; reel++)
            {
                symbols_configuration[reel] = ending_reelstrips.reelstrips[reel].spin_info.display_symbols;
            }
            EvaluateWinningSymbols(symbols_configuration); //TODO Determine if Bonus or Special symbols were triggered
            return Task.CompletedTask;
        }

        public Task EvaluateWinningSymbols(int[][] symbols_configuration)
        {
            //Initialize variabled needed for caching
            List<WinningPayline> payline_won = new List<WinningPayline>();
            List<int> symbols_in_row = new List<int>();
            List<int> matching_symbols_list;
            int primary_symbol_index;//index for machine_symbols_list with the symbol to check for in the payline.
            winning_paylines = CheckForWinningPaylinesDynamic(ref symbols_configuration);

            //Iterate through each payline and check for a win 
            //for (int payline = active_payline_range_lower; payline < active_payline_range_upper; payline++)
            //{
            //    //Gather raw symbols in row
            //    GetSymbolsOnPayline(payline, ref symbols_configuration, out symbols_in_row);

            //    //Initialize variabled needed for checking symbol matches and direction
            //    InitializeMachingSymbolsVars(0, symbols_in_row[0], out matching_symbols_list, out primary_symbol_index);
            //    CheckSymbolsMatchLeftRight(true, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index, ref payline, ref payline_won);
            //    //Time to check right to left
            //    InitializeMachingSymbolsVars(symbols_in_row.Count - 1, symbols_in_row[symbols_in_row.Count - 1], out matching_symbols_list, out primary_symbol_index);
            //    CheckSymbolsMatchLeftRight(false, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index, ref payline, ref payline_won);
            //}
            if (payline_won.Count > 0)
            {
                winning_paylines = payline_won.ToArray();
                //TODO Remove hard coded reference - should have matrix recieve event
                matrix.SetSystemToPresentWin();
            }
            paylines_evaluated = true;
            return Task.CompletedTask;
        }

        private WinningPayline[] CheckForWinningPaylinesDynamic(ref int[][] symbols_configuration)
        {
            List<WinningPayline> output_raw = new List<WinningPayline>();
            //output = dynamic_paylines.root_nodes[dynamic_paylines.root_nodes.Length-1].InitializeAndCheckForWinningPaylines(ref symbols_configuration);
            //for every root node check for winning payline
            for (int root_node = 0; root_node < dynamic_paylines.root_nodes.Length; root_node++)
            {
                output_raw.AddRange(dynamic_paylines.root_nodes[root_node].InitializeAndCheckForWinningPaylines(ref symbols_configuration));
            }
            return output_raw.ToArray();

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

        internal Task ShowWinningPayline(int v)
        {
            current_winning_payline_shown = v;
            Debug.Log(String.Format("Current wining payline shown = {0}", v));
            ShowWinningPayline(winning_paylines[current_winning_payline_shown]);
            return Task.CompletedTask;
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

                    if ((Symbol)symbols_in_row[primary_symbol_index] == Symbol.SA01)
                    {
                        if ((Symbol)symbols_in_row[symbol] != Symbol.SA01)
                        {
                            primary_symbol_index = symbol;
                        }
                    }
                    symbols_list.Add(symbols_in_row[symbol]);
                }
            }
            if (symbols_list.Count > 2)
            {
                AddFileWinningPayline(payline, symbols_list, left_right, ref payline_won);
            }
        }

        private void GetSymbolsOnPayline(int payline, ref int[][] symbols_configuration, out List<int> symbols_in_row)
        {
            //TODO Check Symbol Configuration Reels are length of payline
            symbols_in_row = new List<int>();
            Payline currentPayline = paylines_supported_from_file.Length > 0 ? paylines_supported_from_file[payline] : dynamic_paylines.paylines_supported[payline];
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

        internal void AddFileWinningPayline(int payline, List<int> matching_symbols_list, bool left_right, ref List<WinningPayline> payline_won)
        {
            List<string> symbol_names = new List<string>();
            for (int i = 0; i < matching_symbols_list.Count; i++)
            {
                symbol_names.Add(((Symbol)matching_symbols_list[i]).ToString());
            }
            Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match {2}", payline, left_right ? "left" : "right", String.Join(" ", symbol_names)));

            //Check if Payline symbol configuration are already the list - keep highest winning payline
            
            payline_won.Add(new WinningPayline(paylines_supported_from_file.Length > 0 ? paylines_supported_from_file[payline] : dynamic_paylines.paylines_supported[payline], matching_symbols_list.ToArray(), left_right));
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
                    payline_renderer_manager.ToggleRenderer(false);
                    cycle_paylines = false;
                    ClearWinningPaylines();
                    break;
                case States.Idle_Idle:
                    break;
                case States.Idle_Outro:
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
            GeneratePaylinesFromMatrix(new Matrix_Settings(5, 3));
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

            number_of_paylines = 0;
            dynamic_paylines.paylines_supported = new Payline[0];

            dynamic_paylines.root_nodes = InitializeRootNodes(ref matrix.slots_per_reel).ToArray();
            List<suffix_tree_node> to_finish_list = new List<suffix_tree_node>();

            for (int root_node = 0; root_node < dynamic_paylines.root_nodes.Length; root_node++)
            {
                //Start a new payline that is going to be printed per root node
                List<int> payline = new List<int>();
                //Build all paylines
                BuildPayline(ref payline, ref dynamic_paylines.root_nodes[root_node], ref matrix.slots_per_reel);
            }
        }

        internal void BuildPayline(ref List<int> payline, ref suffix_tree_node node, ref int[] slots_per_reel)
        {
            //Add current node to payline
            payline.Add(node.node_info.row);
            int next_column = node.left_right ? node.node_info.column + 1 : node.node_info.column - 1;
            //Check the column is the last column and continue if it is
            if (node.left_right ?
                next_column >= slots_per_reel.Length:
                next_column < 0)
            {
                Debug.Log("Reached end of payline");
                dynamic_paylines.AddPaylineSupported(payline.ToArray());
                number_of_paylines += 1;
                Debug.Log(string.Join("|", payline));
            }
            else
            {
                suffix_tree_node parent_node = node; //First pass thru this will be nothing
                int rows_in_next_column = slots_per_reel[next_column];
                //First in is parent_node = 0 | Children Column = 1 | slots_per_reel = 5
                node.InitializeNextNodes(next_column, rows_in_next_column, ref parent_node, node.left_right);
                for (int child_nodes = 0; child_nodes < node.connected_nodes_struct.Length; child_nodes++)
                {
                    //Now build out the child refs
                    BuildPayline(ref payline, ref node.connected_nodes_struct[child_nodes], ref slots_per_reel);
                    //Remove payline buildup
                    payline.RemoveRange(node.parent_nodes.Length, payline.Count - node.parent_nodes.Length);
                }
            }
        }

        private List<suffix_tree_node> InitializeRootNodes(ref int[] slots_per_reel)
        {

            List<suffix_tree_node> root_nodes = new List<suffix_tree_node>();
            suffix_tree_node node;
            //Initialize all the rows and the next elements
            switch (evaluation_direction)
            {
                case payline_direction.left:
                    root_nodes.AddRange(BuildRootNodes(0, slots_per_reel[0], true));
                    break;
                case payline_direction.right:
                    root_nodes.AddRange(BuildRootNodes(slots_per_reel.Length - 1, slots_per_reel[slots_per_reel.Length - 1], false));
                    break;
                case payline_direction.both:
                    root_nodes.AddRange(BuildRootNodes(0, slots_per_reel[0], true));
                    root_nodes.AddRange(BuildRootNodes(slots_per_reel.Length - 1, slots_per_reel[slots_per_reel.Length - 1], false));
                    break;
                default:
                    Debug.Log("Please set the evaluation direciton to left, right or both");
                    break;
            }
            return root_nodes;
        }

        private List<suffix_tree_node> BuildRootNodes(int column, int rows_in_root_column, bool left_right)
        {
            List<suffix_tree_node> root_nodes = new List<suffix_tree_node>();
            suffix_tree_node node;
            for (int row = 0;row < rows_in_root_column;row += 1)
            {
                //Build my node
                node = new suffix_tree_node(row, null, new suffix_tree_node_info(-1, -1), column,left_right);
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

        internal void ClearPaylinesSupportedFromFile()
        {
            paylines_supported_from_file = null;
        }

        internal void ShowDynamicPaylineRaw(int payline_to_show)
        {
            if (dynamic_paylines.root_nodes.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < GetSupportedGeneratedPaylines()) // TODO have a number of valid paylines printed
                {
                    payline_renderer_manager.ShowPayline(dynamic_paylines.ReturnPayline(payline_to_show));
                }
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

            internal void AddPaylineSupported(int[] vs)
            {
                if (paylines_supported == null)
                    paylines_supported = new Payline[0];
                paylines_supported = paylines_supported.AddAt<Payline>(paylines_supported.Length,new Payline(vs));
            }
        }

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
            internal WinningPayline[] InitializeAndCheckForWinningPaylines(ref int[][] symbols_configuration)
            {
                Debug.Log("Initialize check for winning paylines");
                //Initialize Winning Symbol List
                List<SymbolWinStruct> winning_symbols = new List<SymbolWinStruct>();
                int primary_linewin_symbol = symbols_configuration[node_info.column][node_info.row];
                //Add the first symbol to the list
                AddWinningSymbol(primary_linewin_symbol, ref winning_symbols,ref node_info);

                //Initialize Winning Paylines
                List<WinningPayline> winning_paylines = new List<WinningPayline>();
                Debug.Log(String.Format("Starting check for winning paylines from node {0}", node_info.Print()));
                //Check all connected nodes for a win using dfs (depth first search) search
                CheckConnectedNodes(ref node_info,ref connected_nodes_struct, ref symbols_configuration, ref winning_symbols, ref winning_paylines, primary_linewin_symbol);
                return winning_paylines.ToArray();
            }

            private void CheckConnectedNodes(ref suffix_tree_node_info current_node,ref suffix_tree_node[] connected_nodes_struct, ref int[][] symbols_configuration, ref List<SymbolWinStruct> winning_symbols, ref List<WinningPayline> winning_paylines, int symbol_to_check_for)
            {
                //if primary_linewin_symbol is a wild then use the next symbol in sequence - if next symbol is a wild then continue
                //Cycle thru each connected node for a winning payline
                for (int connected_node = 0; connected_node < connected_nodes_struct.Length; connected_node++)
                {
                    Debug.Log(String.Format("Checking Connected node {0} from {1}", connected_nodes_struct[connected_node].node_info.Print(),
                        current_node.Print()));
                    //reference list
                    CheckForDynamicWinningPaylines(ref connected_nodes_struct[connected_node], ref symbols_configuration, ref winning_symbols, symbol_to_check_for, ref winning_paylines);
                    //if connected nodes are the same leading up to the end winning symbol use the largest list length
                }
            }

            /// <summary>
            /// Used for recursive check of suffix tree to evaluate winning paylines
            /// </summary>
            /// <param name="suffix_tree_node">Node being checked</param>
            /// <param name="symbols_configuration">symbols configuration to check against</param>
            /// <param name="winning_symbols">winning symbols list</param>
            private void CheckForDynamicWinningPaylines(ref suffix_tree_node suffix_tree_node, ref int[][] symbols_configuration, ref List<SymbolWinStruct> winning_symbols, int symbol_to_check_for, ref List<WinningPayline> winning_paylines)
            {
                Debug.Log(String.Format("Checking node {0}", suffix_tree_node.node_info.Print()));
                int current_symbol_to_check = symbols_configuration[suffix_tree_node.node_info.column][suffix_tree_node.node_info.row];

                if (current_symbol_to_check == symbol_to_check_for || current_symbol_to_check == (int)Symbol.SA01 || symbol_to_check_for == (int)Symbol.SA01)
                {
                    if (symbol_to_check_for == (int)Symbol.SA01)
                    {
                        symbol_to_check_for = current_symbol_to_check;
                    }
                    AddWinningSymbol(current_symbol_to_check, ref winning_symbols, ref suffix_tree_node.node_info);
                    int current_index = winning_symbols.Count - 1;
                    //There is a match - move to the next node if the winning symbols don't equal total columns
                    if (winning_symbols.Count < symbols_configuration.Length)
                    {
                        //Check each connected node
                        CheckConnectedNodes(ref suffix_tree_node.node_info, ref suffix_tree_node.connected_nodes_struct, ref symbols_configuration, ref winning_symbols, ref winning_paylines, symbol_to_check_for);
                    }
                    else
                    {
                        //Reached the end of the payline - add this payline and override others - remove symbol and start down next tree
                        InitializeAndAddDynamicWinningPayline(suffix_tree_node, ref winning_symbols, ref winning_paylines);
                    }
                    //Debug.Log(winning_symbols.PrintElements<int>());
                    RemoveWinningSymbol(ref winning_symbols, current_index);
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
                Debug.Log(String.Format("Payline {0} won!", PrintDynamicPayline(ref winning_symbols)));
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
                Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match {2}", payline, left_right ? "left" : "right", String.Join(" ", symbol_names)));
                Payline payline_won = new Payline(payline);
                WinningPayline new_winning_payline = new WinningPayline(payline_won, matching_symbols_list.ToArray(), left_right);

                //If we have a payline that is similiar enough to our current payline to submit then we need to keep highest value payline
                WinningPayline duplicate_payline;
                //Check if Payline symbol configuration are already the list - keep highest winning payline
                if (IsWinningPaylineInList(new_winning_payline, ref winning_paylines, out duplicate_payline))
                {
                    if (duplicate_payline != new_winning_payline)
                    {
                        Debug.Log(String.Format("New winning payline {0} is higher value than a payline already in the list {1}", 
                            string.Join("|", new_winning_payline.payline.payline_configuration.payline),
                            string.Join("|", duplicate_payline.payline.payline_configuration.payline)));
                        winning_paylines.Remove(duplicate_payline);
                        winning_paylines.Add(new_winning_payline);
                    }
                    else
                    {
                        Debug.Log(String.Format("New winning payline {0} is lower value or already in the list. Not adding to list", string.Join("|", new_winning_payline.payline.payline_configuration.payline)));
                    }
                }
                else
                {
                    Debug.Log(String.Format("adding winning payline {0}", string.Join("|", new_winning_payline.payline.payline_configuration.payline)));
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
                            if (column == shortest_payline_configuration.Length-1)
                            {
                                //Check for largest payline configuration and keep highest
                                if (new_winning_payline_configuration.Length > list_entry_winning_payline_configuration.Length)
                                {
                                    Debug.Log("Duplicate reference = winning_paylines[winning_payline]");
                                    duplicate_payline_reference = winning_paylines[winning_payline];
                                    //We have a similar payline - keep the one with highest value
                                    return true;
                                }
                                else
                                {
                                    Debug.Log("Duplicate reference = new_winning_payline");
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
                Debug.Log("No Duplicate found");
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
                    String.Join("|",payline),
                    String.Join("|", winning_symbol_row)
                    );
            }

            private void RemoveWinningSymbol(ref List<SymbolWinStruct> winning_symbols, int index)
            {
                Debug.Log(String.Format("Removing winning symbol {0}", winning_symbols[index]));
                winning_symbols.RemoveAt(index);
            }

            /// <summary>
            /// Adds a winning symbol to track for dynamic payline evaluation
            /// </summary>
            /// <param name="symbol">symbol to add</param>
            /// <param name="winning_symbols">winning symbols reference list</param>
            private void AddWinningSymbol(int symbol, ref List<SymbolWinStruct> winning_symbols, ref suffix_tree_node_info suffix_tree_node_info)
            {
                Debug.Log(String.Format("Adding winning symbol {0} from node {1}", symbol, suffix_tree_node_info.Print()));
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
    }
}