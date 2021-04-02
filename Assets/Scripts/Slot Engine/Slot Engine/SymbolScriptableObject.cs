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
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "SymbolData", menuName = "ScriptableObjects/SymbolScriptableObject", order = 1)]
public class SymbolScriptableObject : ScriptableObject
{
    public SymbolObject[] symbols;
}


[Serializable]
public struct SymbolObject
{
    /// <summary>
    /// Name of the Symbol
    /// </summary>
    [SerializeField]
    public string symbol_name;
    /// <summary>
    /// Symbol Prefab
    /// </summary>
    [SerializeField]
    public Transform symbol_prefab;
    /// <summary>
    /// Symbol material
    /// </summary>
    [SerializeField]
    public Material symbol_material;
    /// <summary>
    /// Win value of the Symbol
    /// </summary>
    [SerializeField]
    public int win_value;
    /// <summary>
    /// Symbol Weight Info
    /// </summary>
    [SerializeField]
    public WeightedDistribution.IntDistributionItem symbol_weight_info;
}