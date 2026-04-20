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

        // ── During tutorial block door until step 18 is reached ──
        if (GameManager.Instance.isRound0 &&
            TutorialManager.Instance != null &&
            !TutorialManager.Instance.IsDoorStepReached())
        {
            UIManager.Instance.ShowPrompt("Finish stocking your shelves first!");
            return;
        }

        GameManager.Instance.AdvancePhase();
        CustomerPhaseManager.Instance.OpenStore();
        TutorialManager.Instance?.OnDoorOpened();
    }
}