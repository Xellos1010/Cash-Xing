using System;
using UnityEngine;

namespace WeightedDistribution
{
    [System.Serializable]
    public class IntDistributionItem : DistributionItem<int> {}
    [Serializable]
    public class IntDistribution : Distribution<int, IntDistributionItem>
    {
        
    }
}