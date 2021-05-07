//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using static Slot_Engine.Matrix.EndConfigurationManager;

namespace Slot_Engine.Matrix
{
    [System.Serializable]
    public class Strip
    {
        [UnityEngine.SerializeField]
        public StripStruct stripInfo;
        internal static NodeDisplaySymbol[] GenerateReelStripStatic(GameModes currentMode, int slotsPerStrip, ref EndConfigurationManager endConfigurationManager)
        {
            //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
            NodeDisplaySymbol[] reel_spin_symbols = new NodeDisplaySymbol[slotsPerStrip];
            for (int i = 0; i < slotsPerStrip; i++)
            {
                reel_spin_symbols[i] = endConfigurationManager.GetRandomWeightedSymbol(currentMode).Result;
            }
            return reel_spin_symbols;
        }

        internal string ReturnDisplaySymbolsPrint()
        {
            return String.Join(" ", stripInfo.spin_info.displaySymbols);
        }
    }
    [Serializable]
    public struct StripsStruct
    {
        [UnityEngine.SerializeField]
        public StripStruct[] strips;

        public StripsStruct(ref ConfigurationStripStructDisplayZones[] displayZonesPerStrip) : this()
        {
            strips = new StripStruct[displayZonesPerStrip.Length];
            for (int reel_number = 0; reel_number < strips.Length; reel_number++)
            {
                strips[reel_number] = new StripStruct(reel_number,displayZonesPerStrip[reel_number]);
            }
        }

        internal string PrintDisplaySymbols()
        {
            string output = "";
            for (int i = 0; i < strips.Length; i++)
            {
                output += "-" + String.Join("|", strips[i].spin_info.displaySymbols);
            }
            return output;
        }
    }

    [Serializable]
    public struct StripStruct
    {
        /// <summary>
        /// Reel position in sequence
        /// </summary>
        [SerializeField]
        internal int stripColumn;
        /// <summary>
        /// Display Zone Configuration for Strip
        /// </summary>
        [SerializeField]
        internal ConfigurationStripStructDisplayZones stripDisplayZonesSetting;

        /// <summary>
        /// spin_informatino for the reelstrip
        /// </summary>
        [UnityEngine.SerializeField]
        internal StripSpinStruct spin_info;
        internal int total_slot_objects
        {
            get
            {
                int output = 0;
                output += stripDisplayZonesSetting.paddingBefore;
                GetTotalDisplaySlots(ref output);
                return output;
            }
        }
        internal int total_display_slots
        {
            get
            {
                int output = 0;
                GetTotalDisplaySlots(ref output);
                return output;
            }
        }
        internal int total_positions
        {
            get
            {
                int output = 0;
                output += stripDisplayZonesSetting.paddingBefore;
                GetTotalDisplaySlots(ref output);
                output += stripDisplayZonesSetting.paddingAfter;
                return output;
            }
        }

        private void GetTotalDisplaySlots(ref int output)
        {
            if (stripDisplayZonesSetting.stripDisplayZones != null)
            {
                for (int displayZone = 0; displayZone < stripDisplayZonesSetting.stripDisplayZones.Length; displayZone++)
                {
                    output += stripDisplayZonesSetting.stripDisplayZones[displayZone].positionsInZone;
                }
            }
        }

        public StripStruct(int stripNumber, ConfigurationStripStructDisplayZones stripDisplayZonesSetting) : this()
        {
            this.stripColumn = stripNumber;
            Debug.Log($"display_zone number of zones = {stripDisplayZonesSetting.stripDisplayZones.Length}");
            ConfigurationStripStructDisplayZones displayZoneTemp = new ConfigurationStripStructDisplayZones(stripDisplayZonesSetting);
            this.stripDisplayZonesSetting = displayZoneTemp;
            Debug.Log($"this.display_zone number of zones = {this.stripDisplayZonesSetting.stripDisplayZones.Length}");
        }

        internal void SetSpinConfigurationTo(StripSpinStruct reelStripStruct)
        {
            spin_info = reelStripStruct;
        }

        internal BaseSpinEvaluatorScriptableObject GetSpinParametersAs()
        {
            return stripDisplayZonesSetting.spinParameters;
        }
        /// <summary>
        /// Gets the first instance of an evaluation object of sub-class
        /// </summary>
        /// <typeparam name="T">Type of evaluation manager to return</typeparam>
        /// <returns>Type if in list or null if nothing</returns>
        internal static T GetSpinParametersAs<T>(ref StripSpinEvaluatorBaseScriptableObject baseSpinParameters)
        {
            object output = null;
            if(baseSpinParameters.GetType() == typeof(T))
            {
                output = baseSpinParameters;
            }
            return (T)Convert.ChangeType(output, typeof(T)); ;
        }
    }
    /// <summary>
    /// The structure or the reel strip spin loop strip and the end configuration placed within the reelstrip
    /// </summary>
    [Serializable]
    public struct StripSpinStruct
    {
        /// <summary>
        /// end reel configuration symbols for this spin
        /// </summary>
        [UnityEngine.SerializeField]
        public NodeDisplaySymbol[] displaySymbols;
        /// <summary>
        /// The display symbols reel will cycle thru on loop
        /// </summary>
        [UnityEngine.SerializeField]
        public NodeDisplaySymbol[] stripSpinSymbols;
        /// <summary>
        /// Holds the lower and upper range of array where ending symbols were placed in spin symbols
        /// </summary>
        [UnityEngine.SerializeField]
        public int[] endSymbolDisplayRangeOnStrip;

        public StripSpinStruct(NodeDisplaySymbol[] slotDisplaySymbols) : this()
        {
            displaySymbols = slotDisplaySymbols;
        }

        internal List<int> GetAllDisplaySymbols()
        {
            List<int> output = new List<int>();
            for (int i = 0; i < displaySymbols.Length; i++)
            {
                output.Add(displaySymbols[i].primary_symbol);
            }
            return output;
        }
    }
}