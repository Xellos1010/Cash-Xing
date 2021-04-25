//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slot_Engine.Matrix
{
    /// <summary>
    /// Holds the display information for slot symbol
    /// </summary>
    [Serializable]
    public struct SlotDisplaySymbol
    {
        [SerializeField]
        internal int primary_symbol;
        [SerializeField]
        internal int overlay_symbol;
        [SerializeField]
        internal bool is_overlay;
        /// <summary>
        /// Feature associated
        /// </summary>
        [SerializeField]
        internal List<Features> features;
        /// <summary>
        /// Is this a feature
        /// </summary>
        [SerializeField]
        internal bool is_feature;
        /// <summary>
        /// The index for the wild symbol
        /// </summary>
        [SerializeField]
        internal int wild_symbol;
        /// <summary>
        /// is wild active
        /// </summary>
        [SerializeField]
        internal bool is_wild;

        public SlotDisplaySymbol(int primary_symbol) : this()
        {
            this.primary_symbol = primary_symbol;
        }

        internal void SetOverlaySymbolTo(int symbol)
        {
            overlay_symbol = symbol;
            is_overlay = true;
        }
        internal void AddFeature(Features feature)
        {
            if (features == null)
                features = new List<Features>();
            features.Add(feature);
            is_feature = true;
        }

        internal void AddFeaturesTo(Features[] features)
        {
            if (this.features == null)
                this.features = new List<Features>();
            int index_contain_out = -1;
            for (int feature = 0; feature < features.Length; feature++)
            {
                if(!this.features.Contains(features[feature]))
                {
                    //Debug.Log(String.Format("{0} Adding Feature Counter", features[feature].ToString()));
                    this.features.Add(features[feature]);
                }
            }
            is_feature = true;
        }

        internal void SetWildTo(int symbol)
        {
            this.wild_symbol = symbol;
            is_wild = true;
        }
    }
    
}