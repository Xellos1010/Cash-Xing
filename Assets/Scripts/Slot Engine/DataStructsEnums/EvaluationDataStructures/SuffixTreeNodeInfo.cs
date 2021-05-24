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
    public struct SuffixTreeNodeInfo
    {
        [SerializeField]
        internal int column;
        [SerializeField]
        internal int row;

        internal (int,int) ColumnRow()
        {
            return (column,row);
        }

        public SuffixTreeNodeInfo(int column, int row) : this()
        {
            this.column = column;
            this.row = row;
        }

        internal string Print()
        {
            return String.Format("Node: Column {0} Row {1}", column, row);
        }
    }
}
