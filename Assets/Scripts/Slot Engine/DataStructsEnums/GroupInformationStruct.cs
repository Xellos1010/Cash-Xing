//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
using Slot_Engine.Matrix;
using System;
using UnityEngine;

[Serializable]
public struct GroupInformationStruct
{
    /// <summary>
    /// index in Group Manager Array
    /// </summary>
    [SerializeField]
    internal int index;
    /// <summary>
    /// spin information for the group - includes evaluator for spin based on time
    /// </summary>
    [UnityEngine.SerializeField]
    internal GroupSpinInformationStruct spinInformation;

    public GroupInformationStruct(int index) : this()
    {
        this.index = index;
    }

    internal void SetSpinConfigurationTo(GroupSpinInformationStruct groupSpinInformationStruct)
    {
        //TODO Load spinInformation symbol sequences with the proper spin info
        throw new NotImplementedException();
    }
}
