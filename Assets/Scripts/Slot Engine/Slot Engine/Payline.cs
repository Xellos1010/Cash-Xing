//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
[System.Serializable]
public class Payline
{
    public PaylineConfiguration payline_configuration;

    public int ReturnSlotNumberFromReel(int reel, int reelstrip_length, int reel_start_padding)
    {
        ///Reel strip - length 
        if (payline_configuration.payline[reel] + reel_start_padding < reelstrip_length)
        {
            return payline_configuration.payline[reel] + reel_start_padding;
        }
        else
        {
            UnityEngine.Debug.LogError("Slot payline position couldn't calculate");
            return payline_configuration.payline[reel];
        }
    }
    public Payline(int[] vs)
    {
        payline_configuration.payline = vs;
    }
}

public struct PaylineConfiguration
{
    public int[] payline;
}