//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Features.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using System;
using System.Collections.Generic;

namespace Slot_Engine.Matrix
{

	public enum Features
	{
		None,
		freespin,
		wild,
		multiplier, //ATM overlays trigger a feature - overlay triggers multiplier in instaspins - maybe freespins somewhere else
		overlay,
		Count
	}

	[System.Serializable]
	public struct FeaturesStructSymbolEvaluation
	{
		[UnityEngine.SerializeField]
		public Features feature;
		[UnityEngine.SerializeField]
		public List<SuffixTreeNodeInfo> appeared_on_node;

		public FeaturesStructSymbolEvaluation(Features feature) : this()
		{
			this.feature = feature;
		}

        internal void AddNodeIfNotExist(ref SuffixTreeNodeInfo node_info)
        {
			//UnityEngine.Debug.Log(String.Format("Checking node to add for feature activation {0}",node_info.Print()));
            //bool add_to_list = true;
            if (appeared_on_node == null)
                appeared_on_node = new List<SuffixTreeNodeInfo>();
            if (!appeared_on_node.Contains(node_info))
				appeared_on_node.Add(node_info);
		}
    }
}