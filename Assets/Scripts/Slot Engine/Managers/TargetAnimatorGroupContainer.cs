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
//TODO make conditional namespace - need to develop more
namespace BoomSports.Prototype
{
    /// <summary>
    /// Base class for assigning animator target for conditional active
    /// </summary>
    [Serializable]
    public abstract class TargetAnimatorGroupContainer : BaseTargetGroupContainer
    {
        /// <summary>
        /// The target animator for event to invoke
        /// </summary>
        [SerializeField]
        public Animator[] targetAnimators;
    }
}