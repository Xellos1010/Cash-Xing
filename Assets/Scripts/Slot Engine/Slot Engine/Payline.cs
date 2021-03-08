//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
[System.Serializable]
public class Payline
{
    public int[] payline;

    public Payline(int[] vs)
    {
        payline = vs;
    }
}