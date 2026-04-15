using UnityEngine;

public class Register : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (!CustomerPhaseManager.Instance.storeOpen)
            return "Open the store door first";
        return "Press E to go to register";
    }

    public void Interact()
    {
        if (!CustomerPhaseManager.Instance.storeOpen) return;
        CustomerUI.Instance.PanToRegister();
        CustomerUI.Instance.ShowQueue(CustomerPhaseManager.Instance.GetQueue());
    }
}