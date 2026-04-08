using UnityEngine;

public class Storage : MonoBehaviour, IInteractable

{
    public string GetPromptText()
    {
        return "Press E to open door";
    }

    public void Interact()
    {
        Debug.Log("door opens");
        // Hook into your game manager here later
    }
}

