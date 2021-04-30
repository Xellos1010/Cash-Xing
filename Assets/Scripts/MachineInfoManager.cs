using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MachineInfoManager))]
    class MachineInfoManagerEditor : BoomSportsEditor
    {
        MachineInfoManager myTarget;
        public void OnEnable()
        {
            myTarget = (MachineInfoManager)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("MachineInfoManager Properties");
            
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("MachineInfoManager Controls");
            if (GUILayout.Button("Initialize Machine with test values"))
            {
                myTarget.InitializeTestMachineValues(10000.0f, 0.0f, myTarget.machineInfoScriptableObject.supported_bet_amounts.Length-1, 1, 0);
            }
            if (GUILayout.Button("Use Stock Player Information"))
            {
                myTarget.SetPlayerInformationTo(10000.0f);
            }
            base.OnInspectorGUI();
        }


    }
#endif

    /// <summary>
    /// Manages the info for the machine. Bet amount Increase/Decrease, Multiplier Increase/Decrease, Free Spins Remaining Increase/Decrease, Player Information Handling
    /// </summary>
    public class MachineInfoManager : MonoBehaviour
    {
        public delegate void FloatValueSet(float new_float_value);
        public delegate void DoubleValueSet(double new_double_value);
        public delegate void IntValueSet(int new_int_value);
        public delegate void SetBoolValue(bool toValue);
        public event SetBoolValue setBankEnabled;
        public event FloatValueSet newMultiplier;
        public event FloatValueSet newBetAmount;
        public event DoubleValueSet newBankAmount;
        public event DoubleValueSet newPlayerWalletAmount;
        public event IntValueSet newFreespinAmount;
        public MachineInfoScriptableObject machineInfoScriptableObject;

        internal void DecreaseBetAmount()
        {
            if(machineInfoScriptableObject.current_bet_amount != 0)
            {
                SetBetAmountIndexTo(machineInfoScriptableObject.current_bet_amount - 1);
            }
        }

        internal void IncreaseBetAmount()
        {
            if (machineInfoScriptableObject.current_bet_amount < machineInfoScriptableObject.supported_bet_amounts.Length-1)
            {
                SetBetAmountIndexTo(machineInfoScriptableObject.current_bet_amount + 1);
            }
        }

        
        /// <summary>
        /// This is a test class to implement player info - Player info will be loaded from server config file
        /// </summary>
        public void SetPlayerInformationTo(float player_wallet)
        {
            machineInfoScriptableObject.current_player_information = new PlayerInformation();
            machineInfoScriptableObject.current_player_information.player_wallet = player_wallet;
            SetPlayerInformationFrom(ref machineInfoScriptableObject.current_player_information);
        }
        /// <summary>
        /// Sets the player_wallet and bank roll from player info
        /// </summary>
        /// <param name="current_player_information">Player Information to user</param>
        private void SetPlayerInformationFrom(ref PlayerInformation current_player_information)
        {
            SetPlayerWalletTo(current_player_information.player_wallet);
        }
        internal void OffsetBankBy(double amount)
        {
            Debug.Log(String.Format("Offsetting bank by {0}",amount));
            SetBankTo(machineInfoScriptableObject.bank + amount);
        }

        internal void SetBankTo(double new_bank_amount)
        {
            //Debug.Log(String.Format("Bank is being set to {0}",new_bank_amount));
            machineInfoScriptableObject.bank = new_bank_amount;
            this.newBankAmount?.Invoke(new_bank_amount);
        }

        internal void SetPlayerWalletTo(double new_player_wallet)
        {
            //Debug.Log(String.Format("Player Wallet is being set to {0}", new_player_wallet));
            machineInfoScriptableObject.player_wallet = new_player_wallet;
            newPlayerWalletAmount?.Invoke(new_player_wallet);
        }

        private void SetBetAmountIndexTo(int new_bet_amount)
        {
            //Debug.Log(String.Format("Bet Amount is being set to {0}", new_bet_amount));
            machineInfoScriptableObject.current_bet_amount = new_bet_amount;
            this.newBetAmount?.Invoke(machineInfoScriptableObject.supported_bet_amounts[new_bet_amount]);
        }

        /// <summary>
        /// Sets the multiplier for the game
        /// </summary>
        /// <param name="to_multipler_value">The to value to set multiplier</param>
        public void SetMultiplierTo(float to_multipler_value)
        {
            //Debug.Log(String.Format("Multiplier set to {0}", to_multipler_value));
            machineInfoScriptableObject.multiplier = to_multipler_value;
            newMultiplier?.Invoke(to_multipler_value);
        }
        internal void SetFreeSpinsTo(int new_free_spins)
        {
            //Debug.Log(String.Format("Free Spins is being set to {0}", new_free_spins));
            machineInfoScriptableObject.freespins = new_free_spins;
            this.newFreespinAmount?.Invoke(new_free_spins);
        }

        /// <summary>
        /// Hook - ToDO load player info from config file
        /// </summary>
        public void LoadPlayerInfo()
        {
            //Debug.Log(String.Format("",));
            throw new NotImplementedException();
        }


        internal void InitializeTestMachineValues(float player_wallet, float bank, int bet_amount_index, int multiplier, int freespins)
        {
            SetPlayerInformationTo(player_wallet);
            SetBankTo(bank);
            //Has to be an index within range of supported_bet_amount
            SetBetAmountIndexTo(bet_amount_index);
            SetMultiplierTo(multiplier);
            SetFreeSpinsTo(freespins);
        }

        internal void OffsetPlayerAmountBy(double amount)
        {
            //Add the amount to wallet and Update Text on machine
            SetPlayerWalletTo(machineInfoScriptableObject.player_wallet + amount);
        }

        void OnEnable()
        {
            StateManager.featureTransition += StateManager_FeatureTransition;
            StateManager.add_to_multiplier += StateManager_add_to_multiplier;
        }

        private void StateManager_add_to_multiplier(int multiplier)
        {
            Debug.Log(String.Format("(Obsolete) Setting Multiplier to ", this.machineInfoScriptableObject.multiplier + multiplier));
            SetMultiplierTo(this.machineInfoScriptableObject.multiplier + multiplier);
        }

        /// <summary>
        /// Pull information based on feature being active
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="active_inactive"></param>
        private void StateManager_FeatureTransition(Features feature, bool active_inactive)
        {
            switch (feature)
            {
                case Features.freespin:
                    if (active_inactive)
                    {
                        //Set Freespin Text to 10 remaining
                        SetFreeSpinsTo(10);
                    }
                    break;
                case Features.multiplier:
                    if (active_inactive)
                    {
                        SetFreeSpinsTo(3);
                    }
                    break;
                case Features.Count:
                    break;
                default:
                    break;
            }
        }

        void OnDisable()
        {
            StateManager.featureTransition += StateManager_FeatureTransition;
            StateManager.add_to_multiplier -= StateManager_add_to_multiplier;
        }

        internal void ResetMultiplier()
        {
            SetMultiplierTo(0);
        }

        internal void SetBankView(bool v)
        {
            setBankEnabled?.Invoke(v);
        }
    }
}