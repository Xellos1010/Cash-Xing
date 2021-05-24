//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
#if UNITY_EDITOR
#endif
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeightedDistribution;

namespace BoomSports.Prototype
{
    [Serializable]
    public struct WeightsForMode
    {
        /// <summary>
        /// The symbol weights object to draw from
        /// </summary>
        [SerializeField]
        public WeightsDistributionScriptableObject weightDistributionScriptableObject;
        [SerializeField]
        internal List<float> symbolWeights;

        public WeightsForMode(List<float> symbolWeights) : this()
        {
            this.symbolWeights = symbolWeights;
            SetWeightsForInt(symbolWeights);
        }

        internal void SetWeightsForInt(List<float> value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                //Debug.Log(String.Format("{0} value added = {1} iterator = {2}", item.Key.ToString(), item.Value[i],i));
                //Setting the value to the idex of the symbol so to support reorderable lists 2020.3.3
                //await Task.Delay(20);
                weightDistributionScriptableObject.intDistribution.Add(i, value[i]);
                //await Task.Delay(20);
                weightDistributionScriptableObject.intDistribution.Items[i].Weight = value[i];
            }
        }
    }
}
