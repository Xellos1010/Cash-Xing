using Slot_Engine.Matrix;
using Slot_Engine.Matrix.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix.Managers
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
            winning_paylines = serializedObject.FindProperty("winning_paylines");
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
                        myTarget.configurationObject.managers.paylines_manager.ShowDynamicPaylineRaw(payline_to_show);
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
    public class EvaluationManager : BoomSportsManager
    {
        /// <summary>
        /// These Control the various ways a gridConfiguration can be evaluated for features - Wild allows more winning lines/ways/shapes - Overlays have sub symbols- trigger symbols trigger a feature 
        /// </summary>
        public SlotEvaluationScriptableObject[] slotEvaluationObjects;
        /// <summary>
        /// This is either ways lines or grouped.
        /// </summary>
        public EvaluationScriptableObject[] coreEvaluationObjects;
        //temporary to get working - need to refactor to get list of activated overlays from scriptable object
        public List<SuffixTreeNodeInfo> overlaySymbols
        {
            get
            {
                return GetFirstInstanceFeatureEvaluationObject<OverlayEvaluationScriptableObject>(ref slotEvaluationObjects).nodesActivatingEvaluationConditions;
            }
        }

        /// <summary>
        /// Evaluates the symbols configuration for winning paylines
        /// </summary>
        /// <param name="symbols_configuration"></param>
        /// <returns></returns>
        internal Task<WinningPayline[]> EvaluateSymbolConfigurationForWinningPaylines(ReelSymbolConfiguration[] symbols_configuration)
        {
            //Build a list of evaluation objects based on feature evaluation and core evaluation objects
            List<object> output_raw = new List<object>();

            //Build list of evaluations to make out of features and core objects
            List<EvaluationObjectStruct> evaluationsToTake = new List<EvaluationObjectStruct>();

            //Clear all feature conditions of activated nodes previously


            for (int coreEvaluationObject = 0; coreEvaluationObject < coreEvaluationObjects.Length; coreEvaluationObject++)
            {
                evaluationsToTake.Add(new EvaluationObjectStruct(coreEvaluationObjects[coreEvaluationObject], slotEvaluationObjects, symbols_configuration));
                coreEvaluationObjects[coreEvaluationObject].ClearWinningObjects();
            }
            for (int slotEvaluationObject = 0; slotEvaluationObject < slotEvaluationObjects.Length; slotEvaluationObject++)
            {
                slotEvaluationObjects[slotEvaluationObject].ClearWinningObjects();
            }
            for (int evaluation = 0; evaluation < evaluationsToTake.Count; evaluation++)
            {
                output_raw.Add(evaluationsToTake[evaluation].Evaluate());
            }
            List<WinningPayline> output_filtered = new List<WinningPayline>();
            for (int i = 0; i < output_raw.Count; i++)
            {
                output_filtered.AddRange((WinningPayline[])Convert.ChangeType(output_raw[i], typeof(WinningPayline[])));
            }
            //Check that feature conditions are met and activated
            return Task.FromResult<WinningPayline[]>(output_filtered.ToArray());
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

        internal async Task<WinningPayline[]> EvaluateWinningSymbols(StripSpinStruct[] reelstrips_configuration)
        {
            ReelSymbolConfiguration[] symbols_configuration = new ReelSymbolConfiguration[reelstrips_configuration.Length];
            for (int reel = 0; reel < reelstrips_configuration.Length; reel++)
            {
                symbols_configuration[reel].SetColumnSymbolsTo(reelstrips_configuration[reel].displaySymbols);
            }
            return await EvaluateWinningSymbols(symbols_configuration);
        }

        public async Task<WinningPayline[]> EvaluateWinningSymbols(ReelSymbolConfiguration[] symbols_configuration)
        {
            return await EvaluateSymbolConfigurationForWinningPaylines(symbols_configuration);
        }

        public bool evaluated = false;
        public async void EvaluateWinningSymbolsFromCurrentConfiguration()
        {
            //Debug.Log(String.Format("Evaluating Symbols in configuration {0}", matrix.slot_machine_managers.end_configuration_manager.current_reelstrip_configuration.PrintDisplaySymbols()));
            await EvaluateWinningSymbols(configurationObject.managers.endConfigurationManager.currentReelstripConfiguration);
        }

        internal bool DoesSymbolActivateFeature(SymbolObject symbolObject, Features feature)
        {
            for (int slotEvaluator = 0; slotEvaluator < slotEvaluationObjects.Length; slotEvaluator++)
            {
                if (slotEvaluationObjects[slotEvaluator].featureName == feature || feature == Features.Count)
                {
                    if (slotEvaluationObjects[slotEvaluator].symbolTargetNames.Contains(symbolObject.symbolName))
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
                if (slotEvaluationObjects[slotEvaluator].symbolTargetNames.Contains(symbolObject.symbolName))
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
                if (slotEvaluationObjects[evaluator].symbolTargetNames.Contains(symbolObject.symbolName))
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
                if (slotEvaluationObjects[evaluator].symbolTargetNames.Contains(symbolObject.symbolName))
                {
                    if(!output.Contains(slotEvaluationObjects[evaluator].featureName))
                        output.Add(slotEvaluationObjects[evaluator].featureName);
                }
            }
            return output.ToArray();
        }
    }
}