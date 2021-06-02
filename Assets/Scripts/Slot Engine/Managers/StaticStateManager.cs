//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : StateManager.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//

namespace BoomSports.Prototype.Managers
{
    public static class StaticStateManager
    {
        public static States enCurrentState;
        public static GameModes enCurrentMode;
        /// <summary>
        /// the current active feature reference for the game
        /// </summary>
        public static Features current_feature_active;
        internal static bool isInterupt;
        internal static bool bonusGameTriggered;

        //State Switching Variables
        public delegate void GameModeDelegate(GameModes modeActivated);
        public static event GameModeDelegate gameModeSetTo;
        public delegate void StateDelegate(States State);
        public static event StateDelegate StateChangedTo;
        public static event StateDelegate StateSwitched;
        public delegate void SpinDelegate();
        public delegate void SpinStateChangedTo(SpinStates spinState);
        public static event SpinDelegate spin_activated_event;
        public static event SpinStateChangedTo spin_state_changed;
        public delegate void FeatureActiveDelegate(Features feature, bool active_inactive);
        public static event FeatureActiveDelegate featureTransition;
        public delegate void MultiplierFeatureDelegate(int multiplier);
        public static event MultiplierFeatureDelegate add_to_multiplier;

        //*************

        //Unity Functions

        //*********

        //State Manager Functions
        public static void SetStateTo(States State)
        {
            //UnityEngine.Debug.Log(string.Format("State switched to {0}", State.ToString()));
            enCurrentState = State;
            if (StateChangedTo != null)
                StateChangedTo.Invoke(State);
        }
        public static void SetGameModeActiveTo(GameModes state)
        {
            enCurrentMode = state;
            gameModeSetTo?.Invoke(state);
        }
        /// <summary>
        /// Sets a feature to being active or deactive
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="active_inactive"></param>
        internal static void SetFeatureActiveTo(Features feature, bool active_inactive)
        {
            StaticUtilities.DebugLog(string.Format("feature {0} active set to {1}", feature.ToString(), active_inactive));
            current_feature_active = active_inactive ? feature : Features.None;
            switch (feature)
            {
                case Features.freespin:
                    if (active_inactive)
                        SetGameModeActiveTo(GameModes.freeSpin);
                    else
                        SetGameModeActiveTo(GameModes.baseGame);
                    break;
                case Features.multiplier:
                    if (active_inactive)
                        SetGameModeActiveTo(GameModes.overlaySpin);
                    else
                        SetGameModeActiveTo(GameModes.baseGame);
                    break;
                case Features.overlay:
                    if (active_inactive)
                        SetGameModeActiveTo(GameModes.overlaySpin);
                    else
                        SetGameModeActiveTo(GameModes.baseGame);
                    break;
                default:
                    SetGameModeActiveTo(GameModes.baseGame);
                    break;
            }
            featureTransition?.Invoke(feature, active_inactive);
        }
        internal static void AddToMultiplier(int amount)
        {
            StaticUtilities.DebugLog(string.Format("Multiplier Set to {0}", amount));
            if (add_to_multiplier != null)
                add_to_multiplier.Invoke(amount);
        }
    }
}