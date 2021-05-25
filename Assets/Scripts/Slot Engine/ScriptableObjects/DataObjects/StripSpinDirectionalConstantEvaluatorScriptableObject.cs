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
using System;
using UnityEngine;

/// <summary>
/// Creates the scriptable object for the reels spin parameters
/// </summary>
[CreateAssetMenu(fileName = "StripSpinDirectionalConstantScriptableObject", menuName = "BoomSportsScriptableObjects/ReelStripSpinParametersScriptableObject", order = 2)]
public class StripSpinDirectionalConstantEvaluatorScriptableObject : BaseStripSpinEvaluatorScriptableObject
{
    /// <summary>
    /// Controls the strip spin speed during looping state - traverse the length of the path over time
    /// </summary>
    [SerializeField]
    public float distancePerSecond = 285;
    /// <summary>
    /// Debug Control to limit the speed to a certain fps expected reaction
    /// </summary>
    [SerializeField]
    internal float fpsLock = 30.0f;

    /// <summary>
    /// Evaluates a spin for a directional constant along path
    /// </summary>
    /// <param name="spinTimerCurrent"></param>
    /// <param name="startPosition">Start Position - if different from position in path will cause issues</param>
    /// <param name="pathPositions">Should be formatted on GenerateStrips the first and last position</param>
    /// <param name="timesReachedEndPath">Times object has reached path before trying to evaluate spin</param>
    /// <returns></returns>
    /// https://docs.unity3d.com/Manual/VectorCookbook.html
    public override Vector3 EvaluateSpin(float spinTimerCurrent, ref SpinPath pathPositions)
    {
        //Debug.Log("Evaluating Constant Directional Spin");
        Vector3 output = Vector3.zero;
        //If start position is the last positoin then we will test for position output to be >= last position in path magnitude and reset
        if (pathPositions.startPosition < pathPositions.path.Length)
        {
            //position in path is a standard defined list. hold list as points in path - 2 vectors start and end - distance pre-calculated and held for quick reference.

            //Need to have current iteration in loop sequence as a reference value - Set to 0 on spin start and tracks the amount of times objects has reached path end

            //Evaluate the spin in a direction based on speed and return raw distance to travel
            float calculatedDistanceTravelRaw = distancePerSecond * (float)spinTimerCurrent;
            Vector3 rawToPosition = pathPositions.path[pathPositions.startPosition] + (stripSpinDirection * calculatedDistanceTravelRaw);
            //Since sqr operation is cpu heavy we will sqr our distance to travel to make comparison easier

            //Get Total Distance to travel from start position to next position in path
            float sqrMagnitudeEndOfPath = pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude;
            float rawPositionSqrMagnitude = rawToPosition.sqrMagnitude;
            //Debug.Log($"calculatedDistanceTravelRaw = {calculatedDistanceTravelRaw} rawToPosition = {rawToPosition.ToString()} rawToPosition.sqrMagnitude = {rawToPosition.sqrMagnitude} sqrMagnitudeTillNextPointInPath = {sqrMagnitudeEndOfPath} last position in path = {pathPositions.path[pathPositions.path.Length - 1]} last position in path sqr magnitude = {pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude}");

            Vector3 distanceFirstLast = pathPositions.distanceFirstLastPositionInPath;
            int timesReachedEndOfPath = 0;
            //Directional only for now
            while (rawPositionSqrMagnitude >= sqrMagnitudeEndOfPath)
            {
                timesReachedEndOfPath += 1;
                // Calculate distance between first and last position in path and add to final output
                rawToPosition += distanceFirstLast;
                rawPositionSqrMagnitude = rawToPosition.sqrMagnitude;
                //Debug.Log($"Added {distanceFirstLast.ToString()} to raw position. rawToPosition = {rawToPosition.ToString()} rawToPosition.sqrMagnitude = {rawToPosition.sqrMagnitude} last position in path = {pathPositions.path[pathPositions.path.Length - 1]} last position in path sqr magnitude = {pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude}");
            }
            if(timesReachedEndOfPath != pathPositions.timesReachedEndOfPath)
            {
                pathPositions.changeSymbolGraphic = true;
                pathPositions.timesReachedEndOfPath = timesReachedEndOfPath;
            }
            pathPositions.toPositionEvaluated = rawToPosition;
            return rawToPosition;
        }
        else
        {
            Debug.LogWarning($"Start position supplied {pathPositions.startPosition} is > path positions Length {pathPositions.path.Length}");
        }
        return output;
    }
    /// <summary>
    /// Returns length of objects in group **always full strip clear**
    /// </summary>
    /// <param name="objectsInGroup"></param>
    /// <returns></returns>
    public override int GetSymbolsReplacedPerSpin(int objectsInGroup)
    {
        return objectsInGroup;
    }

    internal override float GetTotalTime()
    {
        return distancePerSecond;
    }

    internal override bool isTimeInPauseState(float spinCurrentTimer)
    {
        Debug.LogWarning("Spin Directional Constant has no pause state");
        throw new NotImplementedException();
    }
}
