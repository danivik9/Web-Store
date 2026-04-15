using UnityEngine;

public class CustomerDoor : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation)
            return "Store is already open";
        return "Press E to open store for customers";
    }

    public void Interact()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation) return;
        GameManager.Instance.AdvancePhase(); // moves to Customer phase
        CustomerPhaseManager.Instance.OpenStore();
    }
}