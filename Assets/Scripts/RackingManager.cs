﻿using System;
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
        public delegate void RackStart(float amountToRack);
        public delegate void RackEnd();
        public event RackStart rackStart;
        public event RackEnd rackEnd;
        [SerializeField]
        private UITextManager ui_text_manager;
        [SerializeField]
        private Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<Matrix>();
                return _matrix;
            }
        }
        private Matrix _matrix;

        public bool is_racking
        {
            get
            {
                if (bank_rack_remaining > 0)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        public float bank_rack_remaining;
        public float bank_rack_total_to_rack;

        /// <summary>
        /// Sets the racking to be instant or a rollup
        /// </summary>
        [SerializeField]
        private bool set_instantly = true;
        public float credit_rack_speed;

        public 

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

        private void OffsetPlayerWalletBy(float amount)
        {
            //This will fire an event and the UI manager will auto set the text based on new player amount
            Debug.Log("Player wallet offset by " + amount);
            matrix.OffetPlayerWalletBy(amount);
        }

        private void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Intro:
                    FinalizeRacking();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Ensure credits are racked and all vars are reset
        /// </summary>
        private void FinalizeRacking()
        {
            SetCreditDisplayToEnd();
        }
        /// <summary>
        /// Used to set Update the player wallet by bank_rack_remaining
        /// </summary>
        private void SetCreditDisplayToEnd()
        {
            if(bank_rack_remaining > 0)
            {
                UpdateCreditRackingRemaining(bank_rack_remaining);
            }
        }

        /// <summary>
        /// Sets the credit amount to rack and starts racking mode
        /// </summary>
        internal void StartRacking()
        {
            Debug.Log("Starting Rack");
            if(matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > 0)
                SetCreditAmountToRack(matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank);
            else
                SetCreditAmountToRack(matrix.slot_machine_managers.paylines_manager.GetTotalWinAmount());
        }
        /// <summary>
        /// Set the total amount to rack
        /// </summary>
        /// <param name="win_amount">the amount won to rack</param>
        private void SetCreditAmountToRack(float win_amount)
        {
            //Debug.Log(String.Format("Setting Credit amount to rack to {0}",win_amount));
            //need to see if win is big enough to present final win amount
            if(set_instantly)
            {
                SetCreditsToRackAtSpeed(win_amount, credit_rack_speed);
                FinalizeRacking();
            }
            else
            {
                Debug.Log(win_amount + " Amount won");
                //OffsetPlayerBankBy(win_amount);
                //Set Credits to rack
                SetCreditsToRackAtSpeed(win_amount, credit_rack_speed);
                //Send Event that credits are racking
                rackStart?.Invoke(win_amount);
            }
        }
        /// <summary>
        /// Set credits to rack and speed
        /// </summary>
        /// <param name="win_amount">amount to rack</param>
        /// <param name="credit_rack_speed">increment to rack every update</param>
        private void SetCreditsToRackAtSpeed(float win_amount, float credit_rack_speed)
        {
            this.credit_rack_speed = credit_rack_speed;
            bank_rack_remaining = win_amount;
        }
        /// <summary>
        /// Remove event hooks
        /// </summary>
        void OnDisable()
        {
            StateManager.StateChangedTo -= StateManager_StateChangedTo;
        }
        /// <summary>
        /// Main racking loop - will iterate if bank_rack_remaining > 0
        /// </summary>
        void Update()
        {
            if (is_racking)
            {
                float amount_to_rack = GetUpdateRackAmount();
                UpdateCreditRackingRemaining(amount_to_rack);
            }
        }

        private void UpdateCreditRackingRemaining(float amount_to_rack)
        {
            bank_rack_remaining -= amount_to_rack;
            if (matrix.slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank > 0)
                OffsetPlayerBankBy(-amount_to_rack);
            OffsetPlayerWalletBy(amount_to_rack);
            if(bank_rack_remaining == 0)
                rackEnd?.Invoke();
        }

        private void OffsetPlayerBankBy(float v)
        {
            matrix.slot_machine_managers.machine_info_manager.OffsetBankBy(v);
        }

        /// <summary>
        /// Gets the rack amount total for the current update
        /// </summary>
        /// <returns>Total amount to rack based on speed</returns>
        private float GetUpdateRackAmount()
        {
            float output = 0;
            if(credit_rack_speed == 0)
            {
                credit_rack_speed = 1;//Something for now so we can still continue with the game
            }
            if(bank_rack_remaining - credit_rack_speed < 0)
            {
                output = bank_rack_remaining;
            }
            else
            {
                output = credit_rack_speed;
            }
            return output;
        }
    }
}