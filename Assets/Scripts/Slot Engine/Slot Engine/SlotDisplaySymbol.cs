//  @ Project : Slot Engine
//  @ Author : Evan McCall
using System;
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
        internal Features[] features;
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
        internal void AddFeature(Features feature, int index)
        {
            if (features == null)
                features = new Features[0];
            features = features.AddTo<Features>(feature);
            is_feature = true;
        }

        internal void AddFeaturesTo(Features[] features)
        {
            if (this.features == null)
                this.features = new Features[0];
            int index_contain_out = -1;
            for (int feature = 0; feature < features.Length; feature++)
            {
                if(!this.features.Contains<Features>(features[feature],out index_contain_out))
                {
                    Debug.Log(String.Format("{0} not", features[feature].ToString()));
                    this.features = this.features.AddTo<Features>(features[feature]);
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