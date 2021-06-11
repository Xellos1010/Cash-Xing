//  @ Project : Slot Engine
//  @ Author : Evan McCall
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BoomSports.Prototype.Managers;

namespace BoomSports.Prototype
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StripConfigurationObject))]
    class MatrixEditor : BoomSportsEditor
    {
        StripConfigurationObject myTarget;
        SerializedProperty state;
        SerializedProperty reel_spin_delay_ms;
        SerializedProperty ending_symbols;
        public void OnEnable()
        {
            myTarget = (StripConfigurationObject)target;
            reel_spin_delay_ms = serializedObject.FindProperty("reel_spin_delay_ms");
            ending_symbols = serializedObject.FindProperty("ending_symbols");
        }
        public override void OnInspectorGUI()
        {
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Matrix Properties");

            EditorGUILayout.EnumPopup(StaticStateManager.enCurrentState);
            BoomEditorUtilities.DrawUILine(Color.white);
            EditorGUILayout.LabelField("Matrix Controls");
            if (GUILayout.Button("Initialize Index in Path for slot objects"))
            {
                myTarget.SetGroupManagersObjectsIndexOnPathToCurrentPositionInPath();
            }
            if (GUILayout.Button("Set Object Group Managers to Children"))
            {
                myTarget.groupObjectManagers = myTarget.transform.GetComponentsInChildren<BaseObjectGroupManager>(true);
            }
            if (GUILayout.Button("Set Slots Display Sequence to current"))
            {
                myTarget.SetSlotsDisplaySequenceToCurrentSequence();
            }
            if (GUILayout.Button("Set Slots Display to current sequence"))
            {
                myTarget.SetSlotsDisplayToCurrentSequence();
            }
            if (GUILayout.Button("Set Slot Render Graphics to current presentation ID"))
            {
                myTarget.SetCurrentSymbolDisplayedToCurrentPresentationID();
            }
            if (GUILayout.Button("Set current presentation ID to current Slot Render Graphics "))
            {
                myTarget.SetPresentationIDToCurrentSymbolDisplayed();
            }
            if (GUILayout.Button("Create Empty Animation Container"))
            {
                myTarget.CreateEmptyAnimationContainer();
            }
            if (GUILayout.Button("Generate Slot Prefab Objects"))
            {
                myTarget.GenerateSlotPrefabs();
            }
            if (GUILayout.Button("Re-Generate Slot Objects"))
            {
                myTarget.ReGenerateSlotPrefabs();
            }
            if (GUILayout.Button("Set Generate Slot Objects to strip children"))
            {
                myTarget.SetSlotReferences();
            }
            if (GUILayout.Button("Set Slot Container Animator sub states"))
            {
                myTarget.SetSubStatesAllSlotAnimatorStateMachines();
            }
            if (GUILayout.Button("Set Slot Container Animator Sync States"))
            {
                myTarget.SetAllSlotAnimatorSyncStates();
            }
            if (GUILayout.Button("Clear Slot Container Animator sub states"))
            {
                myTarget.ClearSubStatesAllSlotAnimatorStateMachines();
            }
            if (GUILayout.Button("Set Managers State Machine Sub States"))
            {
                myTarget.SetManagerStateMachineSubStates();
            }
            if (GUILayout.Button("Set Reel Numbers"))
            {
                myTarget.SetStripInfoStruct();
            }
            if(GUILayout.Button("Set Weights from symbols data"))
            {
                myTarget.SetSymbolWeightsByState();
                //myTarget.EnsureWeightsAreCorrect();
            }
            base.OnInspectorGUI();
        }
    }

