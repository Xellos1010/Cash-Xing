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
/// <summary>
/// A strip can have multiple types of spins - Stepper and Directional Constant are 2 examples
/// </summary>
public abstract class BaseStripSpinEvaluatorScriptableObject : BasePathTransformSpinEvaluatorScriptableObject
{
    /// <summary>
    /// Controls the direction of the spin - affects positioning of path and padding objects
    /// </summary>
    [SerializeField]
    public Vector3 stripSpinDirection;
}
