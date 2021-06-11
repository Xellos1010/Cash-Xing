﻿using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace BoomSports.Prototype.Managers
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PaylineRendererManager))]
    class PaylineRendererManagerEditor : BoomSportsEditor
    {
        PaylineRendererManager myTarget;

        public int line_renderers_to_use = 1;

        public void OnEnable()
        {
            myTarget = (PaylineRendererManager)target;
        }

        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Initialize Line Renderer"))
            {
                myTarget.InitializeLineRendererComponents();
            }
            if (GUILayout.Button("Set Width To 100"))
            {
                myTarget.SetWidth(100, 100);
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Editable Properties");
            base.OnInspectorGUI();
        }
    }
#endif
    public class PaylineRendererManager : MonoBehaviour
    {
        public float standard_payline_width = 50;
        public float highlight_win_width = 100;
        public bool render_paylines = true;

        public TMPro.TextMeshPro winningPaylineText;
        public Transform winningPaylinePrefab;
        public PaylineRenderer[] _payline_renderers; //TODO make private - testing mode only
        private PaylineRenderer[] payline_renderers
        {
            get
            {
                //For now Nuke and reget
                if (_payline_renderers == null)
                {
                    _payline_renderers = GetComponentsInChildren<PaylineRenderer>();
                }
                if (_payline_renderers.Length != matrix.groupObjectManagers.Length - 1)
                {
                    List<PaylineRenderer> renderers = new List<PaylineRenderer>();
                    renderers.AddRange(_payline_renderers);
                    //for (int i = 0; i < matrix.configurationGroupManagers.Length - 1; i++)
                    //{
                    //    renderers.Add(GenerateNewPaylineObject());
                    //}
                    _payline_renderers = renderers.ToArray();
                }
                return _payline_renderers;
            }
        }

        private void DestroyChildren()
        {
            for (int i = transform.childCount - 1; i > 0; i--)
            {
                Destroy(transform.GetChild(i));
            }
        }

        private PaylineRenderer GenerateNewPaylineObject()
        {
            //Generate a new object - add PaylineRender has required component linerenderer
            GameObject new_game_object = new GameObject("Payline_Renderer_Object");
            new_game_object.transform.parent = transform;
            return (PaylineRenderer)new_game_object.AddComponent(typeof(PaylineRenderer));
        }

        private BoomSports.Prototype.StripConfigurationObject matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<BoomSports.Prototype.StripConfigurationObject>();
                return _matrix;
            }
        }
        public StripConfigurationObject _matrix;
        public int line_renderers_to_use = 1;

        internal void SetWidth(float start, float end, ref PaylineRenderer payline_renderer)
        {
            payline_renderer.SetWidth(start, end);
        }

        internal void ShowPayline(Payline paylineToShow)
        {
            List<Vector3> linePositions;
            matrix.ReturnPositionsBasedOnPayline(ref paylineToShow, out linePositions);
            Debug.Log($"Line Positions = {linePositions.PrintElements<Vector3>()}");
            if (line_renderers_to_use > 1)
            {
                for (int i = 0; i < linePositions.Count - 1; i++) //Don't include end linePositions since your get 2 out for array range
                {
                    //Throws arguments out of range if line positions out of range
                    SetLineRendererPositions(linePositions.GetRange(i, 2), ref payline_renderers[i]);
                    SetWidth(standard_payline_width, standard_payline_width, ref payline_renderers[i]);
                }
            }
            else
            {
                SetLineRendererPositions(linePositions, ref payline_renderers[0]);
            }
        }

        private void SetLineRendererPositions(List<Vector3> position_list, ref PaylineRenderer payline_renderer)
        {
            payline_renderer.SetLineRendererPositions(position_list);
        }
        /// <summary>
        /// Show the winning payline and highlight symbols that won with...a bigger line!
        /// </summary>
        /// <param name="payline_to_show">The Winning payline to show</param>
        /// <param name="createAndReturnTextObject">Should we generate and return payline text to be destroyed?</param>
        internal GameObject ShowWinningPayline(WinningPayline payline_to_show, out List<Vector3> linePositions,bool createAndReturnTextObject = false, bool fillEndToEnd = true)
        {
            GameObject output; //Returns the Cash Crossing : text amount object so can lerp to bank if feature active
            int adjacentSlot = -1;
            ToggleLineRendererActive(true);
            Payline toShowPayline = new Payline(payline_to_show.payline);
            //Used to fill the payline end to end
            List<int> paylineTemp = new List<int>();
            paylineTemp.AddRange(toShowPayline.configuration.payline);
            //Used to add payline nodes based on root node and group managers
            if (fillEndToEnd) //Fill the payline to the last reel in based on evaluation direction
            {
                //Check if the length of the payline is the length of the group managers
                if (payline_to_show.payline.configuration.payline.Length < matrix.groupObjectManagers.Length)
                {
                    if (payline_to_show.payline.left_right) //True : left False : right
                    {
                        //TODO Evauate whether needed
                        if (payline_to_show.payline.rootNode.column != 0)
                        {
                            //Fill the payline backwards - the amount difference but as straight line - In future base this off 
                            for (int toFill = payline_to_show.payline.rootNode.column; toFill > 0; toFill--)
                            {
                                adjacentSlot = matrix.groupObjectManagers[toFill].ReturnValidActiveDisplayFromRow(payline_to_show.payline.rootNode.row);
                                //Will fill anything before the root node with the node adjacent
                                paylineTemp.Insert(0, adjacentSlot);
                            }
                        }
                        if (paylineTemp.Count < matrix.groupObjectManagers.Length)
                        {
                            int toFillBase = paylineTemp.Count;
                            //Will fill anything after end of payline raw with the node adjacent
                            for (int toFill = toFillBase; toFill < matrix.groupObjectManagers.Length; toFill++)
                            {
                                adjacentSlot = matrix.groupObjectManagers[toFill].ReturnValidActiveDisplayFromRow(paylineTemp[paylineTemp.Count - 1]);
                                paylineTemp.Add(adjacentSlot);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"payline_to_show.payline.rootNode.column {payline_to_show.payline.rootNode.column} != matrix.configurationGroupManagers.Length-1 {matrix.groupObjectManagers.Length-1} = {payline_to_show.payline.rootNode.column != matrix.groupObjectManagers.Length-1}");
                        if (payline_to_show.payline.rootNode.column != matrix.groupObjectManagers.Length-1) //using 0 indexing based rule. -1 from length to get index last object in array
                        {
                            //Fill the payline backwards - the amount difference but as straight line - In future base this off 
                            for (int toFill = payline_to_show.payline.rootNode.column; toFill < matrix.groupObjectManagers.Length-1; toFill++)
                            {
                                //Will fill anything before the root node with the node adjacent
                                adjacentSlot = matrix.groupObjectManagers[toFill].ReturnValidActiveDisplayFromRow(payline_to_show.payline.rootNode.row);
                                paylineTemp.Insert(0, adjacentSlot);
                            }
                        }
                        if (paylineTemp.Count < matrix.groupObjectManagers.Length)
                        {
                            int toFillBase = paylineTemp.Count;
                            //Will fill anything before the root node with the node adjacent
                            for (int toFill = toFillBase; toFill < matrix.groupObjectManagers.Length; toFill++)
                            {
                                adjacentSlot = matrix.groupObjectManagers[toFill].ReturnValidActiveDisplayFromRow(paylineTemp[paylineTemp.Count - 1]);
                                paylineTemp.Add(adjacentSlot);
                            }
                        }
                    }
                }
                toShowPayline.configuration.payline = paylineTemp.ToArray();
            }
            //Debug.Log($"rendering Winning Payline - {toShowPayline.PrintConfiguration()} root node = {toShowPayline.rootNode.Print()}");

            //Take the positions on the matrix and return the symbol at those positions for the payline always going to be -1 the line position length. last symbol always spinning off reel
            matrix.ReturnPositionsBasedOnPayline(ref toShowPayline, out linePositions);
            Vector3[] winningSymbolPositions = new Vector3[toShowPayline.configuration.payline.Length];
            linePositions.CopyTo(0,winningSymbolPositions,0, winningSymbolPositions.Length);
            //Is this even or odd? use middle position if odd - use lerp half for even
            Vector3 position = winningSymbolPositions.Length % 2 == 0 ?
                Vector3.Lerp(winningSymbolPositions[((int)winningSymbolPositions.Length / 2)-1], winningSymbolPositions[(int)winningSymbolPositions.Length / 2], .5f) : //If line win is even
                winningSymbolPositions[(int)winningSymbolPositions.Length / 2];                                  //If line win is odd
            //Sets the winning amount text
            output = SetWinningAmountDisplay(position,payline_to_show.GetTotalWin(matrix), createAndReturnTextObject);
            if (line_renderers_to_use < 2)
            {
                //Solution for single line renderer
               SetLineRendererPositions(linePositions, ref payline_renderers[0]);
            }
            else
            {
                throw new Exception("Multiple Line Renderers TBD");
            }
            return output;
        }
        private GameObject SetWinningAmountDisplay(Vector3 vector3, float v, bool createAndReturnTextObject)
        {
            if (!createAndReturnTextObject)
            {
                winningPaylineText.transform.position = vector3 + Vector3.back * 5;
                winningPaylineText.text = String.Format("{0:C2}", v);
                winningPaylineText.enabled = true;
                return winningPaylineText.gameObject;
            }
            else
            {
                winningPaylineText.enabled = false;
                Transform winingPaylineTextGameObjectObject = GameObject.Instantiate(winningPaylinePrefab) as Transform;
                //Debug.Log($"winningPaylineText null = {winningPaylineText == null}");

                //winingPaylineTextGameObjectObject.transform.SetParent(transform.parent);
                winingPaylineTextGameObjectObject.transform.position = vector3 + Vector3.back * 5;
                TMPro.TextMeshPro text = winingPaylineTextGameObjectObject.GetComponent<TMPro.TextMeshPro>();
                text.text = String.Format("{0:C2}", v);
                return winingPaylineTextGameObjectObject.gameObject;
            }
        }

        private int ReturnIndexFirstLastFromList(bool left_right, int i, int count)
        {
            return left_right ? i : (count - 1) - i;
        }

        internal void ToggleLineRendererActive(bool on_off)
        {
            //Debug.Log(String.Format("Toggle Renderer {0}",on_off));
            for (int i = 0; i < payline_renderers.Length; i++)
            {
                payline_renderers[i].line_renderer.enabled = on_off;
            }
            if (!on_off)
                winningPaylineText.enabled = false;
        }

        internal void SetWidth(int v1, int v2)
        {
            for (int i = 0; i < payline_renderers.Length; i++)
            {
                SetWidth(v1, v2, ref payline_renderers[i]);
            }
        }

        internal void InitializeLineRendererComponents()
        {
            Debug.Log(string.Format("lineRenderer Initialized with {0} components", payline_renderers.Length.ToString()));
        }

        void OnEnable()
        {
            StaticStateManager.StateChangedTo += StateManager_StateChangedTo;
        }

        private void StateManager_StateChangedTo(States state)
        {
            switch (state)
            {
                default:
                    ToggleLineRendererActive(false);
                    break;
            }
        }

        void OnDisable()
        {
            StaticStateManager.StateChangedTo -= StateManager_StateChangedTo;
        }
    }
}