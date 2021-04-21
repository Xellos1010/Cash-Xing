﻿//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : SlotEngine.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
namespace Slot_Engine.Matrix.ScriptableObjects
{
    using UnityEngine;
    /// <summary>
    /// Creates the scriptable object for Core Prefabs References to be set
    /// </summary>
    [CreateAssetMenu(fileName = "SpinSettings", menuName = "BoomSportsScriptableObjects/SpinSettingsScriptableObject", order = 4)]
    public class SpinSettingsScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Cascading starting reels
        /// </summary>
        public bool reel_spin_delay_start_enabled = false;
        /// <summary>
        /// Cascading ending reels
        /// </summary>
        public bool reel_spin_delay_end_enabled = false;
        /// <summary>
        /// uses predefeined reelstrips to loop thru on spin loop
        /// </summary>
        public bool use_reelstrips_for_spin_loop = true;
        /// <summary>
        /// This allows you to set the reels spin either forward or back (Left to right - right to left )
        /// </summary>
        public bool spin_reels_starting_forward_back = true;
        /// <summary>
        /// Timer used to start each free spin
        /// </summary>
        public float timer_to_start_free_spin = 2.0f;
        /// <summary>
        /// Spin the slot machine until seconds pass
        /// </summary>
        public float spin_loop_until_seconds_pass = 5;
    }

}