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
using System.Collections.Generic;
using UnityEngine;

namespace BoomSports.Prototype.ScriptableObjects
{
    /// <summary>
    /// Creates an evaluation object to associate conditions and activate a Multiplier on linewin
    /// </summary>
    [CreateAssetMenu(fileName = "MultiplierEvaluationReferenceObject", menuName = "BoomSportsScriptableObjects/MultiplierEvaluationReferenceScriptableObject", order = 4)]
    public class MultiplierEvaluationScriptableObject : SlotEvaluationScriptableObject
    {

        public void InitializeOverlaySymbolsEvaluation()
        {
            nodesActivatingEvaluationConditions = new List<SuffixTreeNodeInfo>();
            nodesActivatingEvaluationConditions.Clear();
        }

        public override object EvaluatePaylines(ref EvaluationObjectStruct symbols_configuration)
        {
            InitializeOverlaySymbolsEvaluation();
            object[] objectReturn = new object[0];
            return objectReturn;
        }

        public override int? ReturnEvaluationObjectSupportedRootCount()
        {
            return nodesActivatingEvaluationConditions.Count;
        }

        public override bool EvaluateNodeForConditionsMet(SuffixTreeNodeInfo nodeInfo, WinningObject[] winningObjects)
        {
            for (int winningObject = 0; winningObject < winningObjects.Length; winningObject++)
            {
                //if any conditions are met to the fullest then the node is a valid node
                //First Test for Overlay - Pass On Winning Paylien Check then Count for payline
                for (int condition = 0; condition < nodeEvaluationConditions.Count; condition++)
                {
                    if (nodeEvaluationConditions[condition].EvaluateCondition(winningObjects[winningObject], nodeInfo))
                    {
                        if (condition == nodeEvaluationConditions.Count - 1)
                        {
                            Debug.Log($"Trigger Feature Evaluated to True - {symbolTargetName}");
                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Debug.Log($"Trigger Feature Evaluated to false - {symbolTargetName}");
            return false;
        }

        internal override void ActivateWinningNodesEvents(ConfigurationDisplayZonesStruct[] displayZones)
        {
            throw new System.NotImplementedException();
        }
    }
}