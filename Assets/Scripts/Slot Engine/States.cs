//
//
//  Generated by StarUML(tm) C# Add-In
//
//  @ Project : Slot Engine
//  @ File Name : States.cs
//  @ Date : 5/7/2014
//  @ Author : Evan McCall
//
//

public enum SpinStates
{
    idle_idle,
    spin_start,
    spin_intro,
    spin_idle,
    spin_interrupt,
    spin_outro,
    spin_end,
    end
}

public enum GameStates
{
    preload,
    demoattract,
    baseGame,
    bonusgame,
    freespin,
}

public enum States {
	None = -1,
    preloading,
	Coin_In,
	Coin_Out,
    Idle_Intro,
    Idle_Idle,
    Idle_Outro,
    Spin_Intro,
    Spin_Idle,
    Spin_Outro,
    Spin_End,
    Resolve_Intro,
    Resolve_Win_Idle,
    Resolve_Lose_Idle,
    Resolve_Lose_Outro,
    Resolve_Win_Outro,
    win_presentation,
    racking_start,
    racking_loop,
    racking_end,
	feature_transition_out,
	feature_transition_in,
	total_win_presentation
}
