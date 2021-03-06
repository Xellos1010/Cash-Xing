//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : SlotEngine.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
#if UNITY_EDITOR
#endif
using System;

//public string[] symbol_set_supported = new string[6] { "SF01", "SF02", "MA01" };//Want this list populated by whatever output brent is using. If we are unable to have access from a list then we should pull based on assets provided in skins folder. Read folder names of folders in Base Game/Symbols Directory
namespace Slot_Engine.Matrix
{

    //This is to be able to have multiple display zone's that share the same reel_strip_spin_loop_symbols generated by end_configuration_generater
    //Theory - You have multiple ReelStripStructDisplayZone's - if you have multiple display_slots_per_reel then you have so many active matrix zones
    //for a pyramid stacked matrix (3) 2x5 matrix's which are connected top->bottom 1-> 2 is connected at reel 1,3,5. 2 -> 3 is connected at reel 2,4. 3 has an extra slot in reel 3.
    //So you would need a DisplayZone[] that would be 2x2x2x2x2, 1x0x1x0x1, 2x2x2x2x2, 0x1x0x1x0, 2x2x2x2x2, 0x0x1x0x0
    //A position would have to be made in every reel strip until the lowest point atleast, 9 positions in path for display area - 1 position at end - 10 total 
    /// <summary>
    /// A stackable display zone active display zones will be affected by payline evaluations. in-active zones will be omitted from paylien evaluations
    /// </summary>
    [Serializable]
    public struct ConfigurationDisplayZonesStruct
    {
        /// <summary>
        /// Padding before display zone
        /// </summary>
        [Range(1,50)]
        public int paddingBefore;
        /// <summary>
        /// Padding After Display Zone - default to 1
        /// </summary>
        [Range(1,50)]
        public int paddingAfter;
        /// <summary>
        /// This is where you can stack display zones that are affected or not affected by payline evaluations
        /// </summary>
        [SerializeField]
        public DisplayZoneStruct[] displayZones;
        /// <summary>
        /// Type of Spin to use on this reel. Constant Lerp - Step 1 slot over time.
        /// </summary>
        [SerializeField]
        public BasePathTransformSpinEvaluatorScriptableObject spinParameters;

        public ConfigurationDisplayZonesStruct(ConfigurationDisplayZonesStruct displayZonesSetting) : this()
        {
            spinParameters = displayZonesSetting.spinParameters;
            displayZones = displayZonesSetting.displayZones;
            paddingBefore = displayZonesSetting.paddingBefore;
            paddingAfter = displayZonesSetting.paddingAfter;
        }

        public ConfigurationDisplayZonesStruct(StripStruct stripStruct) : this()
        {
            paddingBefore = stripStruct.stripDisplayZonesSetting.paddingBefore;
            paddingAfter = stripStruct.stripDisplayZonesSetting.paddingBefore;
            displayZones = new DisplayZoneStruct[stripStruct.stripDisplayZonesSetting.displayZones.Length];
            for (int i = 0; i < stripStruct.stripDisplayZonesSetting.displayZones.Length; i++)
            {
                displayZones[i] = new DisplayZoneStruct(stripStruct.stripDisplayZonesSetting.displayZones[i]);
            }
            displayZones = stripStruct.stripDisplayZonesSetting.displayZones;
        }

        /// <summary>
        /// Display zone's total positions
        /// </summary>
        [SerializeField]
        public int displayZonesPositionsTotal
        {
            get
            {
                int output = 0;
                for (int displayZone = 0; displayZone < displayZones.Length; displayZone++)
                {
                    output += displayZones[displayZone].positionsInZone;
                }
                return output;
            }
        }
        /// <summary>
        /// Total positions with padding
        /// </summary>
        internal int totalPositions
        {
            get
            {
                return paddingBefore + paddingAfter + displayZonesPositionsTotal;
            }
        }
    }
}