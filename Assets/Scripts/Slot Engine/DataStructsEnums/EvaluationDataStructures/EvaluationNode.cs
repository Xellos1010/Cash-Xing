//For Parsing Purposes
using Slot_Engine.Matrix;
using System;
using UnityEngine;

[Serializable]
public struct EvaluationNode
{
    [SerializeField]
    public SuffixTreeNodeInfo nodeInfo;
    [SerializeField]
    public int symbol;

    public EvaluationNode(SuffixTreeNodeInfo suffix_tree_node_info, int symbol) : this()
    {
        this.nodeInfo = suffix_tree_node_info;
        this.symbol = symbol;
    }
}
