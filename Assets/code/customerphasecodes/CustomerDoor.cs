using UnityEngine;

public class CustomerDoor : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (CustomerPhaseManager.Instance.storeOpen)
            return "Store is already open";
        return "Press E to open store";
    }

    public void Interact()
    {
        if (CustomerPhaseManager.Instance.storeOpen) return;
        CustomerPhaseManager.Instance.OpenStore();
    }
}