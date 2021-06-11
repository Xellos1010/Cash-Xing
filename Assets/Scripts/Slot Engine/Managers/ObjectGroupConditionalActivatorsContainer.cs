﻿//
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
using System;
using System.Collections.Generic;

namespace BoomSports.Prototype
{
    /// <summary>
    /// Acts as a container for future implementations with Groupped Objects and conditional activators - Need to work in generic gameobject event invoking
    /// </summary>
    [Serializable]
    public class ObjectGroupConditionalActivatorsContainer
    {
        /// <summary>
        /// List of all slot with symbol conditional activators
        /// </summary>
        [SerializeField]
        public SlotSymbolActivatorConditional[] slotSymbolActivators;
        //public SlotWinActivatorConditional[] slotWinActivators;
        /// <summary>
        /// returns all conditionals from all sources in a list
        /// </summary>
        /// <returns></returns>
        internal BaseSlotActivatorEventConditional[] GetAllConditionalChecks()
        {
            //List<BaseSlotActivatorEventConditional> output = new List<BaseSlotActivatorEventConditional>();
            //if(slotSymbolActivators != null)
            //    if(slotSymbolActivators.Length > 0)
            //        output.AddRange(slotSymbolActivators);
            //if(slotWinActivators != null)
            //    if(slotWinActivators.Length > 0)
            //        output.AddRange(slotWinActivators);
            //return output.ToArray();
            return slotSymbolActivators;
        }
    }
}
