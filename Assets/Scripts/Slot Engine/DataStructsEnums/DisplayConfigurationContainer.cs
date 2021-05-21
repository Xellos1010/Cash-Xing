//  @ Project : Slot Engine
//  @ Author : Evan McCall

using System;
using UnityEngine;
/// <summary>
/// Used to track end configuration per spin
/// </summary>
[Serializable]
public struct DisplayConfigurationContainer
{
    /// <summary>
    /// Holds the display configuration for the next spin - Steppers reels lengths will determine length of Display Symbols to generate for each spin
    /// </summary>
    [SerializeField]
    public GroupSpinInformationStruct[] configuration;
    /// <summary>
    /// Prints the display symbols on the configuration
    /// </summary>
    /// <returns></returns>
    internal string PrintDisplaySymbols()
    {
        //Debug.Log($"displaySymbols.Length = {displaySymbols.Length}");
        string output = "";
        for (int i = 0; i < configuration.Length; i++)
        {
            output += "||" + configuration[i].PrintDisplaySymbols();
        }
        return output;
    }
}
