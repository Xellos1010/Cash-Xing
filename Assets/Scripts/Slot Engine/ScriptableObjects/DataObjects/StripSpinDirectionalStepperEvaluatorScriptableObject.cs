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
using BoomSports.Prototype;
using UnityEngine;
/// <summary>
/// Creates the scriptable object for a stepper strip - Flow of Stepper Strip
/// On Spin Start: Wait X Seconds - lerp x units (x = slotSize.y + padding.y) (tweening happens here with lerp % complete to -> from position) - repeat
/// On Slam: Check X seconds remaining to step - step if below 1/2 time to step. Needs to be refactored if RMG requirements come into play
/// </summary>
[CreateAssetMenu(fileName = "StripSpinStepperParameters", menuName = "BoomSportsScriptableObjects/StripSpinStepperParametersScriptableObject", order = 2)]
public class StripSpinDirectionalStepperEvaluatorScriptableObject : BaseStripSpinEvaluatorScriptableObject
{
    /// <summary>
    /// Controls the amount of slots to traverse each step
    /// </summary>
    [SerializeField]
    public int slotsToTraversePerStep = 1;
    /// <summary>
    /// How many steps per spin is the player allowed? **If stepsAllowedPerSpin < 0 then step until stop spin is called**
    /// </summary>
    public int stepsAllowedPerSpin = 1; 
    /// <summary>
    /// Controls the spin amount during looping state
    /// </summary>
    [SerializeField]
    public float timeToPauseAfterStepCompleted = 0.5f;
    /// <summary>
    /// Controls the spin amount during looping state
    /// </summary>
    [SerializeField]
    public float timeToCompleteStep = 0.6777f;
    /// <summary>
    /// = slot (spinDirection * Slot Width Height + Slot.padding) * slotsToTraverse
    /// </summary>
    public Vector3 amountToTraverseDuringStep;
    public Vector3 amountToSkipDuringPauseStep;
    /// <summary>
    /// Sequence for evaluation - spin timer 0 -> lerpOverTime 
    /// </summary>
    /// <param name="spinTimerCurrent"></param>
    /// <param name="spinPath"></param>
    /// <returns></returns>
    public override Vector3 EvaluateSpin(float spinTimerCurrent, ref SpinPath spinPath)
    {
        //Debug.Log("Evaluating Stepper Directional Spin");
        //See how many steps to evaluate
        //Debug.Log($"pathPositions.path.Length = {pathPositions.path.Length} pathPositions.startPosition = {pathPositions.startPosition}  + stepsAllowedPerSpin {stepsAllowedPerSpin} = {pathPositions.startPosition + stepsAllowedPerSpin}");
        //Initalize start position to start position index
        Vector3 initialStartPosition = spinPath.path[spinPath.localStartPositionIndex];
        //Solve for 1 step first - if want infinity step set end positoin to end of path
        //Set the end position to pathPositions.startPosition + stepsAllowedPerSpin - If the allowed steps per spin take the position index over the amount of points in path then you've reached end of path and need to start again
        //Vector3 endPosition = spinPath.path[spinPath.localStartPositionIndex + 1];
        
        //This assumes a slot that has reached the end position will be moved to start of sequence before spin evaluation runs again
        Vector3 nextPosition = spinPath.path[spinPath.localStartPositionIndex + stepsAllowedPerSpin];

        //Initialize distance to travel during first step - is recalculated each time a step is completed in path
        amountToTraverseDuringStep = nextPosition - initialStartPosition;
        //Initialize Output with start position as value
        Vector3 output = spinPath.path[spinPath.localStartPositionIndex];
        //Debug.Log(Log($"amountToTraverseDuringStep = {amountToTraverseDuringStep.ToString()} Initial Output set to start position {output.ToString()}");

        //Initialize Steps completed sequence - used to compare given time on path and initial steps completed whether to change symbol or not - Start Position index is Steps completed in path so far
        int stepsCompletedSoFar = spinPath.localStartPositionIndex;
        int stepsToCompletePath = spinPath.path.Length - 1; //Steps to complete path is last index in path array
        //Need to ensure spin at index in path operates as expected
        //Calling object passes previously evaluated steps that have been completed - SpinPath will contain the times path has reached end position.
        //If the current spin timer is over a sequence step total then we need to pre-determine based on times reached end where in position 
        //(lerpOverTime + timeTillStartLerp) gets the total time of a sequence * the steps to complete the sequence * stepsToCompletePath + 1 - change symbol if >
        float lerpTimeToCompletePath = ((timeToCompleteStep + timeToPauseAfterStepCompleted) * stepsToCompletePath) * (spinPath.timesReachedEndOfPath + 1);

        int timesReachedEndOfPath = 0;
        float currentTimeOnPathEvaluating = ((timeToCompleteStep + timeToPauseAfterStepCompleted) * stepsCompletedSoFar);
        spinTimerCurrent += currentTimeOnPathEvaluating; //SpinTimer now in realtime


        //This controls how many steps per spin to traverse and if steps < 0 then spin until stop
        if (stepsAllowedPerSpin < 0)
        {
            //Initial check to see if the spin timer supplied is > Initial end of path
            if (spinTimerCurrent > lerpTimeToCompletePath)
            {
                output = spinPath.path[0];
                initialStartPosition = output;
                nextPosition = spinPath.path[1];
                amountToTraverseDuringStep = nextPosition - initialStartPosition;
                stepsCompletedSoFar = 0;
                Debug.Log($"SpinTimerCurrent {spinTimerCurrent} > end of path Time {lerpTimeToCompletePath} - output {spinPath.path[0]} = pathPositions.path[0]");
                //Set the path to the start
                //While the spinTimerCurrent > lerpTimeToCompletePath we need to set the evaluation to the start of the path and count how many times we reach the end
                while (spinTimerCurrent > lerpTimeToCompletePath)
                {
                    
                    //Increment times reached end of path for each time end of path is reached
                    timesReachedEndOfPath += 1;
                                             //Make initial time on path the completed path time now
                    currentTimeOnPathEvaluating = lerpTimeToCompletePath;
                    lerpTimeToCompletePath += ((timeToCompleteStep + timeToPauseAfterStepCompleted) * stepsToCompletePath);
                }
                //Debug.Log($" lerpTimeToCompletePath updated to {lerpTimeToCompletePath}");
            }
            //evaluatingTimeOnPath tracks which part of the path to returns
            while (currentTimeOnPathEvaluating < spinTimerCurrent)
            {
                //Debug.Log($"{initialTimeOnPath} < {spinTimerCurrent}");
                //first step from 0 - lerpOverTime
                if (spinTimerCurrent > currentTimeOnPathEvaluating + timeToCompleteStep)
                {
                    //Debug.Log($"amountToTraverseDuringStep {amountToTraverseDuringStep.ToString()} being added to output {output.ToString()}");
                    currentTimeOnPathEvaluating += timeToCompleteStep;
                    //If you are in a pause period only moving to the next position;
                    output += amountToTraverseDuringStep;
                    if (spinTimerCurrent > currentTimeOnPathEvaluating + timeToPauseAfterStepCompleted)
                    {
                        //Debug.Log($"We are in next step - add pause time to lerpTime. {initialTimeOnPath} += {timeTillStartLerp}");
                        currentTimeOnPathEvaluating += timeToPauseAfterStepCompleted;
                        stepsCompletedSoFar += 1;
                        initialStartPosition = output;
                        nextPosition = spinPath.path[stepsCompletedSoFar+1];
                        amountToTraverseDuringStep = nextPosition - initialStartPosition;
                    }
                    else
                    {
                        //Debug.Log($"We are setting final position step - add pause time to lerpTime. {initialTimeOnPath} += {timeTillStartLerp}");
                        currentTimeOnPathEvaluating += spinTimerCurrent - currentTimeOnPathEvaluating;
                    }
                }
                else
                {
                    //Debug.Log($"Adding amountToTraverseDuringStep {amountToTraverseDuringStep} * ((float)(spinTimerCurrent{spinTimerCurrent}/lerpOverTime{lerpOverTime})){(spinTimerCurrent / lerpOverTime)} = {amountToTraverseDuringStep * (spinTimerCurrent / lerpOverTime)} being added to output {output.ToString()}");
                    output += amountToTraverseDuringStep * ((spinTimerCurrent - currentTimeOnPathEvaluating) / timeToCompleteStep);
                    currentTimeOnPathEvaluating += spinTimerCurrent - currentTimeOnPathEvaluating;
                }
            }
        }
        else //Built with Cash Crossing 1 step per spin in mind
        {
            //calculate amount of time to complete step and return percentage along path
            float timeToCompletePath = currentTimeOnPathEvaluating + ((timeToCompleteStep + timeToPauseAfterStepCompleted) * stepsAllowedPerSpin);
            if (spinTimerCurrent < timeToCompletePath) //See which part of stepper sequence are you in - solve for 1 first them many
            {
                //We will use startTimeOnPath to add sequences in path until point in path has been reached.
                while (currentTimeOnPathEvaluating < spinTimerCurrent)
                {
                    //Determine if current time supplied is > startTimeOnPath + activeLerpTimeToCompleteStep and increment the step
                    //Debug.Log(Log($"{evaluatingTimeOnPath} < {spinTimerCurrent} = {evaluatingTimeOnPath < spinTimerCurrent}");
                    if (spinTimerCurrent > currentTimeOnPathEvaluating + timeToCompleteStep)
                    {
                        //Debug.Log($"amountToTraverseDuringStep {amountToTraverseDuringStep.ToString()} being added to output {output.ToString()}");
                        currentTimeOnPathEvaluating += timeToCompleteStep;
                        //If you are in a pause period only moving to the next position;
                        output += amountToTraverseDuringStep;
                        //Update amount to traverse during step to next point in sequence
                        if (spinTimerCurrent > currentTimeOnPathEvaluating + timeToPauseAfterStepCompleted)
                        {
                            //Debug.Log($"We are in next step - add pause time to lerpTime. {initialTimeOnPath} += {timeTillStartLerp}");
                            currentTimeOnPathEvaluating += timeToPauseAfterStepCompleted;
                            stepsCompletedSoFar += 1;
                            if (stepsCompletedSoFar + 1 < spinPath.path.Length)
                            {
                                //You've reached the end of path
                                amountToTraverseDuringStep = spinPath.path[stepsCompletedSoFar + 1] - spinPath.path[stepsCompletedSoFar];
                            }
                            else
                            {
                                stepsCompletedSoFar = 0;
                                timesReachedEndOfPath += 1;
                            }
                        }
                        else
                        {
                            //Debug.Log($"We are setting final position step - add pause time to lerpTime. {spinTimerCurrent - evaluatingTimeOnPath} = spinTimerCurrent {spinTimerCurrent} - startTimeOnPath {evaluatingTimeOnPath} ");
                            currentTimeOnPathEvaluating += spinTimerCurrent - currentTimeOnPathEvaluating;
                        }
                    }
                    else
                    {
                        //Debug.Log($"Adding amountToTraverseDuringStep {amountToTraverseDuringStep} * ((float)(spinTimerCurrent{spinTimerCurrent}/lerpOverTime{lerpOverTime})){(spinTimerCurrent / lerpOverTime)} = {amountToTraverseDuringStep * (spinTimerCurrent / lerpOverTime)} being added to output {output.ToString()}");
                        output += amountToTraverseDuringStep * ((spinTimerCurrent - currentTimeOnPathEvaluating) / timeToCompleteStep);
                        currentTimeOnPathEvaluating += spinTimerCurrent - currentTimeOnPathEvaluating;
                    }
                }
                //Debug.Log($"Mathf.Abs(output.sqrMagnitude) {Mathf.Abs(output.sqrMagnitude)} >= Mathf.Abs(pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude) {Mathf.Abs(pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude)} = {Mathf.Abs(output.sqrMagnitude) >= Mathf.Abs(pathPositions.path[pathPositions.path.Length - 1].sqrMagnitude)}");
                if(Mathf.Abs(output.sqrMagnitude) >= Mathf.Abs(spinPath.path[spinPath.path.Length - 1].sqrMagnitude))
                {
                    Vector3 differenceStartEnd = spinPath.path[spinPath.path.Length - 1] - (spinPath.path[spinPath.spinAtIndexInPath] + offsetAtMoveToTop);
                    while (Mathf.Abs(output.sqrMagnitude) >= Mathf.Abs(spinPath.path[spinPath.path.Length - 1].sqrMagnitude))
                    {
                        //Debug.Log($"output {output} -= differenceStartEnd {differenceStartEnd}");
                        output -= differenceStartEnd;
                        //Increment times reached end of path for each time end of path is reached
                        timesReachedEndOfPath += 1;
                    }
                }
            }
            else //Set to end position on path and return value
            {
                int positionIndex = spinPath.localStartPositionIndex + stepsAllowedPerSpin;
                if (positionIndex >= spinPath.path.Length)
                {
                    while (positionIndex >= spinPath.path.Length)
                    {
                        timesReachedEndOfPath += 1;
                        positionIndex -= (spinPath.path.Length - 1);
                    }
                }
                output = spinPath.path[positionIndex];
            }
        }
        if (timesReachedEndOfPath != spinPath.timesReachedEndOfPath)
        {
            spinPath.timesReachedEndOfPath = timesReachedEndOfPath;
            spinPath.changeSymbolGraphic = true;
        }
        //Need to calculate time till end of path
        spinPath.toPositionEvaluated = output;
        spinPath.currentToIndexInPath = spinPath.FormatRawStepsToPositionInPath(stepsCompletedSoFar,stepsToCompletePath);
        return output;
    }
    /// <summary>
    /// Stepper will only replace number of steps allowed per spin
    /// </summary>
    /// <returns>(int) stepsAllowedPerSpin</returns>
    public override int GetSymbolsReplacedPerSpin(int objectsInGroup, ConfigurationDisplayZonesStruct configurationGroupDisplayZones, int startIndexInPath)
    {
        return stepsAllowedPerSpin;
    }

    internal override float GetTotalTime()
    {
        //Only returns amount to take for 1 step - calling object needs to multiply by point to point in path array Length-1;
        return (timeToCompleteStep + timeToPauseAfterStepCompleted);
    }

    internal override bool isTimeAtEndOfSpin(float spinCurrentTimer)
    {
        float tempTimer = 0;
        bool addActiveTimerAmount = true;
        //Add active time and inactive time until time is > current time
        while (tempTimer < spinCurrentTimer)
        {
            tempTimer += addActiveTimerAmount ? timeToCompleteStep : timeToPauseAfterStepCompleted;
            addActiveTimerAmount = !addActiveTimerAmount;
        }
        //Last added time was active - the time passed is in active state
        return !addActiveTimerAmount;
    }
}