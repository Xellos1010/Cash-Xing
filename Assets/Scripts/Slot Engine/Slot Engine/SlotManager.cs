//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Slot.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SlotManager))]
    class SlotEditor : Editor
    {
        SlotManager myTarget;

        public void OnEnable()
        {
            myTarget = (SlotManager)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("To be Removed");
            base.OnInspectorGUI();

        }
    }
#endif
        public class SlotManager : MonoBehaviour
        {
        /// <summary>
        /// The symbol presenting after the reel stops
        /// </summary>
        public string presentation_symbol_name;
        public int presentation_symbol;
        
        public ReelStripManager reel_parent;

        public bool movement_enabled = false;
        /// <summary>
        /// the end position for the reels to calculate and land on
        /// </summary>
        public Vector3 end_position;
        public bool set_to_display_end_symbol = false;
        public bool slot_in_end_position = false;
        public bool graphics_set_to_end = false;
        public MeshRenderer _meshRenderer;

        public AnimatorStateMachineManager state_machine
        {
            get
            {
                if (_state_machine == null)
                    _state_machine = GetComponent<AnimatorStateMachineManager>();
                return _state_machine;
            }
        }
        [SerializeField]
        internal AnimatorStateMachineManager _state_machine;

        public MeshRenderer meshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                    _meshRenderer = GetComponentInChildren<MeshRenderer>();
                return _meshRenderer;
            }
        }

        //Unity Default Functions

        //*************
        /// <summary>
        /// Sets the material for the mesh renderer to new material
        /// </summary>
        /// <param name="to_material">material to set mesh renderer</param>
        public void SetMeshRendererMaterialTo(Material to_material)
        {
            meshRenderer.material = to_material;
        }
        /// <summary>
        /// Loads from Resources the symbol material
        /// </summary>
        /// <param name="to_symbol">the symbol name to look for</param>
        public void SetSlotGraphicTo(string to_symbol)
        {
            //Debug.Log(String.Format("Settings {0} slot symbol to {1}",transform.name,to_symbol));
            //TODO add test cases for if to_graphic is present in directory
            //TODO Add State Dependant Graphics Loading
            Material to_material = reel_parent.matrix.symbols_material_manager.ReturnSymbolMaterial(to_symbol);
            SetMeshRendererMaterialTo(to_material);
        }

        public void PlayAnimation()
        {
            //Sprite.frameRate = 24;
            //Sprite.Play();
            //TODO Insert Play Animation Logic
        }

        public void StopAnimation()
        {
            //Sprite.frameRate = 24;
            //Sprite.Stop();
            //TODO Insert Stop Animation Logic
        }

        public void StartSpin()
        {
            ResetAllVars();
            StopAnimation();
            SetTriggerTo(supported_triggers.SpinStart);
            movement_enabled = true;
        }

        Vector3 GeneratePositionUpdateSpeed(Vector3 amount_to_add) //Needs to be positive to move forwards and negative to move backwards
        {
            return transform.localPosition + amount_to_add; //new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        void Update()
        {
            if (movement_enabled)
            {
                Vector3 toPosition;
               
                toPosition = GeneratePositionUpdateSpeed(reel_parent.reel_spin_speed_direction * reel_parent.reel_spin_speed_current);
                //Check X Y and Z and move slot to opposite

                //Check if to far left or right and move

                //Check if to far down or up and move
                if (reel_parent.reel_spin_speed_direction.y < 0)
                {
                    if (toPosition.y <= reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1], true);
                }
                else if (reel_parent.reel_spin_speed_direction.y > 0)
                {
                    if (toPosition.y >= reel_parent.positions_in_path_v3[0].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1], false);
                }
                //TODO setup to set end position based on number of slots - slot width + padding and direction of spin
                if(set_to_display_end_symbol && graphics_set_to_end)
                    if (toPosition.y <= end_position.y)
                    {
                        toPosition = end_position;
                        slot_in_end_position = true;
                        ResetAllVars();
                    }
                transform.localPosition = toPosition;
            }
        }

        private void ResetAllVars()
        {
            SetSlotMovementEnabledTo(false);
            set_to_display_end_symbol = false;
            graphics_set_to_end = false;
            //Set state of reel to end
        }


        private void ShiftToPositionBy(ref Vector3 toPosition, Vector3 lastPosition, bool upDown)
        {
            if(upDown)
                toPosition = new Vector3(toPosition.x,toPosition.y - lastPosition.y, toPosition.z);
            else
                toPosition = new Vector3(toPosition.x, toPosition.y + lastPosition.y, toPosition.z);

            if(set_to_display_end_symbol)
            {
                //Set Graphics and end position
                graphics_set_to_end = true;
                end_position = reel_parent.positions_in_path_v3[(reel_parent.positions_in_path_v3.Length - 2) - reel_parent.end_symbols_set_from_config];

                if (reel_parent.end_symbols_set_from_config < reel_parent.ending_symbols.Length)
                {
                    SetDisplaySymbolTo(reel_parent.ending_symbols.Length - 1 - reel_parent.end_symbols_set_from_config);
                }
                else
                {
                    SetPresentationSymbolTo(-1); //TODO Define whether to set the top slot graphic
                }
                //Debug.Log("Slot " + transform.name + " symbol presentation = " + presentation_symbol + " end position = " + end_position);
            }
            else
            {
                bool symbol_set = false;
                if (reel_parent.change_symbol_graphic_on_spin_idle)
                {
                    //If Symbol Generated = opverlay - Generate Sub Symbol and attach 2 materials
                    if (reel_parent.reel_strip_to_use_for_spin.reel_spin_symbols != null)
                    {
                        if (reel_parent.reel_strip_to_use_for_spin.reel_spin_symbols.Length > 0)
                        {
                            int symbol = reel_parent.ReturnNextSymbolInStrip();
                            SetSlotGraphicTo(ReturnSymbolNameFromInt(symbol));
                            SetPresentationSymbolTo(symbol);
                            symbol_set = true;
                        }
                    }
                    if (!symbol_set)
                    {
                        int symbol = reel_parent.matrix.end_configuration_manager.GetRandomWeightedSymbol();
                        SetSlotGraphicTo(ReturnSymbolNameFromInt(symbol));
                        SetPresentationSymbolTo(symbol);
                    }
                }
            }
        }

        private string ReturnSymbolNameFromInt(int symbol)
        {
            return ((Symbol)symbol).ToString();
        }

        internal void SetSymbolResolveWin()
        {
            SetBoolTo(supported_bools.SymbolResolve, true);
            SetBoolTo(supported_bools.LoopPaylineWins, true);
            //SetFloatMotionTimeTo(0.0f);
        }
        private void SetFloatMotionTimeTo(float v)
        {
            state_machine.SetFloatTo(supported_floats.MotionTime,0.0f);
        }

        private void SetBoolTo(supported_bools bool_name, bool v)
        {
            state_machine.SetBool(bool_name,v);
        }

        internal void SetSymbolResolveToLose()
        {
            SetBoolTo(supported_bools.SymbolResolve, false);
        }

        internal void SetOverrideControllerTo(AnimatorOverrideController animatorOverrideController)
        {
            state_machine.SetRuntimeControllerTo(animatorOverrideController);
        }

        internal void SetDisplaySymbolTo(int symbol_to_display)
        {
            //Debug.Log(string.Format("Set Display symbol to {0}", v));
            SetPresentationSymbolTo(reel_parent.ending_symbols[symbol_to_display]);
            SetSlotGraphicTo(presentation_symbol_name);
            reel_parent.end_symbols_set_from_config += 1;
        }

        private void SetPresentationSymbolTo(Symbol to_symbol)
        {
            SetPresentationSymbolTo((int)to_symbol);
        }
        private void SetPresentationSymbolTo(int to_symbol)
        {
            if (to_symbol < 0)
                presentation_symbol_name = "Not on Matrix";
            else
                presentation_symbol_name = ReturnSymbolNameFromInt(to_symbol);
            presentation_symbol = to_symbol;
        }
        internal void SetToStopSpin()
        {
            //TODO setup state machine
            set_to_display_end_symbol = true;
        }

        internal void SetSlotMovementEnabledTo(bool enable_disable)
        {
            movement_enabled = enable_disable;
            if (enable_disable)
                slot_in_end_position = false;
        }

        internal void ResetAnimator()
        {
            state_machine.InitializeAnimator();
        }

        internal void InitializeAnimatorToPresentWin()
        {
            state_machine.InitializeAnimator();
            state_machine.SetBool(supported_bools.WinRacking, true);
            state_machine.SetBool(supported_bools.ResolveSpin, true);
            
        }

        internal void SetTriggerTo(supported_triggers to_trigger)
        {
            state_machine.SetTrigger(to_trigger);
        }
    }
}