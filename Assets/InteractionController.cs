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
        private Matrix matrix;
        public bool canTriggerSet = false;

        public float distance_to_invoke_event = 50.0f;
        public Vector2 position_on_began;
        private bool draw_line_gizmo;
        private Ray camera_ray_out;

        void OnDrawGizmos()
        {
            if (draw_line_gizmo)
                Gizmos.DrawLine(camera_ray_out.origin, camera_ray_out.direction*1000);
        }

        // Update is called once per frame
        void Update()
        {
            //Modify Bet Amount
            if (StateManager.enCurrentState == States.Idle_Idle)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    Debug.Log(String.Format("Mouse position = {0}", Input.mousePosition));
                    RaycastForUIFromPosition(Input.mousePosition);
                }
                //#if UNITY_ANDROID
                if (Input.touchCount > 0)
                {
                    Touch temp = Input.touches[0];
                    //if (temp.phase == TouchPhase.Began)
                    //{
                    //    position_on_began = temp.position;
                    //}

                    //Swipe Event
                    //if (position_on_began.magnitude - temp.position.magnitude > distance_to_invoke_event)
                    //{
                    //    if (temp.position.magnitude > position_on_began.magnitude)
                    //    {

                    //    }
                    //}
                    //else
                    //{
                    //    Debug.Log("Trigger for spin/slam pressed");
                    //    if (StateManager.enCurrentState == States.Idle_Idle)
                    //    {
                    //        SetTrigger(supported_triggers.SpinStart);
                    //    }
                    //    else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_loop)
                    //    {
                    //        SetTrigger(supported_triggers.SpinSlam);
                    //    }
                    //}
                    if (temp.phase == TouchPhase.Ended)
                    {
                        RaycastForUIFromPosition(temp.position);
                    }
                }

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

            if (canTriggerSet)
            {
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                Touch temp = Input.touches[0];
                if (temp.phase == TouchPhase.Began)
                {
                    Debug.Log("Trigger for spin/slam pressed");
                    if (StateManager.enCurrentState == States.idle_idle)
                    {
                        SetTrigger(supported_triggers.SpinStart);
                    }
                    else if (StateManager.enCurrentState == States.spin_start || StateManager.enCurrentState == States.spin_loop)
                    {
                        SetTrigger(supported_triggers.SpinSlam);
                    }
                }
            }
#endif
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Trigger for spin/slam pressed");
                    if (StateManager.enCurrentState == States.Idle_Idle)
                    {
                        SetTriggerTo(supported_triggers.SpinStart);
                    }
                    else if (StateManager.enCurrentState == States.Spin_Intro || StateManager.enCurrentState == States.Spin_Idle)
                    {
                        SlamSpin();
                    }
                }
            }
            else
            {
                if (StateManager.enCurrentState == States.Idle_Intro)
                {
                    canTriggerSet = true;
                    SetTriggerTo(supported_triggers.End);
                }
            }
        }

        private void RaycastForUIFromPosition(Vector3 position)
        {
            Debug.Log(String.Format("testing position = {0} for raycast hit", position));
            RaycastHit hit_info;
            Ray ray_to_use = Camera.main.ScreenPointToRay(position);
            Physics.Raycast(ray_to_use, out hit_info, 1000f);
            EnableDrawLineGizmo(ray_to_use);
            Debug.Log(hit_info.transform.gameObject.name);
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
            }
        }

        private void EnableDrawLineGizmo(Ray camera_ray_out)
        {
            draw_line_gizmo = true;
            this.camera_ray_out = camera_ray_out;
        }

        private void IncreaseBetAmount()
        {
            matrix.machine_information_manager.IncreaseBetAmount();
        }

        private void DecreaseBetAmount()
        {
            matrix.machine_information_manager.DecreaseBetAmount();
        }

        private void SetTriggerTo(supported_triggers to_trigger)
        {
            StateMachineController.SetTrigger(to_trigger);
        }

        public void SlamSpin()
        {
            SetTriggerTo(supported_triggers.SpinSlam);
        }


        private void SetStateTo(States to_state)
        {
            canTriggerSet = false;
            StateManager.SetStateTo(to_state);
        }

    }
}