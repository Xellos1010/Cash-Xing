using Slot_Engine.Matrix.ScriptableObjects;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
//************
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Slot_Engine.Matrix.Managers
{
    /// <summary>
    /// Display's Winning Objects and Manages Debug Rendering of Winning Objects and winable configurations. 
    /// </summary>
#if UNITY_EDITOR
    [CustomEditor(typeof(WinningObjectManager))]
    class PayLinesEditor : BoomSportsEditor
    {
        WinningObjectManager myTarget;
        public void OnEnable()
        {
            myTarget = (WinningObjectManager)target;
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
    /// <summary>
    /// Store's Winning Paylines and Manages Rendering. 
    /// </summary>
    public class WinningObjectManager : MonoBehaviour
    {
        [SerializeField]
        internal WinningPayline[] winningObjects
        {
            get
            {
                return configurationObject.managers.evaluationManager.ReturnWinningObjectsAsWinningPaylines();
            }
        }
        public int current_winning_payline_shown = -1;
        //**
        public bool paylines_evaluated = false;
        public bool cycle_paylines = true;
        //TODO Change this to access animator length of state
        public float delayBetweenWinningObjectDisplayed = .5f;
        public float winningObjectDisplayTime = 2;

        [SerializeField]
        internal PaylinesEvaluationScriptableObject? dynamicPaylineObject;
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
        internal StripConfigurationObject configurationObject
        {
            get
            {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<StripConfigurationObject>();
                return _matrix;
            }
        }
        [SerializeField]
        private StripConfigurationObject _matrix;

        public CancellationToken cancelTaskToken;
        public Task cycle_paylines_task;

        /// <summary>
        /// Gets the total amount from wininng paylines
        /// </summary>
        internal float GetTotalWinAmount()
        {
            float output = 0;
            for (int i = 0; i < winningObjects.Length; i++)
            {
                //Debug.Log($"Get Total Win Amount = {output}");
                output += winningObjects[i].GetTotalWin(configurationObject);
            }
            //Debug.Log($"Returning Total Win Amount = {output}");
            return output;
        }

        private int GetSupportedGeneratedPaylines()
        {
            return (int)EvaluationManager.GetFirstInstanceCoreEvaluationObject<PaylinesEvaluationScriptableObject>(ref configurationObject.managers.evaluationManager.coreEvaluationObjects).ReturnEvaluationObjectSupportedRootCount();
        }

        internal async Task CancelCycleWins()
        {
            Debug.Log("Canceling Cycle Wins");
            cycle_paylines = false;
            //Cancel the cycle winning paylines and wait for the animators to return to pause state 
            await SymbolAnimatorsAtEndOfState("Resolve_Intro");
        }

        private async Task SymbolAnimatorsAtEndOfState(string state)
        {
            await configurationObject.WaitForSymbolToResolveState(state);
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
            configurationObject.managers.soundManager.PlayAudioForWinningPayline(payline_to_show);
            configurationObject.SetSymbolsForWinConfigurationDisplay(payline_to_show);
            return Task.CompletedTask;
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
            //On First Pass thru
            //matrix.InitializeSymbolsForWinConfigurationDisplay();
            int payline_to_show = current_winning_payline_shown + 1 < winningObjects.Length ? current_winning_payline_shown + 1 : 0;
            //Debug.Log(String.Format("Showing Payline {0}", payline_to_show));
            ShowWinningPayline(payline_to_show);
            //Debug.Log(String.Format("Waiting for {0} seconds", wininng_payline_highlight_time));
            yield return new WaitForSeconds(winningObjectDisplayTime);
            //Debug.Log("Hiding Payline");
            yield return HideWinningPayline();
            //Debug.Log(String.Format("Delaying for {0} seconds", delay_between_wininng_payline));
            yield return new WaitForSeconds(delayBetweenWinningObjectDisplayed);
        }

        private IEnumerator HideWinningPayline()
        {
            yield return configurationObject.InitializeSymbolsForWinConfigurationDisplay();
        }

        internal Task ShowWinningPayline(int v)
        {
            if (v < winningObjects.Length)
            {
                current_winning_payline_shown = v;
                //Debug.Log(String.Format("Current wining payline shown = {0}", v));
                RenderWinningPayline(winningObjects[current_winning_payline_shown]);
            }
            return Task.CompletedTask;
        }

        internal void ClearWinningPaylines()
        {
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
        //TODO move into Evaluation Manager
        internal void GenerateDynamicPaylinesFromMatrix()
        {
            dynamicPaylineObject = EvaluationManager.GetFirstInstanceCoreEvaluationObject<PaylinesEvaluationScriptableObject>(ref configurationObject.managers.evaluationManager.coreEvaluationObjects);
            dynamicPaylineObject.GenerateDynamicPaylinesFromConfigurationObjectsGroupManagers(ref configurationObject.configurationSettings.displayZones);
        }

        internal void ShowDynamicPaylineRaw(int paylineToShow)
        {
            Debug.Log($"Showing Payline {paylineToShow} - dynamicPaylineObject?.dynamic_paylines.rootNodes.Length = {dynamicPaylineObject?.dynamic_paylines.paylineNodes.Length}");
            if (dynamicPaylineObject?.dynamic_paylines.paylineNodes.Length > 0)
            {
                if (paylineToShow >= 0 && paylineToShow < GetSupportedGeneratedPaylines()) // TODO have a number of valid paylines printed
                {
                    payline_renderer_manager.ShowPayline(dynamicPaylineObject?.dynamic_paylines.ReturnPayline(paylineToShow));
                }
            }
        }
    }
}