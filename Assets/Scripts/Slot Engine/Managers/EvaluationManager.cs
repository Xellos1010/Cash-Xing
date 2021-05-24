using BoomSports.Prototype;
using BoomSports.Prototype.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
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

        [Serializable]
        public struct SymbolSlotEvaluationsReturnContainer
        {
            /// <summary>
            /// Dispplay symbol searched
            /// </summary>
            [SerializeField]
            public NodeDisplaySymbolContainer symbolChecked;
            /// <summary>
            /// Connected Evaluators to symbol
            /// </summary>
            [SerializeField]
            public SlotEvaluationScriptableObject[] connectedEvaluators;

            public SymbolSlotEvaluationsReturnContainer(NodeDisplaySymbolContainer displaySymboltoCheck, SlotEvaluationScriptableObject[] connectedEvaluators) : this()
            {
                symbolChecked = displaySymboltoCheck;
                this.connectedEvaluators = connectedEvaluators;
            }
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
        public EvaluationScriptableObject[] coreEvaluationObjects;
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
        internal Task<WinningPayline[]> EvaluateSymbolConfigurationForWinningPaylines(DisplayConfigurationContainer configurationToEvaluate)
        {
            //Build a list of evaluation objects based on feature evaluation and core evaluation objects
            List<object> output_raw = new List<object>();

            //Build list of evaluations to make out of features and core objects
            List<EvaluationObjectStruct> coreEvaluationsToRun = new List<EvaluationObjectStruct>();

            //Clear all feature conditions of activated nodes previously

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

            //Build the raw winning paylines
            for (int evaluation = 0; evaluation < coreEvaluationsToRun.Count; evaluation++)
            {
                output_raw.Add(coreEvaluationsToRun[evaluation].Evaluate());
            }
            //TODO Abstract Winning Paylines to Winning Objects
            List<WinningPayline> output_filtered = new List<WinningPayline>();
            for (int i = 0; i < output_raw.Count; i++)
            {
                output_filtered.AddRange((WinningPayline[])Convert.ChangeType(output_raw[i], typeof(WinningPayline[])));
            }
            //Check that feature conditions are met and activated after return
            return Task.FromResult<WinningPayline[]>(output_filtered.ToArray());
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
        internal static T GetFirstInstanceCoreEvaluationObject<T>(ref EvaluationScriptableObject[] coreEvaluationObjects)
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
            //
            //DisplayConfigurationSymbolsGroup[] displayConfiguration = new DisplayConfigurationSymbolsGroup[configurationToEvaluateContainer.configuration.Length];
            //for (int symbolGroup = 0; symbolGroup < configurationToEvaluateContainer.configuration.Length; symbolGroup++)
            //{
            //    displayConfiguration[symbolGroup].SetColumnSymbolsTo(configurationToEvaluateContainer.configuration[symbolGroup].displaySymbols);
            //}
            return await EvaluateWinningSymbols(configurationToEvaluateContainer);
        }

        public async Task<WinningPayline[]> EvaluateWinningSymbols(DisplayConfigurationContainer configurationContainer)
        {
            return await EvaluateSymbolConfigurationForWinningPaylines(configurationContainer);
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
            await EvaluateSymbolConfigurationForWinningPaylines(temp);
        }

        private DisplayConfigurationContainer BuildStripSpinStructArrayFromSymbolsOnDisplay()
        {
            DisplayConfigurationContainer output = new DisplayConfigurationContainer();
                output.configuration = new GroupSpinInformationStruct[configurationObject.configurationGroupManagers.Length];
            for (int i = 0; i < output.configuration.Length; i++)
            {
                output.configuration[i] = new GroupSpinInformationStruct(configurationObject.configurationGroupManagers[i].GetNodeDisplaySymbols());
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