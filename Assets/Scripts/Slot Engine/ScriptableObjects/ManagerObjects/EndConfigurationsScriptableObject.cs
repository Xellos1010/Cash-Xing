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

[Serializable]
public class SpinConfigurationStorage : SerializableDictionary.Storage<StripSpinStruct[]>
{
    public SpinConfigurationStorage(StripSpinStruct[] result)
    {
         data = result;
    }
}
[Serializable]
public class GameStateConfigurationStorage : SerializableDictionary.Storage<List<SpinConfigurationStorage>> { }
[Serializable]
public class GameStateConfigurationDictionary : SerializableDictionary<GameModes, GameStateConfigurationStorage> { }
[Serializable]
public class GameStateDistributionDictionary : SerializableDictionary<GameModes, WeightsDistributionScriptableObject> { }
/// <summary>
/// Creates the scriptable object for end configurations to be stored
/// </summary>
[CreateAssetMenu(fileName = "EndConfigurationsObject", menuName = "BoomSportsScriptableObjects/EndConfigurationsScriptableObject", order = 3)]
public class EndConfigurationsScriptableObject : ScriptableObject
{
    /// <summary>
    /// Current end reelstrip configuration in use
    /// </summary>
    public StripSpinStruct[] currentReelstripConfiguration;
    /// <summary>
    /// end reelstrips to display in sequence
    /// </summary>
    public GameStateConfigurationDictionary endReelstripsPerState;
    /// <summary>
    /// reelstrips that have been used
    /// </summary>
    public List<StripSpinStruct[]> end_reelstrips_used;

    public StripSpinStruct[] savedReelConfiguration;
    internal void AddReelstripToUsedList(StripSpinStruct[] current_reelstrip_configuration)
    {
        if (end_reelstrips_used == null)
            end_reelstrips_used = new List<StripSpinStruct[]>();
        end_reelstrips_used.Add(current_reelstrip_configuration);
    }
}

