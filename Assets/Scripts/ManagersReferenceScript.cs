using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slot_Engine.Matrix.Managers;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ManagersReferenceScript))]
    class ManagersReferenceScriptEditor : BoomSportsEditor
    {
        ManagersReferenceScript myTarget;
        public void OnEnable()
        {
            myTarget = (ManagersReferenceScript)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("ManagersReference Properties");

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("ManagersReference Controls");
            if (GUILayout.Button("Set All References"))
            {
                Debug.Log(String.Format("myTarget.matrix = {0}", myTarget.matrix.gameObject.name));
                Debug.Log(String.Format("myTarget.animator_statemachine_master = {0}", myTarget.animator_statemachine_master.gameObject.name));
                Debug.Log(String.Format("myTarget.spin_manager = {0}", myTarget.spin_manager.gameObject.name));
                Debug.Log(String.Format("myTarget.interaction_controller = {0}", myTarget.interaction_controller.gameObject.name));
                Debug.Log(String.Format("myTarget.paylines_manager = {0}", myTarget.paylines_manager.gameObject.name));
                Debug.Log(String.Format("myTarget.end_configuration_manager = {0}", myTarget.end_configuration_manager.gameObject.name));
                Debug.Log(String.Format("myTarget.racking_manager = {0}", myTarget.racking_manager.gameObject.name));
            }
            base.OnInspectorGUI();
        }
    }
#endif
    public class ManagersReferenceScript : MonoBehaviour
    {
        public Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.GetComponentInChildren<Matrix>();
                return _matrix;
            }
        }
        internal Matrix _matrix;
        public AnimatorStateMachineManager animator_statemachine_master
        {
            get
            {
                return CheckReturnComponent<AnimatorStateMachineManager>(ref _animator_statemachine_master);
            }
        }
        internal AnimatorStateMachineManager _animator_statemachine_master;
        public SpinManager spin_manager
        {
            get
            {
                return CheckReturnComponent<SpinManager>(ref _spin_manager);
            }
        }
        internal SpinManager _spin_manager;
        public InteractionController interaction_controller
        {
            get
            {
                return CheckReturnComponent<InteractionController>(ref _interaction_controller);
            }
        }
        internal InteractionController _interaction_controller;
        /// <summary>
        /// Manages the reference for paylines_manager
        /// </summary>
        public PaylinesManager paylines_manager
        {
            get
            {
                return CheckReturnComponent<PaylinesManager>(ref _paylines_manager);
            }
        }

        internal PaylinesManager _paylines_manager;
        /// <summary>
        /// Manages the reference for end configuration manager
        /// </summary>
        public EndConfigurationManager end_configuration_manager
        {
            get
            {
                return CheckReturnComponent<EndConfigurationManager>(ref _end_configuration_manager);
            }
        }
        internal EndConfigurationManager _end_configuration_manager;
        /// <summary>
        /// Manages the reference for symbols_material_manager
        /// </summary>
        public SoundManager soundManager;
        public RackingManager racking_manager
        {
            get
            {
                return CheckReturnComponent<RackingManager>(ref _racking_manager);
            }
        }
        internal RackingManager _racking_manager;
        /// <summary>
        /// Manages the reference for the machine information manager
        /// </summary>
        public MachineInfoManager machine_info_manager
        {
            get
            {
                return CheckReturnComponent<MachineInfoManager>(ref _machine_info_manager);
            }
        }
        internal MachineInfoManager _machine_info_manager;

        public LerpToMe lerpToMe;
        public EvaluationManager evaluationManagger;

        private T CheckReturnComponent<T>(ref T component_referenece)
        {
            if (component_referenece == null)
                component_referenece = GetComponentFromChild<T>();
            return component_referenece;
        }

        public T GetComponentFromChild<T>()
        {
            return transform.GetComponentInChildren<T>();
        }
    }

}