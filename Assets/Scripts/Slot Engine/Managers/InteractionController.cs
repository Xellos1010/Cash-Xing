using System;
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
        private ReelStripConfigurationObject matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.parent.GetComponentInChildren<ReelStripConfigurationObject>();
                return _matrix;
            }
        }

        public enum Actions {
            DecreaseBet,
            IncreaseBet,
            PlaceBet,
            Spin,
            Slam,
            ChangeLayout
        }

        private ReelStripConfigurationObject _matrix;
        public bool can_spin_slam = false;

        public bool locked = false;

        public float distance_to_invoke_swipe_event = 50.0f;
        public float distance_to_invoke_tap_event = 5.0f;
        public Vector2 position_on_began;
        private bool draw_line_gizmo;
        private Ray camera_ray_out;
        public Animator spin_btn_animator;
        public bool compactExpandedToggle = true;
        public Animator compactExpandeedLayoutController;

        void OnDrawGizmos()
        {
            if (draw_line_gizmo)
                Gizmos.DrawLine(camera_ray_out.origin, camera_ray_out.direction * 1000);
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                PerformAction(Actions.ChangeLayout);
            }
            if (!locked)
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
                                PerformAction(Actions.IncreaseBet);
                            }
                            else
                            {
                                PerformAction(Actions.DecreaseBet);
                            }
                        }
                    }
                }
#endif
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        PerformAction(Actions.DecreaseBet);
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        PerformAction(Actions.IncreaseBet);
                    }
                    if (Input.GetKeyDown(KeyCode.Space) && can_spin_slam)
                    {
                        //If Idle_Idle then can place bet
                        PerformAction(Actions.PlaceBet);
                    }
                }
                else if(StateManager.enCurrentState == States.bonus_idle_idle)
                {
                    //ToDO this needs to be built a more safe reference
                    if (Input.GetKeyDown(KeyCode.Space) && can_spin_slam)
                    {
                        PerformAction(Actions.Spin);
                    }
                }
                else
                {
                    //ToDO this needs to be built a more safe reference
                    if (Input.GetKeyDown(KeyCode.Space) && can_spin_slam)
                    {
                        PerformAction(Actions.Slam);
                    }
                }
            }
        }

        internal void LockInteractions()
        {
            locked = true;
        }

        private void PerformAction(Actions action)
        {
            locked = true;
            switch (action)
            {
                case Actions.DecreaseBet:
                    DecreaseBetAmount();
                    break;
                case Actions.IncreaseBet:
                    IncreaseBetAmount();
                    break;
                case Actions.PlaceBet:
                    CheckStateToSpinSlam();
                    break;
                case Actions.Spin:
                    CheckStateToSpinSlam();
                    break;
                case Actions.Slam:
                    CheckStateToSpinSlam();
                    break;
                case Actions.ChangeLayout:
                    ToggleLayout();
                    break;
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
            Debug.Log(String.Format("testing position = {0} for raycast hit", position));
            RaycastHit hit_info;
            Ray ray_to_use = Camera.main.ScreenPointToRay(position);
            Physics.Raycast(ray_to_use, out hit_info, 1000f);
            EnableDrawLineGizmo(ray_to_use);
            Debug.Log(hit_info.transform?.gameObject.name);
            if (hit_info.collider != null)
            {
                if (StateManager.enCurrentState == States.Idle_Idle)
                {
                    if (hit_info.collider.gameObject.tag == "Layout")
                    {
                        ToggleLayout();
                    }
                    if (hit_info.collider.gameObject.tag == "BetUp")
                    {
                        PerformAction(Actions.IncreaseBet);
                    }
                    else if (hit_info.collider.gameObject.tag == "BetDown")
                    {
                        PerformAction(Actions.DecreaseBet);
                    }
                    else if (hit_info.collider.gameObject.tag == "Spin" && can_spin_slam)
                    {
                        PerformAction(Actions.PlaceBet);
                    }
                }
                else if(StateManager.enCurrentState == States.bonus_idle_idle)
                {
                    if (hit_info.collider.gameObject.tag == "Spin" && can_spin_slam)
                    {
                        PerformAction(Actions.Spin);
                    }
                }
                else
                {
                    if (hit_info.collider.gameObject.tag == "Spin" && can_spin_slam)
                    {
                        PerformAction(Actions.Slam);
                    }
                }
            }
        }

        private void ToggleLayout()
        {
            compactExpandedToggle = !compactExpandedToggle;
            compactExpandeedLayoutController.SetBool(supportedAnimatorBools.Compact.ToString(), compactExpandedToggle);
            locked = false;
        }

        private void EnableDrawLineGizmo(Ray camera_ray_out)
        {
            draw_line_gizmo = true;
            this.camera_ray_out = camera_ray_out;
        }

        private void IncreaseBetAmount()
        {
            if (StateManager.enCurrentState == States.Idle_Idle)
                matrix.slotMachineManagers.machine_info_manager.IncreaseBetAmount();
            locked = false;
        }

        private void DecreaseBetAmount()
        {
            if(StateManager.enCurrentState == States.Idle_Idle)
                matrix.slotMachineManagers.machine_info_manager.DecreaseBetAmount();
            locked = false;
        }

        private void SetTriggerTo(supportedAnimatorTriggers to_trigger)
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

        private async void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Idle:
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Idle");
                    UnlockSlamSpin();
                    break;
                case States.Spin_Idle:
                    UnlockSlamSpin();
                    break;
                case States.Resolve_Intro:
                    UnlockSlamSpin();
                    break;
                case States.bonus_idle_idle:
                    await matrix.isAllAnimatorsThruStateAndAtPauseState("Idle_Idle");
                    if(locked)
                        UnlockSlamSpin();
                    break;
                case States.bonus_spin_loop:
                    UnlockSlamSpin();
                    break;
                default:
                    break;
            }
        }

        private void UnlockSlamSpin()
        {
            Debug.Log("Unlock Slam Spin");
            locked = false;
            can_spin_slam = true;
        }
    }
}