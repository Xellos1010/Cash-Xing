//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomSports.Prototype.Managers
{
    [Serializable]
    public class StripManager
    {
        [UnityEngine.SerializeField]
        public GroupInformationStruct stripInfo;
        internal static NodeDisplaySymbolContainer[] GenerateReelStripStatic(GameModes currentMode, int slotsPerStrip, ref EndConfigurationManager endConfigurationManager)
        {
            //Generate new reel symbols array and assign based on weighted distribution - then add the display symbols at the end for now
            NodeDisplaySymbolContainer[] reel_spin_symbols = new NodeDisplaySymbolContainer[slotsPerStrip];
            for (int i = 0; i < slotsPerStrip; i++)
            {
                reel_spin_symbols[i] = endConfigurationManager.GetRandomWeightedSymbol(currentMode).Result;
            }
            return reel_spin_symbols;
        }

        internal string ReturnDisplaySymbolsPrint()
        {
            return String.Join(" ", stripInfo.spinInformation.displaySymbolSequence);
        }
    }
}