//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
[System.Serializable]
public class Payline
{
    public int[] payline;

    public int ReturnSlotNumberFromReel(int reel, int reelstrip_length, int reel_start_padding)
    {
        if (payline[reel] + reel_start_padding < reelstrip_length)
        {
            return payline[reel] + reel_start_padding;
        }
        else
        {
            UnityEngine.Debug.LogError("Slot payline position couldn't calculate");
            return payline[reel];
        }
    }
    public Payline(int[] vs)
    {
        payline = vs;
    }
}