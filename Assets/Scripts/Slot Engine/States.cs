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
    SpinStart,
    SpinLoop,
    SpinInterrupt,
    SpinEnd,
}

public enum States {
	None,
    PreLoading,
	DemoAttract,
	CoinIn,
	CoinOut,
	BaseGame,
    BaseGameSpinStart,
    BaseGameSpinLoop,
    BaseGameSpinEnd,
    BaseGameWinPresentation,
    BaseGameRacking,
	BonusTransitionIntro,
	BonusTransitionLoop,
	BonusTransitionOutro,
	BonusGame,
    BonusGameSpinStart,
    BonusGameSpinLoop,
    BonusGameSpinEnd,
    BonusGameWinPresentation,
    BonusGameRacking,
	TotalWinPresentation
}
