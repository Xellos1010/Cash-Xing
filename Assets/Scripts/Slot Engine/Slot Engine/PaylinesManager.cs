using UnityEngine;

//For Parsing Purposes
using System.IO;
using System.Collections.Generic;
using System;
//************

/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(PaylinesManager))]
    class PayLinesEditor : BoomSportsEditor
    {
        PaylinesManager myTarget;
        SerializedProperty paylines_supported;
        SerializedProperty winning_paylines;
        SerializedProperty paylines_evaluated;
        private int payline_to_show;
        private int winning_payline_to_show;
        public void OnEnable()
        {
            myTarget = (PaylinesManager)target;
            paylines_supported = serializedObject.FindProperty("paylines_supported");
            winning_paylines = serializedObject.FindProperty("winning_paylines");
            paylines_evaluated = serializedObject.FindProperty("paylines_evaluated");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");

            if (GUILayout.Button("Set Paylines"))
            {
                myTarget.SetPaylines();
                serializedObject.ApplyModifiedProperties();
            }
            if (paylines_supported.arraySize > 0)
            {
                payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, paylines_supported.arraySize - 1);
                if (GUILayout.Button("Show Payline"))
                {
                    myTarget.ShowPayline(payline_to_show);
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
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            EditorGUILayout.PropertyField(paylines_supported);
            EditorGUILayout.PropertyField(winning_paylines);
            base.OnInspectorGUI();
        }
    }
#endif


    public class PaylinesManager : MonoBehaviour
    {
        public Payline[] paylines_supported;
        [SerializeField]
        internal WinningPayline[] winning_paylines;
        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal int GetTotalWinAmount()
        {
            int output = 0;
            for (int i = 0; i < winning_paylines.Length; i++)
            {
                output += winning_paylines[i].GetTotalWin(matrix.intWeightedDistributionSymbols);
            }
            return output;
        }

        //The range for active paylines to use when evaluating paylines
        public int active_payline_range_lower = 0;
        public int active_payline_range_upper = 98;
        //**
        public int paylines_active;
        public bool paylines_evaluated = false;
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
        private Matrix matrix
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
            paylines_supported = paylineListOutput.ToArray();
        }

        int ReturnLengthStreamReader(StreamReader Reader)
        {
            int i = 0;
            while (Reader.ReadLine() != null) { i++; }
            return i;
        }

        public void ShowPayline(int payline_to_show)
        {
            if (payline_to_show >= 0 && payline_to_show < paylines_supported.Length)
                payline_renderer_manager.ShowPayline(paylines_supported[payline_to_show]);
        }
        public void ShowWinningPayline(WinningPayline payline_to_show)
        {
            payline_renderer_manager.ShowWinningPayline(payline_to_show);
        }

        public void SetReelConfiguration()
        {
            //matrix.SetSymbolsOnMatrixTo();
        }
        public void EvaluateWinningSymbols()
        {
            //Cycle through paylines active
            //Check for symbols matching both ways.
            int[][] symbols_configuration = new int[5][] {
            new int[3] {0,1,2},
            new int[3] {0,1,2},
            new int[3] {0,1,2},
            new int[3] {0,1,2},
            new int[3] {0,1,2}}; //TODO pull reel configuration from matrix
            EvaluateWinningSymbols(symbols_configuration);//matrix.end_configuration_manager.GetConfigurationToDisplay());
        }

        internal void EvaluateWinningSymbols(ReelStrip[] ending_reelstrips)
        {
            int[][] symbols_configuration = new int[ending_reelstrips.Length][];
            for (int reel = 0; reel < ending_reelstrips.Length; reel++)
            {
                symbols_configuration[reel] = ending_reelstrips[reel].display_symbols;
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
                InitializeMachingSymbolsVars(0, symbols_in_row[0],out matching_symbols_list,out primary_symbol_index);
                CheckSymbolsMatchLeftRight(true, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index,ref payline, ref payline_won);
                //Time to check right to left
                InitializeMachingSymbolsVars(0, symbols_in_row[symbols_in_row.Count - 1], out matching_symbols_list, out primary_symbol_index);
                CheckSymbolsMatchLeftRight(false, ref symbols_in_row, ref matching_symbols_list, ref primary_symbol_index, ref payline, ref payline_won);
            }
            if (payline_won.Count > 0)
            {
                winning_paylines = payline_won.ToArray();
                matrix.animator_state_machine.SetTrigger(supported_triggers.SpinResolve);
                matrix.animator_state_machine.SetBool(supported_bools.WinRacking,true);
            }
            paylines_evaluated = true;
        }

        private void CheckSymbolsMatchLeftRight(bool left_right, ref List<int> symbols_in_row, ref List<int> matching_symbols_list, ref int primary_symbol_index, ref int payline, ref List<WinningPayline> payline_won)
        {
            for (int symbol = left_right ? 1 : symbols_in_row.Count - 2;
                left_right ? symbol < symbols_in_row.Count : symbol >= 0;
                symbol += left_right ? 1 : -1)
            {
                try
                {
                    if (!CheckSymbolsMatch(matching_symbols_list[primary_symbol_index], symbols_in_row[symbol]))
                    {
                        break;
                    }
                    else
                    {
                        //If the primary symbol is a wild then auto match with next symbol. if next symbol regular symbol that becomes primary symbol
                        if (matching_symbols_list[primary_symbol_index] == (int)Symbol.BW01)
                            if (CheckNextSymbolWild(matching_symbols_list[primary_symbol_index], symbols_in_row[symbol]))
                            {
                                primary_symbol_index = symbol;
                            }
                        matching_symbols_list.Add(symbols_in_row[symbol]);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(string.Format("Payline {0} on symbol count {1} failed.", payline, symbol));
                    Debug.LogError(e.Message);
                }
            }
            if (matching_symbols_list.Count > 2)
            {
                AddWinningPayline(payline, matching_symbols_list, left_right, ref payline_won);
            }
        }

        private void GetSymbolsOnPayline(int payline, ref int[][] symbols_configuration, out List<int> symbols_in_row)
        {
            symbols_in_row = new List<int>();
            Payline currentPayline = paylines_supported[payline];
            for (int reel = 0; reel < currentPayline.payline.Length; reel++)
            {
                symbols_in_row.Add(symbols_configuration[reel][currentPayline.payline[reel]]);
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

            payline_won.Add(new WinningPayline(paylines_supported[payline], matching_symbols_list.ToArray(), left_right));
        }
        private bool CheckSymbolsMatch(int primary_symbol, int symbol_to_check)
        {
            //Now see if the next symbol is a wild or the same as the primary symbol. check false condition first
            if (symbol_to_check == (int)Symbol.BW01 || symbol_to_check == primary_symbol) // Wild symbol - look to match next symbol to wild or set symbol 
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
            if (v1 == (int)Symbol.BW01)
            {
                if (v2 != (int)Symbol.BW01)
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
                    payline_renderer_manager.ToggleRenderer(false);
                    break;
                case States.Idle_Outro:
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_End:
                    break;
                case States.win_presentation:
                    payline_renderer_manager.ToggleRenderer(true);
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

        internal WinningPayline GetWinningPayline(int winning_payline_to_show)
        {
            return winning_paylines[winning_payline_to_show];
        }

    }
}
