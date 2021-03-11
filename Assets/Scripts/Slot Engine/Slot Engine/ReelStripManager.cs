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
        SerializedProperty display_slots;
        SerializedProperty reel_number;
        SerializedProperty positions_in_path_v3;
        SerializedProperty use_ease_inOut_spin;
        SerializedProperty ending_symbols;

        public void OnEnable()
        {
            my_target = (ReelStripManager)target;
            reel_spin_speed_current = serializedObject.FindProperty("reel_spin_speed_current");
            positions_in_path_v3 = serializedObject.FindProperty("positions_in_path_v3");
            use_ease_inOut_spin = serializedObject.FindProperty("use_ease_inOut_spin");
            reel_number = serializedObject.FindProperty("reel_number");
            display_slots = serializedObject.FindProperty("display_slots");
            ending_symbols = serializedObject.FindProperty("ending_symbols");
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Controls");
            if (my_target.is_reel_spinning)
            {
                if (GUILayout.Button("Spin Reel Test"))
                {
                    my_target.SpinReel();
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
            EditorGUILayout.LabelField(String.Format("Spin Properties for Reel {0}", reel_number.intValue));
            EditorGUILayout.LabelField(String.Format("Slots in display area = {0}", display_slots.intValue));
            if (my_target.is_reel_spinning)
            {
                EditorGUILayout.LabelField("Current Slot speed = " + reel_spin_speed_current.floatValue.ToString());
            }
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
        internal Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.parent.GetComponent<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        internal Matrix _matrix;
        [SerializeField]
        internal int reel_number = 0;
        /// <summary>
        /// returns viewable slots - static number right now
        /// </summary>
        /// //TODO refactor and figure out logic of how to get viewable slots
        [SerializeField]
        internal int display_slots = 3;
        //Controls amount of slots showing on reel
        internal int padding_slots_top = 1;
        internal int padding_slots_bottom = 1;
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
        /// The direction to spin the slots in. reel_spin_speed_direction * reelSpinSpeed will be the distance the slot travels
        /// </summary>
        public Vector3 reel_spin_speed_direction;

        /// <summary>
        /// Holds the reel strip to cycle thru symbols for spin and end symbol configuration for reel
        /// </summary>
        [SerializeField]
        public ReelStrip reel_strip_to_use_for_spin;
        /// <summary>
        /// On Spin Start uses a curve editor to determine speed over time - 0 -> -100 -> 0 -> +200 for example over for seconds is ease out
        /// </summary>
        public bool use_ease_inOut_spin = false;
        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[] positions_in_path_v3;
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
                switch (current_spin_state)
                {
                    case SpinStates.idle_idle:
                        return false;
                    case SpinStates.end:
                        return false;
                    default:
                        return true;
                }
            }
        }
        public int reel_strip_counter = 0;
        

        internal string ReturnNextSymbolInStrip()
        {
            string output = ((Symbol)reel_strip_to_use_for_spin.reel_spin_symbols[reel_strip_counter]).ToString();
            if(reel_strip_counter+1 >= reel_strip_to_use_for_spin.reel_spin_symbols.Length)
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
            DrawDebugForPositionsOnPath();
        }    
        /// <summary>
        /// Used to draw positions in path
        /// </summary>
        private void DrawDebugForPositionsOnPath()
        {
            if (positions_in_path_v3 != null)
            { 
            if (positions_in_path_v3.Length > 0)
                {
                    for (int i = 0; i < positions_in_path_v3.Length; i++)
                    {
                        if (i == 0)
                            Gizmos.DrawSphere(positions_in_path_v3[i], .5f);//String.Format("Slot Start Position {0}", i));
                        else if (i == (positions_in_path_v3.Length - 1))
                            Gizmos.DrawSphere(positions_in_path_v3[i], .5f);//String.Format("Slot End Position {0}", i));
                        else
                            Gizmos.DrawSphere(positions_in_path_v3[i], .5f);//String.Format("Slot Display Position {0}", i));
                    }
                }
            }
        }
        /// <summary>
        /// Initialize the number of slots in reel
        /// </summary>
        /// <param name="number_of_slots">Determine direction of slots</param>
        /// <param name="matrix_settings">Slot size and matrix settings</param>
        /// <returns>task that can be awaited</returns>
        public async Task InitializeSlotsInReel(Vector3 number_of_slots, Matrix matrix_settings, int ending_slot_padding)
        {
            //Set the matrix parent to get settings from
            this._matrix = matrix_settings;
            //TODO Refactor to support omni direction reel generation
            display_slots = (int)number_of_slots.y;
            int ending_count = (int)number_of_slots.y + (int)matrix.reel_slot_padding.y;
            //Setup the positions the slot will hit along path. Based on size and padding
            positions_in_path_v3 = new Vector3[ending_count + ending_slot_padding];//Spin into empty slot then move to top
            //get slots_in_reel: destroy slots if to many - add slots if to few
            SetSlotObjectsInReelTo(ending_count);
            Vector3 reel_spin_off_position = GetSlotPositionBasedOnReelPosition(positions_in_path_v3.Length - 1);
            positions_in_path_v3[positions_in_path_v3.Length - 1] = reel_spin_off_position;
        }

        internal void SetSpinDirectionTo(Vector3 new_direction)
        {
            reel_spin_speed_direction = new_direction;
            UpdatePositionInPathForDirection();
        }

        /// <summary>
        /// Set the number of slot objects in reel
        /// </summary>
        /// <param name="to_slots"></param>
        private void SetSlotObjectsInReelTo(int to_slots)
        {
            List<SlotManager> slots_in_reel = new List<SlotManager>();
            if (this.slots_in_reel != null)
                slots_in_reel.AddRange(this.slots_in_reel);
            bool add_substract = slots_in_reel.Count < to_slots ? true : false;
            for (int position_in_reel = add_substract ? slots_in_reel.Count : slots_in_reel.Count-1;
                add_substract ? position_in_reel < to_slots : position_in_reel >= to_slots;
                position_in_reel = add_substract ? position_in_reel++: position_in_reel--)
            {
                if(add_substract)
                {
                    slots_in_reel.Add(GenerateSlotObject(position_in_reel));
                }
                else
                {
                    Destroy(slots_in_reel[position_in_reel].gameObject);
                    slots_in_reel.RemoveAt(position_in_reel);
                }
            }
            this.slots_in_reel = slots_in_reel.ToArray();
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
        /// Assumes the reels haven't spun - once the reels spin you need to re-order slots_in_reel to decend based on position of slots
        /// </summary>
        internal void TestDisplayEndSymbols()
        {
            for (int i = slots_in_reel.Length - 1; i > 0; i--)
            {
                slots_in_reel[i].SetDisplaySymbolTo((int)ending_symbols[i]);
            }
        }
        /// <summary>
        /// Used by the matrix to set display symbols
        /// </summary>
        /// <param name="reelstrip"></param>
        internal void SetEndingDisplaySymbolsTo(ReelStrip reelstrip)
        {
            SetEndingSymbolsTo(reelstrip.display_symbols);
        }

        internal List<SlotManager> GetSlotsDecending()
        {
            List<SlotManager> output = new List<SlotManager>();
            for (int position_to_check = 0; position_to_check < positions_in_path_v3.Length; position_to_check++)
            {
                for (int slot = 0; slot < slots_in_reel.Length; slot++)
                {
                    if(slots_in_reel[slot].transform.localPosition == positions_in_path_v3[position_to_check])
                        output.Add(slots_in_reel[slot]);
                }
            }
            return output;
        }

        /// <summary>
        /// Generates a Slot Gameobject
        /// </summary>
        /// <param name="slot_position_in_reel">the slot in reel generating the object for</param>
        /// <returns></returns>
        private SlotManager GenerateSlotObject(int slot_position_in_reel)
        {
            Vector3 slot_position_on_path = GetSlotPositionBasedOnReelPosition(slot_position_in_reel);
            SlotManager generated_slot = InstantiateSlotGameobject(slot_position_in_reel, this, slot_position_on_path, Vector3.one);
            generated_slot.reel_parent = this;
            positions_in_path_v3[slot_position_in_reel] = slot_position_on_path;
            return generated_slot;
        }

        public void UpdatePositionInPathForDirection()
        {
            //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
            if(reel_spin_speed_direction.y < 0)
                for (int i = 0; i < positions_in_path_v3.Length; i++)
                {
                    float positions_in_path_v3_y = -Math.Abs(positions_in_path_v3[i].y);
                    positions_in_path_v3[i] = new Vector3(Math.Abs(positions_in_path_v3[i].x),positions_in_path_v3_y,0);
                }
            //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
            if (reel_spin_speed_direction.y > 0)
                for (int i = 0; i < positions_in_path_v3.Length; i++)
                {
                    positions_in_path_v3[i] = new Vector3(Math.Abs(positions_in_path_v3[i].x), Math.Abs(positions_in_path_v3[i].y), 0);
                }
            for (int i = 0; i < slots_in_reel.Length; i++)
            {
                slots_in_reel[i].transform.localPosition = GetSlotPositionBasedOnReelPosition(i);
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
        internal SlotManager InstantiateSlotGameobject(int slot_number, ReelStripManager parent_reel, Vector3 start_position, Vector3 scale)
        {
            GameObject ReturnValue = Instantiate(Resources.Load("Prefabs/Slot")) as GameObject; // TODO Refactor to include custom sot container passable argument
            ReturnValue.gameObject.name = String.Format("Slot_{0}",slot_number);
            ReturnValue.transform.parent = parent_reel.transform;
            ReturnValue.transform.GetChild(0).localScale = scale;
            ReturnValue.transform.localPosition = start_position;
            return ReturnValue.GetComponent<SlotManager>();
        }
        /// <summary>
        /// Spin the Reels
        /// </summary>
        /// <returns>async task to track</returns>
        public async Task SpinReel()
        {
            if (!is_reel_spinning)
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
            StopReel(new ReelStrip(new int[3] { 0, 1, 2 }));
        }
        /// <summary>
        /// Sets the reel to end state and slots to end configuration
        /// </summary>
        public async Task StopReel(ReelStrip reelStrip)
        {
            end_symbols_set_from_config = 0;
            SetSpinStateTo(SpinStates.spin_outro);
            await StopReel(reelStrip.display_symbols); //This will control ho wfast the reel goes to stop spin
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

        /// <summary>
        /// Generates a position for a slot in reel
        /// </summary>
        /// <param name="position_in_reel">Position in Reel slot will be</param>
        /// <returns>Slot Position Vector3</returns>
        Vector3 GetSlotPositionBasedOnReelPosition(int position_in_reel)
        {
            //Change later to enter customizes reel starting height (Matrix 3x4x5x4x3)
            //Need To Determine How many Slots are in the Reel and calculate the iExtraSlotsPerReel (-1 to include the end slot not being active)
            //of the reel into the starting Y Position
            //TODO refactor to include which direction building only supports left to right atm
            float x = reel_number * matrix.slot_size.x + reel_number * matrix.padding.x;
            float y = (position_in_reel * matrix.slot_size.y + position_in_reel * matrix.padding.y) * reel_spin_speed_direction.y;
            Debug.Log("Generate Local Position = " + x.ToString() + " , " + y.ToString() + "," + 0);
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
                slots_in_reel[i].transform.position = GetSlotPositionBasedOnReelPosition(i);
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
    }
}
