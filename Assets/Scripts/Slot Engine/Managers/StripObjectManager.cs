//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Slot.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BoomSports.Prototype.Managers
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StripObjectManager))]
    class SlotEditor : BoomSportsEditor
    {
        StripObjectManager myTarget;
        [Range(0,50)]
        float sliderTimerSpin;
        BasePathTransformSpinEvaluatorScriptableObject temp;
        public void OnEnable()
        {
            myTarget = (StripObjectManager)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Calculate MoveObjectToSpinPosition()"))
            {
                Debug.Log($"MoveObjectToSpinPosition() output toPosition = {myTarget.MoveObjectToSpinPosition()}");
            }
            if (GUILayout.Button("Set Sub Animators State Machine"))
            {
                myTarget.SetSubStateMachineAnimators();
            }
            if (GUILayout.Button("Set Animators To Sync State Machine"))
            {
                myTarget.SetStateMachineAnimators();
            }
            if (GUILayout.Button("Test Evaluate next path symbol Set Animators To Sync State Machine"))
            {
                myTarget.SignalParentToEvaluateConditionsForNextSlotInPathViaSymbol();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            EditorGUI.BeginChangeCheck();
            //temp = myTarget.stripManager.GetSpinParameters();
            //sliderTimerSpin = EditorGUILayout.Slider(sliderTimerSpin, 0, 2*(temp.GetTotalTime() * myTarget.stripManager.localPositionsInStrip.Length -1));
            //if(EditorGUI.EndChangeCheck())
            //{
            //    myTarget.MoveObjectToSpinPosition(sliderTimerSpin);
            //}
            base.OnInspectorGUI();
        }
    }

#endif
    public class StripObjectManager : BaseObjectManager
    {
        /// <summary>
        /// Reference to the start position - always set on instantiate and spinEnd
        /// </summary>
        [SerializeField]
        internal int startPositionIndex;
        /// <summary>
        /// times object has reached end of path - set to 0 on spin start and instantiate
        /// </summary>
        [SerializeField]
        internal int timesReachedEndOfPath;

        /// <summary>
        /// StripGroupManager is a group of StripObjects
        /// </summary>
        public StripObjectGroupManager stripManager
        {
            get
            {
                return baseObjectGroupParent as StripObjectGroupManager;
            }
        }
        /// <summary>
        /// Used to pass a test constant spin timer update
        /// </summary>
        internal bool test = false;
        internal bool endSpin;

        /// <summary>
        /// Starts a Spin
        /// </summary>
        public override void StartSpin( bool test = false)
        {
            timesReachedEndOfPath = 0;
            endSpin = false;
            this.test = test;
            SetStartPosition();
            base.StartSpin();
        }

        internal override void SetStartPosition()
        {
            startPositionIndex = GetIndexFromLocalPositions();
        }

        internal int GetIndexFromLocalPositions()
        {
            for (int i = 0; i < stripManager.localPositionsInStrip.Length; i++)
            {
                if(transform.localPosition.sqrMagnitude == stripManager.localPositionsInStrip[i].sqrMagnitude)
                {
                    return i;
                }
            }
            Debug.LogWarning("Position not found in local position manager. returning -1");
            return -1;
        }

        Vector3 SetPositionTo(Vector3 toPosition) //Needs to be positive to move forwards and negative to move backwards
        {
            //Debug.Log($"Setting transform.localPosition = {amount}");
            transform.localPosition = toPosition;
            return toPosition; //new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        Vector3 OffsetPositionBy(Vector3 amountToAdd) //Needs to be positive to move forwards and negative to move backwards
        {
            //Debug.Log($"Offsetting transform.localPosition {transform.localPosition} by {amountToAdd}");
            return transform.localPosition + amountToAdd; //new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        internal override void Update()
        {
            if (spinMovementEnabled)
            {
                MoveObjectToSpinPosition();
            }
        }
        /// <summary>
        /// Moves an objects along SpinCycle (Spin Sequence/Path) and returns the calculated to position based on spinCurrentTimer;
        /// </summary>
        /// <returns></returns>
        internal Vector3 MoveObjectToSpinPosition()
        {
            if (Application.isPlaying)
            {
                //This is where you add functionality for reel reveal control
                spinCurrentTimer += Time.deltaTime;
            }
            return MoveObjectToSpinPosition(spinCurrentTimer);
        }
        /// <summary>
        /// Moves an objects along SpinCycle (Spin Sequence/Path) and returns the calculated to position based on spinCurrentTimer;
        /// </summary>
        /// <returns></returns>
        internal Vector3 MoveObjectToSpinPosition(float spinCurrentTimer)
        {
            //Debug.Log($"{gameObject.name} is MoveObjectToSpinPosition( spinCurrentTimer ={spinCurrentTimer})");
            toPosition = Vector3.zero;
            BasePathTransformSpinEvaluatorScriptableObject spinParameters = stripManager.GetSpinParameters();
            //TODO Test Generic Evaluate Spin - TODO Add abtract function to return positions in object group
            StripObjectGroupManager temp2 = baseObjectGroupParent as StripObjectGroupManager;
            //Debug.Log($"new SpinPath({temp2.localPositionsInStrip}, {startPositionIndex},{temp2.configurationObjectParent.configurationSettings.slotSize}, {temp2.configurationObjectParent.configurationSettings.slotPadding});");
            //Sets up our spin path - calculates sqr magnitudes between each point in path - Compare absolute sqr magnitude of object local position and last position in path to move to start of path
            SpinPath pathToEvaluate = new SpinPath(temp2.localPositionsInStrip, startPositionIndex,temp2.configurationObjectParent.configurationSettings.slotSize, temp2.configurationObjectParent.configurationSettings.slotPadding);
            //Debug.Log($"pathToEvaluate.GetType() == null = {pathToEvaluate.GetType() == null}");
            //Stepper Logic - The evaluating object checks if you have a set amount of steps or rotations to make in spin then to return constant value once ceiling has been reached 
            spinParameters.EvaluateSpin(spinCurrentTimer, ref pathToEvaluate);
            indexOnPath = pathToEvaluate.currentToIndexInPath;
            toPosition = pathToEvaluate.toPositionEvaluated;
            //Debug.Log($"{gameObject.name} toPosition = {toPosition} timesReachedEndOfPath = {timesReachedEndOfPath} pathToEvaluate.timesReachedEndOfPath = {pathToEvaluate.timesReachedEndOfPath} pathToEvaluate.changeSymbolGraphic {pathToEvaluate.changeSymbolGraphic} ");
            if (timesReachedEndOfPath != pathToEvaluate.timesReachedEndOfPath)
            {
                timesReachedEndOfPath = pathToEvaluate.timesReachedEndOfPath;
                if (Application.isPlaying)
                    SetSymbolGraphics();
            }
            //Finalize position of evaluation if spin end
            if (Application.isPlaying)
            {
                //TODO Set Presentation symbol to end automatically for stepper reels
                if (setDisplaySymbolOnrfeachEndOfPath && presentationSymbolSetToEnd)
                {
                    if (Mathf.Abs(toPosition.sqrMagnitude) >= Mathf.Abs(stopSpinEndPosition.sqrMagnitude))
                    {
                        toPosition = stopSpinEndPosition;
                        objectInEndPosition = true;
                        spinMovementEnabled = false;
                    }
                }
                else if (endSpin == true)
                {
                    //Debug.Log($"{gameObject.name} endSpin = true - Checking spinParameters.isTimeInPauseState({spinCurrentTimer}) {spinParameters.isTimeAtEndOfSpin(spinCurrentTimer)}");
                    //Check if the spin is in a pause state
                    if (!spinParameters.isTimeAtEndOfSpin(spinCurrentTimer))
                    {
                        //position set to top handled in evaluator
                        /*if (toPosition == stripManager.localPositionsInStrip[stripManager.localPositionsInStrip.Length - 1])
                        {
                            toPosition = stripManager.localPositionsInStrip[0];
                        }*/
                        stopSpinEndPosition = toPosition;
                        objectInEndPosition = true;
                        spinMovementEnabled = false;
                    }
                }
                toPosition = SetPositionTo(toPosition);
            }
            return toPosition;
        }

        private void SetSymbolGraphics()
        {
            bool symbolSet = false;
            NodeDisplaySymbolContainer symbol = new NodeDisplaySymbolContainer();
            //First we check if we have symbols in a list to present next - this takes priority over pre-generated strips for testing purposes
            if (stripManager.debugNextSymbolsToLoad.Count > 0)
            {
                symbol = stripManager.configurationObjectParent.managers.endConfigurationManager.GetNodeDisplaySymbol(stripManager.debugNextSymbolsToLoad.Pop<int>()).Result;
            }
            else //We have no debug symbols - set either random to to next symbol in display sequence
            {
                if (setDisplaySymbolOnrfeachEndOfPath) //set graphic to next symbol in path
                {
                    string debug = "";
                    for (int i = 0; i < stripManager.symbolsDisplaySequence.Length; i++)
                    {
                        debug += $"|{stripManager.symbolsDisplaySequence[i].primarySymbol}";
                    }
                    Debug.Log($"{debug} Display symbol sequence from {stripManager.gameObject.name}");

                    //Set Graphics and end position
                    Debug.Log($"Setting {gameObject.name} Display symbol on reel {baseObjectGroupParent.gameObject.name} stripManager.localPositionsInStrip.Length = {stripManager.localPositionsInStrip.Length} (stripManager.localPositionsInStrip.Length - 2 {stripManager.localPositionsInStrip.Length - 2}) - stripManager.endSymbolsSetFromConfiguration {stripManager.endSymbolsSetFromConfiguration}");
                    presentationSymbolSetToEnd = true;
                    stopSpinEndPosition = stripManager.localPositionsInStrip[(stripManager.localPositionsInStrip.Length - 2) - stripManager.endSymbolsSetFromConfiguration];

                    if (stripManager.endSymbolsSetFromConfiguration < stripManager.symbolsDisplaySequence.Length)
                    {
                        int indexForLastNewSymbolInSequence = stripManager.GetIndexOfEndSymbolsStartInStrip();
                        //Hack: to get this working - TODO refactor to return proper value
                        if(indexForLastNewSymbolInSequence >= stripManager.symbolsDisplaySequence.Length)
                        {
                            indexForLastNewSymbolInSequence = stripManager.symbolsDisplaySequence.Length - 1;
                        }
                        Debug.Log($"stripManager.endSymbolsSetFromConfiguration {stripManager.endSymbolsSetFromConfiguration} < stripManager.symbolsDisplaySequence.Length {stripManager.symbolsDisplaySequence.Length} is true indexForLastNewSymbolInSequence = {indexForLastNewSymbolInSequence} Display Symbol index = {indexForLastNewSymbolInSequence - stripManager.endSymbolsSetFromConfiguration} symbol = {stripManager.symbolsDisplaySequence[indexForLastNewSymbolInSequence - stripManager.endSymbolsSetFromConfiguration].primarySymbol}");
                        //Set display symbol to symbol in sequence
                        //Reset all symbols on strip pull last first
                        //Reset partial symbols - set last index to pull to stripManager.symbolsDisplaySequence.Length - 1 - 
                        SetDisplaySymbolTo(stripManager.symbolsDisplaySequence[indexForLastNewSymbolInSequence - stripManager.endSymbolsSetFromConfiguration]);
                        
                        stopSpinEndPosition = stripManager.localPositionsInStrip[(stripManager.localPositionsInStrip.Length - 1) - stripManager.configurationGroupDisplayZones.paddingAfter - stripManager.endSymbolsSetFromConfiguration];
                        
                        stripManager.endSymbolsSetFromConfiguration += 1;
                        symbolSet = true;
                        //Set end position to paddig after = stripManager.endSymbolsSetFromConfiguration

                    }
                    else //Set to end
                    {
                        Debug.Log($"stripManager.endSymbolsSetFromConfiguration{stripManager.endSymbolsSetFromConfiguration} < stripManager.symbolsDisplaySequence.Length {stripManager.symbolsDisplaySequence.Length} is false");
                        SetPresentationSymbolTo(-1); //TODO Define whether to set the top slot graphic
                        stopSpinEndPosition = stripManager.localPositionsInStrip[(stripManager.localPositionsInStrip.Length - 1) - stripManager.configurationGroupDisplayZones.paddingAfter - stripManager.endSymbolsSetFromConfiguration];
                        stripManager.endSymbolsSetFromConfiguration += 1;
                        symbolSet = true;
                        presentationSymbolSetToEnd = true;
                    }
                    Debug.Log("Slot " + transform.name + " symbol presentation = " + currentPresentingSymbolID + " end position = " + stopSpinEndPosition);
                }
                else
                {
                    if (stripManager.randomSetSymbolOnEndOfSequence)
                    {
                        //If Symbol Generated = opverlay - Generate Sub Symbol and attach 2 materials
                        if (stripManager.stripInfo.spinInformation.spinIdleSymbolSequence != null)
                        {
                            if (stripManager.stripInfo.spinInformation.spinIdleSymbolSequence.Length > 0)
                            {
                                symbol = stripManager.ReturnNextSymbolInStrip();
                                symbolSet = true;
                            }
                        }
                    }
                }

                //In-case nothing was set set to random 
                if (!symbolSet)
                {
                    Debug.LogWarning("Symbol was not set - auto setting random");
                    //Determines an overlay symbol
                    symbol = stripManager.configurationObjectParent.managers.endConfigurationManager.GetRandomWeightedSymbol(StaticStateManager.enCurrentMode).Result;
                }
                SetDisplaySymbolTo(symbol);
            }
            
        }

        internal Animator SetOverlayAnimatorToFeatureAndGet()
        {
            Animator output;
            //Compare to Symbols
            for (int i = 0; i < stripManager.configurationObjectParent.symbolDataScriptableObject.symbols.Length; i++)
            {
                if (stripManager.configurationObjectParent.isSymbolOverlay(i))
                {
                    output = animatorStateMachine.animator_state_machines.sub_state_machines_values.sub_state_machines[0].sub_state_animators[i];
                    SetBoolTo(ref output, supportedAnimatorBools.FeatureTrigger, true);
                    return output;
                }
            }
            return null;
        }
        /// <summary>
        /// Used to set the slot to go to end position
        /// </summary>
        internal override void SetToStopSpin()
        {
            if (stripManager.configurationGroupDisplayZones.spinParameters.GetType() == typeof(StripSpinDirectionalStepperEvaluatorScriptableObject))
            {
                //Debug.Log($"{stripManager.gameObject.name} {gameObject.name} spin parameters = {stripManager.configurationGroupDisplayZones.spinParameters.GetType()}");
                endSpin = true;
            }
            else
            {
                setDisplaySymbolOnrfeachEndOfPath = true;
                objectInEndPosition = false;
                presentationSymbolSetToEnd = false;
            }
        }
    }
}