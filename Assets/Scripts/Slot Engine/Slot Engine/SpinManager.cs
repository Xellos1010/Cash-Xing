#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(SpinManager))]
    class SpinManagerEditor : BoomSportsEditor
    {
        SpinManager myTarget;
        SerializedProperty state;
        SerializedProperty reel_spin_delay_start_enabled;
        SerializedProperty reel_spin_delay_end_enabled;
        SerializedProperty spin_reels_starting_forward_back;
        SerializedProperty reel_spin_delay_ms;
        SerializedProperty spin_speed;
        SerializedProperty spin_direction;
        public void OnEnable()
        {
            myTarget = (SpinManager)target;
            reel_spin_delay_start_enabled = serializedObject.FindProperty("reel_spin_delay_start_enabled");
            reel_spin_delay_end_enabled = serializedObject.FindProperty("reel_spin_delay_end_enabled");
            spin_reels_starting_forward_back = serializedObject.FindProperty("spin_reels_starting_forward_back");
            reel_spin_delay_ms = serializedObject.FindProperty("reel_spin_delay_ms");
            spin_speed = serializedObject.FindProperty("spin_speed");
            spin_direction = serializedObject.FindProperty("spin_direction");
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SpinManager Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);

            EditorGUI.BeginChangeCheck();
            spin_reels_starting_forward_back.boolValue = EditorGUILayout.Toggle("Toggle Spin Forward Or Back- On Forward off Backward", spin_reels_starting_forward_back.boolValue);
            reel_spin_delay_start_enabled.boolValue = EditorGUILayout.Toggle("Toggle Reel Spin Delay On Start", reel_spin_delay_start_enabled.boolValue);
            reel_spin_delay_end_enabled.boolValue = EditorGUILayout.Toggle("Toggle Reel Spin Delay On End", reel_spin_delay_start_enabled.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.BeginChangeCheck();
            spin_speed.floatValue = EditorGUILayout.Slider("Set Spin Speed", spin_speed.floatValue,-1000,1000);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                myTarget.SetSpinSpeedTo(spin_speed.floatValue);
            }
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
        public bool spin_slam_enabled;
        /// <summary>
        /// Cascading starting reels
        /// </summary>
        public bool reel_spin_delay_start_enabled = false;
        /// <summary>
        /// Cascading ending reels
        /// </summary>
        public bool reel_spin_delay_end_enabled = false;
        /// <summary>
        /// This allows you to set the reels spin either forward or back (Left to right - right to left - top to bottom - bottom to top)
        /// </summary>
        public bool spin_reels_starting_forward_back = true;
        /// <summary>
        /// The constant rate the slots should spin at
        /// </summary>
        public float spin_speed = 50;
        /// <summary>
        /// Timer used to start each free spin
        /// </summary>
        public float timer_to_start_free_spin = 2.0f;
        /// <summary>
        /// What direction should the slots spin? will reposition slots based on direction - please use -1, 0 or 1
        /// </summary>
        public Vector3 spin_direction = Vector3.down; // Initial state is down
        /// <summary>
        /// Spin the slot machine until seconds pass
        /// </summary>
        public float spin_loop_until_seconds_pass = 5;
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
        /// <summary>
        /// uses predefeined reelstrips to loop thru on spin loop
        /// </summary>
        [SerializeField]
        internal bool use_reelstrips_for_spin_loop = true;
        [SerializeField]
        internal bool isInterrupted = false;

        void Update()
        {
//#if UNITY_ANDROID || UNITY_IPHONE
//            if (Input.touchCount > 0)
//            {
//                Touch temp = Input.touches[0];
//                if (temp.phase == TouchPhase.Began)
//                {
//                    CheckSpinEnabled();
//                }
//            }
//#else
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                CheckSpinEnabled();
//            }
//#endif

            if (use_timer)
            {
                if (StateManager.enCurrentState == States.Spin_Idle)
                {
                    time_counter += Time.deltaTime;
                    if (time_counter > spin_loop_until_seconds_pass)
                    {
                        ResetUseTimer();
                        controller.SlamSpin();
                        StateManager.SetStateTo(States.Spin_Outro);//matrix.bonus_game_triggered ? States.bonus_spin_outro : States.Spin_Outro);
                    }
                }
                else
                {
                    if (time_counter > 0)
                        time_counter = 0;
                    if(isInterrupted)
                    {
                        ResetUseTimer();
                        controller.SlamSpin();
                        StateManager.SetStateTo(States.Spin_Outro);//matrix.bonus_game_triggered ? States.bonus_spin_outro : States.Spin_Outro);
                    }
                }
            }
        }

        private void ResetUseTimer()
        {
            time_counter = 0;
            use_timer = false;
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
                    StateManager.SetStateTo(States.Spin_Intro);
                    //Start the reels spinning
                    await StartSpinReels();
                    if(!isInterrupted)
                        StateManager.SetStateTo(States.Spin_Idle);
                    else
                        StateManager.SetStateTo(States.Spin_Outro);
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
                    isInterrupted = false;
                    await StopReelsSpinning();
                    //Debug.Log("All reels have stopped - setting state to spin end");
                    StateManager.SetStateTo(States.Spin_End);
                    break;
                case SpinStates.end:
                    break;
                default:
                    break;
            }
        }

        internal void InterruptSpin()
        {
            StateManager.SetStateTo(States.Spin_Outro);
        }

        //Engine Functions
        /// <summary>
        /// Start spinning the reels
        /// </summary>
        /// <returns></returns>
        public async Task StartSpinReels()
        {
            //The end reel configuration is set when spin starts to the next item in the list
            ReelStripSpinStruct[] end_reel_configuration = matrix.slot_machine_managers.end_configuration_manager.UseNextConfigurationInList();
            //Evaluation is ran over those symbols and if there is a bonus trigger the matrix will be set into display bonus state
            matrix.slot_machine_managers.paylines_manager.EvaluateWinningSymbolsFromCurrentConfiguration();

            await SpinReels(end_reel_configuration);
        }
        /// <summary>
        /// Used to start spinning the reels
        /// </summary>
        internal async Task SpinReels(ReelStripSpinStruct[] end_reel_configuration)
        {
            //If we want to use ReelStrips for the spin loop we need to stitch the end_reel_configuration and display symbols together
            if (use_reelstrips_for_spin_loop)
            {
                //Generate Reel strips if none are present
                matrix.GenerateReelStripsToLoop(ref end_reel_configuration);
                //TODO Set each reelstripmanager to spin thru the strip
                //TODO Insert end_reelstrips_to_display into generated reelstrips
            }
            //Spin the reels - if there is a delay between reels then wait delay amount
            for (int i = spin_reels_starting_forward_back ? 0: matrix.reel_strip_managers.Length-1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                spin_reels_starting_forward_back ? i < matrix.reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = spin_reels_starting_forward_back ? i+1:i-1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                await matrix.reel_strip_managers[i].StartSpin();
            }
        }
        /// <summary>
        /// Stop the reels - include display reel highlight if the feature is toggled
        /// </summary>
        internal async Task StopReelsSpinning()
        {
            //Get the end display configuration and set per reel
            ReelStripSpinStruct[] configuration_to_use = matrix.slot_machine_managers.end_configuration_manager.GetCurrentConfiguration();
            //Determine whether to stop reels forwards or backwards.
            for (int i = spin_reels_starting_forward_back ? 0 : matrix.reel_strip_managers.Length - 1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                spin_reels_starting_forward_back ? i < matrix.reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = spin_reels_starting_forward_back ? i + 1 : i - 1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                //If reel strip delays are enabled wait between strips to stop
                if (reel_spin_delay_end_enabled)
                {
                    await matrix.reel_strip_managers[i].StopReel(configuration_to_use[i]);//Only use for specific reel stop features
                }
                else
                {
                    matrix.reel_strip_managers[i].StopReel(configuration_to_use[i]);//Only use for specific reel stop features
                }
            }
            //Wait for all reels to be in spin.end state before continuing
            await WaitForAllReelsToStop(matrix.reel_strip_managers);
        }

        private async Task WaitForAllReelsToStop(ReelStripManager[] reel_strip_managers)
        {
            bool lock_task = true;
            while (lock_task)
            {
                for (int i = 0; i < reel_strip_managers.Length; i++)
                {
                    if (reel_strip_managers[i].current_spin_state == SpinStates.spin_end)
                    {
                        if (i == reel_strip_managers.Length - 1)
                        {
                            lock_task = false;
                            break;
                        }
                    }
                    else 
                    {
                        await Task.Delay(100);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set the spin speed for all the reels
        /// </summary>
        /// <param name="to_speed">new spin speed for all reel strips</param>
        internal void SetSpinSpeedTo(float to_speed)
        {
            for (int i = 0; i < matrix.reel_strip_managers.Length; i++)
            {
                matrix.reel_strip_managers[i].SetSpinSpeedTo(to_speed);
            }
        }

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

        private async void StateManager_StateChangedTo(States State)
        {
            //Debug.Log(String.Format("Checking for state dependant logic for SpinManager via state {0}",State.ToString()));
            switch (State)
            {
                case States.Idle_Idle:
                    isInterrupted = false;
                    SetSpinStateTo(SpinStates.idle_idle);
                    break;
                case States.Idle_Outro:
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
                    //Set all animators triggers to spin start again
                    matrix.SetAllAnimatorsTriggerTo(supported_triggers.SpinStart, true);
                    SetSpinStateTo(SpinStates.spin_start);
                    break;
                case States.Spin_Interrupt:
                    isInterrupted = true;
                    matrix.SetAllAnimatorsTriggerTo(supported_triggers.SpinSlam, true);
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
                    isInterrupted = false;
                    SetSpinStateTo(SpinStates.end);
                    break;
                case States.bonus_idle_outro:
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
                    matrix.SetAllAnimatorsTriggerTo(supported_triggers.SpinStart, true);
                    SetSpinStateTo(SpinStates.spin_start);
                    break;
                case States.bonus_idle_idle:
                    isInterrupted = false;
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
                    isInterrupted = false;
                    SetSpinStateTo(SpinStates.end);
                    break;
                default:
                    break;
            }
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
    }
}
