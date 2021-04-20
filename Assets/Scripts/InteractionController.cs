﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractionController))]
    class InteractionControllerEditor : BoomSportsEditor
    {
        InteractionController myTarget;
        public void OnEnable()
        {
            myTarget = (InteractionController)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Interaction Controller Properties");

            EditorGUILayout.EnumPopup(StateManager.enCurrentState);

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Interaction Controller Controls");
            base.OnInspectorGUI();
        }


    }
#endif

    public class InteractionController : MonoBehaviour
    {
        public AnimatorStateMachineManager StateMachineController
        {
            get
            {
                if (_StateMachineController == null)
                    _StateMachineController = GameObject.FindGameObjectWithTag("StateMachine").GetComponent<AnimatorStateMachineManager>();
                return _StateMachineController;
            }
        }
        public AnimatorStateMachineManager _StateMachineController;
        [SerializeField]
        private Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.parent.GetComponentInChildren<Matrix>();
                return _matrix;
            }
        }
        private Matrix _matrix;
        public bool can_spin_slam = false;

        public float distance_to_invoke_swipe_event = 50.0f;
        public float distance_to_invoke_tap_event = 5.0f;
        public Vector2 position_on_began;
        private bool draw_line_gizmo;
        private Ray camera_ray_out;

        void OnDrawGizmos()
        {
            if (draw_line_gizmo)
                Gizmos.DrawLine(camera_ray_out.origin, camera_ray_out.direction * 1000);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log(String.Format("Mouse position = {0}", Input.mousePosition));
                RaycastForUIFromPosition(Input.mousePosition);
            }
            //Modify Bet Amount
            if (StateManager.enCurrentState == States.Idle_Idle)
            {

#if UNITY_ANDROID
                //#if UNITY_ANDROID
                if (Input.touchCount > 0)
                {
                    Touch temp = Input.touches[0];
                    if (temp.phase == TouchPhase.Began)
                    {
                        position_on_began = temp.position;
                    }

                    if (temp.phase == TouchPhase.Ended)
                    {
                        if (!CheckPositionBeginEndDistance(temp.position.x - position_on_began.x, distance_to_invoke_swipe_event, true))
                        {
                            if (CheckPositionBeginEndDistance(temp.position.x - position_on_began.x, distance_to_invoke_tap_event, false))
                                RaycastForUIFromPosition(temp.position);
                            else
                            {
                                Debug.Log("No tap or swipe event");
                            }
                        }
                        else
                        {
                            if (temp.position.x > position_on_began.x)
                            {
                                IncreaseBetAmount();
                            }
                            else
                            {
                                DecreaseBetAmount();
                            }
                        }
                    }
                }
#endif
                //#endif
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    DecreaseBetAmount();
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    IncreaseBetAmount();
                }
            }
            if (can_spin_slam)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Trigger for spin/slam pressed");
                    CheckStateToSpinSlam();
                }


#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                Touch temp = Input.touches[0];
                if (temp.phase == TouchPhase.Began)
                {
                    //CheckForSpinSlam();
                }
            }
#endif
            }
        }

        internal void CheckStateToSpinSlam()
        {
            if (StateManager.enCurrentState == States.Idle_Idle || StateManager.enCurrentState == States.bonus_idle_idle)
            {
                StartSpin();
            }
            else if (StateManager.enCurrentState == States.Idle_Outro || 
                StateManager.enCurrentState == States.Spin_Intro || 
                StateManager.enCurrentState == States.Spin_Idle || 
                StateManager.enCurrentState == States.bonus_idle_outro || 
                StateManager.enCurrentState == States.bonus_spin_intro || 
                StateManager.enCurrentState == States.bonus_spin_loop)
            {
                SlamSpin();
            }
            //TODO Have this based off game mode
            else if (StateManager.enCurrentState == States.Resolve_Intro)
            {
                SlamLoopingPaylines();
            }
        }

        private void SlamLoopingPaylines()
        {
            Debug.Log("Slamming Paylines Cycling");
            can_spin_slam = false;
            //Matrix needs to reset animators for slots and state machine needs to be set to Resolve_Outro
            matrix.SlamLoopingPaylines();
        }

        private void StartSpin()
        {
            if(StateManager.enCurrentState == States.Idle_Idle)
                DisableInteractionAndSetStateTo(States.Idle_Outro);
            //TODO have this based off game mode
            else if (StateManager.enCurrentState == States.bonus_idle_idle)
                DisableInteractionAndSetStateTo(States.bonus_idle_outro);
        }
        
        public void DisableSlam()
        {
            can_spin_slam = false;
        }

        private void DisableInteractionAndSetStateTo(States to_state)
        {
            can_spin_slam = false;
            //Set state to idle outro then in idle outro set trigger for StartSpin
            StateManager.SetStateTo(to_state);
        }

        private bool CheckForTapEvent(Vector2 position)
        {
            throw new NotImplementedException();
        }

        private bool CheckPositionBeginEndDistance(float distance_traveled, float distance_to_invoke_event, bool greater_less)
        {
            if (greater_less ? 
                Mathf.Abs(distance_traveled)  >= distance_to_invoke_swipe_event :
                Mathf.Abs(distance_traveled)  <= distance_to_invoke_swipe_event
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void RaycastForUIFromPosition(Vector3 position)
        {
            //Debug.Log(String.Format("testing position = {0} for raycast hit", position));
            RaycastHit hit_info;
            Ray ray_to_use = Camera.main.ScreenPointToRay(position);
            Physics.Raycast(ray_to_use, out hit_info, 1000f);
            EnableDrawLineGizmo(ray_to_use);
            //Debug.Log(hit_info.transform?.gameObject.name);
            if (hit_info.collider != null)
            {
                if (hit_info.collider.gameObject.tag == "BetUp")
                {
                    IncreaseBetAmount();
                }
                else if (hit_info.collider.gameObject.tag == "BetDown")
                {
                    DecreaseBetAmount();
                }
                else if (hit_info.collider.gameObject.tag == "Spin")
                {
                    CheckStateToSpinSlam();
                }
            }
        }

        private void EnableDrawLineGizmo(Ray camera_ray_out)
        {
            draw_line_gizmo = true;
            this.camera_ray_out = camera_ray_out;
        }

        private void IncreaseBetAmount()
        {
            if (StateManager.enCurrentState == States.Idle_Idle)
                matrix.slot_machine_managers.machine_info_manager.IncreaseBetAmount();
        }

        private void DecreaseBetAmount()
        {
            if(StateManager.enCurrentState == States.Idle_Idle)
                matrix.slot_machine_managers.machine_info_manager.DecreaseBetAmount();
        }

        private void SetTriggerTo(supported_triggers to_trigger)
        {
            StateMachineController.SetAllTriggersTo(to_trigger);
        }

        public void SlamSpin()
        {
            DisableInteractionAndSetStateTo(States.Spin_Interrupt);
        }

        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }

        private void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Idle:
                    can_spin_slam = true;
                    break;
                case States.Idle_Outro:
                    //Can slam even if Idle_Outro Animations haven't played. Disable Slam until Resolve_Intro or Idle_Idle
                    can_spin_slam = true;
                    break;
                case States.Resolve_Intro:
                    can_spin_slam = true;
                    break;
                case States.bonus_idle_idle:
                    can_spin_slam = true;
                    break;
                case States.bonus_idle_outro:
                    can_spin_slam = true;
                    break;
                default:
                    break;
            }
        }

    }
}