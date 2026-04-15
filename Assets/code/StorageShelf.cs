using UnityEngine;

public class StorageShelf : MonoBehaviour, IInteractable
{
    public string GetPromptText() => "Press E to open Storage";

    public void Interact()
    {
        StorageShelfUI.Instance.OpenShelf();
    }
}