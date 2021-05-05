﻿//
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
using System;

//public string[] symbol_set_supported = new string[6] { "SF01", "SF02", "MA01" };//Want this list populated by whatever output brent is using. If we are unable to have access from a list then we should pull based on assets provided in skins folder. Read folder names of folders in Base Game/Symbols Directory
namespace Slot_Engine.Matrix
{
    /// <summary>
    /// Controls a display zone for a reel strip
    /// </summary>
    [Serializable]
    public struct ReelStripStructDisplayZone
    {
        /// <summary>
        /// The Reel strip slot amount
        /// </summary>
        [SerializeField]
        public int positionsInZone;
        /// <summary>
        /// Is this an active zone for payline evaluations?
        /// </summary>
        [SerializeField]
        public bool active_payline_evaluations;
    }
}