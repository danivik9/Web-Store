using UnityEngine;
using System.Collections.Generic;

public class StorageInventory : MonoBehaviour
{
    public static StorageInventory Instance;
    public const int MAX_STORAGE = 25;
    private List<BugToken> storedItems = new List<BugToken>();

    void Awake()
    {
        Instance = this;
    }

    public bool AddItem(BugToken token)
    {
        if (storedItems.Count >= MAX_STORAGE)
        {
            Debug.Log("Storage full!");
            return false;
        }
        storedItems.Add(token);
        SortItems();
        return true;
    }

    public bool RemoveItem(BugToken token)
    {
        return storedItems.Remove(token);
    }

    public void SortItems()
    {
        storedItems.Sort((a, b) =>
        {
            int aExpiry = a.expiryRound == 99 ? int.MaxValue : a.expiryRound;
            int bExpiry = b.expiryRound == 99 ? int.MaxValue : b.expiryRound;
            return aExpiry.CompareTo(bExpiry);
        });
    }

    public int ProcessWaste(int currentRound)
    {
        int penalty = 0;
        List<BugToken> expired = new List<BugToken>();

        foreach (BugToken token in storedItems)
        {
            if (token.expiryRound != 99 && token.expiryRound <= currentRound) // ← <=
                expired.Add(token);
        }

        foreach (BugToken token in expired)
        {
            storedItems.Remove(token);
            penalty++;
            Debug.Log($"{token.bugType.bugName} expired in storage!");
        }

        return penalty;
    }

    public string GetStickyNoteText()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (BugToken token in storedItems)
        {
            if (!counts.ContainsKey(token.bugType.bugName))
                counts[token.bugType.bugName] = 0;
            counts[token.bugType.bugName]++;
        }

        if (counts.Count == 0) return "Storage empty";

        string result = "IN STORAGE\n";
        foreach (var kvp in counts)
            result += $"{kvp.Value}x {kvp.Key}\n";
        return result;
    }

    public List<BugToken> GetItems() => storedItems;
    public int GetCount() => storedItems.Count;
    public int GetRemainingSpace() => MAX_STORAGE - storedItems.Count;
}