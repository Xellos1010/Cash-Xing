//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using System;
using UnityEngine;

namespace Slot_Engine.Matrix
{
    [System.Serializable]
    public class ReelStrip
    {
        [UnityEngine.SerializeField]
        public ReelStripStruct reelStrip;
        public ReelStrip(int[] display_symbols)
        {
            reelStrip.spin_info.display_symbols = display_symbols;
        }

        internal void GenerateReelStrip(int slots_per_strip_onSpinLoop, WeightedDistribution.IntDistribution intDistribution)
        {
            //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
            reelStrip.spin_info.reel_spin_symbols = GenerateReelStripStatic(slots_per_strip_onSpinLoop, intDistribution);
        }

        internal static int[] GenerateReelStripStatic(int slots_per_strip_onSpinLoop, WeightedDistribution.IntDistribution intDistribution)
        {
            //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
            int[] reel_spin_symbols = new int[slots_per_strip_onSpinLoop];
            for (int i = 0; i < slots_per_strip_onSpinLoop; i++)
            {
                reel_spin_symbols[i] = intDistribution.Draw();
            }
            return reel_spin_symbols;
        }

        internal string ReturnDisplaySymbolsPrint()
        {
            return String.Join(" ", reelStrip.spin_info.display_symbols);
        }
    }
    [Serializable]
    public struct ReelStripsStruct
    {
        [UnityEngine.SerializeField]
        public ReelStripStruct[] reelstrips;

        public ReelStripsStruct(ReelStripStructDisplayZones[] display_zones_per_reel) : this()
        {
            reelstrips = new ReelStripStruct[display_zones_per_reel.Length];
            for (int reel_number = 0; reel_number < reelstrips.Length; reel_number++)
            {
                reelstrips[reel_number] = new ReelStripStruct(reel_number,display_zones_per_reel[reel_number]);
            }
        }
    }
    [Serializable]
    public struct ReelStripStruct
    {
        /// <summary>
        /// The direction to spin the slots in. reel_spin_speed_direction * reelSpinSpeed will be the distance the slot travels
        /// </summary>
        [SerializeField]
        internal Vector3 reel_spin_direction;
        /// <summary>
        /// Reel position in sequence
        /// </summary>
        [SerializeField]
        internal int reel_number;
        
        /// <summary>
        /// Controls how many positions to generate after the display area for the slots to spin off-screen
        /// </summary>
        [SerializeField]
        internal int padding_before;
        /// <summary>
        /// Controls the active display zones
        /// </summary>
        [UnityEngine.SerializeField]
        internal ReelStripStructDisplayZone[] display_zones;
        /// <summary>
        /// Controls the empty positions generated after the display zone
        /// </summary>
        [UnityEngine.SerializeField]
        internal int padding_after;
        /// <summary>
        /// spin_informatino for the reelstrip
        /// </summary>
        [UnityEngine.SerializeField]
        internal ReelStripSpinStruct spin_info;
        internal int total_slot_objects
        {
            get
            {
                int output = 0;
                GetTotalSlotObjects(ref output);
                return output;
            }
        }

        internal int total_positions
        {
            get
            {
                int output = 0;
                GetTotalSlotObjects(ref output);
                output += padding_after;
                return output;
            }
        }

        private void GetTotalSlotObjects(ref int output)
        {
            output += padding_before;
            for (int display_zone = 0; display_zone < display_zones.Length; display_zone++)
            {
                output += display_zones[display_zone].slots_in_reelstrip_zone;
            }
        }

        public ReelStripStruct(int reel_number, ReelStripStructDisplayZones display_zone) : this()
        {
            this.reel_number = reel_number;
            this.display_zones = display_zone.reelstrip_display_zones;
            this.padding_before = display_zone.padding_before;
            this.padding_after = display_zone.padding_after;
        }

        internal void SetSpinDirectionTo(Vector3 direction)
        {
            reel_spin_direction = direction;
        }

        internal void SetSpinConfigurationTo(ReelStripStruct reelStripStruct)
        {
            spin_info = reelStripStruct.spin_info;
        }
    }
    /// <summary>
    /// The structure or the reel strip spin loop strip and the end configuration placed within the reelstrip
    /// </summary>
    [Serializable]
    public struct ReelStripSpinStruct
    {
        /// <summary>
        /// end reel configuration symbols for this spin
        /// </summary>
        [UnityEngine.SerializeField]
        public int[] display_symbols;
        /// <summary>
        /// The display symbols reel will cycle thru on loop
        /// </summary>
        [UnityEngine.SerializeField]
        public int[] reel_spin_symbols;
        /// <summary>
        /// Holds the lower and upper range of array where ending symbols were placed in spin symbols
        /// </summary>
        [UnityEngine.SerializeField]
        public int[] display_symbol_range;
    }
}