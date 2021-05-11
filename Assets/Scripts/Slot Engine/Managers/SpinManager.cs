#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Slot_Engine.Matrix.ScriptableObjects;

namespace Slot_Engine.Matrix.Managers
{

#if UNITY_EDITOR
    [CustomEditor(typeof(SpinManager))]
    class SpinManagerEditor : BoomSportsEditor
    {
        SpinManager myTarget;
        public void OnEnable()
        {
            myTarget = (SpinManager)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SpinManager Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);
            EditorGUILayout.EnumPopup(StateManager.enCurrentMode);
            EditorGUILayout.EnumPopup(StateManager.current_feature_active);

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SpinManager Controls");
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start Test Spin"))
                {
                    //This should put the reels into a spin state without relying on the Animator
                    myTarget.DebugSetSpinStateTo(SpinStates.spin_start);
                }
                if (GUILayout.Button("Test Spin - Last Spin Configuration"))
                {
                    myTarget.SetReelsLastConfigurationAndSpin();
                }
                if (GUILayout.Button("End Test Spin"))
                {
                    myTarget.DebugSetSpinStateTo(SpinStates.spin_outro);
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif

    public class SpinManager : MonoBehaviour
    {
        //TODO abstract reference to base configurationObject class
        /// <summary>
        /// Class get for connected Configuration Object
        /// </summary>
        public StripConfigurationObject configurationObject
        {
            get
            {
                if (_configurationObject == null)
                    _configurationObject = GameObject.FindGameObjectWithTag("ConfigurationObject").GetComponent<StripConfigurationObject>();
                return _configurationObject;
            }
        }
        /// <summary>
        /// Reference for configuration object
        /// </summary>
        public StripConfigurationObject _configurationObject;
        /// <summary>
        /// The Base Settings - how long till autospin - timer for spin loop to switch to spin outro - etc...
        /// </summary>
        public BaseSpinSettingsScriptableObject baseSpinSettingsScriptableObject;
        /// <summary>
        /// the interaction controller used to inact a spin - todo remove reference reduce calls between scripts
        /// </summary>
        [SerializeField]
        private InteractionController controller;
        /// <summary>
        /// Are we allows to spin?
        /// </summary>
        public bool spinEnabled;
        /// <summary>
        /// Can we slam
        /// </summary>
        public bool slamEnabled;
        /// <summary>
        /// Use a timer to stop the reels
        /// </summary>
        [SerializeField]
        private bool useTimer = false;
        /// <summary>
        /// Counter used to measure time passed in loop state - TODO refactor into call with clock
        /// </summary>
        [SerializeField]
        internal float timeCounter = 0.0f;
        /// <summary>
        /// For reference only to what state our spin manager is in
        /// </summary>
        public SpinStates current_state;

        void Update()
        {
            if (useTimer && !StateManager.isInterupt)
            {
                if (StateManager.enCurrentState == States.Spin_Idle)
                {
                    timeCounter += Time.deltaTime;
                    if (timeCounter > baseSpinSettingsScriptableObject.spin_loop_until_seconds_pass)
                    {
                        StateManager.SetStateTo(States.Spin_Interrupt);
                    }
                }
                else if (StateManager.enCurrentState == States.bonus_idle_idle)
                {
                    timeCounter += Time.deltaTime;
                    if (timeCounter > 1)
                    {
                        ResetUseTimer();
                        configurationObject.managers.interaction_controller.LockInteractions();
                        configurationObject.managers.interaction_controller.CheckStateToSpinSlam();
                    }
                }
                else
                {
                    if (timeCounter > 0)
                        ResetUseTimer();
                }
            }

            else
            {
                if (timeCounter > 0)
                    ResetUseTimer();
            }
        }

        private void ResetUseTimer()
        {
            timeCounter = 0;
            useTimer = false;
        }
        /// <summary>
        /// Interrupts the spin and sets to spin outro state
        /// </summary>
        internal async Task InterruptSpin()
        {
            configurationObject.SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.SpinSlam, true);
            Debug.Log("Slam Spin Set Waiting for Spin_Outro");
            await configurationObject.isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
            Debug.Log("Waiting for Spin_Outro on all sot animators");
            await configurationObject.isAllSlotAnimatorsReadyAndAtPauseState("Spin_Outro");
            StateManager.SetStateTo(States.Spin_Outro);
        }
        //Engine Functions
        /// <summary>
        /// Start spinning the reels
        /// </summary>
        /// <returns></returns>
        public async Task StartSpinReels()
        {
            await configurationObject.SpinReels();
        }
        /// <summary>
        /// Stop the reels - include display reel highlight if the feature is toggled
        /// </summary>
        internal async Task ReelsStopSpinning()
        {
            await configurationObject.StopReels();
        }
        

        internal void TriggerFeatureWithSpin(Features feature)
        {
            //Add configuration to the sequence to trigger feature
            configurationObject._managers.endConfigurationManager.AddConfigurationToSequence(feature);
            //Go through interaction controller to disable slamming during transition to idle_outro
            configurationObject.managers.interaction_controller.CheckStateToSpinSlam();
        }

        internal void SetReelsLastConfigurationAndSpin()
        {
            //Add configuration to the sequence to trigger feature
            configurationObject._managers.endConfigurationManager.AddConfigurationToSequence(GameModes.baseGame,configurationObject.managers.endConfigurationManager.endConfigurationsScriptableObject.currentReelstripConfiguration);
            //Go through interaction controller to disable slamming during transition to idle_outro
            configurationObject.managers.interaction_controller.CheckStateToSpinSlam();
        }

        internal void TriggerSpinWin(int[] symbols, int numberOfSymbols)
        {
            StripSpinStruct[] configuration = new StripSpinStruct[0];
            configuration = new StripSpinStruct[configurationObject.configurationGroupManagers.Length];
            for (int i = 0; i < configuration.Length; i++)
            {
                configuration[i].displaySymbols = new NodeDisplaySymbol[3]
                {
                    new NodeDisplaySymbol(symbols[0]),
                    new NodeDisplaySymbol(symbols[1]),
                    new NodeDisplaySymbol(UnityEngine.Random.Range(0,9))
                };
            }
                configurationObject.managers.endConfigurationManager.AddConfigurationToSequence(GameModes.baseGame, configuration);
            configurationObject.managers.interaction_controller.CheckStateToSpinSlam();

        }

        //This is where we hook into the state manager and listen for state specific events to sync with.
        //****Unity Default Functions

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }
        //***********
        /// <summary>
        /// Handles interaction with the main state machine. Will send events to Mode Agnostic State Machine that runs the spin manger. same results expected in different modes.
        /// </summary>
        /// <param name="State"></param>
        private async void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Idle:
                    SetSpinStateTo(SpinStates.idle_idle);
                    break;
                case States.Spin_Interrupt:
                    Debug.Log("Spin Controller IsInterupt = true");
                    StateManager.isInterupt = true;
                    SetSpinStateTo(SpinStates.spin_interrupt);
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    SetSpinStateTo(SpinStates.spin_idle);
                    break;
                case States.Spin_Outro:
                    SetSpinStateTo(SpinStates.spin_outro);
                    break;
                case States.Spin_End:
                    SetSpinStateTo(SpinStates.end);
                    break;
                case States.bonus_idle_idle:
                    SetSpinStateTo(SpinStates.idle_idle);
                    useTimer = true;
                    break;
                case States.bonus_spin_loop:
                    SetSpinStateTo(SpinStates.spin_idle);
                    break;
                case States.bonus_spin_outro:
                    SetSpinStateTo(SpinStates.spin_outro);
                    break;
                case States.bonus_spin_end:
                    StateManager.isInterupt = false;
                    SetSpinStateTo(SpinStates.end);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Set behaviour depending on which spin state to enact
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal async void SetSpinStateTo(SpinStates state)
        {
            //Debug.Log(String.Format("current state = {0}",state));
            current_state = state;
            //Debug.Log(String.Format("Setting Spin Manager Spin State to {0}",state.ToString()));            
            switch (state)
            {
                case SpinStates.idle_idle:
                    spinEnabled = true;
                    break;
                case SpinStates.spin_start:
                    Debug.Log("Starting Spin - waiting for Idle_Outro");
                    await configurationObject.isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
                    Debug.Log("Setting Animation Controller to SpinStart");
                    configurationObject.SetAllAnimatorsBoolTo(supportedAnimatorBools.SpinStart, true);
                    await configurationObject.isAllMainAnimatorsThruState("Idle_Outro");
                    await configurationObject.isAllSlotAnimatorsThruState("Idle_Outro");
                    StateManager.SetStateTo(States.Spin_Intro);
                    //Start the reels spinning
                    await StartSpinReels();
                    if (!StateManager.isInterupt)
                        StateManager.SetStateTo(States.Spin_Idle);
                    else
                        StateManager.SetStateTo(States.Spin_Outro);
                    break;
                case SpinStates.spin_intro:
                    break;
                case SpinStates.spin_idle:
                    useTimer = true;
                    break;
                case SpinStates.spin_interrupt:
                    InterruptSpin();
                    break;
                case SpinStates.spin_outro:
                    ResetUseTimer();
                    Debug.Log("Timer Reset");
                    await ReelsStopSpinning();
                    configurationObject.SetAllAnimatorsBoolTo(supportedAnimatorBools.SpinStart, false);
                    Debug.Log("All reels Stopped Spinning");
                    await configurationObject.isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
                    Debug.Log("All Animators resolved spin_outro stateSpinning");
                    StateManager.SetStateTo(States.Spin_End);
                    break;
                case SpinStates.end:
                    StateManager.isInterupt = false;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Set behaviour depending on which spin state to enact
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal async void DebugSetSpinStateTo(SpinStates state)
        {
            //Debug.Log(String.Format("current state = {0}",state));
            current_state = state;
            //Debug.Log(String.Format("Setting Spin Manager Spin State to {0}",state.ToString()));            
            switch (state)
            {
                case SpinStates.idle_idle:
                    spinEnabled = true;
                    break;
                case SpinStates.spin_start:
                    Debug.Log("Starting Spin - Debug - no Animator Hooks");
                    StateManager.SetStateTo(States.Spin_Intro);
                    //Start the reels spinning
                    await StartSpinReels();
                    if (!StateManager.isInterupt)
                        StateManager.SetStateTo(States.Spin_Idle);
                    else
                        StateManager.SetStateTo(States.Spin_Outro);
                    break;
                case SpinStates.spin_intro:
                    break;
                case SpinStates.spin_idle:
                    //Debug.Log("Using Timer");
                    useTimer = true;
                    break;
                case SpinStates.spin_interrupt:
                    InterruptSpin();
                    break;
                case SpinStates.spin_outro:
                    ResetUseTimer();
                    Debug.Log("Timer Reset");
                    await ReelsStopSpinning();
                    configurationObject.SetAllAnimatorsBoolTo(supportedAnimatorBools.SpinStart, false);
                    Debug.Log("All reels Stopped Spinning");
                    StateManager.SetStateTo(States.Spin_End);
                    break;
                case SpinStates.end:
                    StateManager.isInterupt = false;
                    break;
                default:
                    break;
            }
        }
    }
}
