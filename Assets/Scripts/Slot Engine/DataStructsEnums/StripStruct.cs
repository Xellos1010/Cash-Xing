//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using Slot_Engine.Matrix;
using System;
using UnityEngine;

[Serializable]
public struct StripStruct
{
    /// <summary>
    /// Reel position in sequence
    /// </summary>
    [SerializeField]
    internal int stripColumn;
    /// <summary>
    /// Display Zone Configuration for Strip
    /// </summary>
    [SerializeField]
    internal ConfigurationDisplayZonesStruct stripDisplayZonesSetting;

    /// <summary>
    /// spin_informatino for the reelstrip
    /// </summary>
    [UnityEngine.SerializeField]
    internal StripSpinStruct spin_info;
    internal int total_slot_objects
    {
        get
        {
            int output = 0;
            output += stripDisplayZonesSetting.paddingBefore;
            GetTotalDisplaySlots(ref output);
            return output;
        }
    }
    internal int total_display_slots
    {
        get
        {
            int output = 0;
            GetTotalDisplaySlots(ref output);
            return output;
        }
    }
    internal int total_positions
    {
        get
        {
            int output = 0;
            output += stripDisplayZonesSetting.paddingBefore;
            GetTotalDisplaySlots(ref output);
            output += stripDisplayZonesSetting.paddingAfter;
            return output;
        }
    }

    private void GetTotalDisplaySlots(ref int output)
    {
        if (stripDisplayZonesSetting.displayZones != null)
        {
            for (int displayZone = 0; displayZone < stripDisplayZonesSetting.displayZones.Length; displayZone++)
            {
                output += stripDisplayZonesSetting.displayZones[displayZone].positionsInZone;
            }
        }
    }

    public StripStruct(int stripNumber, ConfigurationDisplayZonesStruct stripDisplayZonesSetting) : this()
    {
        this.stripColumn = stripNumber;
        //Debug.Log($"display_zone number of zones = {stripDisplayZonesSetting.displayZones.Length}");
        ConfigurationDisplayZonesStruct displayZoneTemp = new ConfigurationDisplayZonesStruct(stripDisplayZonesSetting);
        this.stripDisplayZonesSetting = displayZoneTemp;
        //Debug.Log($"this.display_zone number of zones = {this.stripDisplayZonesSetting.displayZones.Length}");
    }

    public StripStruct(StripStruct stripStruct) : this()
    {
        stripColumn = stripStruct.stripColumn;
        stripDisplayZonesSetting = stripStruct.stripDisplayZonesSetting;
        spin_info = stripStruct.spin_info;
    }

    internal void SetSpinConfigurationTo(StripSpinStruct reelStripStruct)
    {
        spin_info = reelStripStruct;
    }

    internal BasePathTransformSpinEvaluatorScriptableObject GetSpinParameters()
    {
        return stripDisplayZonesSetting.spinParameters;
    }
    /// <summary>
    /// Gets the first instance of an evaluation object of sub-class
    /// </summary>
    /// <typeparam name="T">Type of evaluation manager to return</typeparam>
    /// <returns>Type if in list or null if nothing</returns>
    internal static T GetSpinParametersAs<T>(ref StripSpinEvaluatorBaseScriptableObject baseSpinParameters)
    {
        object output = null;
        if (baseSpinParameters.GetType() == typeof(T))
        {
            output = baseSpinParameters;
        }
        return (T)Convert.ChangeType(output, typeof(T)); ;
    }
}
