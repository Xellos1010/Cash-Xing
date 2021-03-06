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
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Slot_Engine.Matrix
{
    /// <summary>
    /// Base class 
    /// </summary>
    public class BaseObjectGroupManager : MonoBehaviour
    {
        /// <summary>
        /// Used to send an event that an object group has started/stopped spin and which one
        /// </summary>
        /// <param name="objectNumber"></param>
        public delegate void ObjectGroupEventEvent(int objectNumber);
        public event ObjectGroupEventEvent objectGroupStartSpin;
        public event ObjectGroupEventEvent objectGroupEndSpin;
        /// <summary>
        /// current spinState of Group
        /// </summary>
        [SerializeField]
        internal SpinStates currentSpinState;
        /// <summary>
        /// Index of group manager in parent array
        /// </summary>
        [SerializeField]
        public int indexInGroupManager;
        /// <summary>
        /// Display Zones for group
        /// </summary>
        [SerializeField]
        public ConfigurationDisplayZonesStruct configurationGroupDisplayZones;
        [SerializeField]
        internal BaseConfigurationObject configurationObjectParent;
        /// <summary>
        /// Object managers in Group
        /// </summary>
        [SerializeField] //If base inspector enabled can check references
        internal BaseObjectManager[] objectsInGroup;

        /// <summary>
        /// The Ending symbols to Set To 
        /// </summary>
        [SerializeField]
        internal NodeDisplaySymbol[] ending_symbols;

        /// <summary>
        /// Enables you to change the symbol graphic when slot exits the viewable area of a configuration to a predefined strip or random draw weighte distribution symbol
        /// </summary>
        public bool randomSetSymbolOnEndOfSequence = true;
        /// <summary>
        /// UI Indicator to ensure operation for setting symbols to end configuration has performed
        /// </summary>
        internal int endSymbolsSetFromConfiguration = 0;

        internal bool are_slots_spinning
        {
            get
            {
                bool output = true;
                for (int i = 0; i < objectsInGroup.Length; i++)
                {
                    if (!objectsInGroup[i].objectInEndPosition)
                    {
                        break;
                    }
                    if (i == objectsInGroup.Length - 1)
                    {
                        output = false;
                    }
                }
                return output;
            }
        }

        /// <summary>
        /// is the reel in a spin state
        /// </summary>
        internal bool isSpinning
        {
            get
            {
                bool is_spinning = true;
                if (objectsInGroup != null)
                {
                    for (int slot = 0; slot <= objectsInGroup.Length; slot++)
                    {
                        if (slot < objectsInGroup.Length)
                        {
                            if (objectsInGroup[slot] != null)
                            {
                                if (slot == objectsInGroup.Length)
                                {
                                    is_spinning = false;
                                    break;
                                }
                                if (objectsInGroup[slot].spinMovementEnabled)
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

        /// <summary>
        /// Spin the Reels
        /// </summary>
        /// <returns>async task to track</returns>
        public virtual Task StartSpin()
        {
            objectGroupStartSpin?.Invoke(indexInGroupManager);
            //Debug.Log(string.Format("Spinning reel {0}",reelstrip_info.reel_number));
            InitializeVarsForNewSpin();

            SetSpinStateTo(SpinStates.spin_start);

            //TODO hooks for reel state machine
            for (int i = 0; i < objectsInGroup.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                objectsInGroup[i].StartSpin(); // Tween to the same position then evaluate
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
        public void SpinGroupNow(bool test = false)
        {
            InitializeVarsForNewSpin();
            //When reel is generated it's vector3[] path is generated for reference from slots
            SetSpinStateTo(SpinStates.spin_start);
            //TODO hooks for reel state machine
            for (int i = 0; i < objectsInGroup.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                objectsInGroup[i].StartSpin(test); // Tween to the same position then evaluate
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
            currentSpinState = state;
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
            for (int i = 0; i < objectsInGroup.Length; i++)
            {
                objectsInGroup[i].SetObjectMovementEnabledTo(enable_disable);
            }
        }
        /// <summary>
        /// Set slots to Stop Spin
        /// </summary>
        private void SetSlotsToStopSpinning()
        {
            for (int i = 0; i < objectsInGroup.Length; i++)
            {
                //Last slot needs to ease in and out to the "next position" but 
                objectsInGroup[i].SetToStopSpin(); // Tween to the same position then evaluate\
            }
        }
        /// <summary>
        /// Instant sets a slot of reel to a configuration
        /// </summary>
        /// <param name="reelStripStruct"></param>
        internal void SetSymbolEndSymbolsAndDisplay(StripSpinStruct reelStripStruct)
        {
            endSymbolsSetFromConfiguration = 0;
            BaseObjectManager[] slotsDecendingOrder = GetSlotsDecending().ToArray();
            Debug.Log($"{gameObject.name} slotsDecendingOrder.Length = {slotsDecendingOrder.Length} Slot Order = {PrintGameObjectNames(slotsDecendingOrder)}");
            Debug.Log($"reelStripStruct.displaySymbols.Length = {reelStripStruct.displaySymbols.Length}");

            List<NodeDisplaySymbol> symbolsToDisplay = new List<NodeDisplaySymbol>();
            for (int symbol = 0; symbol < reelStripStruct.displaySymbols.Length; symbol++)
            {
                symbolsToDisplay.Add(reelStripStruct.displaySymbols[symbol]);
            }
            Debug.Log($"symbolsToDisplay.Count = {symbolsToDisplay.Count}");

            SetEndingSymbolsTo(symbolsToDisplay.ToArray());

            Debug.Log($"configurationGroupDisplayZones.paddingBefore {configurationGroupDisplayZones.paddingBefore}");
            //Get padding before reel and set slots for all slots on matrix
            for (int slot = configurationGroupDisplayZones.paddingBefore; slot < slotsDecendingOrder.Length; slot++)
            {
                Debug.Log($"Setting {slotsDecendingOrder[slot].gameObject.name} to symbol reelStripStruct.displaySymbols[{endSymbolsSetFromConfiguration}]");
                slotsDecendingOrder[slot].SetDisplaySymbolTo(reelStripStruct.displaySymbols[endSymbolsSetFromConfiguration]);
                endSymbolsSetFromConfiguration += 1;
            }
        }

        private string PrintGameObjectNames(BaseObjectManager[] slotsDecendingOrder)
        {
            string output = "";
            for (int i = 0; i < slotsDecendingOrder.Length; i++)
            {
                output += slotsDecendingOrder[i].gameObject.name;
            }
            return output;
        }

        internal virtual List<BaseObjectManager> GetSlotsDecending()
        {
            List<BaseObjectManager> output = new List<BaseObjectManager>();
            Debug.LogWarning("Not Implemented for base class");
            return output;
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
            objectGroupEndSpin?.Invoke(indexInGroupManager);
            //Debug.Log(String.Format("All slots stopped spinning for reel {0}",transform.name));
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

        internal void SetAllSlotContainersAnimatorSyncStates()
        {
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                objectsInGroup[slot].SetStateMachineAnimators();
            }
        }

        internal void AddSlotAnimatorsToList(ref List<Animator> output)
        {
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                objectsInGroup[slot].AddAnimatorsToList(ref output);
            }
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

        //internal virtual void GenerateLocalPositions(ConfigurationSettingsScriptableObject configurationSettings)
        //{
        //    throw new NotImplementedException();
        //}

        internal void SetAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                objectsInGroup[slot].SetAllSubStateAnimators();
            }
        }

        internal void ClearAllSlotContainersSubAnimatorStates()
        {
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                objectsInGroup[slot].ClearAllSubStateAnimators();
            }
        }

        internal AnimatorSubStateMachine[] ReturnAllValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                values.AddRange(objectsInGroup[slot].stateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines);
            }
            return values.ToArray();
        }
        internal string[] ReturnAllKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int slot = 0; slot < objectsInGroup.Length; slot++)
            {
                keys.AddRange(objectsInGroup[slot].stateMachine.animator_state_machines.sub_state_machines_keys);
            }
            return keys.ToArray();
        }
    }
}
