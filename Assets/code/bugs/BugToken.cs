[System.Serializable]
public class BugToken
{
    public BugType bugType;
    public int roundPurchased;
    public int expiryRound;

    public BugToken(BugType type, int currentRound)
    {
        bugType = type;
        roundPurchased = currentRound;

        // Moths never expire, use 99 as infinite
        expiryRound = type.expiryDays == 99
            ? 99
            : currentRound + type.expiryDays;
    }

    public int DaysUntilExpiry(int currentRound)
    {
        if (expiryRound == 99) return 99;
        return expiryRound - currentRound;
    }
}
