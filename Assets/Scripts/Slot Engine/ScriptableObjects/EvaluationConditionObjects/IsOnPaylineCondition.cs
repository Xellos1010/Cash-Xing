﻿//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : WinConditions.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//

using BoomSports.Prototype;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IsOnPaylineConditionObject", menuName = "BoomSportsScriptableObjects/IsOnPaylineConditionScriptableObject", order = 4)]
[Serializable]
public class IsOnPaylineCondition : NodeEvaluationCondition
{
	/// <summary>
	/// Evaluates if the condition was met
	/// </summary>
	/// <param name="winningObject">winningSymbols in the WinningObject as object</param>
	/// <returns></returns>
	public override bool EvaluateCondition(WinningObject winningObject, SuffixTreeNodeInfo nodeInfo)
	{
		//Debug.Log($"Checking Winning Object {winningObject.PrintWinningNodes()} contains node {nodeInfo.Print()}");
		return winningObject.ContainsNode((SuffixTreeNodeInfo)nodeInfo);
	}

    internal override bool EvaluateCondition(SuffixTreeNodeInfo suffixTreeNodeInfo)
    {
        throw new NotImplementedException();
    }

    internal override bool EvaluateCondition(SuffixTreeNodeInfo[] suffixTreeNodeInfo)
    {
        throw new NotImplementedException();
    }
}
