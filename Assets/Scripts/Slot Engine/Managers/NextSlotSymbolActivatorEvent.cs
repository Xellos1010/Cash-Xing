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
using BoomSports.Prototype.Managers;
namespace BoomSports.Prototype
{
    /// <summary>
    /// A slot activator that triggered by a symbol - 
    /// </summary>
    [Serializable]
    public class NextSlotSymbolActivatorEvent : BaseSlotActivatorEventConditional
    {
        /// <summary>
        /// The symbol that activates event conditions when present in slot
        /// </summary>
        [SerializeField]
        public int symbolIDThatActivatesCondition;

        //Cash Crossing Specific hard coded
        public int rowThatActivates = 1;
        public int rowThatDeactivates = 5;
        public override bool EvaluateCondition(BaseObjectManager objectToEvaluate)
        {
            if (objectToEvaluate.currentPresentingSymbolID == symbolIDThatActivatesCondition)
            {
                //Get next position in strip
                int nextPosition = objectToEvaluate.indexOnPath + 1;
                if(nextPosition >= rowThatActivates && nextPosition <= rowThatDeactivates)
                {
                    for (int target = 0; target < targetConditionalContainer.targetsForConditionalTrue.Length; target++)
                    {
                        Debug.Log($"{objectToEvaluate.gameObject.name} with index on path as {objectToEvaluate.indexOnPath} is activating a condition");
                        //Animators are 0-5 but start on row 1 - index on path is current index which is same number as index in animator without adding 0-1 etc
                        targetConditionalContainer.targetsForConditionalTrue[target].ActivateConditionalAtIndex(objectToEvaluate.indexOnPath);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
