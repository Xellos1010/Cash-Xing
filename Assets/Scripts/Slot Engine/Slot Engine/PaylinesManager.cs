using System;
using System.Collections;
using System.Collections.Generic;
//For Parsing Purposes
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static Slot_Engine.Matrix.EndConfigurationManager;
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
        SerializedProperty winning_paylines;
        SerializedProperty paylines_evaluated;
        SerializedProperty dynamic_paylines_evaluation;

        private int payline_to_show;
        private int winning_payline_to_show;
        public void OnEnable()
        {
            myTarget = (PaylinesManager)target;
            winning_paylines = serializedObject.FindProperty("winning_paylines");
            paylines_evaluated = serializedObject.FindProperty("paylines_evaluated");
            dynamic_paylines_evaluation = serializedObject.FindProperty("dynamic_paylines_evaluation");
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if(dynamic_paylines_evaluation.type != null)
            {
                if (GUILayout.Button("Generate Paylines from Matrix"))
                {
                    //todo get matrix from script
                    myTarget.GenerateDynamicPaylinesFromMatrix();
                    serializedObject.ApplyModifiedProperties();
                }

                if (myTarget.dynamic_paylines_evaluation.dynamic_paylines.paylines_supported.Length > 0)
                {
                    EditorGUILayout.LabelField("Dynamic Paylines Commands");
                    EditorGUI.BeginChangeCheck();
                    payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, myTarget.dynamic_paylines_evaluation.dynamic_paylines.paylines_supported.Length - 1);
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
            }
            else
            {
                EditorGUILayout.PropertyField(dynamic_paylines_evaluation);
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

        public PaylinesEvaluationScriptableObject dynamic_paylines_evaluation;

        public Task cycle_paylines_task;

        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal float GetTotalWinAmount()
        {
            float output = 0;
            for (int i = 0; i < winning_paylines.Length; i++)
            {
                output += winning_paylines[i].GetTotalWin(matrix);
            }
            return output;
        }

        int ReturnLengthStreamReader(StreamReader Reader)
        {
            int i = 0;
            while (Reader.ReadLine() != null) { i++; }
            return i;
        }

        private int GetSupportedGeneratedPaylines()
        {
            dynamic_paylines_evaluation.number_of_paylines = 0;
            for (int i = 0; i < dynamic_paylines_evaluation.dynamic_paylines.root_nodes.Length; i++)
            {
                dynamic_paylines_evaluation.number_of_paylines += GetPossiblePaylineCombinations(ref dynamic_paylines_evaluation.dynamic_paylines.root_nodes[i]);
            }
            return dynamic_paylines_evaluation.number_of_paylines;
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

        internal async Task CancelCycleWins()
        {
            Debug.Log("Canceling Cycle Wins");
            cycle_paylines = false;
            await SymbolWinAnimatorsInResolveIntro();
        }

        private async Task SymbolWinAnimatorsInResolveIntro()
        {
            await matrix.WaitForSymbolWinResolveToIntro();
        }

        /// <summary>
        /// Renderes the line for winniing payline
        /// </summary>
        /// <param name="payline_to_show"></param>
        /// <returns></returns>
        [ExecuteInEditMode]
        public Task RenderWinningPayline(WinningPayline payline_to_show)
        {
            payline_renderer_manager.ShowWinningPayline(payline_to_show);
            matrix.SetSymbolsForWinConfigurationDisplay(payline_to_show);
            return Task.CompletedTask;
        }

        public async void EvaluateWinningSymbolsFromCurrentConfiguration()
        {
            //Debug.Log(String.Format("Evaluating Symbols in configuration {0}", matrix.slot_machine_managers.end_configuration_manager.current_reelstrip_configuration.PrintDisplaySymbols()));
            await EvaluateWinningSymbols(matrix.slot_machine_managers.end_configuration_manager.current_reelstrip_configuration);
            paylines_evaluated = true;
        }

        internal Task EvaluateWinningSymbols(ReelStripsStruct reelstrips_configuration)
        {
            ReelSymbolConfiguration[] symbols_configuration = new ReelSymbolConfiguration[reelstrips_configuration.reelstrips.Length];
            for (int reel = 0; reel < reelstrips_configuration.reelstrips.Length; reel++)
            {
                symbols_configuration[reel].SetReelSymbolsTo(reelstrips_configuration.reelstrips[reel].spin_info.display_symbols);
            }
            EvaluateWinningSymbols(symbols_configuration);
            return Task.CompletedTask;
        }
        [Serializable]
        public struct ReelSymbolConfiguration
        {
            [SerializeField]
            public SlotDisplaySymbol[] reel_slots;

            internal void SetReelSymbolsTo(SlotDisplaySymbol[] display_symbols)
            {
                reel_slots = display_symbols;
            }
        }
        public Task EvaluateWinningSymbols(ReelSymbolConfiguration[] symbols_configuration)
        {
            //Initialize variabled needed for caching
            List<int> matching_symbols_list;
            int primary_symbol_index;//index for machine_symbols_list with the symbol to check for in the payline.
            //TODO refactor and make settable by Unity Editor
            Dictionary<Features, List<suffix_tree_node_info>> feature_active_count = new Dictionary<Features, List<suffix_tree_node_info>>();
            winning_paylines = CheckForWinningPaylinesDynamic(ref symbols_configuration, ref feature_active_count);
            Debug.Log(String.Format("feature_active_count.keys = {0}", feature_active_count.Keys.Count));
            Debug.Log(String.Format("Looking for features that activated"));
            foreach (KeyValuePair<Features, List<suffix_tree_node_info>> item in feature_active_count)
            {
                Debug.Log(String.Format("Feature name = {0}, counter = {1}",item.Key.ToString(), item.Value.Count));
                if(item.Key == Features.freespin)
                    if (item.Value.Count > 2)
                        StateManager.SetFeatureActiveTo(Features.freespin, true);
                if (item.Key == Features.multiplier)
                {
                    StateManager.SetFeatureActiveTo(Features.freespin, true);
                    StateManager.AddToMultiplier(item.Value.Count);
                }
            }
            if (winning_paylines.Length > 0)
            {
                matrix.SetSystemToPresentWin();
            }
            paylines_evaluated = true;
            return Task.CompletedTask;
        }

        private WinningPayline[] CheckForWinningPaylinesDynamic(ref ReelSymbolConfiguration[] symbols_configuration, ref Dictionary<Features, List<suffix_tree_node_info>> special_symbols)
        {
            List<WinningPayline> output_raw = new List<WinningPayline>();
            List<WinningPayline> output_filtered = new List<WinningPayline>();
            for (int root_node = 0; root_node < dynamic_paylines_evaluation.dynamic_paylines.root_nodes.Length; root_node++)
            {
                output_raw.AddRange(dynamic_paylines_evaluation.dynamic_paylines.root_nodes[root_node].InitializeAndCheckForWinningPaylines(ref symbols_configuration, ref special_symbols));
                FilterRawOutputForDuplicateRootNodeEntries(ref output_filtered, ref output_raw);
                output_filtered.AddRange(output_raw);
                output_raw.Clear();
                //Debug.Log(String.Format("winning paylines Count = {0} for root_node {1} info = {2}", output_filtered.Count,root_node, dynamic_paylines_evaluation.dynamic_paylines.root_nodes[root_node].node_info.Print()));
            }
            return output_filtered.ToArray();

        }

        private void FilterRawOutputForDuplicateRootNodeEntries(ref List<WinningPayline> output_filtered, ref List<WinningPayline> output_raw)
        {
            List<WinningPayline> duplicate_paylines = new List<WinningPayline>();
            WinningPayline raw_payline;
            for (int payline = 0; payline < output_raw.Count; payline++)
            {
                raw_payline = output_raw[payline];
                //Compare both ends of a line win spanning the reels.length
                if(raw_payline.winning_symbols.Length == matrix.reel_strip_managers.Length)
                {
                    //Check for a duplicate entry already in output filter
                    if(IsFullLineWinInList(raw_payline,ref output_filtered))
                    {
                        duplicate_paylines.Add(raw_payline);
                        //I can either keep the first one or second one at this point
                        Debug.Log("Entry was removed ");
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
                Debug.Log(String.Format("left_root_node_winning_payline = {0} | left_root_node_new_winning_payline = {1} | right_root_node_winning_payline = {2} | right_root_node_new_winning_payline = {3}", left_root_node_winning_payline,left_root_node_new_winning_payline,right_root_node_winning_payline,right_root_node_new_winning_payline));
                if(left_root_node_winning_payline == left_root_node_new_winning_payline && right_root_node_new_winning_payline == right_root_node_winning_payline)
                {
                    return true;
                }
            }
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

        internal void PlayCycleWins()
        {
            cycle_paylines = true;
            current_winning_payline_shown = -1;
            payline_renderer_manager.ToggleRenderer(true);
            StartCoroutine(InitializeAndCycleWinningPaylines());

        }
        /// <summary>
        /// Initializes and Cycles thru winning paylines
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitializeAndCycleWinningPaylines()
        {
            current_winning_payline_shown = -1;
            while (cycle_paylines)
            {
                yield return CycleWinningPaylines();
            }
        }
        /// <summary>
        /// Cycles thru winning paylines
        /// </summary>
        /// <returns></returns>
        private IEnumerator CycleWinningPaylines()
        {
            //matrix.InitializeSymbolsForWinConfigurationDisplay();
            int payline_to_show = current_winning_payline_shown + 1 < winning_paylines.Length ? current_winning_payline_shown + 1 : 0;
            Debug.Log(String.Format("Showing Payline {0}", payline_to_show));
            yield return ShowWinningPayline(payline_to_show);
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
            RenderWinningPayline(winning_paylines[current_winning_payline_shown]);
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
            Payline currentPayline = dynamic_paylines_evaluation.dynamic_paylines.paylines_supported[payline];
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
            
            payline_won.Add(new WinningPayline(dynamic_paylines_evaluation.dynamic_paylines.paylines_supported[payline], matching_symbols_list.ToArray()));
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

        internal void GenerateDynamicPaylinesFromMatrix()
        {
            GenerateDynamicPaylinesFromMatrix(ref matrix.reel_strip_managers);
        }
        /// <summary>
        /// Use a suffix tree to generate paylines
        /// </summary>
        /// <param name="reel_strip_managers"></param>
        internal void GenerateDynamicPaylinesFromMatrix(ref ReelStripManager[] reel_strip_managers)
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

            dynamic_paylines_evaluation.number_of_paylines = 0;
            dynamic_paylines_evaluation.dynamic_paylines.paylines_supported = new Payline[0];

            dynamic_paylines_evaluation.dynamic_paylines.root_nodes = InitializeRootNodes(ref matrix.reel_strip_managers).ToArray();
            List<suffix_tree_node> to_finish_list = new List<suffix_tree_node>();

            for (int root_node = 0; root_node < dynamic_paylines_evaluation.dynamic_paylines.root_nodes.Length; root_node++)
            {
                //Start a new payline that is going to be printed per root node
                List<int> payline = new List<int>();
                //Build all paylines
                BuildPayline(ref payline, ref dynamic_paylines_evaluation.dynamic_paylines.root_nodes[root_node], ref matrix.reel_strip_managers);
            }
        }

        internal void BuildPayline(ref List<int> payline, ref suffix_tree_node node, ref ReelStripManager[] reel_strip_managers)
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
                dynamic_paylines_evaluation.dynamic_paylines.AddPaylineSupported(payline.ToArray(), node.left_right);
                dynamic_paylines_evaluation.number_of_paylines += 1;
            }
            else
            {
                suffix_tree_node parent_node = node; //First pass thru this will be nothing
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

        private List<suffix_tree_node> InitializeRootNodes(ref ReelStripManager[] reel_strip_managers)
        {
            List<suffix_tree_node> root_nodes = new List<suffix_tree_node>();
            suffix_tree_node node;
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
                    root_nodes.AddRange(BuildRootNodes(reel_strip_managers.Length - 1,ref reel_strip_managers[reel_strip_managers.Length - 1], false));
                    break;
                default:
                    Debug.Log("Please set the evaluation direciton to left, right or both");
                    break;
            }
            return root_nodes;
        }

        private List<suffix_tree_node> BuildRootNodes(int column, ref ReelStripManager reel_strip_manager, bool left_right)
        {
            List<suffix_tree_node> root_nodes = new List<suffix_tree_node>();
            suffix_tree_node node;
            //Used to assign each row in the column - active or inactive payline evaluation
            int row = 0;
            for (int display_zone = 0; display_zone < reel_strip_manager.reelstrip_info.display_zones.Length; display_zone++)
            {
                ReelStripStructDisplayZone reel_display_zone = reel_strip_manager.reelstrip_info.display_zones[display_zone];
                if (reel_display_zone.active_payline_evaluations)
                {
                    for (int slot = 0; slot < reel_display_zone.slots_in_reelstrip_zone; slot++)
                    {
                        //Build my node
                        node = new suffix_tree_node(column, row, null, new suffix_tree_node_info(-1, -1), left_right);
                        root_nodes.Add(node);
                        //Debug.Log(String.Format("Registering Root Node {0}", node.node_info.Print()));
                        row += 1;
                    }
                }
                else
                {
                    Debug.Log(String.Format("Non-active pay zone- skipping {0} rows ", reel_display_zone.slots_in_reelstrip_zone));
                    for (int slot = 0; slot < reel_display_zone.slots_in_reelstrip_zone; slot++)
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
                    for (int slot = 0; slot < display_zones[i].slots_in_reelstrip_zone; slot++)
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
                    active_slot += display_zones[i].slots_in_reelstrip_zone;
                }
            }
            return false;
        }

        internal void ShowDynamicPaylineRaw(int payline_to_show)
        {
            if (dynamic_paylines_evaluation.dynamic_paylines.root_nodes.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < GetSupportedGeneratedPaylines()) // TODO have a number of valid paylines printed
                {
                    payline_renderer_manager.ShowPayline(dynamic_paylines_evaluation.dynamic_paylines.ReturnPayline(payline_to_show));
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
    }
}