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
            EditorGUILayout.LabelField("Initialize Machine Properties");
            
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("MachineInfoManager Controls");
            if (GUILayout.Button("Initialize Machine with test values"))
            {
                myTarget.InitializeTestMachineValues(10000.0f, 0.0f, myTarget.supported_bet_amounts.Length-1, 1, 0);
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
        public delegate void IntValueSet(int new_int_value);
        public event FloatValueSet new_multiplier_set;
        public event FloatValueSet new_bet_amount;
        public event FloatValueSet new_bank_amount;
        public event FloatValueSet new_player_wallet_amount;

        internal void DecreaseBetAmount()
        {
            if(current_bet_amount != 0)
            {
                SetBetAmountIndexTo(current_bet_amount - 1);
            }
        }

        public event IntValueSet new_freespin_amount;

        internal void IncreaseBetAmount()
        {
            if (current_bet_amount < supported_bet_amounts.Length-1)
            {
                SetBetAmountIndexTo(current_bet_amount + 1);
            }
        }

        /// <summary>
        /// What bet amounts are supported for this game - Multiply bet amount by symbol value to get amount won - 100 credits are $1.00
        /// </summary>
        [SerializeField]
        internal float[] supported_bet_amounts = new float[10] { .01f, .05f, .25f, .50f, 1.0f, 5.0f, 10.0f, 25.0f, 50.0f, 100.0f};
        public int current_bet_amount = 4;
        /// <summary>
        /// Machine stored values to coin_in/coin_out
        /// </summary>
        public float bank = 0, player_wallet = 0;
        /// <summary>
        /// bet_amount starts at 1.0f
        /// </summary>
        public float bet_amount
        {
            get
            {
                return supported_bet_amounts[current_bet_amount];
            }
        }
        /// <summary>
        /// Multiplier to apply to the win evaluation. (bet_amount * symbol_value) * multiplier = win total
        /// </summary>
        public float multiplier = 1;
        /// <summary>
        /// Free Spins Remaining
        /// </summary>
        public int freespins;
        /// <summary>
        /// Current player that is loaded into machine
        /// </summary>
        public PlayerInformation current_player_information;
        /// <summary>
        /// This is a test class to implement player info - Player info will be loaded from server config file
        /// </summary>
        public void SetPlayerInformationTo(float player_wallet)
        {
            current_player_information = new PlayerInformation();
            current_player_information.player_wallet = player_wallet;
            SetPlayerInformationFrom(ref current_player_information);
        }
        /// <summary>
        /// Sets the player_wallet and bank roll from player info
        /// </summary>
        /// <param name="current_player_information">Player Information to user</param>
        private void SetPlayerInformationFrom(ref PlayerInformation current_player_information)
        {
            SetPlayerWalletTo(current_player_information.player_wallet);
        }

        private void SetBankTo(float new_bank_amount)
        {
            Debug.Log(String.Format("Bank is being set to {0}",new_bank_amount));
            bank = new_bank_amount;
            this.new_bank_amount?.Invoke(new_bank_amount);
        }

        internal void SetPlayerWalletTo(float new_player_wallet)
        {
            Debug.Log(String.Format("Player Wallet is being set to {0}", new_player_wallet));
            player_wallet = new_player_wallet;
            new_player_wallet_amount?.Invoke(new_player_wallet);
        }

        private void SetBetAmountIndexTo(int new_bet_amount)
        {
            Debug.Log(String.Format("Bet Amount is being set to {0}", new_bet_amount));
            current_bet_amount = new_bet_amount;
            this.new_bet_amount?.Invoke(supported_bet_amounts[new_bet_amount]);
        }

        /// <summary>
        /// Sets the multiplier for the game
        /// </summary>
        /// <param name="to_multipler_value">The to value to set multiplier</param>
        public void SetMultiplierTo(float to_multipler_value)
        {
            Debug.Log(String.Format("Multiplier set to {0}", to_multipler_value));
            multiplier = to_multipler_value;
            new_multiplier_set?.Invoke(to_multipler_value);
        }
        private void SetFreeSpinsTo(int new_free_spins)
        {
            Debug.Log(String.Format("Free Spins is being set to {0}", new_free_spins));
            freespins = new_free_spins;
            this.new_freespin_amount?.Invoke(new_free_spins);
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

        internal void OffsetPlayerAmountBy(float amount)
        {
            //Add the amount to wallet and Update Text on machine
            SetPlayerWalletTo(player_wallet + amount);
        }
    }
}