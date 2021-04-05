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
using UnityEditorInternal;
#endif
using System;

//public string[] symbol_set_supported = new string[6] { "SF01", "SF02", "MA01" };//Want this list populated by whatever output brent is using. If we are unable to have access from a list then we should pull based on assets provided in skins folder. Read folder names of folders in Base Game/Symbols Directory
namespace Slot_Engine.Matrix
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MatrixGenerator))]
    class MatrixGeneratorEditor : BoomSportsEditor
    {
        ReorderableList display_zone_reorderable_list;
        MatrixGenerator myTarget;
        SerializedProperty padding_xyz;
        SerializedProperty slot_size_xyz;
        SerializedProperty display_zones_per_reel;
        SerializedProperty spin_parameters;

        SerializedProperty skin_graphics;
        SerializedProperty connected_matrix;

        public void OnEnable()
        {
            myTarget = (MatrixGenerator)target;
            padding_xyz = serializedObject.FindProperty("matrix_padding");
            slot_size_xyz = serializedObject.FindProperty("slot_size");
            RefreshPropertiesDisplayZones();
            spin_parameters = serializedObject.FindProperty("spin_parameters");
            skin_graphics = serializedObject.FindProperty("skin_graphics");
            connected_matrix = serializedObject.FindProperty("connected_matrix");
        }

        private void RefreshPropertiesDisplayZones()
        {
            display_zones_per_reel = serializedObject.FindProperty("display_zones_per_reel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            BoomEditorUtilities.DrawUILine(Color.white);
            GenerateEditorHeader();
            BoomEditorUtilities.DrawUILine(Color.white);
            GenerateEditorBody();
            BoomEditorUtilities.DrawUILine(Color.white);
            base.OnInspectorGUI();
        }

        private void GenerateEditorBody()
        {
            //Reel Size Editor Menu
            int reel_size = GenerateReelSizeEditor();
            
            EditorGUI.BeginChangeCheck();
            
            GenerateDisplayZoneEditor(ref display_zones_per_reel);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                myTarget.UpdateSlotObjectsPerReel();
            }
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Modify Spin Information");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spin_parameters);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                myTarget.UpdateSpinParameters();
            }
            if (GUILayout.Button("Force Spin Parameters Update"))
            {
                myTarget.UpdateSpinParameters();
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
            }
        }

        private void GenerateDisplayZoneEditor(ref SerializedProperty display_zones_per_reel)
        {

            EditorGUILayout.LabelField("Display Zones per reel");
            EditorGUILayout.PropertyField(display_zones_per_reel);
        }

        /// <summary>
        /// Draws the header for the reorderable list
        /// </summary>
        /// <param name="rect"></param>
        private void DrawHeaderCallbackDisplayZoneList(Rect rect)
        {
            EditorGUI.LabelField(rect, "Display Zones Per Reel");
        }


        /// <summary>
        /// This methods decides how to draw each element in the list
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isactive"></param>
        /// <param name="isfocused"></param>
        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused) //TODO Enable configuration of display zones
        {
            //Get the element we want to draw from the list. The element is an array of struct data int and bool
            SerializedProperty display_zones_element = display_zone_reorderable_list.serializedProperty.GetArrayElementAtIndex(index);

            //We get the name property of our element so we can display this in our list.
            SerializedProperty reelstrip_display_zones = display_zones_element.FindPropertyRelative("reelstrip_display_zones");
            int properties_in_struct = 2; //This is hard coded
            rect.y += 2 * (reelstrip_display_zones.arraySize * properties_in_struct);
            //Generate a reorderable list per element
            string elementTitle = String.Format("ReelStrip {0} Display Zones",index);

            //Need to display each sub-element
            for (int display_zone = 0; display_zone < reelstrip_display_zones.arraySize; display_zone++)
            {
                SerializedProperty display_zone_element = reelstrip_display_zones.GetArrayElementAtIndex(display_zone);
                EditorGUILayout.LabelField(String.Format("ReelStrip {0} Display Zone {1}", index,display_zone));
                //Draw an int Slider for slots and a bool toggle for active or in-active
                SerializedProperty slots_in_reelstrip_zone = display_zone_element.FindPropertyRelative("slots_in_reelstrip_zone");
                SerializedProperty active_payline_evaluations = display_zone_element.FindPropertyRelative("active_payline_evaluations");
                slots_in_reelstrip_zone.intValue = EditorGUILayout.IntSlider("ReelStrip-Zone_length",slots_in_reelstrip_zone.intValue, 0, 50);
                active_payline_evaluations.boolValue = EditorGUILayout.Toggle("Active Payline Evaluation Zone?", active_payline_evaluations.boolValue);
            }
        }


        /// <summary>
        /// Calculates the height of a single element in the list.
        /// This is extremely useful when displaying list-items with nested data.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float ElementHeightCallback(int index)
        {
            //Gets the height of the element. This also accounts for properties that can be expanded, like structs.
            float propertyHeight =
                EditorGUI.GetPropertyHeight(display_zone_reorderable_list.serializedProperty.GetArrayElementAtIndex(index), true);

            float spacing = EditorGUIUtility.singleLineHeight / 2;

            return propertyHeight + spacing;
        }

        /// <summary>
        /// Defines how a new list element should be created and added to our list.
        /// </summary>
        /// <param name="list"></param>
        private void OnAddCallback(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
        }

        private int GenerateReelSizeEditor()
        {
            EditorGUILayout.LabelField("Modify Matrix - Reel Size");
            EditorGUI.BeginChangeCheck();
            int reel_size = EditorGUILayout.IntSlider(this.display_zones_per_reel.arraySize, 0, 20);
            if (EditorGUI.EndChangeCheck())
            {
                myTarget.SetReelSizeTo(reel_size);
                serializedObject.Update();
                RefreshPropertiesDisplayZones();
            }

            return reel_size;
        }

        private void GenerateEditorHeader()
        {
            if (myTarget.transform.childCount < 1)
            {
                EditorGUILayout.LabelField("Commands");
                //Display zones, Slot Size, Slot Padding and all the managers need to be initialized
                if (GUILayout.Button("Generate Matrix From Current Configuration"))
                {
                    //Will need to generate managers and all UI tools
                    myTarget.CreateMatrix();
                }
            }
            else
            {
                if (connected_matrix.objectReferenceValue == null && myTarget.transform.GetComponentInChildren<Matrix>())
                {
                    EditorGUILayout.LabelField("Commands");
                    if (GUILayout.Button("Connect matrix generator to child matrix"))
                    {
                        myTarget.ConnectMatrixToChild();
                        serializedObject.Update();
                    }
                }
                else if (connected_matrix.objectReferenceValue == null && !myTarget.transform.GetComponentInChildren<Matrix>())
                {
                    EditorGUILayout.LabelField("Child gameobject must have matrix component");
                    if (GUILayout.Button("Generate Matrix From Current Configuration"))
                    {
                        //Will need to generate managers and all UI tools
                        myTarget.CreateMatrix();
                    }
                }
                else if (connected_matrix.objectReferenceValue != null)
                {
                    EditorGUILayout.LabelField("Modifying below will modify Connected Matrix");
                    if(GUILayout.Button("Ensure reels and slot objects are generated"))
                    {
                        myTarget.UpdateSlotObjectsPerReel();
                    }
                    if (GUILayout.Button("Re-generate slot objects"))
                    {
                        myTarget.RegenerateSlotObjects();
                    }
                }
            }
        }
    }
