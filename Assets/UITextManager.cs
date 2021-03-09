﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(UITextManager))]
    class UITextManagerEditor : BoomSportsEditor
    {
        UITextManager myTarget;
        public void OnEnable()
        {
            myTarget = (UITextManager)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("UI TextManager Properties");

            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("UI TextManager Controls");
            base.OnInspectorGUI();
        }


    }
#endif
    /// <summary>
    /// Manages the machines UI text Fields for displaying machine and player info
    /// </summary>
    public class UITextManager : MonoBehaviour
    {
        [SerializeField]
        private Matrix matrix;
        public TextMeshPro bank, multiplier, freespin_info, player_wallet, bet_amount;
        public void SetBankTo(float value)
        {
            SetTextMeshProTextTo(ref bank, String.Format("${0:n}", value));
            //Testing Purposes Only - To be animated
            bank.enabled = value > 0 ? true : false;
        }
        public void SetMultiplierTo(float value)
        {
            SetTextMeshProTextTo(ref multiplier, String.Format("{0}x", value));
            //Testing Purposes Only - To be animated
            multiplier.enabled = value <= 1  ? false : true;
        }

        public void SetFreeSpinRemainingTo(int value)
        {
            SetTextMeshProTextTo(ref freespin_info, String.Format("{0} Free Spin{1} Remaining", value, value > 1 ? "s" : ""));
            //Testing Purposes Only - To be animated
            freespin_info.enabled = value > 0 ? true : false;
        }
        /// <summary>
        /// Sets the player wallet text to ${0:n}
        /// </summary>
        /// <param name="to_value">new player wallet value</param>
        public void Set_Player_Wallet_To(float to_value)
        {
            SetTextMeshProTextTo(ref player_wallet, String.Format("${0:n}", to_value));
        }
        /// <summary>
        /// Sets the bet amount text to ${0:n}
        /// </summary>
        /// <param name="to_value">new bet amount value</param>
        public void SetBetAmountTo(float to_value)
        {
            SetTextMeshProTextTo(ref bet_amount, String.Format("${0:n}", to_value));
        }
        /// <summary>
        /// Sets a TextMeshPro text field to value
        /// </summary>
        /// <param name="tmp_element">TMP Object reference to modify</param>
        /// <param name="to_value">to value of TMP.text</param>
        private void SetTextMeshProTextTo(ref TextMeshPro tmp_element, string to_value)
        {
            tmp_element.text = to_value;
        }
        /// <summary>
        /// Sets references for event manager from matrix.machine_information_manager
        /// </summary>
        void OnEnable()
        {
            matrix.machine_information_manager.new_multiplier_set += Machine_information_manager_new_multiplier_set;
            matrix.machine_information_manager.new_bet_amount += Machine_information_manager_new_bet_amount;
            matrix.machine_information_manager.new_bank_amount += Machine_information_manager_new_bank_amount;
            matrix.machine_information_manager.new_player_wallet_amount += Machine_information_manager_new_player_wallet_amount;
            matrix.machine_information_manager.new_freespin_amount += Machine_information_manager_new_freespin_amount;
        }

        private void Machine_information_manager_new_freespin_amount(int new_freespin_value)
        {
            SetFreeSpinRemainingTo(new_freespin_value);
        }

        void OnDisable()
        {
            matrix.machine_information_manager.new_multiplier_set -= Machine_information_manager_new_multiplier_set;
            matrix.machine_information_manager.new_bet_amount -= Machine_information_manager_new_bet_amount;
            matrix.machine_information_manager.new_bank_amount -= Machine_information_manager_new_bank_amount;
            matrix.machine_information_manager.new_player_wallet_amount -= Machine_information_manager_new_player_wallet_amount;
            matrix.machine_information_manager.new_freespin_amount -= Machine_information_manager_new_freespin_amount;
        }
        /// <summary>
        /// Handles new float value setting for player wallet
        /// </summary>
        /// <param name="new_player_wallet_amount"></param>
        private void Machine_information_manager_new_player_wallet_amount(float new_player_wallet_amount)
        {
            Set_Player_Wallet_To(new_player_wallet_amount);
        }

        private void Machine_information_manager_new_bank_amount(float new_bank_amount)
        {
            SetBankTo(new_bank_amount);
        }

        private void Machine_information_manager_new_bet_amount(float new_bet_amount)
        {
            SetBetAmountTo(new_bet_amount);
        }

        private void Machine_information_manager_new_multiplier_set(float new_multiplier)
        {
            SetMultiplierTo(new_multiplier);
        }
    }
}