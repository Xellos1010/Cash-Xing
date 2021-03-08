//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : SlotEngine.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;


//public string[] symbol_set_supported = new string[6] { "SF01", "SF02", "MA01" };//Want this list populated by whatever output brent is using. If we are unable to have access from a list then we should pull based on assets provided in skins folder. Read folder names of folders in Base Game/Symbols Directory
namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(MatrixGenerator))]
    class MatrixGeneratorEditor : Editor
    {
        MatrixGenerator myTarget;
        SerializedProperty padding_xyz;
        SerializedProperty slot_size_xyz;
        SerializedProperty matrix; //Vector3[] slot to generate and which direction 

        SerializedProperty symbols_supported;
        SerializedProperty skin_graphics;

        public void OnEnable()
        {
            myTarget = (MatrixGenerator)target;
            padding_xyz = serializedObject.FindProperty("matrix_padding");
            slot_size_xyz = serializedObject.FindProperty("slot_size");
            matrix = serializedObject.FindProperty("matrix");
            symbols_supported = serializedObject.FindProperty("symbols_supported");
            skin_graphics = serializedObject.FindProperty("skin_graphics");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Commands");
            if (GUILayout.Button("Generate Matrix"))
            {
                myTarget.CreateMatrix();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Modify Matrix - Reel Size");
            EditorGUI.BeginChangeCheck();
            int reel_size = EditorGUILayout.IntSlider(matrix.arraySize, 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                if(reel_size > matrix.arraySize)
                {
                    for (int x = matrix.arraySize; x <= reel_size; x++)
                        matrix.InsertArrayElementAtIndex(x);
                }
                else
                {
                    for (int x = matrix.arraySize-1; x >= reel_size; x--)
                        matrix.DeleteArrayElementAtIndex(x);
                }
                serializedObject.ApplyModifiedProperties();
                //TODO reduce to 1 call - Update Matrix Position
                myTarget.UpdateReelSlotPositions();
                myTarget.UpdateSlotsInReels();
            }
            EditorGUILayout.LabelField("Modify Matrix - Slot Number per reel");
            List<int[]> slots_per_reel = new List<int[]>();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.BeginChangeCheck();
            for (int x = 0; x < reel_size; x++)
            {
                BoomEditorUtilities.DrawUILine(Color.white);
                EditorGUILayout.LabelField("Modify Matrix - Reel " + x.ToString() + " Slots per reel");
                //TODO Select slot appropriate based on direction - hard coded for now
                slots_per_reel.Add(new int[3] { 0, 0, 0 });
                slots_per_reel[x][0] = EditorGUILayout.IntSlider((int)matrix.GetArrayElementAtIndex(x).vector3Value.x,0,10);
                slots_per_reel[x][1] = EditorGUILayout.IntSlider((int)matrix.GetArrayElementAtIndex(x).vector3Value.y, 0, 10);
                slots_per_reel[x][2] = EditorGUILayout.IntSlider((int)matrix.GetArrayElementAtIndex(x).vector3Value.z, 0, 10);
            }
            if (EditorGUI.EndChangeCheck())
            {
                for (int x = 0; x < reel_size; x++)
                {
                    matrix.GetArrayElementAtIndex(x).vector3Value = new Vector3(slots_per_reel[x][0], slots_per_reel[x][1], slots_per_reel[x][2]);
                }
                serializedObject.ApplyModifiedProperties();
                myTarget.UpdateSlotsInReels();

            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Modify Padding");
            EditorGUI.BeginChangeCheck();
            float padding_x = EditorGUILayout.Slider(padding_xyz.vector3Value.x, 0, 800);
            float padding_y = EditorGUILayout.Slider(padding_xyz.vector3Value.y, 0, 800);
            float padding_z = EditorGUILayout.Slider(padding_xyz.vector3Value.z, 0, 800);
            if (EditorGUI.EndChangeCheck())
            {
                padding_xyz.vector3Value = new Vector3(padding_x, padding_y, padding_z);
                serializedObject.ApplyModifiedProperties();
                //TODO reduce to 1 call - Update Matrix Position
                myTarget.UpdateReelSlotPositions();
                myTarget.UpdateSlotsInReels();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Modify Slot Size");
            EditorGUI.BeginChangeCheck();
            float slot_size_x = EditorGUILayout.Slider(slot_size_xyz.vector3Value.x, 0, 800);
            float slot_size_y = EditorGUILayout.Slider(slot_size_xyz.vector3Value.y, 0, 800);
            float slot_size_z = EditorGUILayout.Slider(slot_size_xyz.vector3Value.z, 0, 800);
            if (EditorGUI.EndChangeCheck())
            {
                slot_size_xyz.vector3Value = new Vector3(slot_size_x, slot_size_y, slot_size_z);
                serializedObject.ApplyModifiedProperties();
                myTarget.UpdateReelSlotPositions();
                myTarget.UpdateSlotsInReels();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
        }
    }
#endif

    public class MatrixGenerator : MonoBehaviour
    {
        /// <summary>
        /// Player Information
        /// </summary>
        public PlayerInformation user_info;

        //Static Self Reference
        private static MatrixGenerator instance;
        public static MatrixGenerator _instance
        {
            get
            {
                if (instance == null)
                    instance = GameObject.FindGameObjectWithTag("Slot Engine").GetComponent<MatrixGenerator>();
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        //*****************


        //Engine Options
        /// <summary>
        /// What folder should I use for loading graphics from. TODO build file explorer folder selection and Resources path parse
        /// </summary>
        public string skin_graphics;
        /// <summary>
        /// list of supported symbols. TODO Display editor which allows you to add based on enum dropdown select and add. Do not display symbols in dropdown already in list
        /// </summary>
        public string[] symbols_supported;

        public Vector3 slot_size;
        public Vector3 matrix_padding = new Vector3(10, 10, 0);
        public Vector3 matrix_anchor_top_left;
        public Vector3[] matrix; // number of elements is number of reels - Vector3 is slot direction and how many

        /// <summary>
        /// Used to cushion the top and bottom of the reel
        /// </summary>
        [Range(1, 5)]
        public int slot_spin_paddingSlots = 1;
        //********

        public async void CreateMatrix() //Main matric Create Function
        {
            if(transform.childCount > 0)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
            Type[] MatrixComponents = new Type[1];
            MatrixComponents[0] = typeof(Matrix);
            Matrix generated_matrix = new GameObject("MatrixObject", MatrixComponents).GetComponent<Matrix>();
            generated_matrix.transform.tag = "Matrix";
            generated_matrix.transform.parent = transform;
            await generated_matrix.GenerateMatrix(matrix,slot_size,matrix_padding); // TODO add ability to insert offset from anchor
        }
        //**************************

        internal void UpdateReelSlotPositions()
        {
            Matrix matrixInUse = FindObjectOfType<Matrix>();
            if(matrixInUse != null)
                for (int i = 0; i < matrixInUse.transform.childCount; i++)
                {
                    matrixInUse.transform.GetChild(i).GetComponent<ReelStripManager>().SetSlotPositionToStart();
                }
        }

        internal void UpdateSlotsInReels()
        {
            Matrix matrixInUse = FindObjectOfType<Matrix>();
            if(matrixInUse != null)
                matrixInUse.UpdateNumberOfSlotsInReel();
        }
        //******************
    }
}