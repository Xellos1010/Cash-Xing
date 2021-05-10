using System;
using System.Collections.Generic;
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
        public List<Payline> paylinesSupported;

        internal Payline ReturnPayline(int payline_to_show)
        {
            return paylinesSupported[payline_to_show];
        }

        internal void AddPaylineSupported(int[] payline, bool leftRight)
        {
            Payline toAdd = new Payline(payline, leftRight);
            if (paylinesSupported == null)
                paylinesSupported = new List<Payline>();
            //Debug.Log($"Raw Payline File = {String.Join("|", payline)}");
            //Debug.Log($"Payline added configuration = {toAdd.PrintConfiguration()}");
            paylinesSupported.Add(toAdd);
        }
    }
}
