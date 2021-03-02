using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PaylineRenderer))]
[RequireComponent(typeof(Slot_Engine.Matrix.Matrix))]
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
public class PaylineRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    private Slot_Engine.Matrix.Matrix matrix
        {
            get
            {
                if (_matrix == null)
                    _matrix = GetComponent<Slot_Engine.Matrix.Matrix>();
                return _matrix;
            }
        }
    private Slot_Engine.Matrix.Matrix _matrix;
    private LineRenderer lineRenderer
    {
        get
        {
            if (_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();
            return _lineRenderer;
        }
    }

    internal void SetWidth(float v)
    {
        lineRenderer.startWidth = v;
        lineRenderer.endWidth = v;
    }

    internal void ShowPayline(Payline paylines_supported)
    {
        List<Vector3> linePositions = new List<Vector3>();
        //TODO add validation payline is same length as reels
        for (int i = 0; i < matrix.rReels.Length; i++)
        {
            Vector3 position_cache = matrix.rReels[i].slots_in_reel[paylines_supported.payline[i] + 1].transform.position;
            position_cache = new Vector3(position_cache.x, position_cache.y, -10); //TODO Change Hardcoded Value
            //TOOD change to get slot at position at path to return x and y
            linePositions.Add(matrix.rReels[i].slots_in_reel[paylines_supported.payline[i] + 1].transform.position);
        }
        lineRenderer.SetPositions(linePositions.ToArray());
    }

    internal void EnableRenderer()
    {
        lineRenderer.enabled = true;
    }

    internal void DisableRenderer()
    {
        lineRenderer.enabled = false;
    }
}
