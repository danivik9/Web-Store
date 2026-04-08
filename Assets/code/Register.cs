using UnityEngine;

public class Register : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        return "Press E to open Register";
    }

    public void Interact()
    {
        Debug.Log("Register opened!");
        // Hook into your game manager here later
    }
}