#endif
    public class StripConfigurationObject : BaseConfigurationObjectManager
    {
        //Hacks for InstaSpin
        //public AnimatorOverrideController[] characterTier;
        //public Animator character;
        //public AnimatorOverrideController[] multiplierTier;
        //public Animator multiplierChar;

        public Animator background;
        public AnimatorOverrideController[] backgroundACO;
        public TMPro.TextMeshPro freespinText;

        public Animator rackingRollupAnimator;
        public TMPro.TextMeshPro rackingRollupText;
        
        private WinningPayline current_payline_displayed;

        /// <summary>
        /// Controls how many slots to include in reel strip generated ob
        /// </summary>
        [SerializeField]
        internal int slotsPerStripLoop = 50;

        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[][] positions_in_path_v3_global
        {
            get
            {
                if (groupObjectManagers != null)
                    if (groupObjectManagers.Length > 0)
                    {
                        List<Vector3[]> temp = new List<Vector3[]>();
                        StripObjectGroupManager temp2;
                        for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                        {
                            temp2 = groupObjectManagers[reel] as StripObjectGroupManager;
                            temp.Add(temp2.localPositionsInStrip);
                        }
                        return temp.ToArray();
                    }
                Debug.Log("No local positions available from reelStrip managers - Check ConfigurationObject");
                return null;
            }
        }
        /// <summary>
        /// Holds the reference for the slots position in path from entering to exiting reel area
        /// </summary>
        [SerializeField]
        internal Vector3[][] positions_in_path_v3_local
        {
            get
            {
                if (groupObjectManagers != null)
                    if (groupObjectManagers.Length > 0)
                    {
                        StripObjectGroupManager temp2;
                        List<Vector3[]> temp = new List<Vector3[]>();
                        for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                        {
                            temp2 = groupObjectManagers[reel] as StripObjectGroupManager;
                            temp.Add(temp2.localPositionsInStrip);
                        }
                        return temp.ToArray();
                    }
                Debug.Log("No local positions available from reelStrip managers - Check ConfigurationObject");
                return null;
            }
        }

        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payline"></param>
        /// <param name="out_positions"></param>
        /// <param name="fullLineWin">Used to determine if full line win was used</param>
        internal void ReturnPositionsBasedOnPayline(ref Payline payline, out List<Vector3> out_positions, bool fullLineWin = true)
        {
            out_positions = new List<Vector3>();
            int payline_positions_set = 0;
            bool rootNodeHit = false;
            Vector3 payline_posiiton_on_reel;
            //Debug.Log($"returning Positions based on payline configuration = {payline.PrintConfiguration()}");

            for (int strip = payline.left_right ? 0 : groupObjectManagers.Length - 1;
                payline.left_right ? strip < groupObjectManagers.Length : strip >= 0;
                strip += payline.left_right ? 1 : -1)
            {
                if (!fullLineWin)
                {
                    //need to see if root node is on reel - if not then next reel
                    if (!rootNodeHit)
                    {
                        if (strip == payline.rootNode.column)
                        {
                            Debug.Log($"Strip = {strip} payline.rootNode.column = {payline.rootNode.column}");
                            rootNodeHit = true;
                        }
                    }
                    if (rootNodeHit)
                    {
                        try
                        {
                            if (payline_positions_set < payline.configuration.payline.Length)
                            {
                                payline_posiiton_on_reel = ReturnPositionOnStripForPayline(ref groupObjectManagers[strip], payline.configuration.payline[payline_positions_set]);
                                payline_positions_set += 1;
                                out_positions.Add(payline_posiiton_on_reel);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(e.Message);
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (payline_positions_set < payline.configuration.payline.Length)
                        {
                            payline_posiiton_on_reel = ReturnPositionOnStripForPayline(ref groupObjectManagers[strip], payline.configuration.payline[payline_positions_set]);
                            payline_positions_set += 1;
                            out_positions.Add(payline_posiiton_on_reel);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.Message);
                    }
                }
            }
            //Debug.Log($"Out Positions = {PrintVectorList(out_positions)}");
        }

        private string PrintVectorList(List<Vector3> out_positions)
        {
            string output = "";

            for (int i = 0; i < out_positions.Count; i++)
            {
                output += "|" + out_positions[i].ToString();
            }
            return output;
        }

        internal async Task PlayFeatureAnimation(List<SuffixTreeNodeInfo> overlaySymbols)
        {
            //this.multiplierChar.SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), true);
            Debug.Log("Playing Feature Animation");
            List<Animator> symbolAnimators = new List<Animator>();
            for (int i = 0; i < overlaySymbols.Count; i++)
            {
                symbolAnimators.Add(SetAnimatorFeatureTriggerAndReturn(overlaySymbols[i]));
            }
            //Play the Feature Animation thru
            bool lock_function = true;
            while (lock_function)
            {
                for (int i = 0; i < symbolAnimators.Count; i++)
                {
                    if (isAnimatorThruState("Feature_Outro", symbolAnimators[i]))
                    {
                        if (i == symbolAnimators.Count - 1)
                        {
                            lock_function = false;
                        }
                    }
                    else
                    {
                        await Task.Delay(300);
                    }
                }
            }
            ////Lerp to Multiplier Bank
            //for (int i = 0; i < overlaySymbols.Count; i++)
            //{
            //    await LerpToMeFinished(symbolAnimators[i].transform);
            //    _managers.machine_info_manager.SetMultiplierTo(_managers.machine_info_manager.machineInfoScriptableObject.multiplier + 1);
            //}

            //await isAnimatorThruStateAndAtPauseState(this.multiplierChar, "Feature_Outro");
            //this.multiplierChar.SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), false);
            for (int i = 0; i < symbolAnimators.Count; i++)
            {
                symbolAnimators[i].SetBool(supportedAnimatorBools.FeatureTrigger.ToString(), false);
            }
        }

        public bool isLerping = false;
        private async Task LerpToMeFinished(Transform transform)
        {
            isLerping = true;
            managers.multiplierLerpToMe.lerpComplete += MultiplierLerpToMe_lerpComplete;
            managers.multiplierLerpToMe.AddLerpToMeObject(transform);
            while (isLerping)
            {
                await Task.Delay(100);
            }
        }
        private void MultiplierLerpToMe_lerpComplete()
        {
            Debug.Log("isLerping = False");
            isLerping = false;
            managers.multiplierLerpToMe.lerpComplete -= MultiplierLerpToMe_lerpComplete;
        }
        internal override async Task WaitForSymbolToResolveState(string state)
        {
            if (current_payline_displayed != null)
            {
                List<BaseObjectManager> winning_slots, losing_slots;
                ReturnWinLoseSlots(current_payline_displayed, managers.winningObjectsManager.linePositions, out winning_slots, out losing_slots, ref groupObjectManagers);
                bool is_all_animations_resolve_intro = false;
                while (!is_all_animations_resolve_intro)
                {
                    for (int slot = 0; slot < winning_slots.Count; slot++)
                    {
                        if (!winning_slots[slot].isSymbolAnimationFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                        if (slot == winning_slots.Count - 1)
                            is_all_animations_resolve_intro = true;
                    }
                }
                current_payline_displayed = null;
                winning_slots = new List<BaseObjectManager>();
                losing_slots = new List<BaseObjectManager>();
            }
        }

        internal Features[] GetSymbolFeatures(int symbol)
        {
            return managers.evaluationManager.GetSymbolFeatures(symbolDataScriptableObject.symbols[symbol]);
        }

        private Vector3 ReturnPositionOnStripForPayline(ref BaseObjectGroupManager group, int indexInGroup)
        {
            StripObjectGroupManager temp = group as StripObjectGroupManager;
            //Debug.Log($"Transforming Local Position temp.localPositionsInStrip[{indexInGroup}]{temp.localPositionsInStrip[indexInGroup]} to world space {group.transform.TransformPoint(temp.localPositionsInStrip[indexInGroup] + (Vector3.back * 10))}");
            return group.transform.TransformPoint(temp.localPositionsInStrip[indexInGroup] + (Vector3.back * 10));
        }

        internal IEnumerator InitializeSymbolsForWinConfigurationDisplay()
        {
            //set slots animators bool
            SetSlotsAnimatorBoolTo(supportedAnimatorBools.LoopPaylineWins, false);
            yield return 0;
        }
        [Serializable]
        public struct BridgeAnimatorTriggerSignalerContainer
        {
            [SerializeField]
            public BridgeAnimatorTriggerSignaler bridgeAnimatorTriggerSignaler;
            [SerializeField]
            public int columnTarget;
        }

        //Build Bridge Animator Reference - Hack Cash Crossing
        public BridgeAnimatorTriggerSignalerContainer leftBridgeAnimatorTriggerSignaler;
        public BridgeAnimatorTriggerSignalerContainer centerBridgeAnimatorTriggerSignaler;
        public BridgeAnimatorTriggerSignalerContainer rightBridgeAnimatorTriggerSignaler;
        internal Task SetSymbolsForWinConfigurationDisplay(WinningPayline winning_payline, List<Vector3> linePositions)
        {
            //Set Bridge Animator in outer reel or center reel on column win
            //Debug.Log(String.Format("Showing Winning Payline {0} with winning symbols {1}",String.Join(" ", winning_payline.payline.payline_configuration.ToString()), String.Join(" ",winning_payline.winning_symbols)));
            //Get Winning Slots and loosing slots
            current_payline_displayed = winning_payline;
            ReturnWinLoseSlots(winning_payline, linePositions,out winning_slots, out losing_slots, ref groupObjectManagers);
            //Initialize and activate bridge Animators - Hook into Cycle Changed as well and register to event
            InitializeBridgeAnimatorsAndSetWinningOn(winning_payline);

            SetSlotsToResolveWinLose(ref winning_slots, true);
            SetSlotsToResolveWinLose(ref losing_slots, false);

            return Task.CompletedTask;
        }
        public List<SetTriggerForBridgeByNodeDataContainer> dataContainerBridgeCurrentlyActiveCyclePayline
        {
            get
            {
                if (_dataContainerBridgeCurrentlyActiveCyclePayline == null)
                    _dataContainerBridgeCurrentlyActiveCyclePayline = new List<SetTriggerForBridgeByNodeDataContainer>();
                return _dataContainerBridgeCurrentlyActiveCyclePayline;
            }
        }
        public List<SetTriggerForBridgeByNodeDataContainer> _dataContainerBridgeCurrentlyActiveCyclePayline;
        private void InitializeBridgeAnimatorsAndSetWinningOn(WinningPayline winning_payline)
        {
            int[] targetBridgeColumn = new int[3]
            {
                leftBridgeAnimatorTriggerSignaler.columnTarget,
                centerBridgeAnimatorTriggerSignaler.columnTarget,
                rightBridgeAnimatorTriggerSignaler.columnTarget
            };
            SuffixTreeNodeInfo[] bridgeNodesToActivateColumns = winning_payline.ContainsNodeFromColumns(targetBridgeColumn);
            Debug.Log($"WinningNodes {winning_payline.PrintWinningNodes()} bridgeNodesToActivate = {bridgeNodesToActivateColumns.Length}");
            if (bridgeNodesToActivateColumns.Length > 0)
            {
                //Debug.Log($"Getting target bridge for node = }");
                SetTriggerForBridgeByNodeDataContainer dataContainer;
                BridgeAnimatorTriggerSignalerContainer targetBridge;
                //Set bridge activators on columns at row to active
                for (int i = 0; i < bridgeNodesToActivateColumns.Length; i++)
                {
                    Debug.Log($"Getting target bridge for node = {bridgeNodesToActivateColumns[i].Print()}");
                    targetBridge = ReturnBridgeSignalerFromNode(bridgeNodesToActivateColumns[i]);
                    Debug.Log($"targetBridge = {targetBridge.bridgeAnimatorTriggerSignaler.gameObject.name}");
                    dataContainer = new SetTriggerForBridgeByNodeDataContainer(bridgeNodesToActivateColumns[i],BaseConfigurationObjectManager.instance.configurationSettings.displayZones, targetBridge);
                    ActivateBridgeAnimatorAtIndex(dataContainer);
                    dataContainerBridgeCurrentlyActiveCyclePayline.Add(dataContainer);
                }
                paylineCycleStateUpdated += StripConfigurationObject_paylineCycleStateUpdated;
            }
        }

        private void StripConfigurationObject_paylineCycleStateUpdated(PaylineCycleStates newState)
        {
            Debug.Log($"{newState.ToString()} new state recieved");
            if(newState == PaylineCycleStates.hide)
            {
                Debug.Log($"{newState.ToString()} was activated - initializing bridge animators");
                for (int i = dataContainerBridgeCurrentlyActiveCyclePayline.Count - 1; i >= 0; i--)
                {
                    Debug.Log($"DeActivateBridgeAnimator(dataContainerBridgeCurrentlyActiveCyclePayline[{i}] = {dataContainerBridgeCurrentlyActiveCyclePayline[i].targetBridge.bridgeAnimatorTriggerSignaler.gameObject.name}");
                    DeActivateBridgeAnimator(dataContainerBridgeCurrentlyActiveCyclePayline[i]);
                    dataContainerBridgeCurrentlyActiveCyclePayline.RemoveAt(i);
                }
            }
            paylineCycleStateUpdated -= StripConfigurationObject_paylineCycleStateUpdated;
        }

        private BridgeAnimatorTriggerSignalerContainer ReturnBridgeSignalerFromNode(SuffixTreeNodeInfo suffixTreeNodeInfo)
        {
            Debug.Log($"Returning Target Bridge for {suffixTreeNodeInfo.Print()} leftBridgeAnimatorTriggerSignaler.columnTarget = {leftBridgeAnimatorTriggerSignaler.columnTarget} centerBridgeAnimatorTriggerSignaler.columnTarget = {centerBridgeAnimatorTriggerSignaler.columnTarget} rightBridgeAnimatorTriggerSignaler.columnTarget = {rightBridgeAnimatorTriggerSignaler.columnTarget}");

            //Hack Cash Crossing - Activate Bridge Animators
            //Need to - padding before matrix of strip from row due to padding display zones start symbol used in evaluation manager as inactive payzone
            //Debug.Log($"suffixTreeNodeInfo.column == leftBridgeAnimatorTriggerSignaler.columnTarget {suffixTreeNodeInfo.column == leftBridgeAnimatorTriggerSignaler.columnTarget}");
            //Debug.Log($"suffixTreeNodeInfo.column == centerBridgeAnimatorTriggerSignaler.columnTarget {suffixTreeNodeInfo.column == centerBridgeAnimatorTriggerSignaler.columnTarget}");
            //Debug.Log($"suffixTreeNodeInfo.column == rightBridgeAnimatorTriggerSignaler.columnTarget {suffixTreeNodeInfo.column == rightBridgeAnimatorTriggerSignaler.columnTarget}");

            if (suffixTreeNodeInfo.column == leftBridgeAnimatorTriggerSignaler.columnTarget)
            {
                return leftBridgeAnimatorTriggerSignaler;
            }
            else if (suffixTreeNodeInfo.column == centerBridgeAnimatorTriggerSignaler.columnTarget)
            {
                return centerBridgeAnimatorTriggerSignaler;
            }
            else if (suffixTreeNodeInfo.column == rightBridgeAnimatorTriggerSignaler.columnTarget)
            {
                return rightBridgeAnimatorTriggerSignaler;
            }
            Debug.Log($"Node {suffixTreeNodeInfo.Print()} has no associated bridge - defaulting return to null");
            return new BridgeAnimatorTriggerSignalerContainer();
        }

        internal void SetPresentingBridgeAnimatorsOff(WinningPayline winning_payline)
        {
            SuffixTreeNodeInfo[] bridgeNodesToActivateColumns = winning_payline.ContainsNodeFromColumns(new int[3] { 0, 4, 7 });
            Debug.Log($"bridgeNodesToActivate = {bridgeNodesToActivateColumns.Length}");
            if (bridgeNodesToActivateColumns.Length > 0)
            {
                SetTriggerForBridgeByNodeDataContainer dataContainer;
                BridgeAnimatorTriggerSignalerContainer targetBridge;
                //Set bridge activators on columns at row to active
                for (int i = 0; i < bridgeNodesToActivateColumns.Length; i++)
                {
                    targetBridge = ReturnBridgeSignalerFromNode(bridgeNodesToActivateColumns[i]);
                    dataContainer = new SetTriggerForBridgeByNodeDataContainer(bridgeNodesToActivateColumns[i], BaseConfigurationObjectManager.instance.configurationSettings.displayZones, targetBridge);
                    DeActivateBridgeAnimator(dataContainer);
                }
            }
        }
        [Serializable]
        public struct SetTriggerForBridgeByNodeDataContainer
        {
            [SerializeField]
            public BridgeAnimatorTriggerSignalerContainer targetBridge;
            [SerializeField]
            public SuffixTreeNodeInfo suffixTreeNodeInfo;
            /// <summary>
            /// Used to get padding information
            /// </summary>
            [SerializeField]
            public ConfigurationDisplayZonesStruct[] displayZones;
            /// <summary>
            /// Initialize Container and set bridge signaller reference later when logic parsed for which bridge to target
            /// </summary>
            /// <param name="suffixTreeNodeInfo"></param>
            /// <param name="displayZones"></param>
            public SetTriggerForBridgeByNodeDataContainer(SuffixTreeNodeInfo suffixTreeNodeInfo, ConfigurationDisplayZonesStruct[] displayZones, BridgeAnimatorTriggerSignalerContainer targetBridge)
            {
                this.suffixTreeNodeInfo = suffixTreeNodeInfo;
                this.displayZones = displayZones;
                this.targetBridge = targetBridge;
            }

            internal int ReturnTargetAnimatorIndex()
            {
                return suffixTreeNodeInfo.row - displayZones[suffixTreeNodeInfo.column].paddingBefore;
            }
        }
        

        private void ActivateBridgeAnimatorAtIndex(SetTriggerForBridgeByNodeDataContainer dataContainer)
        {
            int bridgeIndex = dataContainer.ReturnTargetAnimatorIndex();
            Debug.Log($"dataContainer.bridgeAnimatorTriggerSignaler == null is {dataContainer.targetBridge.bridgeAnimatorTriggerSignaler == null}");
            Debug.Log($"dataContainer.bridgeAnimatorTriggerSignaler.bridgeAnimators == null is {dataContainer.targetBridge.bridgeAnimatorTriggerSignaler == null}");

            if (bridgeIndex < dataContainer.targetBridge.bridgeAnimatorTriggerSignaler.bridgeAnimators.Length)
            {
                Debug.Log($"dataContainer.{dataContainer.targetBridge.bridgeAnimatorTriggerSignaler.gameObject.name}SetTriggerOnBridgeAnimatorAtIndexTo({bridgeIndex}, supportedAnimatorTriggers.FeatureWin)");
                dataContainer.targetBridge.bridgeAnimatorTriggerSignaler.SetTriggerOnBridgeAnimatorAtIndexTo(bridgeIndex, supportedAnimatorTriggers.FeatureWin);
            }
            else
            {
                Debug.LogWarning($"Index {bridgeIndex} is bigger than length of bridge animators");
            }
        }
        private void DeActivateBridgeAnimator(SetTriggerForBridgeByNodeDataContainer dataContainer)
        {

            int bridgeIndex = dataContainer.ReturnTargetAnimatorIndex();
            if (bridgeIndex < dataContainer.targetBridge.bridgeAnimatorTriggerSignaler.bridgeAnimators.Length)
            {
                dataContainer.targetBridge.bridgeAnimatorTriggerSignaler.SetTriggerOnBridgeAnimatorAtIndexTo(bridgeIndex, supportedAnimatorTriggers.FeatureOff);
            }
            else
            {
                Debug.LogWarning($"Index {bridgeIndex} is bigger than length of bridge animators");
            }

        }

        private void ReturnWinLoseSlots(WinningPayline winningPayline, List<Vector3> linePositions, out List<BaseObjectManager> winningSlots, out List<BaseObjectManager> losingSlots, ref BaseObjectGroupManager[] objectGroupManagers)
        {
            //Wait until root node for payline is being evaluated - get slots in decending order - compare slot.transform.localposition != linePositions[strip] - get winning slot based on transform value held in linePositions
            //Debug.Log($"Returning Win Lose Slots for winning payline {winningPayline.payline.PrintConfiguration()} left_right = {winningPayline.payline.left_right}");
            winningSlots = new List<BaseObjectManager>();
            losingSlots = new List<BaseObjectManager>();
            //Iterate over each reel and get the winning slot

            int winningSymbolsAdded = 0;
            bool winningSlotSet = false;
            bool isReelOnRootNode = false;
            List<BaseObjectManager> slotInStripGroupDecending;
            //used to increment the line position once root node is reached
            int linePositionIndex = 0;
            //Used to get the slots decending and local positions on strips
            StripObjectGroupManager temp;
            //Used to compare the slot in strip to winning position in strip if 1 is available;
            Vector3 winningLinePositionToCompareCurrent = Vector3.zero;
            //Left right should be determined in order objectGroupManagers come in
            for (int strip = winningPayline.payline.left_right ? 0 : objectGroupManagers.Length - 1;
                winningPayline.payline.left_right ? strip < objectGroupManagers.Length : strip >= 0;
                strip += winningPayline.payline.left_right ? 1 : -1)
            {
                temp = (objectGroupManagers[strip] as StripObjectGroupManager);
                //To account for if payline starts outer or inner in caash crossing
                if (!isReelOnRootNode)
                {
                    if (strip == winningPayline.payline.rootNode.column)
                    {
                        //Debug.Log($"strip {strip} contains root node!");
                        isReelOnRootNode = true;
                    }
                }
                if (isReelOnRootNode)
                {

                    //Return Slots decending and get the local position 
                    slotInStripGroupDecending = temp.GetSlotsDecending();
                    //Debug.Log($"Strip = {objectGroupManagers[strip].gameObject.name} Slots decending = {PrintDecendingSlots(slotInStripGroupDecending)}");
                    //Accounts for any padding positions before the strip displays symbols - Efficiency
                    int firstDisplaySlot = objectGroupManagers[strip].configurationGroupDisplayZones.paddingBefore;
                    //Check we have winning symbols left to add and set new position in strip that is winning position
                    if (winningSymbolsAdded < winningPayline.winningNodes.Length)
                    {
                        //Debug.Log($"winningPayline.winningNodes.Count = {winningPayline.winningNodes.Length} strip < Length = {strip < winningPayline.winningNodes.Length}");
                        //Debug.Log($"winningLinePositionToCompareCurrent = temp.localPositionsInStrip[winningPayline.winningNodes[{winningSymbolsAdded}].nodeInfo.row {winningPayline.winningNodes[winningSymbolsAdded].nodeInfo.row} = {temp.localPositionsInStrip[winningPayline.winningNodes[winningSymbolsAdded].nodeInfo.row]}");
                        winningLinePositionToCompareCurrent = temp.localPositionsInStrip[winningPayline.winningNodes[winningSymbolsAdded].nodeInfo.row];
                    }
                    for (int slot = firstDisplaySlot; slot < slotInStripGroupDecending.Count; slot++)
                    {
                        if (winningSymbolsAdded < winningPayline.payline.configuration.payline.Length && !winningSlotSet)
                        {
                            if (linePositionIndex < linePositions.Count) //TODO remove reference replace with more stable reference
                            {
                                //Debug.Log($"Checking if slot {slotInStripGroupDecending[slot].gameObject.name} is a winning slot based on transform: {slotInStripGroupDecending[slot].transform.localPosition.sqrMagnitude} == {winningLinePositionToCompareCurrent.sqrMagnitude}] {slotInStripGroupDecending[slot].transform.localPosition.sqrMagnitude == winningLinePositionToCompareCurrent.sqrMagnitude}");
                                if (slotInStripGroupDecending[slot].transform.localPosition.sqrMagnitude == winningLinePositionToCompareCurrent.sqrMagnitude)
                                {
                                    //Debug.Log($"Adding Winning Slot {slotInStripGroupDecending[slot].gameObject.name} on strip {strip} slot{slot}");
                                    winningSlots.Add(slotInStripGroupDecending[slot]);
                                    winningSymbolsAdded += 1;
                                    winningSlotSet = true;
                                }
                                else
                                {
                                    losingSlots.Add(slotInStripGroupDecending[slot]);
                                }
                            }
                            else
                            {
                                losingSlots.Add(slotInStripGroupDecending[slot]);
                            }
                        }
                        else
                        {
                            losingSlots.Add(slotInStripGroupDecending[slot]);
                        }
                    }
                    //Increment strip line position after all slots are processed in strip
                    linePositionIndex += 1;
                }

                winningSlotSet = false;
            }
        }

        private object PrintDecendingSlots(List<BaseObjectManager> slots_decending_in_reel)
        {
            string output = "";
            for (int i = 0; i < slots_decending_in_reel.Count; i++)
            {
                output += "|" + slots_decending_in_reel[i].gameObject.name;
            }
            return output;
        }

        internal void SlamLoopingPaylines()
        {
            //Pause and interrupt racking before continue
            managers.rackingManager.PauseRackingOnInterrupt();
            StaticStateManager.SetStateTo(States.Resolve_Outro);
        }

        internal void SetSystemToPresentWin()
        {
            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true);
        }

        internal void CycleWinningPaylinesMode()
        {
            managers.winningObjectsManager.PlayCycleWins();
        }
        /// <summary>
        /// Set Free spin information for all Animators
        /// </summary>
        /// <param name="onOff"></param>
        internal void ToggleFreeSpinActive(bool onOff)
        {
            Debug.Log("Bonus Active = " + onOff);
            StaticStateManager.bonusGameTriggered = onOff;
            SetAllAnimatorsBoolTo(supportedAnimatorBools.BonusActive, onOff);
            if (!onOff)
                managers.machineInfoManager.SetMultiplierTo(0);
        }

        internal void SetAllAnimatorsBoolTo(supportedAnimatorBools bool_to_set, bool value)
        {
            managers.animatorStateMachineMaster.SetBoolAllStateMachines(bool_to_set, value);
            SetSlotsAnimatorBoolTo(bool_to_set, value);
        }

        private void PrepareSlotMachineToSpin()
        {
            Debug.Log("Preparing Slot Machine for Spin");
        }

        internal void SetSymbolsToDisplayOnConfigurationObjectTo(DisplayConfigurationContainer currentConfiguration)
        {
            Debug.Log($"configurationGroupManagers.Length = {groupObjectManagers.Length}");
            Debug.Log($"currentConfiguration.configuration?.Length = {currentConfiguration.configuration?.Length}");
            for (int groupManagerIndex = 0; groupManagerIndex < groupObjectManagers.Length; groupManagerIndex++)
            {
                Debug.Log($"configurationGroupManagers[{groupManagerIndex}].SetSymbolEndSymbolsAndDisplay({String.Join("|", currentConfiguration.configuration[groupManagerIndex].GetAllDisplaySymbolsIndex())})");
                groupObjectManagers[groupManagerIndex].SetSymbolEndSymbolsAndDisplay(currentConfiguration.configuration[groupManagerIndex]);
            }
        }

        private void SetSlotsAnimatorBoolTo(supportedAnimatorBools bool_name, bool v)
        {
            //Debug.Log(String.Format("Setting Slot Animator {0} to {1}",bool_name.ToString(),v));
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                {
                    groupObjectManagers[reel].objectsInGroup[slot].SetBoolStateMachines(bool_name, v);
                }
            }
        }

        private List<StripObjectGroupManager> FindReelstripManagers()
        {
            //Initialize reelstrips managers
            List<StripObjectGroupManager> reelstrip_managers = new List<StripObjectGroupManager>();
            if (groupObjectManagers == null)
            {
                StripObjectGroupManager[] reelstrip_managers_intiialzied = transform.GetComponentsInChildren<StripObjectGroupManager>(true);
                if (reelstrip_managers_intiialzied.Length < 1)
                    groupObjectManagers = new StripObjectGroupManager[0];
                else
                    groupObjectManagers = reelstrip_managers_intiialzied;
            }
            //Load any reelstrip managers that are already initialized
            reelstrip_managers.AddRange(groupObjectManagers.Cast<StripObjectGroupManager>());
            return reelstrip_managers;
        }

        internal void GenerateReelStripsToLoop(ref GroupSpinInformationStruct[] reelConfiguration)
        {
            //Generate reel strips based on number of reels and symbols per reel - Insert ending symbol configuration and hold reference for array range
            GenerateReelStripsFor(groupObjectManagers.Cast<StripObjectGroupManager>().ToArray(), ref reelConfiguration, slotsPerStripLoop);
        }

        private void GenerateReelStripsFor(StripObjectGroupManager[] reelStripManagers, ref GroupSpinInformationStruct[] spinConfiguration, int slots_per_strip_onSpinLoop)
        {
            EndConfigurationManager temp;
            temp = managers.endConfigurationManager;
            //Loop over each reelstrip and assign reel strip
            for (int i = 0; i < reelStripManagers.Length; i++)
            {
                if (spinConfiguration[i].spinIdleSymbolSequence?.Length != slots_per_strip_onSpinLoop)
                {
                    //Generates reelstrip based on weights
                    spinConfiguration[i].spinIdleSymbolSequence = StripManager.GenerateReelStripStatic(StaticStateManager.enCurrentMode, slots_per_strip_onSpinLoop, ref temp);
                }
                //Assign reelstrip to reel
                reelStripManagers[i].groupInfo.SetSpinConfigurationTo(spinConfiguration[i]);
            }
        }


        internal void InitializeAllAnimators()
        {
            managers.animatorStateMachineMaster.InitializeAnimator();
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                {
                    groupObjectManagers[reel].objectsInGroup[slot].animatorStateMachine.InitializeAnimator();
                }
            }
        }

        internal void ReturnSymbolPositionsOnPayline(ref Payline payline, out List<Vector3> linePositions)
        {
            linePositions = new List<Vector3>();
            //Return List Slots In Order 0 -> -
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                //Get current slot order based on slot transform compared to positions in path.
                List<BaseObjectManager> slots_decending_in_reel = groupObjectManagers[reel].GetSlotsDecending();
                //Cache the position of the slot that we need from this reel
                linePositions.Add(ReturnSlotPositionOnPayline(payline.configuration.payline[reel], ref slots_decending_in_reel, ref groupObjectManagers[reel]));
            }
        }

        private Vector3 ReturnSlotPositionOnPayline(int payline_slot, ref List<BaseObjectManager> slots_decending_in_reel, ref BaseObjectGroupManager reelStripManager)
        {
            //Calculate the reel display area - Take Display Slots and start is
            BaseObjectManager[] display_slots = ReturnDisplaySlots(ref slots_decending_in_reel, ref reelStripManager);
            return display_slots[payline_slot].transform.position;
        }

        private BaseObjectManager[] ReturnDisplaySlots(ref List<BaseObjectManager> slots_decending_in_reel, ref BaseObjectGroupManager baseGroupManager)
        {
            //throw new Exception("Todo refactor");
            return slots_decending_in_reel.GetRange(baseGroupManager.configurationGroupDisplayZones.paddingBefore, baseGroupManager.configurationGroupDisplayZones.displayZones.Length - baseGroupManager.configurationGroupDisplayZones.paddingBefore).ToArray();
        }

        internal void PlayerHasBet(float amount)
        {
            //Set the UI to remove player wallet amount and update the player information to remove amount
            OffetPlayerWalletBy(-amount);
        }

        internal void OffetPlayerWalletBy(double amount)
        {
            managers.machineInfoManager.OffsetPlayerAmountBy(amount);
        }

        private string PrintSpinSymbols(ref GroupSpinInformationStruct[] stripInitial)
        {
            string output = "";
            for (int strip = 0; strip < stripInitial.Length; strip++)
            {
                output += ReturnDisplaySymbolsPrint(stripInitial[strip]);
            }
            return output;
        }

        private string ReturnDisplaySymbolsPrint(GroupSpinInformationStruct reelstrip_info)
        {
            return String.Join("|", reelstrip_info.displaySymbolsToLoad);
        }

        void OnEnable()
        {
            StaticStateManager.StateChangedTo += StateManager_StateChangedTo;
            StaticStateManager.featureTransition += StateManager_FeatureTransition;
            StaticStateManager.gameModeSetTo += StateManager_gameModeSetTo;
        }

        private void StateManager_gameModeSetTo(GameModes modeActivated)
        {
            switch (modeActivated)
            {
                case GameModes.baseGame:
                    break;
                case GameModes.overlaySpin:
                    break;
                case GameModes.freeSpin:
                    break;
                default:
                    break;
            }
        }

        private async void StateManager_add_to_multiplierAsync(int multiplier)
        {

        }

        private async Task isAnimatorThruState(Animator animator, string state)
        {
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            bool wait = true;
            while (wait)
            {
                state_info = animator.GetCurrentAnimatorStateInfo(0);

                Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                //Check if time has gone thru
                if (!state_info.IsName(state))
                {
                    wait = false;
                }
                else
                {
                    await Task.Delay(300);
                }
            }
        }
        private async Task isAnimatorsThruState(Animator[] animators, string state)
        {
            //TODO Refactor to set each animator into its own cancelable task that can wait for all until continue - this is to support parallel task management and cancelable tokens
            for (int animator = 0; animator < animators.Length; animator++)
            {
                await isAnimatorThruState(animators[animator], state);
            }
        }
        /// <summary>
        /// Used to receive and execute functions based on feature active or inactive
        /// </summary>
        /// <param name="feature">feature reference</param>
        /// <param name="active_inactive">state</param>
        private void StateManager_FeatureTransition(Features feature, bool active_inactive)
        {
            //Debug.Log(String.Format("Feature Transition for matrix = ",feature.ToString()));
            switch (feature)
            {
                case Features.freespin:
                    ToggleFreeSpinActive(active_inactive);
                    if (active_inactive)
                    {
                        background.runtimeAnimatorController = backgroundACO[0];
                        background.SetBool(supportedAnimatorBools.BonusActive.ToString(), true);
                    }
                    else
                    {
                        background.SetBool(supportedAnimatorBools.BonusActive.ToString(), false);
                    }
                    break;
                case Features.multiplier:
                    ToggleFreeSpinActive(active_inactive);
                    background.runtimeAnimatorController = backgroundACO[1];
                    break;
                default:
                    StaticStateManager.bonusGameTriggered = false;
                    break;
            }
        }

        void OnDisable()
        {
            StaticStateManager.StateChangedTo -= StateManager_StateChangedTo;
            StaticStateManager.featureTransition -= StateManager_FeatureTransition;
            StaticStateManager.gameModeSetTo -= StateManager_gameModeSetTo;
            managers.rackingManager.rackEnd -= Racking_manager_rackEnd;
        }
        /// <summary>
        /// Matrix State Machine
        /// </summary>
        /// <param name="State"></param>
        private async void StateManager_StateChangedTo(States State)
        {
            switch (State)
            {
                case States.Idle_Intro:
                    //Reset the state of all slots to
                    await isAllMainAnimatorsThruState("Idle_Intro");
                    //Fall thru to Idle_Idle State - ATM the animator falls thru Idle_Intro
                    StaticStateManager.SetStateTo(States.Idle_Idle);
                    break;
                case States.Idle_Outro:
                    //Decrease Bet Amount
                    PlayerHasBet(managers.machineInfoManager.machineInfoScriptableObject.bet_amount);
                    await StartAnimatorSpinAndSpinState();
                    break;
                case States.Spin_End:
                    bool resolve_intro = false;
                    Debug.LogWarning($"mode = {StaticStateManager.enCurrentMode} Bonus Game triggered = {StaticStateManager.bonusGameTriggered} FreeSpins Remaining = {managers.machineInfoManager.machineInfoScriptableObject.freespins} Multiplier = {managers.machineInfoManager.machineInfoScriptableObject.multiplier}");

                    //Can Trigger Free Spins without having a win
                    //TODO calculate symbol win with multiplier symbol on first evaluate trailing multiplier
                    if (CheckForWin()) //Check for winning objects and calculate total winnings
                    {
                        //need to fix feature
                        await CycleWinningPaylinesOneShot();
                        //await CheckForOverlayAndPlay();
                        //Bonus Game needs to be set before entering freespins mode and after last spin is accounted for
                        //Cash crossing specific
                        if (!StaticStateManager.bonusGameTriggered) // Base game win with no bonus trigger - trailling multipliers
                        {
                            //Debug.Log("Win in Base Game Triggered");
                            //Calculate Rack Amount and either skip resolve phase or 
                            //Proceed to next state and sync state machine
                            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true); //Cycle winning payline
                            //Display total win amount
                            SetFreespinBannerTextTo(String.Format("{0:C2} Total Won this Spin", GetTotalSpinAmountWon()));
                            if (StaticStateManager.enCurrentMode != GameModes.baseGame)
                                StaticStateManager.SetFeatureActiveTo(Features.freespin, false);
                            //Debug.LogWarning("Setting Resolve Intro true");
                            resolve_intro = true;
                        }
                        else // Will her here
                        {
                            //Rack win to bank and continue to next spin
                            Debug.Log("There is a win and bonus game triggered");
                            //Bank Lerped Already
                            //slot_machine_managers.machine_info_manager.OffsetBankBy(slot_machine_managers.paylines_manager.GetTotalWinAmount());
                            SetFreespinBannerTextTo(String.Format("{0:C2} Total Won this Spin", GetTotalSpinAmountWon()));
                            ToggleTMPFreespin(true);
                            SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, false); // dont rack wins
                            if (managers.machineInfoManager.machineInfoScriptableObject.freespins > 0)
                            {
                                Debug.LogWarning($"Freespins Remaining = {managers.machineInfoManager.machineInfoScriptableObject.freespins}");
                            }
                            else
                            {
                                Debug.LogWarning("Setting Resolve Intro true");
                                resolve_intro = true;
                            }
                        }
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                    }
                    else
                    {
                        // Set Trigger for state machine to SymbolResolve and WinRacking to false
                        if (StaticStateManager.bonusGameTriggered)
                        {
                            Debug.Log(String.Format("Bonus Game no win", managers.machineInfoManager.machineInfoScriptableObject.bank));
                            if (managers.machineInfoManager.machineInfoScriptableObject.freespins <= 0)
                            {
                                Debug.Log("Bonus Game No Win No Freespins- Setting Resolve Intro to True");
                                resolve_intro = true;
                            }
                        }
                        else if (!StaticStateManager.bonusGameTriggered && managers.machineInfoManager.machineInfoScriptableObject.bank > 0)
                        {
                            Debug.Log(String.Format("Bonus game ended and bank has amount to rack = {0}", managers.machineInfoManager.machineInfoScriptableObject.bank));
                            resolve_intro = true;
                        }
                        else
                        {
                            Debug.Log(String.Format("Base Game no win - return to idle", managers.machineInfoManager.machineInfoScriptableObject.bank));
                        }
                    }
                    await isAllAnimatorsThruStateAndAtPauseState("Spin_Outro");
                    await isAllSlotSubAnimatorsReady("Spin_Outro");

                    //Where we resolve animations and racking rolups
                    if (resolve_intro)
                    {
                        //Debug.Log("Playing Resolve Into in Spin End");
                        SetOverridesBasedOnTiers();
                        await Task.Delay(20);
                        SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, true);
                        SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.SpinResolve, true);
                        //Wait till we play resolve intro animations and switch to resolve intro state
                        await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                        await isAllSlotSubAnimatorsReady("Resolve_Intro");
                        StaticStateManager.SetStateTo(States.Resolve_Intro);
                    }
                    else
                    {
                        Debug.Log("Not Playing Resolve Outro in Spin End");
                        //If the spin has ended and there are no wining paylines or freespins left then disable freespin mode

                        if (managers.machineInfoManager.machineInfoScriptableObject.freespins <= 0 && StaticStateManager.bonusGameTriggered)
                        {
                            Debug.Log($"Freespins = {managers.machineInfoManager.machineInfoScriptableObject.freespins} Setting Freespins Inactive");
                            StaticStateManager.SetFeatureActiveTo(Features.freespin, false);
                        }
                        SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.SpinResolve, true);
                        await isAllSlotSubAnimatorsReady("Idle_Intro");
                        await isAllAnimatorsThruStateAndAtPauseState("Idle_Idle");
                        if (StaticStateManager.bonusGameTriggered)
                            StaticStateManager.SetStateTo(States.bonus_idle_intro);
                        else
                        {
                            StaticStateManager.SetStateTo(States.Idle_Idle);
                        }
                    }
                    break;
                case States.Resolve_Intro:
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Intro");
                    Debug.Log($"Resolve Intro state entered - Bonus Game Triggered = {StaticStateManager.bonusGameTriggered} Toal Win = {GetTotalSpinAmountWon()}");
                    //The amount total even with multiplier
                    double totalSpinAwardeded = GetTotalSpinAmountWon();
                    double totalAwardeded = totalSpinAwardeded + managers.machineInfoManager.machineInfoScriptableObject.bank + totalSpinAwardeded;
                    //If multiplier > 0 then combine to present total
                    await DisplayWinAmount(totalSpinAwardeded, managers.machineInfoManager.machineInfoScriptableObject.bank);
                    //Check to reset freespin state now that everything is calculated
                    if (managers.machineInfoManager.machineInfoScriptableObject.freespins <= 0)
                    {
                        //Debug.Log($"Resolve Intro Freespins = {managers.machineInfoManager.machineInfoScriptableObject.freespins} Setting Freespins Inactive");
                        StaticStateManager.SetFeatureActiveTo(Features.freespin, false);
                    }
                    WinningObject[] winningObjects = managers.evaluationManager.ReturnWinningObjects();
                    if (winningObjects.Length > 0)
                    {
                        if (StaticStateManager.enCurrentMode != GameModes.freeSpin)
                            CycleWinningPaylinesMode();
                        else if (StaticStateManager.enCurrentMode == GameModes.freeSpin && (managers.machineInfoManager.machineInfoScriptableObject.freespins == 0 || managers.machineInfoManager.machineInfoScriptableObject.freespins == 10))
                            CycleWinningPaylinesMode();
                    }
                    break;
                case States.Resolve_Outro:
                    await managers.winningObjectsManager.CancelCycleWins();
                    //TODO Refactor hack
                    if (managers.machineInfoManager.machineInfoScriptableObject.freespins < 1)
                    {
                        SetFreespinBannerTextTo("");
                        freespinText.enabled = false;
                    }
                    else
                    {
                        managers.machineInfoManager.SetFreeSpinsTo(managers.machineInfoManager.machineInfoScriptableObject.freespins);
                    }
                    //Set animator to Resolve_Outro State
                    SetAllAnimatorsBoolTo(supportedAnimatorBools.WinRacking, false);
                    SetAllAnimatorsBoolTo(supportedAnimatorBools.LoopPaylineWins, false);
                    //If going back to base game initialize vars for next bonus trigger
                    if (StaticStateManager.enCurrentMode == GameModes.baseGame)
                    {
                        SetAllAnimatorsBoolTo(supportedAnimatorBools.BonusActive, false);
                        //ensure multiplier set to 0
                        managers.machineInfoManager.ResetMultiplier();
                    }
                    //Wait for animator to play all resolve outro animations
                    await isAllAnimatorsThruStateAndAtPauseState("Resolve_Outro");
                    await isAllSlotAnimatorsReady("Resolve_Outro");
                    //Need to refactor to integrate
                    //await isAnimatorThruStateAndAtPauseState(this.multiplierChar,"Resolve_Outro");
                    SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.ResolveEnd, true);
                    //this.multiplierChar.SetTrigger(supportedAnimatorTriggers.ResolveEnd.ToString());
                    if (!StaticStateManager.bonusGameTriggered)
                    {                        //TODO wait for all animators to go thru idle_intro state
                        StaticStateManager.SetStateTo(States.Idle_Intro);
                    }
                    else
                    {
                        StaticStateManager.SetStateTo(States.bonus_idle_intro);
                    }
                    break;
                case States.bonus_idle_intro:
                    await isAllMainAnimatorsThruState("Idle_Intro");
                    StaticStateManager.SetStateTo(States.bonus_idle_idle);
                    break;
                case States.bonus_idle_outro:
                    ReduceFreeSpinBy(1);
                    await StartAnimatorSpinAndSpinState();
                    break;
            }
        }

        private async Task CheckForOverlayAndPlay()
        {
            if (managers.evaluationManager.overlaySymbols.Count > 0)
            {
                Debug.Log("Overlay in winning Line - Setting to play feature");
                if (managers.machineInfoManager.machineInfoScriptableObject.bank > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9 ||
        managers.winningObjectsManager.GetTotalWinAmount() > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9)
                {
                    Debug.Log("Present Character Big Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 2);
                }
                else if (managers.machineInfoManager.machineInfoScriptableObject.bank > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 5 ||
                    managers.winningObjectsManager.GetTotalWinAmount() > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 5)
                {
                    Debug.Log("Present Character Medium Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 1);

                }
                else
                {
                    Debug.Log("Present Character Small Win");
                    //SetAnimatorOverrideControllerTo(ref this.multiplierChar, ref multiplierTier, 0);
                }
                await PlayFeatureAnimation(managers.evaluationManager.overlaySymbols);
                Debug.Log("All Overlay Animators are finished");
            }
        }

        private async Task CycleWinningPaylinesOneShot()
        {
            Debug.LogWarning("Implement Cycle Winning Paylines Oneshot");
            //TODO cycle paylines from highest value to lowest and triggering feature first
            //await slotMachineManagers.paylines_manager.CyclePaylinesOneShot();
        }

        private void ToggleTMPFreespin(bool v)
        {
            freespinText.enabled = v;
        }

        private double GetTotalSpinAmountWon()
        {
            double output = 0;
            output = managers.winningObjectsManager.GetTotalWinAmount();

            //if (!StateManager.bonusGameTriggered)
            //{
            //    //First offset bank by win then rack
            //    //TODO have offset occur when winning payline is animated to bank
            //}
            //else
            //{
            //    if (slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
            //    {
            //        output = slot_machine_managers.paylines_manager.GetTotalWinAmount() * slot_machine_managers.machine_info_manager.machineInfoScriptableObject.multiplier;
            //    }
            //    else
            //    {
            //        output = slot_machine_managers.paylines_manager.GetTotalWinAmount();
            //    }
            //}
            return output;
        }

        //public Animator plateGraphicAnimatorWinBank;
        private async Task DisplayWinAmount(double spinWinAmount, double bankAmount)
        {
            SetFreespinBannerTextTo(String.Format("{0:C2} {1}", spinWinAmount, " Won This Spin!"));
            ToggleMeshRendererForGameObject(rackingRollupText.gameObject, false);
            //If you are 9X bet or you have a multiplier win - multiplier win will be phased out for own mechanism
            if (spinWinAmount + bankAmount > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9 || managers.machineInfoManager.machineInfoScriptableObject.multiplier > 0)
            {
                //Present and set callback event for bigwin display
                PresentBigWinDisplayAnimator();
                managers.rackingManager.rackEnd += CloseBigWinDisplay;
                //This is a special feature for instaspin - multiplier smash. Need to build in UI based event sequencer
                //if (managers.machine_info_manager.machineInfoScriptableObject.multiplier > 0)
                //{
                //    //await Smash together the Multiplier and Bank text Field.
                //    Animator[] winbankMultiplier = new Animator[1] { plateGraphicAnimatorWinBank };
                //    SetAnimatorsToTriggerFeature(winbankMultiplier, true);
                //    await isAllAnimatorsThruStateAndAtPauseStateTriggerEventAt(winbankMultiplier, States.Resolve_Win_Idle.ToString(), 1f, PresentBigWinDisplayAnimator);
                //    SetAnimatorsToTriggerFeature(winbankMultiplier, false);
                //    spinWinAmount = (bankAmount + spinWinAmount) * managers.machine_info_manager.machineInfoScriptableObject.multiplier;
                //}
            }
            SetRackingtextTo(spinWinAmount);
            ToggleMeshRendererForGameObject(rackingRollupText.gameObject, true);
            managers.machineInfoManager.machineInfoScriptableObject.bank = spinWinAmount;
            //Waits to present amount then racks down
            await Task.Delay(2000);
            managers.rackingManager.rackEnd += Racking_manager_rackEnd;
            amountRacked = spinWinAmount;
            managers.rackingManager.StartRacking();//spinWinAmount); //This is to resolve wins in resolve intro
        }

        private void SetRackingtextTo(double spinWinAmount)
        {
            //Debug.Log(String.Format("{0:C2}", spinWinAmount));
            rackingRollupText.text = String.Format("{0:C2}", spinWinAmount);
        }

        private void ToggleMeshRendererForGameObject(GameObject gameObject, bool v)
        {
            MeshRenderer textRenderer = gameObject.GetComponent<MeshRenderer>();
            textRenderer.enabled = v;
        }

        private void SetFreespinBannerTextTo(string toText)
        {
            //Debug.Log($"Setting Freespin Text To {toText}");
            freespinText.text = toText;
        }

        //Temporary placeholder to hold amount to rack so at end display won amount in freespin bar
        private double amountRacked;
        private void Racking_manager_rackEnd()
        {
            Debug.Log("Matrix Recieved rack end ");
            SetFreespinBannerTextTo(String.Format("{0:C2} {1}", amountRacked, " Total Winnings"));
            ToggleTMPFreespin(true);
            amountRacked = 0;
            managers.rackingManager.rackEnd -= Racking_manager_rackEnd;
        }

        private void PresentBigWinDisplayAnimator()
        {
            rackingRollupAnimator.SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), true);
            rackingRollupAnimator.SetBool(supportedAnimatorBools.SymbolResolve.ToString(), true);
        }

        private void SetAnimatorsToTriggerFeature(Animator[] animators, bool onOff)
        {
            for (int animator = 0; animator < animators.Length; animator++)
            {
                animators[animator].SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), onOff);
                animators[animator].SetBool(supportedAnimatorBools.SymbolResolve.ToString(), onOff);
            }
        }

        private void CloseBigWinDisplay()
        {
            rackingRollupAnimator.SetBool(supportedAnimatorBools.LoopPaylineWins.ToString(), false);
            rackingRollupAnimator.SetBool(supportedAnimatorBools.SymbolResolve.ToString(), false);
            managers.rackingManager.rackEnd -= CloseBigWinDisplay;
        }

        internal async Task isAnimatorThruStateAndAtPauseState(Animator multiplier, string state)
        {
            AnimatorStateInfo state_info;
            bool wait = true;
            while (wait)
            {
                state_info = multiplier.GetCurrentAnimatorStateInfo(0);

                Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                //Check if time has gone thru
                if (state_info.IsName(state) && state_info.normalizedTime >= 1)
                {
                    wait = false;
                }
                else
                {
                    await Task.Delay(300);
                }
            }
        }

        private void SetOverridesBasedOnTiers()
        {
            //Debug.Log(String.Format("slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bank  = {0} slot_machine_managers.machine_info_manager.machineInfoScriptableObject.bet_amount * 9 = {1} slot_machine_managers.paylines_manager.GetTotalWinAmount() = {2}", managers.machineInfoManager.machineInfoScriptableObject.bank, managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9, managers.winningObjectsManager.GetTotalWinAmount()));
            if (managers.machineInfoManager.machineInfoScriptableObject.bank > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9 ||
                managers.winningObjectsManager.GetTotalWinAmount() > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 9)
            {
                //Debug.Log("Present Big Win First");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier,2);
            }
            else if (managers.machineInfoManager.machineInfoScriptableObject.bank > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 5 ||
                managers.winningObjectsManager.GetTotalWinAmount() > managers.machineInfoManager.machineInfoScriptableObject.bet_amount * 5)
            {
                Debug.Log("Presenting Medium win");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier, 1);

            }
            else
            {
                Debug.Log("Presenting Small win");
                //SetAnimatorOverrideControllerTo(ref character, ref characterTier, 0);
            }
        }

        private void SetAnimatorOverrideControllerTo(ref Animator character, ref AnimatorOverrideController[] characterTier, int v)
        {
            Debug.Log("No Character Controllers in this project");
            //character.runtimeAnimatorController = characterTier[v];
        }

        private void PlayAnimationOnAllSlots(string v)
        {
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                {
                    //Temporary fix because Animator decides to sometimes play an animation override and sometimes not
                    groupObjectManagers[reel].objectsInGroup[slot].PlayAnimationOnPresentationSymbol("Resolve_Outro");
                }
            }
        }

        private async Task StartAnimatorSpinAndSpinState()
        {
            SetAllAnimatorsTriggerTo(supportedAnimatorTriggers.StartSpin, true);
            await isAllAnimatorsThruStateAndAtPauseState("Idle_Outro");
            await isAllSlotSubAnimatorsReady("Idle_Outro");
            //Tell the spin manager to start spinning - animator is ready
            managers.spinManager.SetSpinStateTo(SpinStates.spin_start);
        }

        private async Task isAllSlotSubAnimatorsReady(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                {
                    for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!groupObjectManagers[reel].objectsInGroup[slot].isAllAnimatorsFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == groupObjectManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal bool CheckForWin()
        {
            return EvaluationManager.instance.ReturnWinningObjects().Length > 0 ? true : false;
        }
        internal async Task isAllSlotAnimatorsThruState(string state)
        {
            await isAllAnimatorsThruState(GetAllSlotAnimators(), state);
        }

        private Animator[] GetAllSlotAnimators()
        {
            List<Animator> output = new List<Animator>();
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                groupObjectManagers[reel].AddSlotAnimatorsToList(ref output);
            }
            return output.ToArray();
        }

        internal async Task isAllMainAnimatorsThruState(string state)
        {
            await isAllAnimatorsThruState(_managers.animatorStateMachineMaster.animator_state_machines.state_machines_to_sync, state);
        }

        internal async Task isAllAnimatorsThruState(Animator[] animators, string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            AnimatorStateInfo state_info;
            AnimatorStateInfo next_state_info;
            bool wait = true;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);

                    while (wait)
                    {
                        state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);
                        //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
                        //Check if time has gone thru
                        if (!state_info.IsName(state))
                        {
                            wait = false;
                        }
                        else
                        {
                            await Task.Delay(300);
                        }
                    }
                    //Debug.Log("All States Resolved");
                    if (state_machine == animators.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        internal async Task isAllAnimatorsThruStateAndAtPauseState(Animator[] animators, string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    if (Application.isPlaying)
                    {
                        if (isAnimatorThruState(state, animators[state_machine]))
                        {
                            if (state_machine == animators.Length - 1)
                            {
                                is_all_animators_resolved = true;
                            }
                        }
                        else
                        {
                            await Task.Delay(300);
                        }
                    }
                    else
                    {
                        is_all_animators_resolved = true;
                        break;
                    }
                }
            }
        }

        internal delegate void presentBigWin();

        internal async Task isAllAnimatorsThruStateAndAtPauseStateTriggerEventAt(Animator[] animators, string state, float triggerEventAt, presentBigWin presentBigWin)
        {
            //Check all animators are on given state before continuing
            bool isAllAnimatorResolved = false;
            bool isEventTriggered = false;
            AnimatorStateInfo state_info;
            while (!isAllAnimatorResolved)
            {
                for (int state_machine = 0; state_machine < animators.Length; state_machine++)
                {
                    if (!isEventTriggered)
                    {
                        state_info = animators[state_machine].GetCurrentAnimatorStateInfo(0);
                        if ((state_info.normalizedTime >= triggerEventAt) && state_machine > animators.Length)
                        {
                            presentBigWin();
                            isEventTriggered = true;
                        }
                    }
                    if (isAnimatorThruState(state, animators[state_machine]))
                    {
                        if (state_machine == animators.Length - 1)
                        {
                            isAllAnimatorResolved = true;
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            }
        }
        internal async Task isAllSlotAnimatorsReadyAndAtPauseState(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                {
                    for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!groupObjectManagers[reel].objectsInGroup[slot].isSymbolAnimatorFinishedAndAtPauseState(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == groupObjectManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }
        internal async Task isAllAnimatorsThruStateAndAtPauseState(string state)
        {
            await isAllAnimatorsThruStateAndAtPauseState(_managers.animatorStateMachineMaster.animator_state_machines.state_machines_to_sync, state);
        }

        internal bool isAnimatorThruState(string state_to_pause_on, Animator state_machine)
        {
            AnimatorStateInfo state_info;
            state_info = state_machine.GetCurrentAnimatorStateInfo(0);
            //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else"));
            //normalized time - int is amount of times looped - float is percent animation complete
            while (state_info.normalizedTime < 1 || !state_info.IsName(state_to_pause_on))
            {
                return false;
                //Debug.Log(String.Format("Current State Normalized Time = {0} State Checking = {1} State Name = {2} state length = {3}", state_info.normalizedTime, state, state_info.IsName(state) ? state : "Something Else", state_info.length));
            }
            return true;
        }

        internal async Task isAllSlotAnimatorsReady(string state)
        {
            //Check all animators are on given state before continuing
            bool is_all_animators_resolved = false;
            while (!is_all_animators_resolved)
            {
                for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                {
                    for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                    {
                        if (!groupObjectManagers[reel].objectsInGroup[slot].isSymbolAnimationFinished(state))
                        {
                            await Task.Delay(100);
                            break;
                        }
                    }
                    if (reel == groupObjectManagers.Length - 1)
                        is_all_animators_resolved = true;
                }
            }
        }

        private void ReduceFreeSpinBy(int amount)
        {
            if (managers.machineInfoManager.machineInfoScriptableObject.freespins > 0)
            {
                managers.machineInfoManager.SetFreeSpinsTo(managers.machineInfoManager.machineInfoScriptableObject.freespins - amount);
            }
        }

        internal void SetAllAnimatorsTriggerTo(supportedAnimatorTriggers trigger, bool v)
        {
            if (v)
            {
                managers.animatorStateMachineMaster.SetAllTriggersTo(trigger);
                SetSlotsAnimatorTriggerTo(trigger);
            }
            else
            {
                managers.animatorStateMachineMaster.ResetAllTrigger(trigger);
                ResetSlotsAnimatorTrigger(trigger);
            }
        }

        private async Task WaitAllReelsStopSpinning()
        {
            bool task_lock = true;
            while (task_lock)
            {
                for (int reel = 0; reel < groupObjectManagers.Length; reel++)
                {
                    if (groupObjectManagers[reel].areObjectsInEndPosition)
                    {
                        await Task.Delay(100);
                    }
                    else
                    {
                        task_lock = false;
                    }
                }
            }
        }

        private void SetSlotsAnimatorTriggerTo(supportedAnimatorTriggers slot_to_trigger)
        {
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                {
                    groupObjectManagers[reel].objectsInGroup[slot].SetTriggerTo(slot_to_trigger);
                    groupObjectManagers[reel].objectsInGroup[slot].SetTriggerSubStatesTo(slot_to_trigger);
                }
            }
        }

        private void ResetSlotsAnimatorTrigger(supportedAnimatorTriggers slot_to_trigger)
        {
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                for (int slot = 0; slot < groupObjectManagers[reel].objectsInGroup.Length; slot++)
                {
                    groupObjectManagers[reel].objectsInGroup[slot].ResetTrigger(slot_to_trigger);
                    groupObjectManagers[reel].objectsInGroup[slot].ResetTriggerSubStates(slot_to_trigger);
                }
            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("Application Quit");
            StaticStateManager.SetStateTo(States.None);
        }

        internal void SetPlayerWalletTo(float to_value)
        {
            managers.machineInfoManager.SetPlayerWalletTo(to_value);
        }
        internal void SetStripInfoStruct()
        {
            Debug.Log($"Base call for SetStripInfoStruct in {gameObject.name} Display Zones in Base Call = {managers.configurationObject.configurationSettings.PrintDisplayZones()}");
            SetStripInfoStruct(managers.configurationObject);
        }
        internal void SetStripInfoStruct(StripConfigurationObject configurationObject)
        {
            for (int strip = 0; strip < groupObjectManagers.Length; strip++)
            {
                Debug.Log($"configurationObject.configurationSettings.displayZones[{strip}] = {configurationObject.configurationSettings.displayZones[strip]}");
                SetStripInfoStruct(strip, configurationObject.configurationSettings.displayZones[strip]);
            }
        }

        internal void SetStripInfoStruct(int strip, ConfigurationDisplayZonesStruct displayZone)
        {
            StripObjectGroupManager temp = groupObjectManagers[strip] as StripObjectGroupManager;
            groupObjectManagers[strip].indexInGroupManager = strip;
            GroupInformationStruct temp2 = temp.groupInfo;
            temp2.index = strip;
            ConfigurationDisplayZonesStruct temp3 = new ConfigurationDisplayZonesStruct(displayZone);
            temp.groupInfo = temp2;
            Debug.Log($"temp3 as displayZone.totalPositions {temp3.totalPositions}");
            temp.InitializeLocalPositions();
        }
        internal void SetStripInfoStruct(int strip, GroupInformationStruct stripStruct)
        {
            StripObjectGroupManager temp = groupObjectManagers[strip] as StripObjectGroupManager;
            groupObjectManagers[strip].indexInGroupManager = strip;
            GroupInformationStruct tempStripStruct = new GroupInformationStruct();
            tempStripStruct.index = strip;
            temp.groupInfo = tempStripStruct;
            temp.InitializeLocalPositions();
        }
        internal void ClearSubStatesAllSlotAnimatorStateMachines()
        {
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                groupObjectManagers[reel].ClearAllSlotContainersSubAnimatorStates();
            }
        }

        private AnimatorSubStateMachine[] GatherValuesFromSubStates()
        {
            List<AnimatorSubStateMachine> values = new List<AnimatorSubStateMachine>();
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                values.AddRange(groupObjectManagers[reel].ReturnAllValuesFromSubStates());
            }
            return values.ToArray();
        }

        private string[] GatherKeysFromSubStates()
        {
            List<string> keys = new List<string>();
            for (int reel = 0; reel < groupObjectManagers.Length; reel++)
            {
                keys.AddRange(groupObjectManagers[reel].ReturnAllKeysFromSubStates());
            }
            return keys.ToArray();
        }


        internal void EnsureWeightsAreCorrect()
        {
            //TBD
        }

        internal int GetRollupIndexFromAmountToRack(double amountToRack)
        {
            float betAmount = managers.machineInfoManager.machineInfoScriptableObject.supported_bet_amounts[managers.machineInfoManager.machineInfoScriptableObject.current_bet_amount];
            int index = 0;
            while (amountToRack > betAmount)
            {
                index += 1;
                amountToRack -= betAmount;
                //Big win if we are over the total rollups present
                if (index == managers.soundManager.machineSoundsReference.rollups.Length - 1)
                    break;
            }
            return index;
        }

        internal AudioClip ReturnSymbolSound(int winningSymbol)
        {
            return symbolDataScriptableObject.symbols[winningSymbol].winAudioClip;
        }

        internal void CreateEmptyAnimationContainer()
        {
#if UNITY_EDITOR
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            AnimationCurve curve = AnimationCurve.Linear(0.0F, 1.0F, .0001F, 1.0F); // Unity won't let me use 0 length, so use a very small length instead
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(UnityEngine.Animator), "ThisIsAnEmptyAnimationClip"); // Just dummy data
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            AssetDatabase.CreateAsset(clip, "Assets/" + name + ".anim");
#endif
        }

        internal void GenerateSlotPrefabs()
        {
            for (int group = 0; group < groupObjectManagers.Length; group++)
            {
                groupObjectManagers[group].SetRandomDisplaySymbolAll();
            }
        }

        internal void ReGenerateSlotPrefabs()
        {
            //Set all new slots to local positions
            for (int group = 0; group < groupObjectManagers.Length; group++)
            {
                groupObjectManagers[group].RegenerateSlots();
            }
        }

        internal void SetSlotReferences()
        {
            //Set all new slots to local positions
            for (int group = 0; group < groupObjectManagers.Length; group++)
            {
                groupObjectManagers[group].SetSlotsReference();
            }
        }

        internal void SetPresentationIDToCurrentSymbolDisplayed()
        {
            for (int i = 0; i < groupObjectManagers.Length; i++)
            {
                groupObjectManagers[i].SetPresentationIdToDisplaySymbol();
            }
        }

        internal void SetSlotsDisplaySequenceToCurrentSequence()
        {
            for (int i = 0; i < groupObjectManagers.Length; i++)
            {
                groupObjectManagers[i].SetDisplaySymbolsForNextSpinToCurrent();
            }
        }
        /// <summary>
        /// Sets all group managers objects index in path to the objects current position in path - Objects need to be at local position in path
        /// </summary>
        internal void SetGroupManagersObjectsIndexOnPathToCurrentPositionInPath()
        {
            for (int i = 0; i < groupObjectManagers.Length; i++)
            {
                groupObjectManagers[i].SetObjectsIndexOnPathToCurrentPositionInPath();
            }
        }
    }
}
public struct Matrix_Settings
{
    public int reels;

    //can be individual or overall set
    public int[] slots_per_reel;

    public Matrix_Settings(int reels, int slots_per_reel) : this()
    {
        this.reels = reels;
        this.slots_per_reel = SetSlotsPerReelTo(reels, slots_per_reel);
    }

    private int[] SetSlotsPerReelTo(int reels, int slots_per_reel)
    {
        int[] output = new int[reels];
        for (int reel = 0; reel < output.Length; reel++)
        {
            output[reel] = slots_per_reel;
        }
        return output;
    }
}