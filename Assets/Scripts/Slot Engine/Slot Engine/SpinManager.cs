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
                    myTarget.SetSpinStateTo(SpinStates.spin_start);
                }
                if (GUILayout.Button("Start Test Spin - Bonus Trigger"))
                {
                    myTarget.TriggerFeatureWithSpin(Features.freespin);
                }
                if (GUILayout.Button("Start Test Spin - Overlay Trigger"))
                {
                    myTarget.TriggerFeatureWithSpin(Features.overlay);
                }
                if (GUILayout.Button("Start Test Spin - MI01 MI02 5 each"))
                {
                    myTarget.TriggerSpinWin(new int[2] { (int)Symbol.MI01, (int)Symbol.MI02},3);
                }
                if (GUILayout.Button("Test Spin - Last Spin Configuration"))
                {
                    myTarget.SetReelsLastConfigurationAndSpin();
                }
                if (GUILayout.Button("End Test Spin"))
                {
                    myTarget.SetSpinStateTo(SpinStates.spin_outro);
                }
            }
            base.OnInspectorGUI();
        }
    }
#endif

    public class SpinManager : MonoBehaviour
    {
        public Matrix matrix
        {
            get
            {
                if (managers == null)
                    managers = transform.GetComponentInParent<ManagersReferenceScript>();
                return managers.matrix;
            }
        }
        public SpinSettingsScriptableObject spinSettingsScriptableObject;
        public ManagersReferenceScript managers;
        [SerializeField]
        private InteractionController controller;
        //TODO - Define reel strip length per reel - 50 - define time to traverse reel strip - speed is calculated based on traverse time - On Outro set speed to outro traverse time - 50% 
        //Hit spin - spin for 2 seconds - after lapse then land on symbol you need
        //Instead of stiching in reelstrips - see numbers flying by on the reelstrip
        public bool spin_enabled;
        /// <summary>
        /// Can we slam
        /// </summary>
        public bool slam_enabled;
        /// <summary>
        /// Use a timer to stop the reels
        /// </summary>
        [SerializeField]
        private bool use_timer = false;
        /// <summary>
        /// Counter used to measure time passed in loop state
        /// </summary>
        [SerializeField]
        private float time_counter = 0.0f;
        /// <summary>
        /// For reference only to what state our spin manager is in
        /// </summary>
        public SpinStates current_state;
        

        void Update()
        {
            if (use_timer)
            {
                if (StateManager.enCurrentState == States.Spin_Idle)
                {
                    time_counter += Time.deltaTime;
                    if (time_counter > spinSettingsScriptableObject.spin_loop_until_seconds_pass)
                    {
                        ResetUseTimer();
                        InterruptSpin();
                    }
                }
                else
                {
                    if (time_counter > 0)
                        time_counter = 0;
                }
            }
        }

        private void ResetUseTimer()
        {
            time_counter = 0;
            use_timer = false;
        }
        /// <summary>
        /// Interrupts the spin and sets to spin outro state
        /// </summary>
        internal async void InterruptSpin()
        {
            matrix.SetAllAnimatorsTriggerTo(supported_triggers.SpinSlam, true);
            await matrix.isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
            await matrix.isAllSlotAnimatorsReady("Spin_Outro");
            StateManager.SetStateTo(States.Spin_Outro);
        }

        //Engine Functions
        /// <summary>
        /// Start spinning the reels
        /// </summary>
        /// <returns></returns>
        public async Task StartSpinReels()
        {
            await matrix.SpinReels();
        }
        /// <summary>
        /// Stop the reels - include display reel highlight if the feature is toggled
        /// </summary>
        internal async Task ReelsStopSpinning()
        {
            await matrix.StopReels();
        }
        

        internal void TriggerFeatureWithSpin(Features feature)
        {
            //Add configuration to the sequence to trigger feature
            matrix._slot_machine_managers.end_configuration_manager.AddConfigurationToSequence(feature);
            //Go through interaction controller to disable slamming during transition to idle_outro
            matrix.slot_machine_managers.interaction_controller.CheckStateToSpinSlam();
        }

        internal void SetReelsLastConfigurationAndSpin()
        {
            //Add configuration to the sequence to trigger feature
            matrix._slot_machine_managers.end_configuration_manager.AddConfigurationToSequence(GameStates.baseGame,matrix.slot_machine_managers.end_configuration_manager.endConfigurationsScriptableObject.currentReelstripConfiguration);
            //Go through interaction controller to disable slamming during transition to idle_outro
            matrix.slot_machine_managers.interaction_controller.CheckStateToSpinSlam();
        }

        internal void TriggerSpinWin(int[] symbols, int numberOfSymbols)
        {
            ReelStripSpinStruct[] configuration = new ReelStripSpinStruct[0];
            configuration = new ReelStripSpinStruct[matrix.reel_strip_managers.Length];
            for (int i = 0; i < configuration.Length; i++)
            {
                configuration[i].display_symbols = new SlotDisplaySymbol[3]
                {
                            new SlotDisplaySymbol(symbols[0]),
                            new SlotDisplaySymbol(symbols[1]),
                            new SlotDisplaySymbol(UnityEngine.Random.Range(0,9))
                };
            }
                matrix._slot_machine_managers.end_configuration_manager.AddConfigurationToSequence(GameStates.baseGame, configuration);
            matrix.slot_machine_managers.interaction_controller.CheckStateToSpinSlam();

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
                case States.bonus_idle_outro:
                    //Wait for animator to play all idle outro animations then continue with spin.
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
                    //SetSpinStateTo(SpinStates.spin_start);
                    break;
                case States.bonus_idle_idle:
                    SetSpinStateTo(SpinStates.idle_idle);
                    break;
                case States.bonus_spin_intro:
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
                    spin_enabled = true;
                    break;
                case SpinStates.spin_start:
                    Debug.Log("Starting Spin - waiting for Idle_Outro");
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
                    Debug.Log("Setting Animation Trigger");
                    matrix.SetAllAnimatorsTriggerTo(supported_triggers.SpinStart, true);
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Spin_Idle");
                    await matrix.isAllSlotAnimatorsReady("Spin_Idle");
                    StateManager.SetStateTo(States.Spin_Intro);
                    //Start the reels spinning
                    await StartSpinReels();
                    if (!StateManager.isInterupt)
                        StateManager.SetStateTo(States.Spin_Idle);
                    break;
                case SpinStates.spin_intro:
                    break;
                case SpinStates.spin_idle:
                    //Debug.Log("Using Timer");
                    use_timer = true;
                    break;
                case SpinStates.spin_interrupt:
                    InterruptSpin();
                    break;
                case SpinStates.spin_outro:
                    ResetUseTimer();
                    await ReelsStopSpinning();
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
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
