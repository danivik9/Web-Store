using UnityEngine;

public class CustomerDoor : MonoBehaviour, IInteractable
{
    public DoorAnimator doorAnimator;

    public static CustomerDoor Instance;

    void Awake()
    {
        Instance = this;
    }

    public string GetPromptText()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation)
            return "Store is already open";
        return "Press E to open store for customers";
    }

    public void Interact()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation) return;

        if (GameManager.Instance.isRound0 &&
            TutorialManager.Instance != null &&
            !TutorialManager.Instance.IsDoorStepReached())
        {
            UIManager.Instance.ShowPrompt("Finish stocking your shelves first!");
            return;
        }

        if (doorAnimator != null)
            doorAnimator.Open(false);

        GameManager.Instance.AdvancePhase();
        CustomerPhaseManager.Instance.OpenStore();
        TutorialManager.Instance?.OnDoorOpened();
    }

    public void CloseDoor()
    {
        if (doorAnimator != null)
            doorAnimator.ForceClose();
    }
}