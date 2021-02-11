//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : Slot.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Slot))]
    class SlotEditor : Editor
    {
        Slot myTarget;

        public void OnEnable()
        {
            myTarget = (Slot)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("To be Removed");
            base.OnInspectorGUI();

        }
    }
#endif
        public class Slot : MonoBehaviour
        {
        Slot(Symbols Symbol)
        {
            enSymbol = Symbol;
        }

        public Symbols enSymbol;
        public States enSlotState;
        
        public Reel reel_parent;
        public int iPositonInReel = 0;
        public float time_in_path = 0.0f;
        public int iEndPositionInReel = 0;

        public Vector3[] v3CurrentTweenpath;
        private float fEndingPos;
        public Rect[] rects;

        public bool movement_enabled = false;
        public float fSpinTime = 0.006f;

        public bool bLoopPositionSet = false;
        //Unity Default Functions

        //*************

        public void PlayAnimation()
        {
            //Sprite.frameRate = 24;
            //Sprite.Play();
            //TODO Insert Play Animation Logic
        }

        public void StopAnimation()
        {
            //Sprite.frameRate = 24;
            //Sprite.Stop();
            //TODO Insert Stop Animation Logic
        }

        public void StartSpin(Vector3[] start_end_position)
        {
            Debug.Log(gameObject.name + " starting Spin");
            StopAnimation();
            v3CurrentTweenpath = start_end_position;
            movement_enabled = true;
            //iTween.EaseType easetype_to_start = iTween.EaseType.easeInBack; //TODO refactor to enable settingparent
        }
        
        //TODO update for omni directional
        private void SetTimeInPathByPosition(Vector3 localPosition)
        {
            SetTimeInPathTo(reel_parent.GenerateTimeInPath(localPosition.y, reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1].y));
        }

        private void SetTimeInPathTo(float new_time_in_path)
        {
            time_in_path = new_time_in_path;
        }

        public void SwitchSymbol()
        {
            ResetSpinSymbolTexture();
        }

        public void SwitchSymbol(Symbols Symbol)
        {
            enSymbol = Symbol;
            ResetSpinSymbolTexture();
        }

        private void ResetSpinSymbolTexture()
        {
            
            //TODO Fill in set symbol to static image logic
        }

        Symbols GenerateSymbol()
        {
            //TODO Set Symbol based on supported symbol set passed in
            int iRandom = UnityEngine.Random.Range(1, (int)Symbols.End - 1);
            return (Symbols)iRandom;
        }

        Vector3 GeneratePositionUpdateTime(float time_on_path) //TODO remove
        {
            return reel_parent.GetLoopPositionFromTime(time_on_path);
        }
        Vector3 GeneratePositionUpdateSpeed(float amount_to_add) //Needs to be positive to move forwards and negative to move backwards
        {
            return new Vector3(transform.localPosition.x, transform.localPosition.y + amount_to_add, transform.localPosition.z);
        }
        void Update()
        {
            if (movement_enabled)
            {
                Vector3 toPosition;
                if (reel_parent.use_time_speed)
                {
                    
                    time_in_path += Time.deltaTime;
                    if(time_in_path > reel_parent.reel_spin_time)
                    {
                        time_in_path = reel_parent.reel_spin_time - time_in_path;
                    }
                    toPosition = GeneratePositionUpdateTime(time_in_path);
                }
                else
                {
                    toPosition = GeneratePositionUpdateSpeed(reel_parent.reel_spin_speed);
                    if (toPosition.y <= reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1], true);
                    if (toPosition.y >= reel_parent.positions_in_path_v3[0].y)
                        ShiftToPositionBy(ref toPosition, reel_parent.positions_in_path_v3[reel_parent.positions_in_path_v3.Length - 1], false);
                }
                transform.localPosition = toPosition;
            }
        }

        private void ShiftToPositionBy(ref Vector3 toPosition, Vector3 lastPosition, bool upDown)
        {
            if(upDown)
                toPosition = new Vector3(toPosition.x,toPosition.y - lastPosition.y, toPosition.z);
            else
                toPosition = new Vector3(toPosition.x, toPosition.y + lastPosition.y, toPosition.z);
        }

        internal void StopSpin()
        {
            movement_enabled = false;
        }
    }
}