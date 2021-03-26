using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WeightedRandom
{
    //Weights are BW01, MA01, MI01, MI02, MI03, RO01, RO02, RO03, SA_01, SA_02
    //TODO make accessible to unity editor
    private static int[] weights = new int[10]
    {
        100,200,300,400,500,600,700,800,900,1000
    };
    private static int weightTotal
    {
        get
        {
            if (_weightTotal == 0)
            {
                foreach (int w in weights)
                {
                    _weightTotal += w;
                }
            }
            return _weightTotal;
        }
    }
    private static int _weightTotal = 0;


    public static int RandomWeighted()
    {
        int result = 0, total = 0;
        int randVal = Random.Range(0, weightTotal);
        for (result = 0; result < weights.Length; result++)
        {
            total += weights[result];
            if (total > randVal) break;
        }
        return result;
    }
}
