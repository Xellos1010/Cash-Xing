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
    [CustomEditor(typeof(StripManager))]
    class ReelStripManagerEditor : BoomSportsEditor
    {
        StripManager my_target;
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
            my_target = (StripManager)target;
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
                    my_target.UpdateStripPositions();
                }
                if (GUILayout.Button("Update sub state machines slot managers"))
                {
                    my_target.UpdateSlotManagersSubStateMachines();
                }
            }
            if (my_target.isReelSpinning)
            {
                if (GUILayout.Button("Spin Reel Test"))
                {
                    my_target.SpinReelsNow();
                }
                
            }
            else
            {
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
    public partial class ConfigurationGroupManager : MonoBehaviour
    {

    }

    public class StripManager : ConfigurationGroupManager
    {
        public delegate void StripEvent(int stripNumber);
        public event StripEvent reelStartSpin;
        public event StripEvent reelStopSpin;
        [SerializeField]
        internal SpinStates current_spin_state;
        //the matrix associated with the reel_strip
        internal ReelStripConfigurationObject configurationObjectParent
        {
            get
            {
                if (_matrix == null)
                    _matrix = transform.GetComponentInParent<ReelStripConfigurationObject>();
                return _matrix;
            }
        }
        [SerializeField]
        internal ReelStripConfigurationObject _matrix;
        /// <summary>
        /// Slot managers in the reel strip
        /// </summary>
        [SerializeField] //If base inspector enabled can check references
        internal SlotManager[] slotsInStrip;

        /// <summary>
        /// Distance for animation to travel when movement is enabled
        /// </summary>
        [SerializeField] //If base inspector enabled can check references
        internal float reel_spin_speed_current;

        /// <summary>
        /// Holds the reel strip to cycle thru symbols for spin and end symbol configuration for reel
        /// </summary>
        [SerializeField]
        internal StripStruct stripInfo;

        internal void UpdateStripPositions()
        {
            Debug.LogWarning($"reelstrip_info.total_positions = {stripInfo.total_positions} reelstrip_info.total_slot_objects = {stripInfo.total_slot_objects}");
            UpdateLocalPositionsInPath(stripInfo.total_positions);
        }

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
        internal NodeDisplaySymbol[] ending_symbols;
        /// <summary>
        /// Enables you to change the symbol graphic when slot exits the viewable area of a configuration to a predefined strip or random draw weighte distribution symbol
        /// </summary>
        public bool randomSetSymbolsOnTraverseReel = true;
        /// <summary>
        /// UI Indicator to ensure operation for setting symbols to end configuration has performed
        /// </summary>
        internal int endSymbolsSetFromConfiguration = 0;
        
        /// <summary>
        /// is the reel in a spin state
        /// </summary>
        internal bool isReelSpinning
        {
            get
            {
                bool is_spinning = true;
                if (slotsInStrip != null)
                {
                    for (int slot = 0; slot <= slotsInStrip.Length; slot++)
                    {
                        if (slot < slotsInStrip.Length)
                        {
                            if (slotsInStrip[slot] != null)
                            {
                                if (slot == slotsInStrip.Length)
                                {
                                    is_spinning = false;
                                    break;
                                }
                                if (slotsInStrip[slot].movement_enabled)
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
        internal NodeDisplaySymbol ReturnNextSymbolInStrip()
        {
            int stripCounter = 0;
            NodeDisplaySymbol output = stripInfo.spin_info.stripSpinSymbols[stripCounter];
            if(stripCounter+1 >= stripInfo.spin_info.stripSpinSymbols.Length)
            {
                stripCounter = 0;
            }
            else
            {
                stripCounter += 1;
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
        public async Task SetReelStripInfoTo(ReelStripStructDisplayZone[] display_zones, ReelStripConfigurationObject matrix_parent, int before_display_zones_slot_obejcts, int after_display_zones_position_padding)
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
        internal void SetReelConfigurationTo(StripStruct reelstrip_info)
        {
            this.stripInfo = reelstrip_info;
            UpdateStripPositions();
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
                positions_in_path_v3_local[position_in_reel] = GetSlotPositionInStrip(position_in_reel);
                //Debug.LogWarning($"positions_in_path_v3_local[position_in_reel] Generated for strip {gameObject.name} = {positions_in_path_v3_local[position_in_reel].ToString()}");
            }
        }

        internal bool AreSlotsInEndPosition()
        {
            bool output = false;
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                if(slotsInStrip[i].slot_in_end_position)
                {
                    if (i == slotsInStrip.Length - 1)
                        output = true;
                }
            }
            return output;
        }

        internal List<SlotManager> GetSlotsDecending()
        {
            List<SlotManager> output = new List<SlotManager>();
            Debug.Log($"positions_in_path_v3_local.Length == {positions_in_path_v3_local.Length} - slotsInStrip.Length == {slotsInStrip.Length}");
            for (int position_to_check = 0; position_to_check < positions_in_path_v3_local.Length; position_to_check++)
            {
                for (int slot = 0; slot < slotsInStrip.Length; slot++)
                {
                    //Debug.Log($"slotsInStrip[slot].transform.localPosition {slotsInStrip[slot].transform.localPosition} == {positions_in_path_v3_local[position_to_check]} positions_in_path_v3_local[position_to_check] is {slotsInStrip[slot].transform.localPosition == positions_in_path_v3_local[position_to_check]}");
                    if(slotsInStrip[slot].transform.localPosition == positions_in_path_v3_local[position_to_check])
                        output.Add(slotsInStrip[slot]);
                }
            }
            return output;
        }

        internal void SetSymbolCurrentDisplayTo(StripSpinStruct reelStripStruct)
        {
            endSymbolsSetFromConfiguration = 0;
            SlotManager[] slotsDecendingOrder = GetSlotsDecending().ToArray();
            Debug.Log($"{gameObject.name} slotsDecendingOrder.Length = {slotsDecendingOrder.Length} Slot Order = {PrintSlotObjectNames(slotsDecendingOrder)}");
            List<NodeDisplaySymbol> symbolsToDisplay = new List<NodeDisplaySymbol>();
            for (int symbol = 0; symbol < reelStripStruct.displaySymbols.Length; symbol++)
            {
                symbolsToDisplay.Add(reelStripStruct.displaySymbols[symbol]);
            }
            Debug.Log($"symbolsToDisplay.Count = {symbolsToDisplay.Count}");

            SetEndingSymbolsTo(symbolsToDisplay.ToArray());
            for (int slot = GetPaddingBeforeStrip(stripInfo.stripColumn); slot < slotsDecendingOrder.Length; slot++)
            {
                Debug.Log($"Setting {slotsDecendingOrder[slot].gameObject.name} to symbol reelStripStruct.displaySymbols[{endSymbolsSetFromConfiguration}]");
                slotsDecendingOrder[slot].SetDisplaySymbolTo(reelStripStruct.displaySymbols[endSymbolsSetFromConfiguration]);
                endSymbolsSetFromConfiguration += 1;
            }
        }

        private int GetPaddingBeforeStrip(int stripColumn)
        {
            int output = 0;
            output = configurationObjectParent.GetPaddingBeforeStrip(stripColumn);
            Debug.Log($"Padding for strip {stripColumn} is {output}");
            return output;
        }

        private object PrintSlotObjectNames(SlotManager[] slotsList)
        {
            List<string> output = new List<string>();
            for (int slot = 0; slot < slotsList.Length; slot++)
            {
                output.Add(slotsList[slot].gameObject.name);
            }
            return String.Join("|", output);
        }
        
        //public void UpdatePositionInPathForDirection()
        //{
        //    //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
        //    if(reelstrip_info.spinParameters.reel_spin_direction.y < 0)
        //        for (int i = 0; i < positions_in_path_v3_local.Length; i++)
        //        {
        //            float positions_in_path_v3_y = -Math.Abs(positions_in_path_v3_local[i].y);
        //            positions_in_path_v3_local[i] = new Vector3(Math.Abs(positions_in_path_v3_local[i].x),positions_in_path_v3_y,0);
        //        }
        //    //Right now only support up or down. If direction y > 0 then spin up, < 0 spin down
        //    if (reelstrip_info.spinParameters.reel_spin_direction.y > 0)
        //        for (int i = 0; i < positions_in_path_v3_local.Length; i++)
        //        {
        //            positions_in_path_v3_local[i] = new Vector3(Math.Abs(positions_in_path_v3_local[i].x), Math.Abs(positions_in_path_v3_local[i].y), 0);
        //        }
        //    for (int i = 0; i < slots_in_reel.Length; i++)
        //    {
        //        slots_in_reel[i].transform.localPosition = GenerateSlotPositionBasedOnPositionInReel(i);
        //    }
        //}
        
        /// <summary>
        /// Spin the Reels
        /// </summary>
        /// <returns>async task to track</returns>
        public Task StartSpin()
        {
            reelStartSpin?.Invoke(stripInfo.stripColumn);
            //Debug.Log(string.Format("Spinning reel {0}",reelstrip_info.reel_number));
            InitializeVarsForNewSpin();

            SetSpinStateTo(SpinStates.spin_start);
            StripSpinDirectionalConstantEvaluatorScriptableObject temp = stripInfo.GetSpinParametersAs() as StripSpinDirectionalConstantEvaluatorScriptableObject;
            reel_spin_speed_current = temp.spin_speed_constant;

            //TODO hooks for reel state machine
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slotsInStrip[i].StartSpin(); // Tween to the same position then evaluate
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
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slotsInStrip[i].StartSpin(); // Tween to the same position then evaluate
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
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                slotsInStrip[i].SetSlotMovementEnabledTo(enable_disable);
            }
        }
        /// <summary>
        /// Set slots to Stop Spin
        /// </summary>
        private void SetSlotsToStopSpinning()
        {
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                slotsInStrip[i].SetToStopSpin(); // Tween to the same position then evaluate\
            }
        }
        /// <summary>
        /// Sets the reel to end state and slots to end configuration
        /// </summary>
        public async Task StopReel(StripSpinStruct reelStrip)
        {
            endSymbolsSetFromConfiguration = 0;
            //Set State to spin outro
            SetSpinStateTo(SpinStates.spin_outro);
            //Waits until all slots have stopped spinning
            await StopReel(reelStrip.displaySymbols); //This will control ho wfast the reel goes to stop spin
            SetSpinStateTo(SpinStates.spin_end);
        }

        /// <summary>
        /// Stop the reel and set ending symbols
        /// </summary>
        /// <param name="ending_symbols">the symbols to land on</param>
        public async Task StopReel(NodeDisplaySymbol[] ending_symbols)
        {
            SetEndingSymbolsTo(ending_symbols);
            SetSlotsToStopSpinning(); //When slots move to the top of the reel then assign the next symbol in list as name and delete from list
            await AllSlotsStoppedSpinning();
            reelStopSpin?.Invoke(stripInfo.stripColumn);
            //Debug.Log(String.Format("All slots stopped spinning for reel {0}",transform.name));
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
                for (int i = 0; i < slotsInStrip.Length; i++)
                {
                    if (!slotsInStrip[i].slot_in_end_position)
                    {
                        break;
                    }
                    if (i == slotsInStrip.Length - 1)
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
        /// <param name="endingSymbols">ending symbols for reelstrip</param>
        private void SetEndingSymbolsTo(NodeDisplaySymbol[] endingSymbols)
        {
            Debug.Log($"Setting End Symbols To {PrintNodeDisplaySymbolArray(endingSymbols)}");
            this.ending_symbols = endingSymbols;
        }

        private string PrintNodeDisplaySymbolArray(NodeDisplaySymbol[] endingSymbols)
        {
            List<int> output = new List<int>();
            for (int endingSymbol = 0; endingSymbol < endingSymbols.Length; endingSymbol++)
            {
                output.Add(endingSymbols[endingSymbol].primary_symbol);
            }
            return String.Join("|", output);
        }

        /// <summary>
        /// Generates a position for a slot in reel
        /// </summary>
        /// <param name="positionInStrip">Position in Reel slot will be</param>
        /// <returns>Slot Position Vector3</returns>
        internal Vector3 GetSlotPositionInStrip(int positionInStrip)
        {
            //TODO Base on Spin Direction
            Debug.Log(String.Format("reelstrip_info.reel_number = {0} slot size = ({1},{2}) matrix padding.x = {3}", stripInfo.stripColumn, configurationObjectParent.configurationSettings.slotSize.x, configurationObjectParent.configurationSettings.slotSize.y, configurationObjectParent.configurationSettings.slotPadding.x));
            float x = 0;//reelstrip_info.reel_number * (matrix.configurationSettings.slotSize.x + matrix.configurationSettings.slotPadding.x);//Uncomment to set slot position at slot level
            float y = -(positionInStrip * (configurationObjectParent.configurationSettings.slotSize.y + configurationObjectParent.configurationSettings.slotPadding.y));
            //Debug.Log(string.Format("Local Position for slot {0} int reel {1} is ({2},{3},{4}) ",positionInStrip, transform.name,x.ToString(), y.ToString(),0));
            Vector3 return_position = new Vector3(x, y, 0);
            return return_position;
        }

        /// <summary>
        /// Sets the slots as referenced to the position in the reel
        /// </summary>
        internal void SetSlotPositionToStart()
        {
            for (int i = 0; i < slotsInStrip.Length; i++)
            {
                slotsInStrip[i].transform.localPosition = GetSlotPositionInStrip(i);
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
            for (int i = 0; i < slotsInStrip?.Length; i++)
            {
                slotsInStrip[i].SetSubStateMachineAnimators();
                slotsInStrip[i].SetAllSubSymbolsGameobjectActive();
            }
            configurationObjectParent._managers.endConfigurationManager.SetMatrixToReelConfiguration();
        }

        internal void SetAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                slotsInStrip[slot].SetAllSubStateAnimators();
            }
        }

        internal void ClearAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                slotsInStrip[slot].ClearAllSubStateAnimators();
            }
        }

        internal string[] ReturnAllKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                keys.AddRange(slotsInStrip[slot].state_machine.animator_state_machines.sub_state_machines_keys);
            }
            return keys.ToArray();
        }

        internal AnimatorSubStateMachine[] ReturnAllValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                values.AddRange(slotsInStrip[slot].state_machine.animator_state_machines.sub_state_machines_values.sub_state_machines);
            }
            return values.ToArray();
        }

        internal void SetAllSlotContainersAnimatorSyncStates()
        {
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                slotsInStrip[slot].SetStateMachineAnimators();
            }
        }

        internal void AddSlotAnimatorsToList(ref List<Animator> output)
        {
            for (int slot = 0; slot < slotsInStrip.Length; slot++)
            {
                slotsInStrip[slot].AddAnimatorsToList(ref output);
            }
        }
    }
}