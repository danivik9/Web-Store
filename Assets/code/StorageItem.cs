using UnityEngine;

public class StorageItem : MonoBehaviour, IInteractable
{
    public BugToken bugToken;
    private CarrySystem carrySystem;

    void Start()
    {
        carrySystem = FindObjectOfType<CarrySystem>();
    }

    public string GetPromptText()
    {
        if (carrySystem.IsFull())
            return "Carrying too much!";
        return $"Press E to pick up {bugToken.bugType.bugName}";
    }

    public void Interact()
    {
        if (carrySystem.IsFull())
        {
            Debug.Log("Can't carry more!");
            return;
        }

        bool pickedUp = carrySystem.PickUp(bugToken);
        if (pickedUp)
        {
            Destroy(gameObject); // remove from storage
        }
    }
}