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
using System;
//TODO make conditional namespace - need to develop more
namespace BoomSports.Prototype
{
    ///// <summary>
    ///// Base class for assigning animator target for conditional active
    ///// </summary>
    //[Serializable]
    //public abstract class TargetAnimatorContainer : BaseTargetGroupContainer
    //{
    //    /// <summary>
    //    /// The target animator for event to invoke
    //    /// </summary>
    //    [SerializeField]
    //    public Animator targetAnimator;
    //}
    /// <summary>
    /// Used to hold reference for future implementation of scriptable objects
    /// </summary>
    [Serializable]
    public abstract class BaseTargetGroupContainer : BaseTargetContainer
    {
        public abstract void ActivateConditionalAtIndex(int index);
    }
    /// <summary>
    /// The base target container- used to hold references generically
    /// </summary>
    [Serializable]
    public abstract class BaseTargetContainer
    {
        internal abstract void Initialize();
        //public abstract void ActivateConditionalWithNode(SuffixTreeNode node);
    }

}