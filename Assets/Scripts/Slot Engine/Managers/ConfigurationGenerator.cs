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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using System;
using BoomSports.Prototype.Managers;

namespace BoomSports.Prototype
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ConfigurationGenerator))]
    class MatrixGeneratorEditor : BoomSportsEditor
    {
        ConfigurationGenerator myTarget;
        SerializedProperty connectedConfigurationObject;

        public void OnEnable()
        {
            myTarget = (ConfigurationGenerator)target;
            connectedConfigurationObject = serializedObject.FindProperty("connectedConfigurationObject");
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
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Modify Spin Information");
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.PropertyField(spin_parameters);
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
                    myTarget.CreateStripConfiguration();
                }
            }
            else
            {
                if (connectedConfigurationObject.objectReferenceValue == null && myTarget.transform.GetComponentInChildren<StripConfigurationObject>())
                {
                    EditorGUILayout.LabelField("Commands");
                    if (GUILayout.Button("Connect matrix generator to child matrix"))
                    {
                        myTarget.ConnectMatrixToChild();
                        serializedObject.Update();
                    }
                }
                else if (connectedConfigurationObject.objectReferenceValue == null && !myTarget.transform.GetComponentInChildren<StripConfigurationObject>())
                {
                    EditorGUILayout.LabelField("Child gameobject must have matrix component");
                    if (GUILayout.Button("Generate Matrix From Current Configuration"))
                    {
                        //Will need to generate managers and all UI tools
                        myTarget.CreateStripConfiguration();
                    }
                }
                else if (connectedConfigurationObject.objectReferenceValue != null)
                {
                    EditorGUILayout.LabelField("Modifying below will modify Connected Matrix");
                    if(GUILayout.Button("Load Configuration Selected"))
                    {
                        myTarget.LoadConfigurationSelected();
                    }
                }
            }
        }
    }

