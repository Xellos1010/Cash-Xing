using System;
using UnityEngine;
//************
#if UNITY_EDITOR
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace BoomSports.Prototype
{
    [Serializable]
    public struct WinningNode
    {
        [SerializeField]
        internal SuffixTreeNodeInfo nodeInfo;
        [SerializeField]
        internal int symbol;

        public WinningNode(SuffixTreeNodeInfo suffix_tree_node_info, int symbol) : this()
        {
            this.nodeInfo = suffix_tree_node_info;
            this.symbol = symbol;
        }
    }
}
