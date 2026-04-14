using UnityEngine;
using System.Collections.Generic;

public class CarrySystem : MonoBehaviour
{
    public static int MAX_CARRY = 5;

    // Tracks carried bugs — key is BugType, value is list of tokens
    private Dictionary<BugType, List<BugToken>> carriedBugs
        = new Dictionary<BugType, List<BugToken>>();

    private int totalCarried = 0;

    // Try to pick up a bug token
    public bool PickUp(BugToken token)
    {
        if (totalCarried >= MAX_CARRY)
        {
            Debug.Log("Carrying too much!");
            return false;
        }

        if (!carriedBugs.ContainsKey(token.bugType))
            carriedBugs[token.bugType] = new List<BugToken>();

        carriedBugs[token.bugType].Add(token);
        totalCarried++;

        CarryDisplay.Instance.UpdateDisplay(carriedBugs);
        return true;
    }

    // Remove one token of a specific bug type
    public BugToken TakeOne(BugType bugType)
    {
        if (!carriedBugs.ContainsKey(bugType)) return null;
        if (carriedBugs[bugType].Count == 0) return null;

        BugToken token = carriedBugs[bugType][0];
        carriedBugs[bugType].RemoveAt(0);
        totalCarried--;

        if (carriedBugs[bugType].Count == 0)
            carriedBugs.Remove(bugType);

        CarryDisplay.Instance.UpdateDisplay(carriedBugs);
        return token;
    }

    public int TotalCarried() => totalCarried;
    public bool IsFull() => totalCarried >= MAX_CARRY;
    public bool IsCarrying(BugType bugType) => carriedBugs.ContainsKey(bugType);
    public Dictionary<BugType, List<BugToken>> GetCarriedBugs() => carriedBugs;
}