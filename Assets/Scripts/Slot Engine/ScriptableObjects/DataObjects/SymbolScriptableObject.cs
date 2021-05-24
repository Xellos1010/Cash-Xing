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
using BoomSports.Prototype;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SymbolData", menuName = "BoomSportsScriptableObjects/SymbolScriptableObject", order = 1)]
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
    public string symbolName
    {
        get
        {
            return symbolPrefab.name;
        }
    }
    /// <summary>
    /// Symbol Prefab
    /// </summary>
    [SerializeField]
    public Transform symbolPrefab;
    /// <summary>
    /// The win sound to play when symbol wins
    /// </summary>
    [SerializeField]
    public AudioClip winAudioClip;
    /// <summary>
    /// Used for overlay move and collect sounds - other sounds will apply
    /// </summary>
    [SerializeField]
    public AudioClip[] featureSounds;
    /// <summary>
    /// Win value of the Symbol
    /// </summary>
    [SerializeField]
    public int winValue; // Calculkate win value as base amount until Resolve Intro
    /// <summary>
    /// Symbol Weight Info
    /// </summary>
    [SerializeField]
    public symbol_weight_state[] symbolWeights;
}
[Serializable]
public struct symbol_weight_state
{
    /// <summary>
    /// Game state of to apply weights
    /// </summary>
    [SerializeField]
    public GameModes gameState;
    /// <summary>
    /// Symbol weight info
    /// </summary>
    [SerializeField]
    public float symbolWeightInfo;
    /// <summary>
    /// Some indexs in an array from the server won't be used so we need to be able to spin the game with the logic of the server implementation
    /// </summary>
    [SerializeField]
    public int arrayIndexServer;
}