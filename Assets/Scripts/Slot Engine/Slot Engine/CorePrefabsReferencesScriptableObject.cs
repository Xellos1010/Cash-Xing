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
using WeightedDistribution;

[CreateAssetMenu(fileName = "MachineGraphics", menuName = "BoomSportsScriptableObjects/MachineGraphicsScriptableObject", order = 6)]
public class MachineGraphicsScriptableObject : ScriptableObject
{

}

[CreateAssetMenu(fileName = "WeightsDistribution", menuName = "BoomSportsScriptableObjects/WeightsDistributionScriptableObject", order = 5)]
public class WeightsDistributionScriptableObject : ScriptableObject
{
    /// <summary>
    /// The symbol weights object to draw from
    /// </summary>
    public IntDistribution intDistribution;
}


/// <summary>
/// Creates the scriptable object for Core Prefabs References to be set
/// </summary>
[CreateAssetMenu(fileName = "Core_Prefabs_References_Object", menuName = "BoomSportsScriptableObjects/CorePrefabsReferencesScriptableObject", order = 4)]
public class CorePrefabsReferencesScriptableObject : ScriptableObject
{
    /// <summary>
    /// The symbol weights object to draw from
    /// </summary>
    //public Transform symbolWeight;
}

