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
                Debug.Log(String.Format("myTarget.matrix = {0}", myTarget.configurationObject.gameObject.name));
                Debug.Log(String.Format("myTarget.animator_statemachine_master = {0}", myTarget.animatorStateMachineMaster.gameObject.name));
                Debug.Log(String.Format("myTarget.spin_manager = {0}", myTarget.spinManager.gameObject.name));
                Debug.Log(String.Format("myTarget.interaction_controller = {0}", myTarget.interactionController.gameObject.name));
                Debug.Log(String.Format("myTarget.paylines_manager = {0}", myTarget.winningObjectsManager.gameObject.name));
                Debug.Log(String.Format("myTarget.end_configuration_manager = {0}", myTarget.endConfigurationManager.gameObject.name));
                Debug.Log(String.Format("myTarget.racking_manager = {0}", myTarget.rackingManager.gameObject.name));
            }
            base.OnInspectorGUI();
        }
    }
#endif
    public class ManagersReferenceScript : MonoBehaviour
    {
        public StripConfigurationObject configurationObject
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.GetComponentInChildren<StripConfigurationObject>();
                return _matrix;
            }
        }
        internal StripConfigurationObject _matrix;
        public AnimatorStateMachineManager animatorStateMachineMaster
        {
            get
            {
                return CheckReturnComponent<AnimatorStateMachineManager>(ref _animatorStateMachineMaster);
            }
        }
        internal AnimatorStateMachineManager _animatorStateMachineMaster;
        public SpinManager spinManager
        {
            get
            {
                return CheckReturnComponent<SpinManager>(ref _spinManager);
            }
        }
        internal SpinManager _spinManager;
        public InteractionController interactionController
        {
            get
            {
                return CheckReturnComponent<InteractionController>(ref _interactionController);
            }
        }
        internal InteractionController _interactionController;
        /// <summary>
        /// Manages the reference for paylines_manager
        /// </summary>
        public WinningObjectManager winningObjectsManager
        {
            get
            {
                return CheckReturnComponent<WinningObjectManager>(ref _winningObjectsManager);
            }
        }

        internal WinningObjectManager _winningObjectsManager;
        /// <summary>
        /// Manages the reference for end configuration manager
        /// </summary>
        public EndConfigurationManager endConfigurationManager
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
        public RackingManager rackingManager
        {
            get
            {
                return CheckReturnComponent<RackingManager>(ref _rackingManager);
            }
        }
        internal RackingManager _rackingManager;
        /// <summary>
        /// Manages the reference for the machine information manager
        /// </summary>
        public MachineInfoManager machineInfoManager
        {
            get
            {
                return CheckReturnComponent<MachineInfoManager>(ref _machineInfoManager);
            }
        }
        internal MachineInfoManager _machineInfoManager;

        public EvaluationManager evaluationManager;
        public LerpToMe multiplierLerpToMe;

            private T CheckReturnComponent<T>(ref T componentReferenece)
        {
            if (componentReferenece == null)
                componentReferenece = GetComponentFromChild<T>();
            return componentReferenece;
        }

        public T GetComponentFromChild<T>()
        {
            return transform.GetComponentInChildren<T>();
        }
    }

}