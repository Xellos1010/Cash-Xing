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
        public Payline[] paylines_supported;
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
            paylines_supported = paylineListOutput.ToArray();
        }

        int ReturnLengthStreamReader(StreamReader Reader)
        {
            int i = 0;
            while (Reader.ReadLine() != null) { i++; }
            return i;
        }

        public void ShowPaylineRaw(int payline_to_show)
        {
            if (payline_to_show >= 0 && payline_to_show < paylines_supported.Length)
                payline_renderer_manager.ShowPayline(paylines_supported[payline_to_show]);
        }

        internal void CancelCycleWins()
        {
            Debug.Log("Canceling Cycle Wins");
            cycle_paylines = false;
            StopAllCoroutines();
        }

        public IEnumerator ShowWinningPayline(WinningPayline payline_to_show)
        {
            yield return matrix.SetSymbolsForWinConfigurationDisplay(payline_to_show);
        }

        public void SetReelConfiguration()
        {
            
        }
        public void EvaluateWinningSymbols()
        {
            EvaluateWinningSymbols(matrix.end_configuration_manager.current_reelstrip_configuration);
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
            Payline currentPayline = paylines_supported[payline];
            if (currentPayline.payline.Length != symbols_configuration.Length)
                Debug.LogWarning(String.Format("currentPayline.payline.Length = {0} symbols_configuration.Length = {1}", currentPayline.payline.Length, symbols_configuration.Length));

            for (int reel = 0; reel < symbols_configuration.Length; reel++)
            {
                try
                {
                    //Get Symbol on payline 
                    symbols_in_row.Add(symbols_configuration[reel][currentPayline.payline[reel]]);
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

            payline_won.Add(new WinningPayline(paylines_supported[payline], matching_symbols_list.ToArray(), left_right));
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

    }
}
