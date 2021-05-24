//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BoomSports.Prototype
{
    /// <summary>
    /// Holds a state defined for weights scriptable object
    /// </summary>
    [Serializable]
    public struct ModeWeights
    {
        [SerializeField]
        public GameModes gameMode;
        [SerializeField]
        public WeightsForMode weightsForModeDistribution;

        public ModeWeights(GameModes key, List<float> value)
        {
            gameMode = key;
            WeightsForMode weightsTemp = new WeightsForMode(value);
            weightsForModeDistribution = weightsTemp;
        }
    }
}
