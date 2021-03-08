﻿#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

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
            if (reel_spin_delay_start_enabled.boolValue || reel_spin_delay_end_enabled.boolValue)
            {
                reel_spin_delay_ms.intValue = EditorGUILayout.IntSlider("Delay between reels star spin ms", reel_spin_delay_ms.intValue, 0, 2000);
            }
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
            EditorGUI.BeginChangeCheck();
            spin_direction.vector3Value = EditorGUILayout.Vector3Field("Set Spin Direction", spin_direction.vector3Value);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                myTarget.SetSpinDirectionTo(spin_direction.vector3Value);
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SpinManager Controls");
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start Test Spin"))
                {
                    myTarget.SetSpinStateTo(SpinStates.spin_start);
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
                if (_matrix == null)
                    _matrix = GetComponent<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        private Matrix _matrix;
        [SerializeField]
        private InteractionController controller;
        //TODO - Define reel strip length per reel - 50 - define time to traverse reel strip - speed is calculated based on traverse time - On Outro set speed to outro traverse time - 50% 
        //Hit spin - spin for 2 seconds - after lapse then land on symbol you need
        //Instead of stiching in reelstrips - see numbers flying by on the reelstrip
        public bool spin_enabled;
        public bool spin_slam_enabled;
        public bool reel_spin_delay_start_enabled = false;
        public bool reel_spin_delay_end_enabled = false;
        public int reel_spin_delay_ms = 0;
        /// <summary>
        /// This allows you to set the reels spin either forward or back (Left to right - right to left - top to bottom - bottom to top)
        /// </summary>
        public bool spin_reels_starting_forward_back = true;
        public float spin_speed = 50;
        public Vector3 spin_direction = Vector3.down; // Initial state is down
        public float spin_loop_until_seconds_pass = 5;
        public bool use_timer = false;
        public float time_counter = 0.0f;
        /// <summary>
        /// For reference only to what state our spin manager is in
        /// </summary>
        public SpinStates current_state;

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
                        //StateManager.SetStateTo(States.spin_outro);
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

        internal void CheckSpinEnabled()
        {
            if (spin_enabled)
            {
                Debug.Log("Spin is enabled and ready to go to start state");
                //StateManager.SetStateTo(States);
            }
            else if (spin_slam_enabled && StateManager.enCurrentState == States.Spin_Idle)
            {
                Debug.Log("Handle Spin Slam");
                //SetSpinStateTo(SpinStates.interrupt);
            }
        }

        internal void SetSpinStateTo(SpinStates state)
        {
            Debug.Log(String.Format("Setting Spin Manager Spin State to {0}",state.ToString()));            
            switch (state)
            {
                case SpinStates.idle_idle:
                    spin_enabled = true;
                    break;
                case SpinStates.spin_start:
                    StartSpin();
                    break;
                case SpinStates.spin_intro:
                    break;
                case SpinStates.spin_idle:
                    use_timer = true;
                    break;
                case SpinStates.spin_interrupt:
                    InterruptSpin();
                    break;
                case SpinStates.spin_outro:
                    ResetUseTimer();
                    StopReels();
                    break;
                case SpinStates.end:
                    break;
                default:
                    break;
            }
            current_state = state;
        }

        private void InterruptSpin()
        {
            EndSpin();
        }

        //Engine Functions
        public void StartSpin()
        {
            SpinReels();
        }

        public void EndSpin()
        {
            StopReels();
        }
        async void SpinReels()
        {
            //Generate reelstrip (this is temporary)
            //await matrix.end_configuration_manager.GenerateEndReelStripsConfiguration();
            //Evaluate winning paysymbols before spin starts
            matrix.paylines_manager.EvaluateWinningSymbols(matrix.end_configuration_manager.end_reelstrips_to_display);
            //Generate ReelStrips to cycle through if there is no reelstrip present
            if (matrix.use_reelstrips_for_spin_loop)
            {
                matrix.GenerateReelStripsToSpinThru();
            }
            //Insert end_reelstrips_to_display into generated reelstrips
            for (int i = spin_reels_starting_forward_back ? 0: matrix.reel_strip_managers.Length-1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                spin_reels_starting_forward_back ? i < matrix.reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = spin_reels_starting_forward_back ? i+1:i-1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                //Delay the reelstrip if toggled
                if(reel_spin_delay_start_enabled)
                    await Task.Delay(reel_spin_delay_ms);
                await matrix.reel_strip_managers[i].SpinReel();
            }
            //SetSpinStateTo(SpinStates.loop);
            //StateManager.SetStateTo(States.spin_loop);
        }

        async void StopReels()
        {
            //StateManager.SetStateTo(States.spin_end);
            //This handles spinning forward or back with ? operator
            for (int i = spin_reels_starting_forward_back ? 0 : matrix.reel_strip_managers.Length - 1; //Forward start at 0 - Backward start at length of reels_strip_managers.length - 1
                spin_reels_starting_forward_back ? i < matrix.reel_strip_managers.Length : i >= 0;  //Forward set the iterator to < length of reel_strip_managers - Backward set iterator to >= 0
                i = spin_reels_starting_forward_back ? i + 1 : i - 1)                                     //Forward increment by 1 - Backwards Decrement by 1
            {
                if (reel_spin_delay_end_enabled)
                    await Task.Delay(reel_spin_delay_ms);
                matrix.reel_strip_managers[i].StopReel(matrix.end_configuration_manager.end_reelstrips_to_display[i]);//Only use for specific reel stop features
            }
            matrix.end_configuration_manager.RemoveCurrentDisplayReelConfiguration();
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

        private void StateManager_StateChangedTo(States State)
        {
            Debug.Log(String.Format("Checking for state dependant logic for SpinManager via state {0}",State.ToString()));
            switch (State)
            {
                case States.None:
                    break;
                case States.preloading:
                    break;
                case States.coinin:
                    break;
                case States.coinout:
                    break;
                case States.idle_intro:
                    break;
                case States.idle_idle:
                    SetSpinStateTo(SpinStates.idle_idle);
                    break;
                case States.idle_outro:
                    break;
                case States.Spin_Start:
                    SetSpinStateTo(SpinStates.spin_start);
                    break;
                case States.Spin_Idle:
                    SetSpinStateTo(SpinStates.spin_idle);
                    break;
                case States.spin_outro:
                    SetSpinStateTo(SpinStates.spin_outro);
                    break;
                case States.spin_end:
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
            if (State == States.Spin_Start)
            {
            }
            else if (State == States.spin_outro)
            {
            }
        }

        internal void SetSpinDirectionTo(Vector3 new_spin_direction)
        {
            for (int i = 0; i < matrix.reel_strip_managers.Length; i++)
            {
                matrix.reel_strip_managers[i].SetSpinDirectionTo(new_spin_direction);
            }
        }
    }
}