#endif

    public partial class ConfigurationGenerator : MonoBehaviour
    {
        //Static Self Reference
        private static ConfigurationGenerator instance;
        public static ConfigurationGenerator _instance
        {
            get
            {
                if (instance == null)
                    instance = GameObject.FindGameObjectWithTag("ConfigurationGenerator").GetComponent<ConfigurationGenerator>();
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        //*****************

        public ConfigurationSettingsScriptableObject configurationGeneratorSettings;

        //Engine Options
        /// <summary>
        /// What Game Asset folder is currently loaded - To Be Implemented
        /// </summary>
        public string configurationObjectLoadedName;
        //********
        //Associate the instance that gets updated with Generate Matrix
        public BaseConfigurationObjectManager connectedConfigurationObject;
        /// <summary>
        /// Used to generate backplate and covers for the slots
        /// </summary>
        public GameObject backPlateCoverSceneObject;
        /// <summary>
        /// Creates a configuration based on current settings
        /// </summary>
        public async void CreateStripConfiguration() //Main matrix Create Function
        {
            //Check for a child object. If there is then connect and modify otherwise create a new one
            GenerateConfigurationObject<StripConfigurationObject>(ref connectedConfigurationObject);
            SetConnectedConfigurationObjectLoadedSettings(ref connectedConfigurationObject, ref configurationGeneratorSettings);
            SetSymbolDataForConfigurationObject(ref connectedConfigurationObject, ref configurationGeneratorSettings);
            string printMessage = "";
            for (int i = 0; i < configurationGeneratorSettings.displayZones.Length; i++)
            {
                printMessage += "+ " + configurationGeneratorSettings.displayZones[i].totalPositions;
            }
            Debug.Log($"Configuration Generator is generating total positions per zone {printMessage}");
            await DisplayConfigurationSettings(ref configurationGeneratorSettings);
            //For Each Local position thats an active display slot generate a backplate prefab for grid effect for now
            GenerateBackplateFromStripConfiguration(connectedConfigurationObject);
            //Update Payline Manager
            Debug.LogWarning($"Select Evaluation Manger Object and run Generate Paylines From Matrix to have update evaluation system");
        }

        private void SetSymbolDataForConfigurationObject(ref BaseConfigurationObjectManager connectedConfigurationObject, ref ConfigurationSettingsScriptableObject configurationGeneratorSettings)
        {
            connectedConfigurationObject.symbolDataScriptableObject = configurationGeneratorSettings.symbolData;
        }

        private void SetConnectedConfigurationObjectLoadedSettings(ref BaseConfigurationObjectManager connectedConfigurationObject, ref ConfigurationSettingsScriptableObject configurationGeneratorSettings)
        {
            connectedConfigurationObject.configurationSettings = configurationGeneratorSettings;
            configurationObjectLoadedName = configurationGeneratorSettings.configurationName;
        }

        private Task DisplayConfigurationSettings(ref ConfigurationSettingsScriptableObject configurationGeneratorSettings)
        {
            Debug.Log($"High level configurationGeneratorSettings.PrintDisplayZones() = {configurationGeneratorSettings.PrintDisplayZones()}");
            SetConfigurationDisplayZones(ref configurationGeneratorSettings.displayZones);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the reelstrips info for the matrix
        /// </summary>
        /// <param name="displayZonesPerStrip">The display zone breakdown per reel</param>
        /// <returns></returns>
        public Task SetConfigurationDisplayZones(ref ConfigurationDisplayZonesStruct[] displayZonesPerStrip)
        {
            //Build reelstrip info 
            StripsStruct stripConfiguration = new StripsStruct(ref displayZonesPerStrip);
            string printMessage = "";
            for (int i = 0; i < displayZonesPerStrip.Length; i++)
            {
                printMessage += $"displayZonesPerStrip[{i}].totalPositions = " + displayZonesPerStrip[i].totalPositions;
            }
            Debug.Log($"SetConfigurationDisplayZones to total positions{printMessage}");
            SetReelsAndSlotsPerReel(stripConfiguration, displayZonesPerStrip);
            //Set each reel display zone information
            ConfigurationDisplayZonesStruct temp;
            for (int group = 0; group < connectedConfigurationObject.groupObjectManagers.Length; group++)
            {
                Debug.Log($"displayZonesPerStrip[group].paddingBefore {displayZonesPerStrip[group].paddingBefore}");
                temp = new ConfigurationDisplayZonesStruct(displayZonesPerStrip[group]);
                connectedConfigurationObject.groupObjectManagers[group].configurationGroupDisplayZones = temp;
                Debug.Log($"connectedConfigurationObject.configurationGroupManagers[group].configurationGroupDisplayZones.paddingBefore{connectedConfigurationObject.groupObjectManagers[group].configurationGroupDisplayZones.paddingBefore}");
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Anytime this is called - the end_configuration, paylines managers need to update.
        /// </summary>
        /// <param name="slots_per_reelstrip"></param>
        internal void SetReelsAndSlotsPerReel(StripsStruct stripsConfiguration, ConfigurationDisplayZonesStruct[] displayZonesPerStrip)
        {
            //Debug.Log($"Setting ReelsAndSlotsPerReel reelstrip info with total positions {stripsConfiguration.PrintStrips()}");
            //Ensure there are enough reel objects
            SetStripObjectsToLength(stripsConfiguration.strips.Length,ref connectedConfigurationObject);
            StripConfigurationObject stripConfiguration = connectedConfigurationObject as StripConfigurationObject;
            //Ensure each strip knows its column position
            for (int i = 0; i < stripsConfiguration.strips.Length; i++)
            {
                Debug.Log($"strips[{i}] info with total positions {displayZonesPerStrip[i].totalPositions} connectedConfigurationObject.configurationSettings.displayZones[i].totalPositions = {connectedConfigurationObject.configurationSettings.displayZones[i].totalPositions}");
                stripConfiguration.SetStripInfoStruct(i, stripsConfiguration.strips[i]);
            }
            if(setStripsInitialPositionFromCode)
                //Ensure the strips are positioned - Will mess with artists modifications in the future
                SetStripObjectsInitialPositions(ref stripConfiguration);
            StripObjectGroupManager objectGroupManager;
            Vector3[] arrayCopy;
            //Set each Reels Configuration - each reel will take care of generating slots
            for (int i = 0; i < stripsConfiguration.strips.Length; i++)
            {
                Debug.Log($"Setting {connectedConfigurationObject.groupObjectManagers[i].gameObject.name} reelstrip info with total positions {displayZonesPerStrip[i].totalPositions}");
                BaseObjectGroupManager temp2 = connectedConfigurationObject.groupObjectManagers[i];
                //Generate Slot Objects
                Debug.Log($"Generate Slot Objects for stripsConfiguration.strips[{i}]{stripsConfiguration.strips[i]}");
                GenerateStripSlotObjects(ref temp2, stripsConfiguration.strips[i]);
                objectGroupManager = connectedConfigurationObject.groupObjectManagers[i] as StripObjectGroupManager;
                arrayCopy = new Vector3[displayZonesPerStrip[i].totalPositions];
                for (int j = 0; j < arrayCopy.Length; j++)
                {
                    arrayCopy[j] = objectGroupManager.localPositionsInStrip[j];
                }
                objectGroupManager.localPositionsInStrip = arrayCopy;
            }
        }

        private void GenerateStripSlotObjects(ref BaseObjectGroupManager objectManager, GroupInformationStruct reelStripStruct)
        {
            //gather slot object child if any
            List<StripObjectManager> childSlots = new List<StripObjectManager>();
            childSlots.AddRange(objectManager.transform.GetComponentsInChildren<StripObjectManager>());
            if(childSlots.Count < objectManager.configurationGroupDisplayZones.slotsToGenerate)
            {
                for (int slotToGenerate = childSlots.Count; slotToGenerate < objectManager.configurationGroupDisplayZones.slotsToGenerate; slotToGenerate++)
                {
                    childSlots.Add(GenerateSlotObject(slotToGenerate,ref objectManager) as StripObjectManager);
                }
            }
            //ToDo refactor to make generic
            objectManager.objectsInGroup = childSlots.ToArray();
        }

        /// <summary>
        /// Sets Strip Objects Initial Positions
        /// </summary>
        /// <param name="connectedConfigurationObject"></param>
        private void SetStripObjectsInitialPositions(ref StripConfigurationObject connectedConfigurationObject)
        {
            for (int strip = 0; strip < connectedConfigurationObject.groupObjectManagers.Length; strip++)
            {
                //Generates the local position for the strip. Slot Movements are calculcated and applied to localPosition;
                connectedConfigurationObject.groupObjectManagers[strip].transform.localPosition = GenerateLocalPositionForStrip(strip, connectedConfigurationObject.groupObjectManagers.Length, connectedConfigurationObject.configurationSettings);
            }
        }

        public enum eAnchor
        {
            TopLeft, TopMiddle, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft,BottomCenter,BottomRight
        } 

        public eAnchor anchor = eAnchor.MiddleCenter;
        public bool setStripsInitialPositionFromCode;

        /// <summary>
        /// Need to control how strips are placed. in future implement anchor point - Anchor Default to top Center
        /// </summary>
        /// <param name="strip"></param>
        /// <param name="lengthOfStrips"></param>
        /// <param name="configurationSettings"></param>
        /// <returns></returns>
        private Vector3 GenerateLocalPositionForStrip(int strip, int lengthOfStrips, ConfigurationSettingsScriptableObject configurationSettings)
        {
            Vector3 output;
            //Offset is based on anchor - middle center we place the reel closest to center - 3x5 - reel 3 offset by half slot size only- 4x6 - reel 3
            //Whole number the offset is 1 slot + padding - odd half slot + padding
            //Debug.LogWarning($"strip = {strip}, lengthOfStrips = {lengthOfStrips}");
            //Debug.LogWarning($"lengthOfStrips % 2 == 0  = {lengthOfStrips % 2 == 0} (int)lengthOfStrips / 2 = {(int)lengthOfStrips / 2}");
            float centerStripOffset = lengthOfStrips % 2 == 0 ? configurationSettings.slotSize.x / 2 : 0;//configurationSettings.slotSize.x : configurationSettings.slotSize.x / 2;
            int centerIndex = (int)lengthOfStrips / 2; // 7 / 2 = 3.5 centerindex = 3
            //At this point anchor offset is either 285 or 285/2 - take 5x7 = 3 reels place left of center - 3 reels to right - 1 in center
            //Start Placement left to right 
            float xPosition = -centerStripOffset;
            //120
            //Apply left most anchor with padding 1-10-2-10-3-10-4-10-5-10-6-10-7
            //Debug.LogWarning($"({centerIndex - strip}) * ({configurationSettings.slotSize.x})");
            xPosition += -(((centerIndex - strip) * (configurationSettings.slotSize.x)) + ((centerIndex - strip) * configurationSettings.slotPadding.x)); // Base Calculation of just slot positions
            //Debug.LogWarning($"anchorXOffsetFrom0 = {centerStripOffset} centerIndex = {centerIndex} of lengthOfStrips {lengthOfStrips} positionX generated = {xPosition}");
            float yPosition = configurationSettings.slotSize.y / 2;
            float zPosition = 0;
            //if anchor y is 
            output = new Vector3(xPosition,yPosition,zPosition);
            return output;
        }
        /// <summary>
        /// This method clones all of the items and serializable properties of the current collection by 
        /// serializing the current object to memory, then deserializing it as a new object. This will 
        /// ensure that all references are cleaned up.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public T CreateSerializedCopy<T>(T oRecordToCopy)
        {
            // Exceptions are handled by the caller

            if (oRecordToCopy == null)
            {
                return default(T);
            }

            if (!oRecordToCopy.GetType().IsSerializable)
            {
                throw new ArgumentException(oRecordToCopy.GetType().ToString() + " is not serializable");
            }

            var oFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using (var oStream = new MemoryStream())
            {
                oFormatter.Serialize(oStream, oRecordToCopy);
                oStream.Position = 0;
                return (T)oFormatter.Deserialize(oStream);
            }
        }
        /// <summary>
        /// Generates Backplates from configuration - todo Generic class
        /// </summary>
        /// <param name="connectedConfigurationObject"></param>
        private void GenerateBackplateFromStripConfiguration(BaseConfigurationObjectManager connectedConfigurationObject)
        {
            //TODO Refactor to create generic reference
            StripConfigurationObject connectedStripConfigurationObject = connectedConfigurationObject as StripConfigurationObject;
            //The positions here need to include the reels X value - will run a replacement for now
            Vector3[][] configurationObjectWorldPosition = connectedStripConfigurationObject.positions_in_path_v3_local;
            //Copy the list without keeping references - Reels are positioned with X & y offset - strips handle positons locally
            List<Vector3> tempFirstDimension = new List<Vector3>();
            List<Vector3[]> tempSecondDimension = new List<Vector3[]>();
            for (int item1 = 0; item1 < configurationObjectWorldPosition.Length; item1++)
            {
                for (int subItem = 0; subItem < configurationObjectWorldPosition[item1].Length; subItem++)
                {
                    //Debug.Log($"Adding {String.Join("|", configurationObjectWorldPosition[item1][subItem])} to array");
                    tempFirstDimension.Add(ReturnNewVector3(configurationObjectWorldPosition[item1][subItem]));
                }
                tempSecondDimension.Add(tempFirstDimension.ToArray());
            }
            Debug.LogWarning($"configurationObjectWorldPosition[0] == tempSecondDimension[0] = {configurationObjectWorldPosition[0] == tempSecondDimension[0]}");
            //Still linked to original reference
            List<Vector3> reelPositionXList = new List<Vector3>();
            Vector3 stripXVector;
            for (int strip = 0; strip < tempSecondDimension.Count; strip++)
            {
                stripXVector = GenerateLocalPositionForStrip(strip, tempSecondDimension.Count, connectedConfigurationObject.configurationSettings);
                for (int position = 0; position < tempSecondDimension[strip].Length; position++)
                {
                    tempSecondDimension[strip][position].x = stripXVector.x;
                    tempSecondDimension[strip][position].y += stripXVector.y;
                }
            }
            //Debug.LogWarning($"connectedConfigurationObject.positions_in_path_v3_local length = {connectedConfigurationObject.positions_in_path_v3_local.Length}");
            CoverFacePlateGameobjectManager temp = null;
            //Ensure there is a Backplate and Cover Object - If not generate one and place as child
            if (backPlateCoverSceneObject == null)
            {
                backPlateCoverSceneObject = GameObject.FindGameObjectWithTag("cover_plate");
                if (backPlateCoverSceneObject == null)
                {
                    temp = CreateNewCoverPlateObject();
                    backPlateCoverSceneObject = temp.gameObject;
                }
                else
                {
                    temp = backPlateCoverSceneObject.GetComponent<CoverFacePlateGameobjectManager>();
                }
            }
            else
            {
                temp = backPlateCoverSceneObject.GetComponent<CoverFacePlateGameobjectManager>();
            }
            DestroyAllChildren(temp.backPlateParent);
            DestroyAllChildren(temp.coverPlateParent);
            int positionTemp = 0;
            //for each display zone - generate a cover only for any padding and backing for any active
            for (int strip = 0; strip < connectedConfigurationObject.configurationSettings.displayZones.Length; strip++)
            {
                for (int position = 0; position < connectedConfigurationObject.configurationSettings.displayZones[strip].paddingBefore; position++)
                {
                    GenerateCoverPrefab(connectedConfigurationObject, tempSecondDimension.ToArray(), temp, strip, position);
                }
                positionTemp = connectedConfigurationObject.configurationSettings.displayZones[strip].paddingBefore;
                for (int displayZoneStrip = 0; displayZoneStrip < connectedConfigurationObject.configurationSettings.displayZones[strip].displayZones.Length; displayZoneStrip++)
                {
                    //Will generate objects based on active of inactive display zone and increment position
                    GenerateCoverOrBackplate(connectedConfigurationObject.configurationSettings.displayZones[strip].displayZones[displayZoneStrip], connectedConfigurationObject, tempSecondDimension.ToArray(), temp, strip, ref positionTemp);
                }
                for (int position = 0; position < connectedConfigurationObject.configurationSettings.displayZones[strip].paddingAfter; position++)
                {
                    GenerateCoverPrefab(connectedConfigurationObject, tempSecondDimension.ToArray(), temp, strip, connectedConfigurationObject.configurationSettings.displayZones[strip].displayZonesPositionsTotal + connectedConfigurationObject.configurationSettings.displayZones[strip].paddingBefore + position);
                }
            }
            //for (int item1 = 0; item1 < configurationObjectWorldPosition.Length; item1++)
            //{
            //    for (int subItem = 0; subItem < configurationObjectWorldPosition[item1].Length; subItem++)
            //    {
            //        Debug.Log($"Reference Vector3 = {String.Join("|", configurationObjectWorldPosition[item1][subItem])} strip local position");
            //    }
            //}
        }

        private Vector3 ReturnNewVector3(Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.y, vector3.z);
        }

        private void DestroyAllChildren(Transform parent)
        {
            for (int child = parent.childCount-1; child >= 0; child--)
            {
                DestroyImmediate(parent.GetChild(child).gameObject);
            }
        }

        private void GenerateCoverOrBackplate(DisplayZoneStruct reelStripStructDisplayZone, BaseConfigurationObjectManager connectedConfigurationObject, Vector3[][] configurationObjectWorldPosition, CoverFacePlateGameobjectManager temp, int strip, ref int positionInStrip)
        {
            for (int i = 0; i < reelStripStructDisplayZone.positionsInZone; i++)
            {
                //Active generate back
                if (reelStripStructDisplayZone.activePaylineEvaluations)
                {
                    GenerateBackPlatePrefab(connectedConfigurationObject, configurationObjectWorldPosition, temp, strip, positionInStrip + i);
                }
                else
                {
                    GenerateCoverPrefab(connectedConfigurationObject, configurationObjectWorldPosition, temp, strip, positionInStrip + i);
                }
            }
            positionInStrip += reelStripStructDisplayZone.positionsInZone;
        }

        private void GenerateCoverPrefab(BaseConfigurationObjectManager connectedConfigurationObject, Vector3[][] configurationObjectWorldPosition, CoverFacePlateGameobjectManager temp, int strip, int position)
        {
            //Debug.LogWarning($"Generating Cover for position {configurationObjectWorldPosition[strip][position]}");
            //Debug.LogWarning($"Using Cover Prefab {connectedConfigurationObject.configurationSettings.symbolCover.name}");
            //Debug.LogWarning($"Cover Plate Parent =  {temp.coverPlateParent.name}");
            GeneratePrefabAtPosition(configurationObjectWorldPosition[strip][position] + Vector3.back, connectedConfigurationObject.configurationSettings.symbolCover, temp.coverPlateParent);
        }

        private void GenerateBackPlatePrefab(BaseConfigurationObjectManager connectedConfigurationObject, Vector3[][] configurationObjectWorldPosition, CoverFacePlateGameobjectManager temp, int strip, int position)
        {
            //Debug.LogWarning($"Generating Backplate for position {configurationObjectWorldPosition[strip][position]}");
            //Debug.LogWarning($"Using Backplate Prefab {connectedConfigurationObject.configurationSettings.symbolBackplatePrefab.name}");
            //Debug.LogWarning($"Backplate Plate Parent =  {temp.backPlateParent.name}");
            GeneratePrefabAtPosition(configurationObjectWorldPosition[strip][position] + Vector3.forward, connectedConfigurationObject.configurationSettings.symbolBackplatePrefab, temp.backPlateParent);
        }



        /// <summary>
        /// Generates a Slot Gameobject
        /// </summary>
        /// <param name="slot_position_in_reel">the slot in reel generating the object for</param>
        /// <returns></returns>
        internal static BaseObjectManager GenerateSlotObject(int slot_position_in_reel, ref BaseObjectGroupManager objectGroupManager)
        {
            //Debug.Log($"objectGroupManager name = {objectGroupManager.gameObject.name}");
            StripObjectGroupManager reelStripManager = objectGroupManager as StripObjectGroupManager;
            //Slot position in group needs to be abstracted

            //TODO Return Slot position based on display zone and index passes
            int localPositionIndex = slot_position_in_reel;
            //Local positions are already generated by this point
            Vector3 slot_position_on_path = reelStripManager.localPositionsInStrip[localPositionIndex];
            StripObjectManager generated_slot = InstantiateSlotGameobject(slot_position_in_reel, reelStripManager, slot_position_on_path, Vector3.one, Quaternion.identity);
            generated_slot.localStartPositionIndex = slot_position_in_reel;
            //Generate a random symbol prefab
            generated_slot.ShowRandomSymbol();
            return generated_slot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberInStrip"></param>
        /// <param name="parentStrip"></param>
        /// <param name="startPosition"></param>
        /// <param name="scale"></param>
        /// <returns>Slot Manager Reference</returns>
        internal static StripObjectManager InstantiateSlotGameobject(int numberInStrip, StripObjectGroupManager parentStrip, Vector3 startPosition, Vector3 scale, Quaternion startRotation)
        {
#if UNITY_EDITOR
            GameObject ReturnValue = PrefabUtility.InstantiatePrefab(Resources.Load("Core/Prefabs/Slot-Container")) as GameObject; // TODO Refactor to include custom sot container passable argument
            ReturnValue.gameObject.name = String.Format("Slot_{0}", numberInStrip);
            ReturnValue.transform.parent = parentStrip.transform;
            StripObjectManager return_component = ReturnValue.GetComponent<StripObjectManager>();
            return_component.baseObjectGroupParent = parentStrip;
            //ReturnValue.transform.GetChild(0).localScale = scale;
            ReturnValue.transform.localPosition = startPosition;
            ReturnValue.transform.localRotation = startRotation;
            return return_component;
#endif
            //Intended - we want to only instantiate these objects in unity_editor
            return null;
        }

        ///// <summary>
        ///// Set the number of slot objects in reel
        ///// </summary>
        ///// <param name="display_zones">Display Slots in reel</param>
        ///// <param name="before_display_slots">amount of slots before display slots to generate objects for - minimum 1</param>
        //internal void UpdateSlotObjectsInReelStrip(int slots_in_reelstrip)
        //{
        //    List<SlotManager> objectsInGroup = new List<SlotManager>();
        //    if (this.objectsInGroup == null)
        //    {
        //        SlotManager[] slots_initialized = transform.GetComponentsInChildren<SlotManager>();
        //        if (slots_initialized.Length > 0)
        //        {
        //            this.objectsInGroup = slots_initialized;
        //        }
        //        else
        //        {
        //            this.objectsInGroup = new SlotManager[0];
        //        }
        //    }
        //    objectsInGroup.AddRange(this.objectsInGroup);

        //    int total_slot_objects_required = slots_in_reelstrip;

        //    SetSlotObjectsInStripTo(ref objectsInGroup, total_slot_objects_required);

        //    this.objectsInGroup = objectsInGroup.ToArray();
        //}

        //internal void RegenerateSlotObjects()
        //{
        //    for (int slot = 0; slot < objectsInGroup.Length; slot++)
        //    {
        //        Debug.Log(String.Format("Reel {0} deleteing slot {1}", reelstrip_info.reel_number, slot));
        //        if (objectsInGroup[slot] != null)
        //            DestroyImmediate(objectsInGroup[slot].gameObject);
        //        objectsInGroup[slot] = GenerateSlotObject(slot);
        //    }
        ////}
        //internal void SetSlotObjectsInStripTo(ref List<SlotManager> slots_in_reel, int total_slot_objects_required)
        //{

        //    //Do we need to add or remove display slot objects on reelstrip
        //    bool add_substract = slots_in_reel.Count < total_slot_objects_required ? true : false;

        //    //Either remove from end or add until we have the amount of display and before display slot obejcts
        //    for (int slot_to_update = add_substract ? slots_in_reel.Count : slots_in_reel.Count - 1; //Set to current count to add or count - 1 to subtract
        //        add_substract ? slot_to_update < total_slot_objects_required : slot_to_update >= total_slot_objects_required; // either add until you have required slot objects or remove
        //        slot_to_update += add_substract ? 1 : -1) //count up or down
        //    {
        //        if (add_substract)
        //        {
        //            slots_in_reel.Add(GenerateSlotObject(slot_to_update));
        //        }
        //        else
        //        {
        //            DestroyImmediate(slots_in_reel[slot_to_update].gameObject);
        //            slots_in_reel.RemoveAt(slot_to_update);
        //        }
        //    }
        //}

        private async void GeneratePrefabAtPosition(Vector3 vector3, Transform prefab, Transform parent)
        {
#if UNITY_EDITOR
            Transform gameobjectGenerated = PrefabUtility.InstantiatePrefab(prefab, parent) as Transform;
            gameobjectGenerated.position = vector3;
            gameobjectGenerated.rotation = Quaternion.identity;
            gameobjectGenerated.localScale = Vector3.one;
#endif
        }

        private CoverFacePlateGameobjectManager CreateNewCoverPlateObject()
        {
            Type[] componentsToAdd = new Type[1] {typeof(CoverFacePlateGameobjectManager) };
            GameObject output = new GameObject("Cover-Face-ObjectsGenerated", componentsToAdd);

            CoverFacePlateGameobjectManager temp = output.GetComponent<CoverFacePlateGameobjectManager>();
            
            GameObject outputcoverParent = new GameObject("Symbol-Covers");
            outputcoverParent.transform.parent = output.transform;
            outputcoverParent.transform.position = Vector3.zero;
            outputcoverParent.transform.rotation = Quaternion.identity;
            outputcoverParent.transform.localScale = Vector3.one;

            GameObject outputbackParent = new GameObject("Symbol-Backplate");
            outputbackParent.transform.parent = output.transform;
            outputbackParent.transform.position = Vector3.zero;
            outputbackParent.transform.rotation = Quaternion.identity;
            outputbackParent.transform.localScale = Vector3.one;

            temp.backPlateParent = outputbackParent.transform;
            temp.coverPlateParent = outputcoverParent.transform;
            output.transform.parent = transform;
            output.transform.position = Vector3.zero;
            output.transform.rotation = Quaternion.identity;
            output.transform.localScale = Vector3.one;

            output.tag = "cover_plate";
            return temp;
        }
        /// <summary>
        /// Generate or remove reelstrip objects based on number of reels set
        /// </summary>
        /// <param name="lengthOfReels">Reels in Configuration</param>
        /// <param name="connectedConfigurationObject.reelStripManagers">reference var to cached reelstrip_managers</param>
        internal void SetStripObjectsToLength(int lengthOfReels, ref BaseConfigurationObjectManager connectedConfigurationObject)
        {
            //Ensure Connected Reel Managers 
            EnsureConnectedConfigurationObjectStripManagersSet(ref connectedConfigurationObject);
            //Add or subtract reel obejcts as needed
            if(lengthOfReels - connectedConfigurationObject.groupObjectManagers.Length < 0)
            {
                //Refactor to make generic
                BaseObjectGroupManager temp;
                //Remove strip objects
                for (int strip = connectedConfigurationObject.groupObjectManagers.Length - 1; strip >= lengthOfReels ; strip--)
                {
                    temp = connectedConfigurationObject.groupObjectManagers[strip];
                    connectedConfigurationObject.groupObjectManagers = connectedConfigurationObject.groupObjectManagers.RemoveAt<BaseObjectGroupManager>(strip);
                    Destroy(temp.gameObject);
                }
            }
            else
            {
                List<StripObjectGroupManager> strips = new List<StripObjectGroupManager>();
                strips.AddRange(connectedConfigurationObject.groupObjectManagers.Cast<StripObjectGroupManager>());
                //Add strip objects
                for (int strip = connectedConfigurationObject.groupObjectManagers.Length; strip < lengthOfReels; strip++)
                {
                    strips.Add(GenerateStripObject(strip));
                }
                connectedConfigurationObject.groupObjectManagers = strips.ToArray();
            }
        }

        private static void EnsureConnectedConfigurationObjectStripManagersSet(ref BaseConfigurationObjectManager connectedConfigurationObject)
        {
            if (connectedConfigurationObject.groupObjectManagers?.Length > 0)
            {
                for (int manager = 0; manager < connectedConfigurationObject.groupObjectManagers.Length; manager++)
                {
                    if (connectedConfigurationObject.groupObjectManagers[manager] == null)
                    {
                        connectedConfigurationObject.groupObjectManagers = connectedConfigurationObject.gameObject.GetComponentsInChildren<BaseObjectGroupManager>();
                    }
                }
            }
            else
            {
                connectedConfigurationObject.groupObjectManagers = connectedConfigurationObject.gameObject.GetComponentsInChildren<BaseObjectGroupManager>();
            }
        }

        StripObjectGroupManager GenerateStripObject(int columnNumber)
        {
            Type[] reelComponents = new Type[1];
            reelComponents[0] = typeof(StripObjectGroupManager);
            StripObjectGroupManager output_reelstrip_manager = StaticUtilities.CreateGameobject<StripObjectGroupManager>(reelComponents, "Strip_" + columnNumber, connectedConfigurationObject.transform);
            return output_reelstrip_manager;
        }

        /// <summary>
        /// Generates a new matrix object child with reelstrips configured
        /// </summary>
        /// <returns>matrix reference for connected matrix</returns>
        private void GenerateConfigurationObject<T>(ref BaseConfigurationObjectManager connectedConfigurationObject)
        {
            if (connectedConfigurationObject == null)
            {
                Type[] MatrixComponents = new Type[1];
                MatrixComponents[0] = typeof(T);
                GameObject gameObject_to_return = new GameObject("ConfigurationObject", MatrixComponents);
                gameObject_to_return.transform.tag = "ConfigurationObject";
                gameObject_to_return.transform.parent = transform;
                connectedConfigurationObject = gameObject_to_return.GetComponent<T>() as BaseConfigurationObjectManager;
            }
        }

        //**************************

        internal void ConnectMatrixToChild()
        {
            //Connect Matrix Child
            connectedConfigurationObject = GetComponentInChildren<StripConfigurationObject>();
        }

        internal void UpdateSpinParameters()
        {
            Debug.LogException(new Exception("Not yet Implemented - apply all spin parameter on each reel"));
            //connected_matrix.SetSpinParametersTo(spin_parameters);
        }

        internal void LoadConfigurationSelected()
        {
            configurationObjectLoadedName = configurationGeneratorSettings.configurationName;
            connectedConfigurationObject.SetConfigurationSettings(configurationGeneratorSettings);
            //Ensure All Objects match Configuration
            CreateStripConfiguration();
        }
        //******************
    }
}