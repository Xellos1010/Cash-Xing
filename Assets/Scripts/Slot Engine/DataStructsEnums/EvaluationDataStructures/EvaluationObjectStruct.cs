using Slot_Engine.Matrix;
using Slot_Engine.Matrix.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Defines a ReelSymboLConfiguration and the coreEvaluationObjects used to evaluate it
/// </summary>
[Serializable]
public struct EvaluationObjectStruct
{
    /// <summary>
    /// Display Configuration that's being evaluated
    /// </summary>
    [SerializeField]
    public DisplayConfigurationContainer displayConfigurationContainerEvaluating;
    /// <summary>
    /// Core Evaluation Object Logic - Ways - Lines - etc...
    /// </summary>
    [SerializeField]
    public EvaluationScriptableObject evaluationScriptableObject;
    /// <summary>
    /// Multiple evaluations methods, Wild, Overlay, Trigger Symbol
    /// </summary>
    [SerializeField]
    public SlotEvaluationScriptableObject[] slotEvaluationObjects;
    /// <summary>
    /// Winning Symbol Nodes
    /// </summary>
    [SerializeField]
    public List<EvaluationNode> winningEvaluationNodes;
    [SerializeField]
    internal Dictionary<Features, List<SuffixTreeNodeInfo>> featureEvaluationActiveCount;

    public EvaluationObjectStruct(EvaluationScriptableObject evaluationScriptableObject, SlotEvaluationScriptableObject[] slotEvaluationObjects, DisplayConfigurationContainer displayConfigurationContainerToEvaluate) : this()
    {
        this.displayConfigurationContainerEvaluating = displayConfigurationContainerToEvaluate;
        this.evaluationScriptableObject = evaluationScriptableObject;
        this.slotEvaluationObjects = slotEvaluationObjects;
    }

    internal int? maxLength
    {
        get
        {
            return displayConfigurationContainerEvaluating.configuration.Length;
        }
    }

    internal object Evaluate()
    {
        return evaluationScriptableObject.EvaluatePaylines(ref this);
    }

    internal void InitializeWinningSymbolsFeaturesActiveCollections()
    {
        winningEvaluationNodes = new List<EvaluationNode>();
    }

    internal bool? ContainsItemWithFeature<T>(Features featureName, ref SlotEvaluationScriptableObject slotEvaluationActivated)
    {
        // dynamic
        for (int feature = 0; feature < slotEvaluationObjects.Length; feature++)
        {
            Debug.Log($"Checking for if passed feature name {featureName.ToString()} matchs checking feature name {slotEvaluationObjects[feature].featureName.ToString()}");
            if (slotEvaluationObjects[feature].featureName.ToString() == featureName.ToString())
            {
                slotEvaluationActivated = slotEvaluationObjects[feature];
                return true;
            }
            else
            {
                Debug.Log("Checking next feature");
            }
        }
        slotEvaluationActivated = null;
        return false;
    }

    /// <summary>
    /// Gets the first instance of a feature evaluation object of sub-class
    /// </summary>
    /// <typeparam name="T">Type of evaluation manager to return</typeparam>
    /// <returns>Type if in list or null if nothing</returns>
    internal T GetFirstInstanceFeatureEvaluationObject<T>()
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
}