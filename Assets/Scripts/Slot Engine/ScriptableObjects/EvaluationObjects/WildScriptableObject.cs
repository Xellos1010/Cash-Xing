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

namespace Slot_Engine.Matrix.ScriptableObjects
{

    /// <summary>
    /// Creates the scriptable object for Wild Paylines Evaluation information to be stored
    /// </summary>
    [CreateAssetMenu(fileName = "WildEvaluationObject", menuName = "BoomSportsScriptableObjects/WildEvaluationScriptableObject", order = 4)]
    public class WildScriptableObject : SlotEvaluationScriptableObject
    {

        public override bool EvaluateNodeForConditionsMet(SuffixTreeNodeInfo nodeInfo, WinningObject[] winningObjects)
        {
            throw new System.NotImplementedException();
        }

        public override object EvaluatePaylines(ref EvaluationObjectStruct symbols_configuration)
        {
            //Called at high level and take symbol names and return feature activating
            WinningPayline[] output = new WinningPayline[0];
            //Evaluate payline configuration with Core Evaluation Logic passed thru
            return output;
        }

        public override int? ReturnEvaluationObjectSupportedRootCount()
        {
            return nodesActivatingEvaluationConditions?.Count;
        }
    }
}
