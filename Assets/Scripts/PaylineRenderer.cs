using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PaylineRenderer))]
[RequireComponent(typeof(LineRenderer))]
class PaylineRendererEditor : Editor
{
    PaylineRenderer myTarget;

    public void OnEnable()
    {
        myTarget = (PaylineRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Commands");
        if (GUILayout.Button("Initialize Line Renderer"))
        {
            myTarget.InitializeLineRendererComponents();
        }
        if (GUILayout.Button("Set Width To 100"))
        {
            myTarget.SetWidth(100,100);
        }
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Editable Properties");
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("To be Removed");
        base.OnInspectorGUI();
    }
}
#endif
[RequireComponent(typeof(LineRenderer))]
public class PaylineRenderer : MonoBehaviour
{
    public float standard_payline_width = 50;
    public float highlight_win_width = 100;
    [SerializeField]
    private LineRenderer _line_renderer;
    internal LineRenderer line_renderer
    {
        get
        {
            if (_line_renderer == null)
            {
                _line_renderer = GetComponent<LineRenderer>();
            }
            return _line_renderer;
        }
    }

    internal void SetWidth(float start, float end)
    {
        line_renderer.startWidth = start;
        line_renderer.endWidth = end;
    }

    internal void SetLineRendererPositions(List<Vector3> position_list)
    {
        line_renderer.positionCount = position_list.Count;
        line_renderer.SetPositions(position_list.ToArray());
    }

    internal void ToggleRenderer(bool on_off)
    {
        line_renderer.enabled = on_off;
    }

    internal void InitializeLineRendererComponents()
    {
        Debug.Log(string.Format("lineRenderer Initialized with {0} components",line_renderer.ToString()));
    }
}