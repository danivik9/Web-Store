using UnityEngine;

public class CustomerDoor : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation)
            return "Store is already open";

        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null && carry.TotalCarried() > 0)
            return "Put your bugs away first!";

        return "Press E to open store for customers";
    }

    public void Interact()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation) return;

        // ── Block if carrying bugs ─────────────────
        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null && carry.TotalCarried() > 0)
        {
            UIManager.Instance.ShowPrompt("Put your bugs away first!");
            return;
        }

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