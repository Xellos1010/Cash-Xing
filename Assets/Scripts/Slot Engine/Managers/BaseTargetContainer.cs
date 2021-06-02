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
    /// <summary>
    /// The base target container- used to hold references generically
    /// </summary>
    [Serializable]
    public abstract class BaseTargetContainer
    {
        internal abstract void Initialize();
        public abstract void ActivateConditional();
        public abstract void ActivateConditionalWithNode(SuffixTreeNode node);
        public abstract void ActivateConditionalAtIndex(int index);
    }

}
