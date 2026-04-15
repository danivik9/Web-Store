using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomerCard", menuName = "WebStore/Customer Card")]
public class CustomerCard : ScriptableObject
{
    [Header("Customer Info")]
    public string customerName;

    [Header("Guaranteed Purchase")]
    public BugType guaranteedBugType;
    public int guaranteedAmount;

    [Header("Random Rolls")]
    public int randomRollCount;
    public DiceEntry[] diceTable;

    [Header("Penalty")]
    public float penalty;

    public BugType GetBugForRoll(int roll)
    {
        foreach (DiceEntry entry in diceTable)
            if (roll >= entry.minRoll && roll <= entry.maxRoll)
                return entry.bugType;
        return null;
    }
}