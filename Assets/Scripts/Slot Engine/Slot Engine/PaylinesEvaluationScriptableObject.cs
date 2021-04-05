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
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Creates the scriptable object for paylines evaluation information to be stored
/// </summary>
[CreateAssetMenu(fileName = "PaylinesEvaluationObject", menuName = "BoomSportsScriptableObjects/PaylinesEvaluationScriptableObject", order = 4)]
public class PaylinesEvaluationScriptableObject : ScriptableObject
{
    /// <summary>
    /// number of paylines supported
    /// </summary>
    public int number_of_paylines = 0;
    /// <summary>
    /// The roote nodes for dynamic paylines using a suffix tree
    /// </summary>
    public suffix_tree_root_nodes dynamic_paylines;
}