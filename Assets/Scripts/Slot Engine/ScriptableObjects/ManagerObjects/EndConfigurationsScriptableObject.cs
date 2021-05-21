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
public class SpinConfigurationStorage : SerializableDictionary.Storage<DisplayConfigurationContainer>
{
    public SpinConfigurationStorage(DisplayConfigurationContainer result)
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
    //A 
    /// <summary>
    /// Current end reelstrip configuration in use
    /// </summary>
    public DisplayConfigurationContainer currentConfigurationInUse;
    /// <summary>
    /// end reelstrips to display in sequence
    /// </summary>
    public GameStateConfigurationDictionary configurationsByState;
    /// <summary>
    /// reelstrips that have been used
    /// </summary>
    public List<DisplayConfigurationContainer> configurationsUsed;
    /// <summary>
    /// Configuration saved for later loading
    /// </summary>
    public DisplayConfigurationContainer savedConfiguration;
    
    internal void AddReelstripToUsedList(DisplayConfigurationContainer usedConfiguration)
    {
        if (configurationsUsed == null)
            configurationsUsed = new List<DisplayConfigurationContainer>();
        configurationsUsed.Add(usedConfiguration);
    }
    /// <summary>
    /// Saves a configuration for later loading
    /// </summary>
    /// <param name="toSave"></param>
    internal void SaveConfiguration(DisplayConfigurationContainer toSave)
    {
        savedConfiguration = toSave;
    }

}

