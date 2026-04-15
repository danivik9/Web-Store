using UnityEngine;

public class Shelf : MonoBehaviour, IInteractable
{
    [Header("Shelf Settings")]
    public string shelfName;
    public BugType acceptedBugType;            // ← new: assign in Inspector
    public ShelfSlot[] slots = new ShelfSlot[5];

    public string GetPromptText()
    {
        if (acceptedBugType == null) return $"Press E to access {shelfName}";

        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry == null) return $"Press E to access {shelfName}";

        int carried = carry.GetCountOfType(acceptedBugType);
        int empty = GetEmptySlotCount();

        if (empty == 0) return $"{shelfName} is full";
        if (carried == 0) return $"Not carrying any {acceptedBugType.bugName}s";
        return $"Press E to place {acceptedBugType.bugName}s ({carried} carried, {empty} slots free)";
    }

    public void Interact()
    {
        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry == null) return;

        int carried = carry.GetCountOfType(acceptedBugType);

        if (carried == 0)
        {
            UIManager.Instance.ShowPrompt($"Not carrying any {acceptedBugType.bugName}s!");
            return;
        }

        int placed = 0;
        foreach (ShelfSlot slot in slots)
        {
            if (slot.IsEmpty() && carry.GetCountOfType(acceptedBugType) > 0)
            {
                BugToken token = carry.TakeOneOfType(acceptedBugType);
                if (token == null) break;
                slot.PlaceBug(token);
                placed++;
            }
        }

        if (placed == 0)
            UIManager.Instance.ShowPrompt($"{shelfName} is full!");
        else
            UIManager.Instance.ShowPrompt($"Placed {placed} {acceptedBugType.bugName}(s) on {shelfName}!");
    }

    // ── Existing methods unchanged ──────────────────

    public bool AddBug(BugToken token)
    {
        foreach (ShelfSlot slot in slots)
        {
            if (!slot.isOccupied)
            {
                slot.PlaceBug(token);
                return true;
            }
        }
        Debug.Log($"{shelfName} is full!");
        return false;
    }

    public int GetOccupiedCount()
    {
        int count = 0;
        foreach (ShelfSlot slot in slots)
            if (slot.isOccupied) count++;
        return count;
    }

    public bool HasSpace() => GetOccupiedCount() < slots.Length;

    public int GetEmptySlotCount()
    {
        int count = 0;
        foreach (ShelfSlot slot in slots)
            if (slot.IsEmpty()) count++;
        return count;
    }
}