using BoomSports.Prototype;
using BoomSports.Prototype.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using BoomSports.Prototype.Containers;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BoomSports.Prototype.Managers
{
#if UNITY_EDITOR

    [CustomEditor(typeof(EvaluationManager))]
    class EvaluationManagerEditor : BoomSportsEditor
    {
        EvaluationManager myTarget;
        SerializedProperty winning_paylines;

        private int payline_to_show;
        private int winning_payline_to_show;
        PaylinesEvaluationScriptableObject paylinesEvaluationObject;
        public void OnEnable()
        {
            myTarget = (EvaluationManager)target;
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            paylinesEvaluationObject = EvaluationManager.GetFirstInstanceCoreEvaluationObject<PaylinesEvaluationScriptableObject>(ref myTarget.coreEvaluationObjects);
            if (paylinesEvaluationObject != null)
            {
                if (GUILayout.Button("Generate Evaluation from Configuration Object"))
                {
                    //todo get matrix from script
                    paylinesEvaluationObject.GenerateDynamicPaylinesFromConfigurationObjectsGroupManagers(ref myTarget.configurationObject.configurationSettings.displayZones);
                    serializedObject.ApplyModifiedProperties();
                }

                if (paylinesEvaluationObject.dynamic_paylines.paylinesSupported.Count > 0)
                {
                    EditorGUILayout.LabelField("Dynamic Paylines Commands");
                    EditorGUI.BeginChangeCheck();
                    payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, paylinesEvaluationObject.dynamic_paylines.paylinesSupported.Count - 1);
                    if (EditorGUI.EndChangeCheck())
                    {
                        myTarget.configurationObject.managers.winningObjectsManager.ShowDynamicPaylineRaw(payline_to_show);
                    }
                    if (paylinesEvaluationObject.winningObjects.Count > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        winning_payline_to_show = EditorGUILayout.IntSlider(winning_payline_to_show, 0, paylinesEvaluationObject.winningObjects.Count - 1);
                        myTarget.configurationObject.managers.winningObjectsManager._winningObjects = paylinesEvaluationObject.winningObjects.ToArray();
                        if (EditorGUI.EndChangeCheck())
                        {
                            myTarget.configurationObject.managers.winningObjectsManager.ShowWinningPayline(winning_payline_to_show);
                        }
                    }
                    if (GUILayout.Button("Show Current End Configuration On Reels"))
                    {
                        myTarget.configurationObject.managers.endConfigurationManager.SetMatrixToReelConfiguration();
                    }
                    if (GUILayout.Button("Evaluate Paylines From current End Configuration"))
                    {
                        myTarget.EvaluateWinningSymbolsFromCurrentConfiguration();
                    }
                }
            }
            else
            {
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            //EditorGUILayout.PropertyField(winning_paylines);
            base.OnInspectorGUI();
        }
    }
#endif
    public class EvaluationManager : BaseBoomSportsManager
    {
        internal static bool CheckSymbolActivatesFeature(int symbolIndex, Features featureToCheck)
        {
            bool output = false;
            for (int i = 0; i < instance.slotEvaluationObjects.Length; i++)
            {
                if(instance.slotEvaluationObjects[i].featureName == featureToCheck)
                {
                    if(instance.configurationObject.symbolDataScriptableObject.symbols[symbolIndex].symbolName.Contains(instance.slotEvaluationObjects[i].symbolTargetName))
                    {
                        output = true;
                        break;
                    }
                }
            }
            return output;
        }
        /// <summary>
        /// Checks if a symbol has evaluators it triggers and returns evaluators
        /// </summary>
        /// <param name="displaySymboltoCheck"></param>
        /// <returns></returns>
        internal static SymbolSlotEvaluationsReturnContainer CheckReturnSymbolHasFeature(NodeDisplaySymbolContainer displaySymboltoCheck)
        {
            List<SlotEvaluationScriptableObject> connectedEvaluators = new List<SlotEvaluationScriptableObject>();
            for (int i = 0; i < instance.slotEvaluationObjects.Length; i++)
            {
                if (instance.configurationObject.symbolDataScriptableObject.symbols[displaySymboltoCheck.primarySymbol].symbolName.Contains(instance.slotEvaluationObjects[i].symbolTargetName))
                {
                    connectedEvaluators.Add(instance.slotEvaluationObjects[i]);
                }
            }
            SymbolSlotEvaluationsReturnContainer output = new SymbolSlotEvaluationsReturnContainer(displaySymboltoCheck, connectedEvaluators.ToArray());
            return output;
        }
        public static EvaluationManager instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<EvaluationManager>();
                return _instance;
            }
        }
        public static EvaluationManager _instance;
        /// <summary>
        /// These Control the various ways a gridConfiguration can be evaluated for features - Wild allows more winning lines/ways/shapes - Overlays have sub symbols- trigger symbols trigger a feature 
        /// </summary>
        public SlotEvaluationScriptableObject[] slotEvaluationObjects;
        /// <summary>
        /// Has the evaluation manager evaluated this spin?
        /// </summary>
        public bool evaluated = false;
        /// <summary>
        /// This is either ways lines or grouped.
        /// </summary>
        public BaseEvaluationScriptableObject[] coreEvaluationObjects;
        //temporary to get working - need to refactor to get list of activated overlays from scriptable object
        public List<SuffixTreeNodeInfo> overlaySymbols
        {
            get
            {
                return GetFirstInstanceFeatureEvaluationObject<TriggerFeatureEvaluationScriptableObject>(ref slotEvaluationObjects).nodesActivatingEvaluationConditions;
            }
        }

        /// <summary>
        /// Evaluates the symbols configuration for winning paylines
        /// </summary>
        /// <param name="configurationToEvaluate"></param>
        /// <returns>Winning Objects</returns> TODO refactor to return base abstract class
        internal Task<T[]> EvaluateSymbolConfigurationForWinningPaylines<T>(DisplayConfigurationContainer configurationToEvaluate)
        {
            //Build a list of evaluation objects based on feature evaluation and core evaluation objects
            List<object> output_raw = new List<object>();

            //Build list of evaluations to make out of features and core objects
            List<EvaluationObjectStruct> coreEvaluationsToRun = new List<EvaluationObjectStruct>();

            //Clear all feature conditions of activated nodes previously
            //Will need 
            Debug.Log($"Symbol Configuration being evaluated = {PrintConfiguration(configurationToEvaluate)}");

            //Clear and build evaluation objects winning objects
            for (int coreEvaluationObject = 0; coreEvaluationObject < coreEvaluationObjects.Length; coreEvaluationObject++)
            {
                coreEvaluationsToRun.Add(new EvaluationObjectStruct(coreEvaluationObjects[coreEvaluationObject], slotEvaluationObjects, configurationToEvaluate));
                coreEvaluationObjects[coreEvaluationObject].ClearWinningObjects();
            }
            for (int slotEvaluationObject = 0; slotEvaluationObject < slotEvaluationObjects.Length; slotEvaluationObject++)
            {
                slotEvaluationObjects[slotEvaluationObject].ClearWinningObjects();
            }

            //Build the raw winning objects to convert to paylines
            for (int evaluation = 0; evaluation < coreEvaluationsToRun.Count; evaluation++)
            {
                output_raw.Add(coreEvaluationsToRun[evaluation].Evaluate());
            }

            //After items are filtered
            //Scan the slot activators for any slots that activate conditions and run connected events
            for (int slotEvaluationObject = 0; slotEvaluationObject < slotEvaluationObjects.Length; slotEvaluationObject++)
            {
                //Cash Crossing Specific - sending display zones to gett padding and activate correct index of bridge animators
                if (slotEvaluationObjects[slotEvaluationObject].nodesActivatingEvaluationConditions.Count > 0)
                {
                    //Use Case Cash Crossing - Activate column event to turn animator on and activate trailing at index in path for x spins - load column with x debug symbol
                    ActivateWinningNodesEvents(slotEvaluationObjects[slotEvaluationObject], BaseConfigurationObjectManager.instance.configurationSettings.displayZones);
                    //Cannot reference scene objects in scriptable object - need to implement on class level
                    //slotEvaluationObjects[slotEvaluationObject].ActivateWinningNodesEvents(BaseConfigurationObjectManager.instance.configurationSettings.displayZones);
                }
            }

            List<T> output_filtered = new List<T>();
            for (int i = 0; i < output_raw.Count; i++)
            {
                output_filtered.AddRange((T[])Convert.ChangeType(output_raw[i], typeof(T[])));
            }

            //for (int payline = 0; payline < output_filtered.Count; payline++)
            //{
            //    if(output_filtered[payline].)
            //    if (temp.nodesActivatingEvaluationConditions[i].column == 0) //Left Bridge Animator
            //    {
            //        targetBridgeAnimatorsLeft.ActivateConditionalAtIndex(indexOfRowInAnimators);
            //    }
            //    else if (temp.nodesActivatingEvaluationConditions[i].column == 6)// Right Bridge Animator
            //    {
            //        targetBridgeAnimatorsRight.ActivateConditionalAtIndex(indexOfRowInAnimators);
            //    }
            //    else if (temp.nodesActivatingEvaluationConditions[i].column == 3)// Center Animator
            //    {
            //        targetBridgeAnimatorCenter.ActivateConditional();
            //    }
            //}

            //Check that feature conditions are met and activated after return
            return Task.FromResult<T[]>(output_filtered.ToArray());
        }
        //Cash Crossing Specific - Needs to b e refactored
        public TargetAnimatorsTriggerSetOnActive targetBridgeAnimatorsLeft;
        public TargetAnimatorsTriggerSetOnActive targetBridgeAnimatorsRight;
        public TargetAnimatorTriggerSetOnActive targetBridgeAnimatorCenter;

        internal void ActivateWinningNodesEvents(SlotEvaluationScriptableObject slotEvaluationScriptableObject, ConfigurationDisplayZonesStruct[] displayZones)
        {
            Debug.Log($"slotEvaluationScriptableObject.GetType() == {slotEvaluationScriptableObject.GetType()}\n slotEvaluationScriptableObject.GetType() == typeof(TriggerFeatureEvaluationScriptableObject) == {slotEvaluationScriptableObject.GetType() == typeof(TriggerFeatureEvaluationScriptableObject)}");
            if (slotEvaluationScriptableObject.GetType() == typeof(TriggerFeatureEvaluationScriptableObject))
            {
                TriggerFeatureEvaluationScriptableObject temp = (TriggerFeatureEvaluationScriptableObject)slotEvaluationScriptableObject;
                Debug.Log($"{temp.featureToTrigger.ToString()} feature being triggered on slots {temp.PrintActivatingNodes()}");
                if (temp.featureToTrigger == Features.trailing)
                {
                    int indexOfRowInAnimators;
                    //Cash Crossing Specific - Needs to be refactored and made generic
                    for (int i = 0; i < temp.nodesActivatingEvaluationConditions.Count; i++)
                    {

                        //Check the nodes column - Target the animator in the column and row - Set to active
                        //Need to use row - padding of display slots
                        indexOfRowInAnimators = temp.nodesActivatingEvaluationConditions[i].row - displayZones[temp.nodesActivatingEvaluationConditions[i].column].paddingBefore;
                        //Check if node has activated already
                        //Temporary hook for cash crossing - may be extracted for further use - Temporary fix - index sent id 1 position off not including padding - todo refactor to remove padding sent to evaluator.
                        StripConfigurationObject.instance.SetSpinAtIndexFrom(temp,temp.nodesActivatingEvaluationConditions[i].column, indexOfRowInAnimators + 1);
                        if (temp.nodesActivatingEvaluationConditions[i].column == 0) //Left Bridge Animator
                        {
                            targetBridgeAnimatorsLeft.ActivateConditionalAtIndex(indexOfRowInAnimators);
                        }
                        else if (temp.nodesActivatingEvaluationConditions[i].column == 6)// Right Bridge Animator
                        {
                            targetBridgeAnimatorsRight.ActivateConditionalAtIndex(indexOfRowInAnimators);
                        }
                        else if (temp.nodesActivatingEvaluationConditions[i].column == 3)// Center Animator
                        {
                            targetBridgeAnimatorCenter.ActivateConditional();
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("TODO implement other SlotEvaluationScriptableObject hacks as needed");
            }
        }

        private string PrintConfiguration(DisplayConfigurationContainer configurationContainer)
        {
            string output = "";
            for (int i = 0; i < configurationContainer.configuration.Length; i++)
            {
                output += "||" + configurationContainer.configuration[i].PrintDisplaySymbols();
            }
            return output;
        }

        /// <summary>
        /// Gets the first instance of an evaluation object of sub-class
        /// </summary>
        /// <typeparam name="T">Type of evaluation manager to return</typeparam>
        /// <returns>Type if in list or null if nothing</returns>
        internal static T GetFirstInstanceCoreEvaluationObject<T>(ref BaseEvaluationScriptableObject[] coreEvaluationObjects)
        {
            object output = null;
            for (int i = 0; i < coreEvaluationObjects.Length; i++)
            {
                if (coreEvaluationObjects[i] != null)
                {
                    if (coreEvaluationObjects[i].GetType() == typeof(T))
                    {
                        output = coreEvaluationObjects[i];
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return (T)Convert.ChangeType(output, typeof(T)); ;
        }
        /// <summary>
        /// Gets the first instance of a feature evaluation object of sub-class
        /// </summary>
        /// <typeparam name="T">Type of evaluation manager to return</typeparam>
        /// <returns>Type if in list or null if nothing</returns>
        internal static T GetFirstInstanceFeatureEvaluationObject<T>(ref SlotEvaluationScriptableObject[] slotEvaluationObjects)
        {
            object output = null;
            for (int i = 0; i < slotEvaluationObjects.Length; i++)
            {
                if (slotEvaluationObjects[i].GetType() == typeof(T))
                {
                    output = slotEvaluationObjects[i];
                    break;
                }
            }
            return (T)Convert.ChangeType(output, typeof(T));
        }

        internal async Task<WinningPayline[]> EvaluateForWinningSymbolsFromConfiguration(DisplayConfigurationContainer configurationToEvaluateContainer)
        {
            return await EvaluateWinningSymbols(configurationToEvaluateContainer);
        }

        public async Task<WinningPayline[]> EvaluateWinningSymbols(DisplayConfigurationContainer configurationContainer)
        {
            return await EvaluateSymbolConfigurationForWinningPaylines<WinningPayline>(configurationContainer);
        }
        /// <summary>
        /// Evaluates configuration from either the symbols displayed or pre-generated. bug with use preGenerated with stepper reels since they don't symbol replace
        /// </summary>
        /// <param name="usePreGenerated"></param>
        public async void EvaluateWinningSymbolsFromCurrentConfiguration(bool usePreGenerated = false)
        {
            Debug.Log($"Evaluating Wins - using PreGenerated reels = {usePreGenerated}");
            DisplayConfigurationContainer temp = usePreGenerated ? EndConfigurationManager.displayConfigurationInUse : BuildStripSpinStructArrayFromSymbolsOnDisplay();
            //Debug.Log(String.Format("Evaluating Symbols in configuration {0}", matrix.slot_machine_managers.end_configuration_manager.current_reelstrip_configuration.PrintDisplaySymbols()));
            await EvaluateSymbolConfigurationForWinningPaylines<WinningPayline>(temp);
        }

        private DisplayConfigurationContainer BuildStripSpinStructArrayFromSymbolsOnDisplay()
        {
            DisplayConfigurationContainer output = new DisplayConfigurationContainer();
                output.configuration = new GroupSpinInformationStruct[configurationObject.groupObjectManagers.Length];
            for (int i = 0; i < output.configuration.Length; i++)
            {
                output.configuration[i] = new GroupSpinInformationStruct(configurationObject.groupObjectManagers[i].GetNodeDisplaySymbols());
            }
            return output;
        }

        internal bool DoesSymbolActivateFeature(SymbolObject symbolObject, Features feature)
        {
            for (int slotEvaluator = 0; slotEvaluator < slotEvaluationObjects.Length; slotEvaluator++)
            {
                if (slotEvaluationObjects[slotEvaluator].featureName == feature || feature == Features.Count)
                {
                    //The prefab of the symbol came with Symbol_ - See if symbol prefab contains symbol defined name
                    if (symbolObject.symbolName.Contains(slotEvaluationObjects[slotEvaluator].symbolTargetName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal Features[] GetFeaturesSymbolActivates(SymbolObject symbolObject)
        {
            List<Features> output = new List<Features>();
            for (int slotEvaluator = 0; slotEvaluator < slotEvaluationObjects.Length; slotEvaluator++)
            {
                if (symbolObject.symbolName.Contains(slotEvaluationObjects[slotEvaluator].symbolTargetName))
                {
                    output.Add(slotEvaluationObjects[slotEvaluator].featureName);
                }
            }
            return output.ToArray();
        }
        internal WinningObject[] ReturnWinningObjects()
        {
            List<WinningObject> output = new List<WinningObject>();
            WinningObject[] temp;
            //TODO Check that T pass is Subclass or same class as WinningObject
            for (int coreEvaluationObject = 0; coreEvaluationObject < coreEvaluationObjects.Length; coreEvaluationObject++)
            {
                output.AddRange(coreEvaluationObjects[coreEvaluationObject].ReturnWinningObjects());
            }

            //Debug.Log($"Returning {output.Count}");
            return output.ToArray();
        }
        internal T[] ReturnWinningObjectsAs<T>()
        {
            T[] output = new T[0];
            List<T> outputGather = new List<T>();
            WinningObject[] objectsWonBase;
            //TODO Check that T pass is Subclass or same class as WinningObject
            for (int coreEvaluationObject = 0; coreEvaluationObject < coreEvaluationObjects.Length; coreEvaluationObject++)
            {
                //Debug.Log("Converting Type");
                objectsWonBase = coreEvaluationObjects[coreEvaluationObject].ReturnWinningObjects();

                //Debug.Log("Converted");
                for (int i = 0; i < objectsWonBase.Length; i++)
                {
                    if(objectsWonBase[i] is T)
                        outputGather.Add((T)Convert.ChangeType(objectsWonBase[i], typeof(T)));
                }
            }
            return output;
        }
        internal WinningPayline[] ReturnWinningObjectsAsWinningPaylines()
        {
            return ReturnWinningObjectsAs<WinningPayline>();
        }
        internal bool IsSymbolFeatureSymbol(SymbolObject symbolObject)
        {
            //for every slot evaluation object see which 
            bool output = false;
            for (int evaluator = 0; evaluator < slotEvaluationObjects.Length; evaluator++)
            {
                if (symbolObject.symbolName.Contains(slotEvaluationObjects[evaluator].symbolTargetName))
                {
                    output = true;
                    break;
                }
            }
            return output;
        }

        internal bool IsSymbolFeatureSymbol(SymbolObject symbolObject, Features featureTriggerToCheck)
        {
            //For every slot evaluation object see which 
            bool output = false;
            for (int evaluator = 0; evaluator < slotEvaluationObjects.Length; evaluator++)
            {
                if (symbolObject.symbolName.Contains(slotEvaluationObjects[evaluator].symbolTargetName))
                {
                    if (slotEvaluationObjects[evaluator].featureName == featureTriggerToCheck)
                    {
                        output = true;
                        break;
                    }
                }
            }
            return output;
        }

        internal Features[] GetSymbolFeatures(SymbolObject symbolObject)
        {
            //for every slot evaluation object see which 
            List<Features> output = new List<Features>();
            for (int evaluator = 0; evaluator < slotEvaluationObjects.Length; evaluator++)
            {
                if (symbolObject.symbolName.Contains(slotEvaluationObjects[evaluator].symbolTargetName))
                {
                    if(!output.Contains(slotEvaluationObjects[evaluator].featureName))
                        output.Add(slotEvaluationObjects[evaluator].featureName);
                }
            }
            return output.ToArray();
        }

    }
}