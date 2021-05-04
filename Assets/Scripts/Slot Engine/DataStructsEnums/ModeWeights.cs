//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Slot_Engine.Matrix
{
    /// <summary>
    /// Holds a state defined for weights scriptable object
    /// </summary>
    [Serializable]
    public class ModeWeights
    {
        [SerializeField]
        public GameModes gameMode;
        [SerializeField]
        public WeightsForMode weightsDistribution;

        public ModeWeights(GameModes key, List<float> value)
        {
            gameMode = key;
            weightsDistribution.SetWeightsForInt(value);
        }
    }
}
