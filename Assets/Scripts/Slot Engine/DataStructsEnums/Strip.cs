//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using Slot_Engine.Matrix;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Strip
{
    [UnityEngine.SerializeField]
    public StripStruct stripInfo;
    internal static NodeDisplaySymbol[] GenerateReelStripStatic(GameModes currentMode, int slotsPerStrip, ref EndConfigurationManager endConfigurationManager)
    {
        //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
        NodeDisplaySymbol[] reel_spin_symbols = new NodeDisplaySymbol[slotsPerStrip];
        for (int i = 0; i < slotsPerStrip; i++)
        {
            reel_spin_symbols[i] = endConfigurationManager.GetRandomWeightedSymbol(currentMode).Result;
        }
        return reel_spin_symbols;
    }

    internal string ReturnDisplaySymbolsPrint()
    {
        return String.Join(" ", stripInfo.spin_info.displaySymbols);
    }
}
