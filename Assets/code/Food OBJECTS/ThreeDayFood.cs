using UnityEngine;

public class ThreeDayFood : MonoBehaviour, IInteractable

{
    public string GetPromptText()
    {
        return "Press E to place food";
    }

    public void Interact()
    {
        Debug.Log("food is placed!");
        // Hook into your game manager here later
    }
}

