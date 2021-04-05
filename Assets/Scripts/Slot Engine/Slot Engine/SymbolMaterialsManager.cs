#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

namespace Slot_Engine.Matrix
{

#if UNITY_EDITOR
    [CustomEditor(typeof(SymbolMaterialsManager))]
    class SymbolMaterialsManagerEditor : BoomSportsEditor
    {
        SymbolMaterialsManager myTarget;
        SerializedProperty supported_symbols_materials;
        public void OnEnable()
        {
            myTarget = (SymbolMaterialsManager)target;
            supported_symbols_materials = serializedObject.FindProperty("_supported_symbols_materials");
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SymbolMaterialsManager Properties");

            EditorGUILayout.PropertyField(supported_symbols_materials);
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("SymbolMaterialsManager Controls");
            if (GUILayout.Button("Find Symbol Materials"))
            {
                SetMaterialsReference();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Enable Default Inspector");
            base.OnInspectorGUI();
            
        }

        private void SetMaterialsReference()
        {
            myTarget.GenerateSupportedSymbolsMaterials();
            serializedObject.ApplyModifiedProperties();
            supported_symbols_materials = serializedObject.FindProperty("_supported_symbols_materials");
            serializedObject.ApplyModifiedProperties();
        }

        private bool ElementsInArrayAreEmpty(SerializedProperty supported_symbols_materials)
        {
            for (int i = 0; i < myTarget.supported_symbols_materials.Length; i++)
            {
                if (supported_symbols_materials.GetArrayElementAtIndex(supported_symbols_materials.arraySize - 1).objectReferenceValue == null)
                    return true;
            }
            return false;
        }
    }
#endif
    public class SymbolMaterialsManager : MonoBehaviour
    {
        internal Matrix matrix
        {
            get {
                if (_matrix == null)
                    _matrix = GameObject.FindObjectOfType<Matrix>();
                return _matrix;
            }
        }
        [SerializeField]
        internal Matrix _matrix;
        //****Unity Default Functions
        public Material[] _supported_symbols_materials;
        public Material[] supported_symbols_materials
        {
            get
            {
                if (_supported_symbols_materials == null || _supported_symbols_materials.Length < supported_symbols.Length)
                {
                    GenerateSupportedSymbolsMaterials();
                }
                return _supported_symbols_materials;
            }
        }
        public string[] supported_symbols
        {
            get
            {
                return GameObject.FindObjectOfType<Matrix>().supported_symbols;
            }
        }

        public StaticUtilities.SerializableDictionary<string, Material> supported_symbols_map_materials;

        internal void GenerateSupportedSymbolsMaterials()
        {
            _supported_symbols_materials = new Material[supported_symbols.Length];
            supported_symbols_map_materials = new StaticUtilities.SerializableDictionary<string, Material>();
            for (int i = 0; i < supported_symbols.Length; i++)
            {
                Debug.Log(string.Format("Getting symbol {0}", supported_symbols[i]));
                _supported_symbols_materials[i] = matrix.GetMaterialFromSymbol(i);
                supported_symbols_map_materials[supported_symbols[i]] = _supported_symbols_materials[i];
            }
        }
        internal Material ReturnSymbolMaterial(string to_symbol)
        {
            try
            {
                return supported_symbols_map_materials[to_symbol];
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(string.Format("key given = {0}", to_symbol));
                return null;
            }
        }

        void Start()
        {
            if (supported_symbols_map_materials == null)
            {
                GenerateSupportedSymbolsMaterials();
            }
        }
    }
}