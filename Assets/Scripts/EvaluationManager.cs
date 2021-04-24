using Slot_Engine.Matrix;
using Slot_Engine.Matrix.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace Slot_Engine.Matrix.Managers
{
    public class EvaluationManager : MonoBehaviour
    {
        /// <summary>
        /// These COntrol the various ways a ReelSymbolConfiguration can be evaluated 
        /// </summary>
        public List<EvaluationScriptableObject> featureEvaluationObjects;
        /// <summary>
        /// This is either ways lines or grouped.
        /// </summary>
        public EvaluationScriptableObject coreEvaluationObject;

        private WinningPayline[] CheckForWinningPaylines(ref ReelSymbolConfiguration[] symbols_configuration, ref Dictionary<Features, List<suffix_tree_node_info>> feature_active_count)
        {
            List<WinningPayline> output_raw = new List<WinningPayline>();
            List<WinningPayline> output_filtered = new List<WinningPayline>();
            for (int root_node = 0; root_node < dynamic_paylines_evaluation.dynamic_paylines.root_nodes.Length; root_node++)
            {
                output_raw.AddRange(dynamic_paylines_evaluation.dynamic_paylines.root_nodes[root_node].InitializeAndCheckForWinningPaylines(ref symbols_configuration, ref feature_active_count));
                FilterRawOutputForDuplicateRootNodeEntries(ref output_filtered, ref output_raw);
                output_filtered.AddRange(output_raw);
                output_raw.Clear();
                //Debug.Log(String.Format("winning paylines Count = {0} for root_node {1} info = {2}", output_filtered.Count,root_node, dynamic_paylines_evaluation.dynamic_paylines.root_nodes[root_node].node_info.Print()));
            }
            if (feature_active_count.ContainsKey(Features.multiplier))
            {
                feature_active_count[Features.multiplier].Clear();
                SlotDisplaySymbol linewin_symbol;
                //Verify multiplier is apart of winning payline
                PaylineNode symbolObject;
                //Debug.Log("Checking Multiplier has only winning payline nodes");
                for (int winning_payline = 0; winning_payline < output_filtered.Count; winning_payline++)
                {
                    //Debug.Log(String.Format("Checking Winning Payline {0}", output_filtered[winning_payline].payline.PrintConfiguration()));

                    for (int symbol = 0; symbol < output_filtered[winning_payline].winning_symbols.Length; symbol++)
                    {
                        symbolObject = output_filtered[winning_payline].winning_symbols[symbol];
                        linewin_symbol = symbols_configuration[symbolObject.suffix_tree_node_info.column].reel_slots[symbolObject.suffix_tree_node_info.row];
                        //Debug.Log(String.Format("Checking Winning Payline {0}, Node = {1} Symbol = {2} isOverlay = {3}", output_filtered[winning_payline].payline.PrintConfiguration(), symbolObject.suffix_tree_node_info.Print(), linewin_symbol.primary_symbol,linewin_symbol.is_overlay));
                        if (!feature_active_count[Features.multiplier].Contains(symbolObject.suffix_tree_node_info) && linewin_symbol.is_overlay)
                        {
                            feature_active_count[Features.multiplier].Add(symbolObject.suffix_tree_node_info);
                        }
                    }
                }
                if (feature_active_count[Features.multiplier].Count == 0)
                    feature_active_count.Remove(Features.multiplier);
            }
            return output_filtered.ToArray();

        }
        internal Task<WinningPayline[]> EvaluateSymbolConfigurationForWinningPaylines(ReelSymbolConfiguration[] symbols_configuration)
        {
            //We are using corePaylineEvaluationObject to evaluate the winning paylines. we pass the features in featureEvaluationObjects to return and activate any necessary features in the engine.
            WinningPayline[] output;
            int primary_symbol_index = -1; //Set to the first node of each payline root node that evaluates (plus shape is center node of shape, matrix first node in column or row, etc...)
            //Tracks the nodes activating a feature defined by evaluation scriptable object target symbols and activation conditions
            Dictionary<Features, List<suffix_tree_node_info>> feature_active_count = new Dictionary<Features, List<suffix_tree_node_info>>();
            //Check for winning paylines based on core evaluation mechanic
            output = CheckForWinningPaylines(symbols_configuration, ref feature_active_count);

                Debug.Log(String.Format("feature_active_count.keys = {0}", feature_active_count.Keys.Count));
                //Debug.Log(String.Format("Looking for features that activated"));
                foreach (KeyValuePair<Features, List<suffix_tree_node_info>> item in feature_active_count)
                {
                    //Multiplier calculated first then mode is applied
                    Debug.Log(String.Format("Feature name = {0}, counter = {1} mode - {2}", item.Key.ToString(), item.Value.Count, StateManager.enCurrentMode));
                    if ((item.Key == Features.overlay || item.Key == Features.multiplier))
                    {
                        Debug.Log("Overlay Symbol Found in Winning Paylines");
                        if (StateManager.enCurrentMode == GameStates.baseGame)
                            StateManager.SetFeatureActiveTo(Features.multiplier, true); //Activate the multiplier feature if in base game for 3 freespins
                        overlaySymbols = item.Value; //Calculate overlay symbols at the end of spin - thats where
                    }
                    if (item.Key == Features.freespin)
                        if (item.Value.Count > 2) // TODO make feature scriptable object for freespin threseholds
                        {
                            //Set Free Spins Amount then activate on spin end
                            StateManager.SetFeatureActiveTo(Features.freespin, true);
                        }
                }
            }
        }
    }



    /// <summary>
    /// Defines the Slot Display Symbols to evaluate
    /// </summary>
    [Serializable]
    public struct ReelSymbolConfiguration
    {
        [SerializeField]
        public SlotDisplaySymbol[] reel_slots;

        internal void SetReelSymbolsTo(SlotDisplaySymbol[] display_symbols)
        {
            reel_slots = display_symbols;
        }
    }
}