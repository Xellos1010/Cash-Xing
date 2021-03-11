using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(RackingManager))]
    class RackingManagerEditor : BoomSportsEditor
    {
        RackingManager myTarget;
        public void OnEnable()
        {
            myTarget = (RackingManager)target;
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("RackingManager Properties");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("RackingManager Controls");
            base.OnInspectorGUI();
        }
    }
#endif
    public class RackingManager : MonoBehaviour
    {
        [SerializeField]
        private UITextManager ui_text_manager;
        [SerializeField]
        private Matrix matrix;
        public bool is_racking = false;

        public float bank_rack_remaining;
        public float bank_rack_total_to_rack;

        
        public float current_player_wallet = 0;
        /// <summary>
        /// Sets the racking to be instant or a rollup
        /// </summary>
        [SerializeField]
        private bool set_instantly = true;
        public float credit_rack_speed;

        //Store amount to increase credits
        //Credit Rack Speed
        //Slam - 
        //On State Spin resolve pull ending configuration value total and set win racking bool to true
        //
        // Start is called before the first frame update
        void OnEnable()
        {
            StateManager.StateChangedTo += StateManager_StateChangedTo;
        }

        private void Setplayer_walletTo(float to_value)
        {
            current_player_wallet = to_value;
            ui_text_manager.Set_Player_Wallet_To(to_value);
        }

        private void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.None:
                    break;
                case States.preloading:
                    break;
                case States.Coin_In:
                    break;
                case States.Coin_Out:
                    break;
                case States.Idle_Intro:
                    break;
                case States.Idle_Idle:
                    break;
                case States.Idle_Outro:
                    break;
                case States.Spin_Intro:
                    break;
                case States.Spin_Idle:
                    break;
                case States.Spin_Outro:
                    break;
                case States.Spin_End:
                    break;
                case States.Resolve_Intro:
                    break;
                case States.Resolve_Win_Idle:
                    break;
                case States.Resolve_Lose_Idle:
                    break;
                case States.Resolve_Win_Outro:
                    break;
                case States.win_presentation:
                    break;
                case States.racking_start:
                    break;
                case States.racking_loop:
                    break;
                case States.racking_end:
                    break;
                case States.feature_transition_out:
                    break;
                case States.feature_transition_in:
                    break;
                case States.total_win_presentation:
                    break;
                default:
                    break;
            }
        }

        internal void GetRackingInformation()
        {
            SetCreditAmountToRack(matrix.paylines_manager.GetTotalWinAmount());
        }

        private void SetCreditAmountToRack(float win_amount)
        {
            if(set_instantly)
            {
                matrix.OffetPlayerWalletBy(win_amount);
            }
            else
            {

                bank_rack_total_to_rack = win_amount;
                //TODO Enable racking overtime and throw bool event when finished racking
                throw new NotImplementedException();
            }
        }

        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }
    }
}