#endif
    //This is to be able to have multiple display zone's that share the same reel_strip_spin_loop_symbols generated by end_configuration_generater
    //Theory - You have multiple ReelStripStructDisplayZone's - if you have multiple display_slots_per_reel then you have so many active matrix zones
    //for a pyramid stacked matrix (3) 2x5 matrix's which are connected top->bottom 1-> 2 is connected at reel 1,3,5. 2 -> 3 is connected at reel 2,4. 3 has an extra slot in reel 3.
    //So you would need a DisplayZone[] that would be 2x2x2x2x2, 1x0x1x0x1, 2x2x2x2x2, 0x1x0x1x0, 2x2x2x2x2, 0x0x1x0x0
    //A position would have to be made in every reel strip until the lowest point atleast, 9 positions in path for display area - 1 position at end - 10 total 
    /// <summary>
    /// A stackable display zone active display zones will be affected by payline evaluations. in-active zones will be omitted from paylien evaluations
    /// </summary>
    [Serializable]
    public struct ReelStripStructDisplayZones
    {
        /// <summary>
        /// Padding before display zone
        /// </summary>
        [Range(1,50)]
        public int padding_before;
        /// <summary>
        /// Padding After Display Zone - default to 1
        /// </summary>
        [Range(1,50)]
        public int padding_after;
        /// <summary>
        /// This is where you can stack display zones that are affected or not affected by payline evaluations
        /// </summary>
        [SerializeField]
        public ReelStripStructDisplayZone[] reelstrip_display_zones; //regular 3x5 matrix needs 3 slots in reelstrip and active_payline_evalutation = true
    }
    /// <summary>
    /// Controls a display zone for a reel strip
    /// </summary>
    [Serializable]
    public struct ReelStripStructDisplayZone
    {
        /// <summary>
        /// The Reel strip slot amount
        /// </summary>
        [SerializeField]
        public int slots_in_reelstrip_zone;
        /// <summary>
        /// Is this an active zone for payline evaluations?
        /// </summary>
        [SerializeField]
        public bool active_payline_evaluations;
    }

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
        /// <summary>
        /// The size of the slot prefabs in the matrix
        /// </summary>
        public Vector3 slot_size;
        /// <summary>
        /// 
        /// </summary>
        public Vector3 matrix_object_position_worldspace;
        /// <summary>
        /// Display Zones per reel. stacked 2x5 inverted pyramid matrix requires ReelStripStructDisplayZone[5]{2x1x2x0x2x0,2x0x2x1x2x0,2x1x2x0x2x1,2x0x2x1x2x0, 2x1x2x0x2x0}
        /// </summary>
        public ReelStripStructDisplayZones[] display_zones_per_reel;

        /// <summary>
        /// Used to cushion the top and bottom of the reel
        /// </summary>
        public int reel_start_padding_slot_objects = 1;
        //********

        public ReelStripSpinParametersScriptableObject spin_parameters;

        //Associate the instance that gets updated with Generate Matrix
        public Matrix connected_matrix;

        public async void CreateMatrix() //Main matrix Create Function
        {
            //Check for a child object. If there is then connect and modify
            connected_matrix = GenerateMatrixObject();
            //
            await connected_matrix.SetMatrixReelStripsInfo(display_zones_per_reel, slot_size); 
        }
        /// <summary>
        /// Generates a new matrix object child with reelstrips configured
        /// </summary>
        /// <returns>matrix reference for connected matrix</returns>
        private Matrix GenerateMatrixObject()
        {
            Type[] MatrixComponents = new Type[1];
            MatrixComponents[0] = typeof(Matrix);
            GameObject gameObject_to_return = new GameObject("MatrixObject", MatrixComponents);
            gameObject_to_return.transform.tag = "Matrix";
            gameObject_to_return.transform.parent = transform;
            return gameObject_to_return.GetComponent<Matrix>();
        }

        //**************************
        /// <summary>
        /// Updates the slot objects and empty positions in path on reels
        /// </summary>
        internal void UpdateSlotObjectsPerReel()
        {
            Debug.Log(String.Format("Length of all display zone information= {0}",display_zones_per_reel.Length));
            //Build reelstrip info 
            ReelStripsStruct reelstrips_configuration = new ReelStripsStruct(display_zones_per_reel);

            connected_matrix?.SetReelsAndSlotsPerReel(reelstrips_configuration);        
        }

        internal void SetReelSizeTo(int reel_size)
        {
            SetDisplayZonesTo(reel_size);
            connected_matrix?.SetReelsTo(reel_size);
        }
        private void SetDisplayZonesTo(int reel_size)
        {
            ReelStripStructDisplayZones[] display_zone_cache;
            //generate enough display zones per reel
            if (display_zones_per_reel != null)
            {
                display_zone_cache = display_zones_per_reel;
            }
            else
            {
                display_zone_cache = new ReelStripStructDisplayZones[0];
            }
            display_zones_per_reel = new ReelStripStructDisplayZones[reel_size];
            for (int i = 0; i < reel_size; i++)
            {
                SetDisplayZoneAtIndex(ref display_zone_cache, i);
            }
        }

        private void SetIntArrayAtIndex(ref int[] int_array_cache,ref int[] int_array, int reel_size)
        {
            for (int i = 0; i < reel_size; i++)
            {
                if (i < int_array_cache.Length)
                    int_array[i] = int_array_cache[i];
                else
                    int_array[i] = 0;
                //SetDisplayZoneAtIndex(ref display_zone_cache, i);
            }
        }

        private void SetDisplayZoneAtIndex(ref ReelStripStructDisplayZones[] display_zone_cache, int index)
        {
            if (index < display_zone_cache.Length)
                display_zones_per_reel[index] = display_zone_cache[index];
            else
                display_zones_per_reel[index] = new ReelStripStructDisplayZones();
        }

        internal void ConnectMatrixToChild()
        {
            //Connect Matrix Child
            connected_matrix = GetComponentInChildren<Matrix>();
            //Get all reels slots per reel and pre-populate the reelstrip config structs
            //InitializeReelsFromConnectedMatrix();
        }

        internal void UpdateSpinParameters()
        {
            connected_matrix.SetSpinParametersTo(spin_parameters);
        }

        internal void RegenerateSlotObjects()
        {
            connected_matrix.RegenerateSlotObjects();
        }
        //******************
    }
}