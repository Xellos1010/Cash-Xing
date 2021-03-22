using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Slot_Engine.Matrix
{
    public class ManagersReferenceScript : MonoBehaviour
    {
        public Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.parent.GetComponentInChildren<Matrix>();
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
        public SymbolMaterialsManager symbol_materials_manager
        {
            get
            {
                return CheckReturnComponent<SymbolMaterialsManager>(ref _symbol_materials_manager);
            }
        }
        internal SymbolMaterialsManager _symbol_materials_manager;
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
        public WeightedDistribution.IntDistribution symbols_weights
        {
            get
            {
                return CheckReturnComponent<WeightedDistribution.IntDistribution>(ref _symbols_weights);
            }
        }
        internal WeightedDistribution.IntDistribution _symbols_weights;

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