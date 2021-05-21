//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using Slot_Engine.Matrix;
using System;

[Serializable]
public struct StripsStruct
{
    [UnityEngine.SerializeField]
    public GroupInformationStruct[] strips;

    public StripsStruct(ref ConfigurationDisplayZonesStruct[] displayZonesPerStrip) : this()
    {
        strips = new GroupInformationStruct[displayZonesPerStrip.Length];
        for (int reel_number = 0; reel_number < strips.Length; reel_number++)
        {
            strips[reel_number] = new GroupInformationStruct(reel_number);
        }
    }

    internal string PrintDisplaySymbols()
    {
        string output = "";
        for (int i = 0; i < strips.Length; i++)
        {
            output += "-" + String.Join("|", strips[i].spinInformation.displaySymbolSequence);
        }
        return output;
    }

    //internal string PrintStrips()
    //{
    //    string output = "";
    //    for (int i = 0; i < strips.Length; i++)
    //    {
    //        output += "-" + strips[i].displayZonesSetting.totalPositions;
    //    }
    //    return output;
    //}
}
