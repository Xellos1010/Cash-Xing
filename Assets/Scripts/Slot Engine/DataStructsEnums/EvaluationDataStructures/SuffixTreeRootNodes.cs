using System;
using UnityEngine;
//************
#if UNITY_EDITOR
#endif
/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

namespace Slot_Engine.Matrix
{
    [Serializable]
    public struct SuffixTreeRootNodes
    {
        [SerializeField]
        internal SuffixTreeNodes[] rootNodes;
        //TODO Abstract and remove - this is Payline mode only to literal for level of abstraction
        [SerializeField]
        public Payline[] paylinesSupported;

        internal Payline ReturnPayline(int payline_to_show)
        {
            return paylinesSupported[payline_to_show];
        }

        internal void AddPaylineSupported(int[] vs, bool leftRight)
        {
            if (paylinesSupported == null)
                paylinesSupported = new Payline[0];
            paylinesSupported = paylinesSupported.AddAt<Payline>(paylinesSupported.Length, new Payline(vs, leftRight));
        }
    }
}
