//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Reel.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    using UnityEditor;

public enum eEaseType
{
    constant,
    ease,
}


    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReelStripManager))]
    class ReelStripManagerEditor : BoomSportsEditor
    {
        ReelStripManager my_target;
        SerializedProperty reel_spin_speed_current;

        /// <summary>
        /// Animation curves for looping along path
        /// </summary>
        //SerializedProperty looping_curves_xyz;
        //SerializedProperty display_slots;
        SerializedProperty reel_number;
        SerializedProperty positions_in_path_v3;
        SerializedProperty use_ease_inOut_spin;
        SerializedProperty ending_symbols;
        SerializedProperty current_spin_state;

        public void OnEnable()
        {
            my_target = (ReelStripManager)target;
            reel_spin_speed_current = serializedObject.FindProperty("reel_spin_speed_current");
            positions_in_path_v3 = serializedObject.FindProperty("positions_in_path_v3");
            use_ease_inOut_spin = serializedObject.FindProperty("use_ease_inOut_spin");
            reel_number = serializedObject.FindProperty("reel_number");
            //display_slots = serializedObject.FindProperty("display_slots");
            ending_symbols = serializedObject.FindProperty("ending_symbols");
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Controls");
            if(!Application.isPlaying)
            {
                if (GUILayout.Button("Set Slot Positions Initial"))
                {
                    my_target.SetSlotPositionToStart();
                }
                if (GUILayout.Button("Update Slot objects and positions local world reference"))
                {
                    my_target.UpdateSlotObjectsAndPositions();
                }
                if (GUILayout.Button("Update sub state machines slot managers"))
                {
                    my_target.UpdateSlotManagersSubStateMachines();
                }
            }
            if (my_target.is_reel_spinning)
            {
                if (GUILayout.Button("Spin Reel Test"))
                {
                    my_target.SpinReelsNow();
                }
                
            }
            else
            {
                if (GUILayout.Button("Stop Reel Test"))
                {
                    my_target.StopReelTest();
                }
                if (GUILayout.Button("Enable Movement"))
                {
                    my_target.SetSlotsMovementEnabled(true);
                }
                if (GUILayout.Button("Disable Movement"))
                {
                    my_target.SetSlotsMovementEnabled(false);
                }
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(ending_symbols);
            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            base.OnInspectorGUI();
        }
    }
#endif
    public class ReelStripManager : MonoBehaviour
    {
        [SerializeField]
        internal SpinStates current_spin_state;
        //the matrix associated with the reel_strip
        internal Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.GetComponentInParent<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        internal Matrix _matrix;
        /// <summary>
        /// Slot managers in the reel strip
        /// </summary>
        [SerializeField] //If base inspector enabled can check references
        internal SlotManager[] slots_in_reel;

        /// <summary>
        /// Distance for animation to travel when movement is enabled
        /// </summary>
        [SerializeField] //If base inspector enabled can check references
        internal float reel_spin_speed_current;

        /// <summary>
        /// Holds the reel strip to cycle thru symbols for spin and end symbol configuration for reel
        /// </summary>
        [SerializeField]
        internal ReelStripStruct reelstrip_info
        {
            get
            {
                return _reelstrip_info;
            }
            set
            {
                ReelStripStruct temp = _reelstrip_info;
                _reelstrip_info = value;
                if (_reelstrip_info.total_positions != temp.total_positions)
                {
                    UpdateSlotObjectsAndPositions();
                }
            }
        }

        internal void UpdateSlotObjectsAndPositions()
        {
            UpdateLocalPositionsInPath(reelstrip_info.total_positions);
            UpdateSlotObjectsInReelStrip(reelstrip_info.total_slot_objects);
        }

        [SerializeField]
        internal ReelStripStruct _reelstrip_info;

        /// <summary>
        /// On Spin Start uses a curve editor to determine speed over time - 0 -> -100 -> 0 -> +200 for example over for seconds is ease out
        /// </summary>
        public bool use_ease_inOut_spin = false;
        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[] positions_in_path_v3_local;
        /// <summary>
        /// The Ending symbols to Set To 
        /// </summary>
        [SerializeField]
        internal Symbol[] ending_symbols;
        /// <summary>
        /// Enable you to change the symbol when slot exits matrix to weighted distribution symbol set
        /// </summary>
        public bool change_symbol_graphic_on_spin_idle = true;
        internal int end_symbols_set_from_config = 0;
        
        /// <summary>
        /// is the reel in a spin state
        /// </summary>
        internal bool is_reel_spinning
        {
            get
            {
                bool is_spinning = true;
                if (slots_in_reel != null)
                {
                    for (int slot = 0; slot <= slots_in_reel.Length; slot++)
                    {
                        if (slot < slots_in_reel.Length)
                        {
                            if (slots_in_reel[slot] != null)
                            {
                                if (slot == slots_in_reel.Length)
                                {
                                    is_spinning = false;
                                    break;
                                }
                                if (slots_in_reel[slot].movement_enabled)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
                return is_spinning;
            }
        }
        public int reel_strip_counter = 0;
        

        internal int ReturnNextSymbolInStrip()
        {
            int output = (int)(Symbol)reelstrip_info.spin_info.reel_spin_symbols[reel_strip_counter];
            if(reel_strip_counter+1 >= reelstrip_info.spin_info.reel_spin_symbols.Length)
            {
                reel_strip_counter = 0;
            }
            else
            {
                reel_strip_counter += 1;
            }
            return output;
        }

        /*
////For Spinning down the yCurve needs to be set for each
///// <summary>
///// start spin easing
///// </summary>
//public AnimationCurve[] start_spin_curve_xyz
//{
//    get
//    {
//        if (_start_spin_curve_xyz == null || _start_spin_curve_xyz.Length < 3)
//        {
//            _start_spin_curve_xyz = new AnimationCurve[3] {
//            AnimationCurve.Linear(0.0f,0,1,reel_spin_speed_direction.magnitude),
//            AnimationCurve.Linear(0.0f,0,1,reel_spin_speed_direction.magnitude),
//            AnimationCurve.Linear(0.0f,0,1,reel_spin_speed_direction.magnitude)
//            };
//        }
//        return _start_spin_curve_xyz;
//    }
//    set
//    {
//        _start_spin_curve_xyz = value;
//    }
//}
////Eases the speed over seconds
///// <summary>
///// Looping Path Time - slot 0 -> end slot - each key is a position in path. Speed - Speed -> pixels to move per Time.deltatime / seconds
///// </summary>
//public AnimationCurve[] looping_curves_xyz
//{
//    get
//    {
//        if (_looping_curves_xyz == null || _looping_curves_xyz.Length < 3)
//        {
//            _looping_curves_xyz = new AnimationCurve[3] {
//            AnimationCurve.Constant(0.0f,0.0f,reel_spin_speed_direction.magnitude), //todo calculate time to move over position in path v#
//            AnimationCurve.Constant(0.0f,0.0f,reel_spin_speed_direction.magnitude),
//            AnimationCurve.Constant(0.0f,0.0f,reel_spin_speed_direction.magnitude)
//            };
//        }
//        return _looping_curves_xyz;
//    }
//    set
//    {
//        _looping_curves_xyz = value;
//    }
//}

/// <summary>
/// end spin easing
/// </summary>
//public AnimationCurve[] end_spin_curve_xyz;
*/
        /// <summary>
        /// Draws debug GUI for User to see data in action
        /// </summary>
        public void OnDrawGizmos()
        {
            //Uncomment to see local positions reference for positions_in_path
            DrawDebugForPositionsOnPath();
        }    
        /// <summary>
        /// Used to draw positions in path
        /// </summary>
        private void DrawDebugForPositionsOnPath()
        {
            if(positions_in_path_v3_local != null)
            { 
            if (positions_in_path_v3_local.Length > 0)
                {
                    for (int i = 0; i < positions_in_path_v3_local.Length; i++)
                    {
                        if (i == 0)
                            Gizmos.DrawSphere(transform.TransformPoint(positions_in_path_v3_local[i]), 100f);//String.Format("Slot Start Position {0}", i));
                        else if (i == (positions_in_path_v3_local.Length - 1))
                            Gizmos.DrawSphere(transform.TransformPoint(positions_in_path_v3_local[i]), 100f);//String.Format("Slot End Position {0}", i));
                        else
                            Gizmos.DrawSphere(transform.TransformPoint(positions_in_path_v3_local[i]), 100f);//String.Format("Slot Display Position {0}", i));
                    }
                }
            }
        }
        /// <summary>
        /// Initialize the number of slots in reel - usually called when generating reelstrips for the first time
        /// </summary>
        /// <param name="display_zones">The various display zones in the matrix</param>
        /// <param name="matrix_parent">Slot size and matrix settings</param>
        /// <param name="before_display_zones_slot_obejcts">number of slot objects to generate before first display_zone</param>
        /// <param name="after_display_zones_position_padding">number of empty position slots to generate after display zones</param>
        /// <returns></returns>
        public async Task SetReelStripInfoTo(ReelStripStructDisplayZone[] display_zones, Matrix matrix_parent, int before_display_zones_slot_obejcts, int after_display_zones_position_padding)
        {
            //Set the matrix parent to get settings from
            this._matrix = matrix_parent;
        }
        /// <summary>
        /// Sets the display slots and number of positions to move after leaving reel before returning to start
        /// </summary>
        /// <param name="number_of_slots">Number of display slots</param>
        /// <param name="start_slot_padding">How many slots to generate ontop of the display slot to move onto the reel</param>
        /// <param name="ending_slot_position_padding">How many positions extra to have reelstrip spinn off until slot object moves to top of reel</param>
        internal void SetReelConfigurationTo(ReelStripStruct reelstrip_info)
        {
            this.reelstrip_info = reelstrip_info;
        }
        /// <summary>
        /// updates number of positions to store reference for based on reel length
        /// </summary>
        /// <param name="positions_to_generate_path_for">how many slots in reelstrip to save position reference</param>
        private void UpdateLocalPositionsInPath(int positions_to_generate_path_for)
        {
            //Setup the positions the slot will hit along path. Based on size and padding
            positions_in_path_v3_local = new Vector3[positions_to_generate_path_for]; //Spin into empty slots then move to top
            //get slots_in_reel: destroy slots if to many - add slots if to few
            for (int position_in_reel = 0; position_in_reel < positions_in_path_v3_local.Length; position_in_reel++)
            {
                positions_in_path_v3_local[position_in_reel] = GenerateSlotPositionBasedOnPositionInreel(position_in_reel);
            }
        }

        /// <summary>
        /// Set the number of slot objects in reel
        /// </summary>
        /// <param name="display_zones">Display Slots in reel</param>
        /// <param name="before_display_slots">amount of slots before display slots to generate objects for - minimum 1</param>
        internal void UpdateSlotObjectsInReelStrip(int slots_in_reelstrip)
        {
            List<SlotManager> slots_in_reel = new List<SlotManager>();
            if (this.slots_in_reel == null)
            {
                SlotManager[] slots_initialized = transform.GetComponentsInChildren<SlotManager>();
                if (slots_initialized.Length > 0)
                {
                    this.slots_in_reel = slots_initialized;
                }
                else
                {
                    this.slots_in_reel = new SlotManager[0];
                }
            }
            slots_in_reel.AddRange(this.slots_in_reel);

            int total_slot_objects_required = slots_in_reelstrip;

            SetSlotObjectsInReelTo(ref slots_in_reel, total_slot_objects_required);

            this.slots_in_reel = slots_in_reel.ToArray();
        }

        internal void SetSlotObjectsInReelTo(ref List<SlotManager> slots_in_reel, int total_slot_objects_required)
        {

            //Do we need to add or remove display slot objects on reelstrip
            bool add_substract = slots_in_reel.Count < total_slot_objects_required ? true : false;

            //Either remove from end or add until we have the amount of display and before display slot obejcts
            for (int slot_to_update = add_substract ? slots_in_reel.Count : slots_in_reel.Count - 1; //Set to current count to add or count - 1 to subtract
                add_substract ? slot_to_update < total_slot_objects_required : slot_to_update >= total_slot_objects_required; // either add until you have required slot objects or remove
                slot_to_update += add_substract ? 1 : -1) //count up or down
            {
                if (add_substract)
                {
                    slots_in_reel.Add(GenerateSlotObject(slot_to_update));
                }
                else
                {
                    DestroyImmediate(slots_in_reel[slot_to_update].gameObject);
                    slots_in_reel.RemoveAt(slot_to_update);
                }
            }
        }

        internal bool AreSlotsInEndPosition()
        {
            bool output = false;
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                if(slots_in_reel[i].slot_in_end_position)
                {
                    if (i == slots_in_reel.Length - 1)
                        output = true;
                }
            }
            return output;
        }

        /// <summary>
        /// Used by the matrix to set display symbols
        /// </summary>
        /// <param name="reelstrip"></param>
        internal void SetEndingDisplaySymbolsTo(ReelStripStruct reelstrip)
        {
            //SetEndingSymbolsTo(reelstrip.display_symbols);
        }

        internal List<SlotManager> GetSlotsDecending()
        {
            List<SlotManager> output = new List<SlotManager>();
            for (int position_to_check = 0; position_to_check < positions_in_path_v3_local.Length; position_to_check++)
            {
                for (int slot = 0; slot < slots_in_reel.Length; slot++)
                {
                    if(slots_in_reel[slot].transform.localPosition == positions_in_path_v3_local[position_to_check])
                        output.Add(slots_in_reel[slot]);
                }
            }
            return output;
        }

        internal void SetSymbolCurrentDisplayTo(ReelStripStruct reelStripStruct)
        {
            SlotManager[] slots_decending_order = GetSlotsDecending().ToArray();
            List<Symbol> symbols_to_display = new List<Symbol>();
            for (int symbol = 0; symbol < reelStripStruct.spin_info.display_symbols.Length; symbol++)
            {
                symbols_to_display.Add((Symbol)reelStripStruct.spin_info.display_symbols[symbol]);
            }
            SetEndingSymbolsTo(symbols_to_display.ToArray());
            for (int slot = 1; slot < slots_decending_order.Length; slot++)
            {
                slots_decending_order[slot].SetDisplaySymbolTo(reelStripStruct.spin_info.display_symbols[slot-1]);
                end_symbols_set_from_config += 1;
            }
        }

        internal void SetSpinParametersTo(ReelStripSpinParametersScriptableObject spin_parameters)
        {
            ReelStripStruct new_reelstrip_info = reelstrip_info;
            new_reelstrip_info.spin_parameters = spin_parameters;
            reelstrip_info = new_reelstrip_info;
        }

        /// <summary>
        /// Generates a Slot Gameobject
        /// </summary>
        /// <param name="slot_position_in_reel">the slot in reel generating the object for</param>
        /// <returns></returns>
        private SlotManager GenerateSlotObject(int slot_position_in_reel)
        {
            //Local positions are already generated by this point
            Vector3 slot_position_on_path = positions_in_path_v3_local[slot_position_in_reel];
            SlotManager generated_slot = InstantiateSlotGameobject(slot_position_in_reel, this, slot_position_on_path, Vector3.one,Quaternion.identity);
            generated_slot.reel_parent = this;
            //Generate a random symbol prefab
            generated_slot.ShowRandomSymbol();
            positions_in_path_v3_local[slot_position_in_reel] = slot_position_on_path;
            return generated_slot;
        }

        internal void RegenerateSlotObjects()
        {
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                Debug.Log(String.Format("Reel {0} deleteing slot {1}",reelstrip_info.reel_number, slot));
                if(slots_in_reel[slot] != null)
                    DestroyImmediate(slots_in_reel[slot].gameObject);
                slots_in_reel[slot] = GenerateSlotObject(slot);
            }
        }

        public void UpdatePositionInPathForDirection()
        {
            //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
            if(reelstrip_info.spin_parameters.reel_spin_direction.y < 0)
                for (int i = 0; i < positions_in_path_v3_local.Length; i++)
                {
                    float positions_in_path_v3_y = -Math.Abs(positions_in_path_v3_local[i].y);
                    positions_in_path_v3_local[i] = new Vector3(Math.Abs(positions_in_path_v3_local[i].x),positions_in_path_v3_y,0);
                }
            //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
            if (reelstrip_info.spin_parameters.reel_spin_direction.y > 0)
                for (int i = 0; i < positions_in_path_v3_local.Length; i++)
                {
                    positions_in_path_v3_local[i] = new Vector3(Math.Abs(positions_in_path_v3_local[i].x), Math.Abs(positions_in_path_v3_local[i].y), 0);
                }
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                slots_in_reel[i].transform.localPosition = GenerateSlotPositionBasedOnPositionInreel(i);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot_number"></param>
        /// <param name="parent_reel"></param>
        /// <param name="start_position"></param>
        /// <param name="scale"></param>
        /// <returns>Slot Manager Reference</returns>
        internal SlotManager InstantiateSlotGameobject(int slot_number, ReelStripManager parent_reel, Vector3 start_position, Vector3 scale, Quaternion start_rotation)
        {
#if UNITY_EDITOR
            GameObject ReturnValue = PrefabUtility.InstantiatePrefab(Resources.Load("Core/Prefabs/Slot-Container")) as GameObject; // TODO Refactor to include custom sot container passable argument
            ReturnValue.gameObject.name = String.Format("Slot_{0}",slot_number);
            ReturnValue.transform.parent = parent_reel.transform;
            SlotManager return_component = ReturnValue.GetComponent<SlotManager>();
            //ReturnValue.transform.GetChild(0).localScale = scale;
            ReturnValue.transform.localPosition = start_position;
            ReturnValue.transform.localRotation = start_rotation;
            return return_component;
#endif
            //Intended - we want to only instantiate these objects in unity_editor
            return null;
        }
        /// <summary>
        /// Spin the Reels
        /// </summary>
        /// <returns>async task to track</returns>
        public Task SpinReel()
        {
            Debug.Log(string.Format("Spinning reel {0}",reelstrip_info.reel_number));
            InitializeVarsForNewSpin();
            //When reel is generated it's vector3[] path is generated for reference from slots
            SetSpinStateTo(SpinStates.spin_start);
            reel_spin_speed_current = reelstrip_info.spin_parameters.spin_speed_constant;

            //TODO hooks for reel state machine
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slots_in_reel[i].StartSpin(); // Tween to the same position then evaluate
            }
            //Task.Delay(time_to_enter_loop);
            //TODO Implement Ease In for Starting spin
            //TODO refactor check for interupt state
            SetSpinStateTo(SpinStates.spin_idle);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Spin the Reels
        /// </summary>
        /// <returns>async task to track</returns>
        public void SpinReelsNow()
        {
            InitializeVarsForNewSpin();
            //When reel is generated it's vector3[] path is generated for reference from slots
            SetSpinStateTo(SpinStates.spin_start);
            //TODO hooks for reel state machine
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slots_in_reel[i].StartSpin(); // Tween to the same position then evaluate
            }
            //Task.Delay(time_to_enter_loop);
            //TODO Implement Ease In for Starting spin
            //TODO refactor check for interupt state
            SetSpinStateTo(SpinStates.spin_idle);
        }
        /// <summary>
        /// Sets the Spin State to state
        /// </summary>
        /// <param name="state">SpinStates state to set reel to</param>
        private void SetSpinStateTo(SpinStates state)
        {
            current_spin_state = state;
        }
        /// <summary>
        /// Initializes variables requires for a new spin
        /// </summary>
        private void InitializeVarsForNewSpin()
        {
            ending_symbols = null;
        }
        /// <summary>
        /// Set the slots in reel movement
        /// </summary>
        /// <param name="enable_disable">Set Slot movement enabled or disabled</param>
        internal void SetSlotsMovementEnabled(bool enable_disable)
        {
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                slots_in_reel[i].SetSlotMovementEnabledTo(enable_disable);
            }
        }
        /// <summary>
        /// Set slots to Stop Spin
        /// </summary>
        private void SetSlotsToStopSpinning()
        {
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slots_in_reel[i].SetToStopSpin(); // Tween to the same position then evaluate\
            }
        }
        /// <summary>
        /// Set reel into end reel state Test only
        /// </summary>
        public void StopReelTest()
        {
            StopReel(new int[3] { 0,  1, 2 });
        }
        /// <summary>
        /// Sets the reel to end state and slots to end configuration
        /// </summary>
        public async Task StopReel(ReelStripStruct reelStrip)
        {
            end_symbols_set_from_config = 0;
            //Set State to spin outro
            SetSpinStateTo(SpinStates.spin_outro);
            //Waits until all slots have stopped spinning
            await StopReel(reelStrip.spin_info.display_symbols); //This will control ho wfast the reel goes to stop spin
            SetSpinStateTo(SpinStates.spin_end);
        }

        /// <summary>
        /// Stop the reel and set ending symbols
        /// </summary>
        /// <param name="ending_symbols">the symbols to land on</param>
        public async Task StopReel(int[] ending_symbols)
        {
            SetEndingSymbolsTo(ending_symbols);
            //When reel is generated it's vector3[] path is generated for reference from slots
            SetSlotsToStopSpinning(); //When slots move to the top of the reel then assign the next symbol in list as name and delete from list
            await AllSlotsStoppedSpinning();
            Debug.Log(String.Format("All slots stopped spinning for reel {0}",transform.name));
        }
        internal async Task AllSlotsStoppedSpinning()
        {
            bool task_lock = true;
            while (task_lock)
            {
                if (are_slots_spinning)
                    await Task.Delay(100);
                else
                {
                    task_lock = false;
                }
            }
        }

        internal bool are_slots_spinning
        {
            get
            {
                bool output = true;
                for (int i = 0; i < slots_in_reel.Length; i++)
                {
                    if (!slots_in_reel[i].slot_in_end_position)
                    {
                        break;
                    }
                    if (i == slots_in_reel.Length - 1)
                    {
                        output = false;
                    }
                }
                return output;
            }
        }
        /// <summary>
        /// Set Ending Symbols variable
        /// </summary>
        /// <param name="ending_symbols">ending symbols for reelstrip</param>
        private void SetEndingSymbolsTo(int[] ending_symbols)
        {
            this.ending_symbols = Array.ConvertAll(ending_symbols, value => (Symbol)value);
        }

        private void SetEndingSymbolsTo(Symbol[] ending_symbols)
        {
            this.ending_symbols = ending_symbols;
        }

        /// <summary>
        /// Generates a position for a slot in reel
        /// </summary>
        /// <param name="position_in_reel">Position in Reel slot will be</param>
        /// <returns>Slot Position Vector3</returns>
        internal Vector3 GenerateSlotPositionBasedOnPositionInreel(int position_in_reel)
        {
            //TODO Base on Spin Direction
            Debug.Log(String.Format("reelstrip_info.reel_number = {0} slot size = ({1},{2}) matrix padding.x = {3}", reelstrip_info.reel_number, matrix.slot_size.x, matrix.slot_size.y, matrix.padding.x));
            float x = reelstrip_info.reel_number * (matrix.slot_size.x + matrix.padding.x);
            float y = -(position_in_reel * (matrix.slot_size.y + matrix.padding.y));
            Debug.Log(string.Format("Local Position for slot {0} int reel {1} is ({2},{3},{4}) ",position_in_reel, transform.name,x.ToString(), y.ToString(),0));
            Vector3 return_position = new Vector3(x, y, 0);
            return return_position;
        }
        /// <summary>
        /// Sets the slots as referenced to the position in the reel
        /// </summary>
        internal void SetSlotPositionToStart()
        {
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                slots_in_reel[i].transform.localPosition = GenerateSlotPositionBasedOnPositionInreel(i);
            }
        }

        internal void ClearReelSlots()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }
        /// <summary>
        /// Sets the slot spin speeds
        /// </summary>
        /// <param name="to_speed"></param>
        internal void SetSpinSpeedTo(float to_speed)
        {
            reel_spin_speed_current = to_speed;
        }
        /// <summary>
        /// Sets all slot managers sub state machines to trigger and set bools
        /// </summary>
        internal void UpdateSlotManagersSubStateMachines()
        {
            for (int i = 0; i < slots_in_reel?.Length; i++)
            {
                slots_in_reel[i].SetSubStateMachineAnimators();
                slots_in_reel[i].SetAllSubSymbolsGameobjectActive();
            }
            matrix._slot_machine_managers.end_configuration_manager.SetMatrixToReelConfiguration();
        }

        internal void SetAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                slots_in_reel[slot].SetAllSubStateAnimators();
            }
        }

        internal void ClearAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                slots_in_reel[slot].ClearAllSubStateAnimators();
            }
        }

        internal string[] ReturnAllKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                keys.AddRange(slots_in_reel[slot].state_machine.animator_state_machines.sub_state_machines_keys);
            }
            return keys.ToArray();
        }

        internal AnimatorSubStateMachine[] ReturnAllValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                values.AddRange(slots_in_reel[slot].state_machine.animator_state_machines.sub_state_machines_values.sub_state_machine);
            }
            return values.ToArray();
        }

        internal void SetAllSlotContainersAnimatorSyncStates()
        {
            for (int slot = 0; slot < slots_in_reel.Length; slot++)
            {
                slots_in_reel[slot].SetStateMachineAnimators();
            }
        }
    }
}
