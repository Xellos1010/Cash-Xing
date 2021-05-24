//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoomSports.Prototype
{
    /// <summary>
    /// Holds the display information for slot symbol
    /// </summary>
    [Serializable]
    public struct NodeDisplaySymbolContainer
    {
        [SerializeField]
        internal int primarySymbol;

        public NodeDisplaySymbolContainer(int primary_symbol) : this()
        {
            this.primarySymbol = primary_symbol;
        }

        //internal void SetOverlaySymbolTo(int symbol)
        //{
        //    overlay_symbol = symbol;
        //    is_overlay = true;
        //}
        //internal void AddFeature(Features feature)
        //{
        //    if (features == null)
        //        features = new List<Features>();
        //    features.Add(feature);
        //    is_feature = true;
        //}

        //internal void AddFeaturesTo(Features[] features)
        //{
        //    if (this.features == null)
        //        this.features = new List<Features>();
        //    int index_contain_out = -1;
        //    for (int feature = 0; feature < features.Length; feature++)
        //    {
        //        if(!this.features.Contains(features[feature]))
        //        {
        //            //Debug.Log(String.Format("{0} Adding Feature Counter", features[feature].ToString()));
        //            this.features.Add(features[feature]);
        //        }
        //    }
        //    is_feature = true;
        //}

        //internal void SetWildTo(int symbol)
        //{
        //    this.wild_symbol = symbol;
        //    is_wild = true;
        //}
    }
    
}