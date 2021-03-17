//  @ Project : Slot Engine
//  @ Author : Evan McCall
#if UNITY_EDITOR
#endif
[System.Serializable]
public class Payline
{
    [UnityEngine.SerializeField]
    public PaylineConfiguration payline_configuration;

    public Payline(int[] vs)
    {
        payline_configuration.payline = vs;
    }
}

[System.Serializable]
public struct PaylineConfiguration
{
    [UnityEngine.SerializeField]
    public int[] payline;
}