using UnityEngine;

public class Shelf : MonoBehaviour, IInteractable
{
    [Header("Shelf Settings")]
    public string shelfName;
    public ShelfSlot[] slots = new ShelfSlot[5];

    public string GetPromptText() => $"Press E to access {shelfName}";
    public void Interact()
    {
        Debug.Log($"{shelfName} opened!");
        // Hook into inventory UI later
    }

    // Try to add a bug to the first empty slot
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

    // Get all occupied slots
    public int GetOccupiedCount()
    {
        int count = 0;
        foreach (ShelfSlot slot in slots)
            if (slot.isOccupied) count++;
        return count;
    }

    // Check if shelf has space
    public bool HasSpace() => GetOccupiedCount() < 5;
}
