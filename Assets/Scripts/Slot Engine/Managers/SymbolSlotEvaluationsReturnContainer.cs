using BoomSports.Prototype.ScriptableObjects;
using System;
using UnityEngine;
namespace BoomSports.Prototype.Containers
{
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
}