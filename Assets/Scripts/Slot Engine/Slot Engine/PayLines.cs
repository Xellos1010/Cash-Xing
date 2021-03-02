using UnityEngine;

//For Parsing Purposes
using System.IO;
using System.Collections.Generic;
using System;

//************

/// <summary>
/// This holds all payline information. Paylines are processed in the Slot Engine Script by cycling through the iPayLines and comparing whether symbols match on those paylines.
/// </summary>

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PayLines))]
[RequireComponent(typeof(Slot_Engine.Matrix.Matrix))]
[RequireComponent(typeof(PaylineRenderer))]
class PayLinesEditor : Editor
{
    PayLines myTarget;
    SerializedProperty paylines_supported;
    SerializedProperty winning_paylines;
    private int payline_to_show;
    private int winning_payline_to_show;
    public void OnEnable()
    {
        myTarget = (PayLines)target;
        paylines_supported = serializedObject.FindProperty("paylines_supported");
        winning_paylines = serializedObject.FindProperty("winning_paylines");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Commands");

        if (GUILayout.Button("Set Paylines"))
        {
            myTarget.SetPaylines();
            serializedObject.ApplyModifiedProperties();
        }
        if (paylines_supported.arraySize > 0)
        {
            payline_to_show = EditorGUILayout.IntSlider(payline_to_show, 0, paylines_supported.arraySize-1);
            if (GUILayout.Button("Show Payline"))
            {
                myTarget.ShowPayline(payline_to_show);
            }
            if (GUILayout.Button("Evaluate Payline"))
            {
                myTarget.EvaluateWinningSymbols();
                serializedObject.ApplyModifiedProperties();
            }
            if (winning_paylines.arraySize > 0)
            {
                winning_payline_to_show = EditorGUILayout.IntSlider(winning_payline_to_show, 0, winning_paylines.arraySize - 1);
                if (GUILayout.Button("Show Winning Payline"))
                {
                    myTarget.ShowPayline(winning_paylines.GetArrayElementAtIndex(winning_payline_to_show).intValue);
                }
                if (GUILayout.Button("Clear Winning Paylines"))
                {
                    myTarget.ClearWinningPaylines();
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("Editable Properties");
        EditorGUILayout.PropertyField(paylines_supported);
        EditorGUILayout.PropertyField(winning_paylines);
        BoomEditorUtilities.DrawUILine(Color.white);
        EditorGUILayout.LabelField("To be Removed");
        base.OnInspectorGUI();
    }
}
#endif


[RequireComponent(typeof(Slot_Engine.Matrix.Matrix))]
public class PayLines : MonoBehaviour
{
    public Payline[] paylines_supported;
    public int[] winning_paylines;
    public int paylines_active;
    public PaylineRenderer payline_renderer
    {
        get
        {
            if(_payline_renderer == null)
            {
                _payline_renderer = GetComponent<PaylineRenderer>();
            }
            return _payline_renderer;
        }
    }
    public PaylineRenderer _payline_renderer;
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
    internal void SetPaylines()
    {
        //Find File - Parse File - Fill Array of int[]
        TextAsset paylines = Resources.Load<TextAsset>("Data/99paylines_m3x5");
        Debug.Log(paylines.text);
        List<int> paylineListRaw = new List<int>();
        List<Payline> paylineListOutput = new List<Payline>();

        for (int i = 0; i < paylines.text.Length; i++)
        {
            if (Char.IsDigit(paylines.text[i]))
            {
                Debug.Log(string.Format("Char {0} is {1}", i, Char.GetNumericValue(paylines.text[i])));
                paylineListRaw.Add((int)Char.GetNumericValue(paylines.text[i]));
                if (paylineListRaw.Count == 5)
                {

                    paylineListOutput.Add(new Payline(paylineListRaw.ToArray()));
                    Debug.Log(paylineListRaw.ToArray().ToString());
                    paylineListRaw.Clear();
                }
            }
        }
        Debug.Log(paylineListOutput.ToArray().ToString());
        paylines_supported = paylineListOutput.ToArray();
    }

    int ReturnLengthStreamReader(StreamReader Reader)
    {
        int i = 0;
        while (Reader.ReadLine() != null) { i++; }
        return i;
    }

    public void ShowPayline(int payline_to_show)
    {
        if(payline_to_show >= 0 && payline_to_show < paylines_supported.Length)
            payline_renderer.ShowPayline(paylines_supported[payline_to_show]);
    }

    public void SetReelConfiguration()
    {
        //matrix.SetSymbolsOnMatrixTo();
    }
    public void EvaluateWinningSymbols()
    {
        //Cycle through paylines active
        //Check for symbols matching both ways.
        int[][] symbols_configuration = new int[5][] {
        new int[3] {0,1,2},
        new int[3] {0,1,2},
        new int[3] {0,1,2},
        new int[3] {0,1,2},
        new int[3] {0,1,2}}; //TODO pull reel configuration from matrix

        List<int> payline_won = new List<int>();
        for (int payline = 0; payline < paylines_supported.Length; payline++)
        {
            List<int> symbols_in_row = new List<int>();
            //Cycle through slots on payline left to right - left first then right
            for (int reel = 0; reel < paylines_supported[payline].payline.Length; reel++)
            {
                symbols_in_row.Add(symbols_configuration[reel][paylines_supported[payline].payline[reel]]);
            }
            int primarySymbol = symbols_in_row[0];

            for (int symbol = 1; symbol < symbols_in_row.Count; symbol++)
            {
                //If the primary symbol is a wild then auto match with next symbol. if next symbol regular symbol that becomes primary symbol
                if (primarySymbol == 0)
                {
                    if (symbols_in_row[symbol] != 0)
                    {
                        primarySymbol = symbols_in_row[symbol];
                    }
                }
                else
                {
                    if (symbols_in_row[symbol] != 0 || symbols_in_row[symbol] != primarySymbol) // Wild symbol - look to match next symbol to wild or set symbol 
                    {
                        if (symbol > 2)
                        {
                            Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match left to right", payline, symbol));
                            payline_won.Add(payline);
                        }
                        break;
                    }
                }
            }
            primarySymbol = symbols_in_row[symbols_in_row.Count - 1];
            for (int symbol = symbols_in_row.Count - 2; symbol >= 0; symbol--)
            {
                //If the primary symbol is a wild then auto match with next symbol. if next symbol regular symbol that becomes primary symbol
                if (primarySymbol == 0)
                {
                    if (symbols_in_row[symbol] != 0)
                    {
                        primarySymbol = symbols_in_row[symbol];
                    }
                }
                else
                {
                    if (symbols_in_row[symbol] != 0 || symbols_in_row[symbol] != primarySymbol) // Wild symbol - look to match next symbol to wild or set symbol 
                    {
                        if (symbol > 2)
                        {
                            Debug.Log(String.Format("a match was found on payline {0}, {1} symbols match right to left", payline, symbol));
                            if(!payline_won.Contains(payline))
                            {
                                payline_won.Add(payline);
                            }
                        }
                        break;
                    }
                }
            }
        }
        winning_paylines = payline_won.ToArray();
    }

    internal void ClearWinningPaylines()
    {
        winning_paylines = new int[0];
    }

    void OnEnable()
    {
        StateManager.StateChangedTo += StateManager_StateChangedTo;
    }

    private void StateManager_StateChangedTo(States State)
    {
        if(State == States.racking_start)
        {
            payline_renderer.EnableRenderer();
        }
        else if(State == States.spin_start)
        {
            payline_renderer.DisableRenderer();
        }
    }

    void OnDisable()
    {
        StateManager.StateChangedTo -= StateManager_StateChangedTo;
    }
}