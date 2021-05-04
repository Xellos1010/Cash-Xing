//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
#if UNITY_EDITOR
#endif
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeightedDistribution;

namespace Slot_Engine.Matrix
{
    [Serializable]
    public class WeightsForMode
    {
        /// <summary>
        /// The symbol weights object to draw from
        /// </summary>
        [SerializeField]
        public IntDistribution intDistribution;

        internal async Task SetWeightsForInt(List<float> value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                //Debug.Log(String.Format("{0} value added = {1} iterator = {2}", item.Key.ToString(), item.Value[i],i));
                //Setting the value to the idex of the symbol so to support reorderable lists 2020.3.3
                await Task.Delay(20);
                intDistribution.Add(i, value[i]);
                await Task.Delay(20);
                intDistribution.Items[i].Weight = value[i];
            }
        }
    }
}
