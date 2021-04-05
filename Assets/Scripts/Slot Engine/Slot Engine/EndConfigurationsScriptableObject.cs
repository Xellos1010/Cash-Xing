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
using Slot_Engine.Matrix;
using System.Collections.Generic;
using System;

/// <summary>
/// Creates the scriptable object for end configurations to be stored
/// </summary>
[CreateAssetMenu(fileName = "EndConfigurationsObject", menuName = "BoomSportsScriptableObjects/EndConfigurationsScriptableObject", order = 3)]
public class EndConfigurationsScriptableObject : ScriptableObject
{
    /// <summary>
    /// Current end reelstrip configuration in use
    /// </summary>
    public ReelStripsStruct current_reelstrip_configuration;
    /// <summary>
    /// end reelstrips to display in sequence
    /// </summary>
    public List<ReelStripsStruct> end_reelstrips_to_display_sequence;
    /// <summary>
    /// reelstrips that have been used
    /// </summary>
    public List<ReelStripsStruct> end_reelstrips_used;

    internal void AddReelstripToUsedList(ReelStripsStruct current_reelstrip_configuration)
    {
        if (end_reelstrips_used == null)
            end_reelstrips_used = new List<ReelStripsStruct>();
        end_reelstrips_used.Add(current_reelstrip_configuration);
    }
}
