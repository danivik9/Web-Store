using UnityEngine;

public class StoreDoor : MonoBehaviour, IInteractable

{
    public string GetPromptText()
    {
        return "open";
    }

    public void Interact()
    {
        Debug.Log("close");
        // Hook into your game manager here later
    }
}

