using Slot_Engine.Matrix.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
//For Parsing Purposes
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
//************
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace Slot_Engine.Matrix.Managers
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PaylinesManager))]
    class PayLinesEditor : BoomSportsEditor
    {
        PaylinesManager myTarget;
        SerializedProperty winning_paylines;

        private int payline_to_show;
        private int winning_payline_to_show;
        public void OnEnable()
        {
            myTarget = (PaylinesManager)target;
            winning_paylines = serializedObject.FindProperty("winning_paylines");
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            base.OnInspectorGUI();
        }
    }
#endif
    public class PaylinesManager : MonoBehaviour
    {
        [SerializeField]
        internal WinningPayline[] winningPaylines;
        public int current_winning_payline_shown = -1;
        //**
        public bool paylines_evaluated = false;
        public bool cycle_paylines = true;
        //TODO Change this to access animator length of state
        public float delay_between_wininng_payline = .5f;
        public float wininng_payline_highlight_time = 2;
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

        public CancellationToken cancelTaskToken;
        public Task cycle_paylines_task;

        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal float GetTotalWinAmount()
        {
            float output = 0;
            for (int i = 0; i < winningPaylines.Length; i++)
            {
                output += winningPaylines[i].GetTotalWin(matrix);
            }
            return output;
        }

        private int GetSupportedGeneratedPaylines()
        {
            return (int)EvaluationManager.GetFirstInstanceCoreEvaluationObject<PaylinesEvaluationScriptableObject>(ref matrix.slotMachineManagers.evaluationManager.coreEvaluationObjects).ReturnEvaluationObjectSupportedRootCount();
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
            //If first time thru then lerp money to bank
            payline_renderer_manager.ShowWinningPayline(payline_to_show);
            matrix.slotMachineManagers.soundManager.PlayAudioForWinningPayline(payline_to_show);
            matrix.SetSymbolsForWinConfigurationDisplay(payline_to_show);
            return Task.CompletedTask;
        }

        //internal async Task EvaluateWinningSymbols(ReelStripSpinStruct[] reelstripsConfiguration)
        //{
        //    ReelSymbolConfiguration[] symbols_configuration = new ReelSymbolConfiguration[reelstripsConfiguration.Length];
        //    for (int reel = 0; reel < reelstripsConfiguration.Length; reel++)
        //    {
        //        symbols_configuration[reel].SetColumnSymbolsTo(reelstripsConfiguration[reel].displaySymbols);
        //    }
        //    await EvaluateWinningSymbols(symbols_configuration);
        //}

        //public async Task EvaluateWinningSymbols(ReelSymbolConfiguration[] symbols_configuration)
        //{
        //    winningPaylines = await matrix.slotMachineManagers.evaluationManager.EvaluateSymbolConfigurationForWinningPaylines(symbols_configuration);
        //    paylines_evaluated = true;
        //}

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
            //On First Pass thru
            //matrix.InitializeSymbolsForWinConfigurationDisplay();
            int payline_to_show = current_winning_payline_shown + 1 < winningPaylines.Length ? current_winning_payline_shown + 1 : 0;
            //Debug.Log(String.Format("Showing Payline {0}", payline_to_show));
            ShowWinningPayline(payline_to_show);
            //Debug.Log(String.Format("Waiting for {0} seconds", wininng_payline_highlight_time));
            yield return new WaitForSeconds(wininng_payline_highlight_time);
            //Debug.Log("Hiding Payline");
            yield return HideWinningPayline();
            //Debug.Log(String.Format("Delaying for {0} seconds", delay_between_wininng_payline));
            yield return new WaitForSeconds(delay_between_wininng_payline);
        }

        private IEnumerator HideWinningPayline()
        {
            yield return matrix.InitializeSymbolsForWinConfigurationDisplay();
        }

        internal Task ShowWinningPayline(int v)
        {
            if (v < winningPaylines.Length)
            {
                current_winning_payline_shown = v;
                //Debug.Log(String.Format("Current wining payline shown = {0}", v));
                RenderWinningPayline(winningPaylines[current_winning_payline_shown]);
            }
            return Task.CompletedTask;
        }

        internal void ClearWinningPaylines()
        {
            winningPaylines = new WinningPayline[0];
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
                case States.Idle_Intro:
                    payline_renderer_manager.ToggleRenderer(false);
                    cycle_paylines = false;
                    ClearWinningPaylines();
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

        PaylinesEvaluationScriptableObject? dynamicPaylineObject;
        //TODO move into Evaluation Manager
        internal void GenerateDynamicPaylinesFromMatrix()
        {
            dynamicPaylineObject = EvaluationManager.GetFirstInstanceCoreEvaluationObject<PaylinesEvaluationScriptableObject>(ref matrix.slotMachineManagers.evaluationManager.coreEvaluationObjects);
            dynamicPaylineObject.GenerateDynamicPaylinesFromMatrix(ref matrix.reel_strip_managers);
        }

        internal void ShowDynamicPaylineRaw(int payline_to_show)
        {
            if (dynamicPaylineObject?.dynamic_paylines.rootNodes.Length > 0)
            {
                if (payline_to_show >= 0 && payline_to_show < GetSupportedGeneratedPaylines()) // TODO have a number of valid paylines printed
                {
                    payline_renderer_manager.ShowPayline(dynamicPaylineObject?.dynamic_paylines.ReturnPayline(payline_to_show));
                }
            }
        }
    }
}