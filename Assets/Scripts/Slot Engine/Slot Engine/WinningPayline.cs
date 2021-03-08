//For Parsing Purposes
[System.Serializable]
public class WinningPayline
{
    public Payline payline;
    public int[] winning_symbols;
    public bool left_right; //true for left to right false for right to left

    public WinningPayline(Payline payline, int[] winning_symbols, bool left_right)
    {
        this.payline = payline;
        this.winning_symbols = winning_symbols;
        this.left_right = left_right;
    }
}