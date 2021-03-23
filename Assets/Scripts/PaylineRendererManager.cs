using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(PaylineRendererManager))]
    class PaylineRendererManagerEditor : BoomSportsEditor
    {
        PaylineRendererManager myTarget;

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
                if (_payline_renderers.Length != matrix.reel_strip_managers.Length - 1)
                {
                    List<PaylineRenderer> renderers = new List<PaylineRenderer>();
                    renderers.AddRange(_payline_renderers);
                    for (int i = 0; i < matrix.reel_strip_managers.Length - 1; i++)
                    {
                        renderers.Add(GenerateNewPaylineObject());
                    }
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

        private Slot_Engine.Matrix.Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<Slot_Engine.Matrix.Matrix>();
                return _matrix;
            }
        }
        public Slot_Engine.Matrix.Matrix _matrix;

        internal void SetWidth(float start, float end, ref PaylineRenderer payline_renderer)
        {
            payline_renderer.SetWidth(start, end);
        }

        internal void ShowPayline(Payline paylines_supported)
        {
            List<Vector3> linePositions;
            matrix.ReturnPositionsBasedOnPayline(ref paylines_supported.payline_configuration.payline, out linePositions);
            for (int i = 0; i < linePositions.Count - 1; i++) //Don't include end linePositions since your get 2 out for array range
            {
                //Throws arguments out of range if line positions out of range
                SetLineRendererPositions(linePositions.GetRange(i, 2), ref payline_renderers[i]);
                SetWidth(standard_payline_width, standard_payline_width, ref payline_renderers[i]);
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
        internal void ShowWinningPayline(WinningPayline payline_to_show)
        {
            ToggleRenderer(render_paylines);
            //initialize the line positions list and 
            List<Vector3> linePositions;
            //Take the positions on the matrix and return the symbol at those positions for the payline always going to be -1 the line position length. last symbol always spinning off reel
            //matrix.ReturnSymbolPositionsOnPayline(ref payline_to_show.payline, out linePositions);
            matrix.ReturnPositionsBasedOnPayline(ref payline_to_show.payline.payline_configuration.payline,out linePositions);
            int renderers_widths_set = 0;
            int payline_renderer_index_to_set = 0;
            int line_position_index = 0;
            int winning_symbols_set = 0;
            List<Vector3> linePositionsToUse;
            for (int i = 0;i < linePositions.Count -1;i++)
            {
                try

                {
                    linePositionsToUse = new List<Vector3>();
                    line_position_index = ReturnIndexFirstLastFromList(payline_to_show.left_right, i, linePositions.Count);
                    linePositionsToUse.Add(linePositions[line_position_index]);
                    linePositionsToUse.Add(linePositions[payline_to_show.left_right ? line_position_index + 1 : line_position_index - 1]);
                    //Set line renderer either highlighting left to right or right to left
                    SetLineRendererPositions(payline_to_show.left_right ?
                        linePositions.GetRange(i, 2) : linePositions.GetRange(i - 1, 2), 
                        ref payline_renderers[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError(String.Format("Setting LineRenderer Positions failed to get range of linePositions[{0}] error message is outside of range. linePositions.Length = {1}. exception message = {2}",i,linePositions.Count.ToString(),e.Message));
                }
                payline_renderer_index_to_set = i;
                if (i >= payline_renderers.Length)
                {
                    if (i >= payline_renderers.Length)
                    {
                        payline_renderer_index_to_set = (payline_renderers.Length - 1) - renderers_widths_set;
                    }
                }
                if (winning_symbols_set < payline_to_show.winning_symbols.Length)
                {
                    winning_symbols_set += 1;
                        //Need to refactor in the future - there will be an issue regarding getting the correct payline_renderer when going right to left payline evaluation
                    SetWidth(highlight_win_width, highlight_win_width, ref payline_renderers[payline_renderer_index_to_set]);
                    renderers_widths_set += 1;
                }
                else
                {
                    SetWidth(standard_payline_width, standard_payline_width, ref payline_renderers[payline_renderer_index_to_set]);
                    renderers_widths_set += 1;
                }
            }
            //Disable any extra line renderers if positions < line renderers
            if (payline_to_show.winning_symbols.Length < matrix.reel_strip_managers.Length)
            {
                //for 5 symbols there are 4 line renderers. for 4 symbools you need to point to - 1 to disable the last line renderer on a 3x5 matrix
                for (int i = payline_to_show.winning_symbols.Length -1 ; i < payline_renderers.Length; i++)
                {
                    payline_renderers[i].ToggleRenderer(false);
                }
            }
        }

        private int ReturnIndexFirstLastFromList(bool left_right, int i, int count)
        {
            return left_right ? i : (count - 1) - i;
        }

        internal void ToggleRenderer(bool on_off)
        {
            Debug.Log(String.Format("Toggle Renderer {0}",on_off));
            for (int i = 0; i < payline_renderers.Length; i++)
            {
                payline_renderers[i].line_renderer.enabled = on_off;
            }
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
    }
}