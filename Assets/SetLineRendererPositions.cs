using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SetLineRendererPositions))]
class SetLineRendererPositionsEditor : Editor
{
    SetLineRendererPositions myTarget;

    public void OnEnable()
    {
        myTarget = (SetLineRendererPositions)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Commands");
        if(GUILayout.Button("Set Line Win To Payline 0"))
        {
            myTarget.SetDisplayPaylineTo(0);
        }

        if (GUILayout.Button("Set Width To 100"))
        {
            myTarget.SetWidth(100f);
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
public class SetLineRendererPositions : MonoBehaviour
{
    public Slot_Engine.Matrix.Matrix matrix;
    private LineRenderer _lineRenderer;

    private LineRenderer lineRenderer
    {
        get
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();
            return _lineRenderer;
        }
    }

    internal void SetDisplayPaylineTo(int v)
    {
        int[] payline = matrix.paylinesSupported[v].payline;
        List<Vector3> linePositions = new List<Vector3>();
        //TODO add validation payline is same length as reels
        for (int i = 0; i < matrix.rReels.Length; i++)
        {
            //TOOD change to get slot at position at path to return x and y
            linePositions.Add(matrix.rReels[i].slots_in_reel[payline[i] + 1].transform.localPosition);
        }
        lineRenderer.SetPositions(linePositions.ToArray());
    }

    internal void SetWidth(float v)
    {
        lineRenderer.startWidth = v;
        lineRenderer.endWidth = v;
    }
}
