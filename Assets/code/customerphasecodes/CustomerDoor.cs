using UnityEngine;

public class CustomerDoor : MonoBehaviour, IInteractable
{
    public DoorAnimator doorAnimator;
    public DoorTrigger doorTrigger;

    public static CustomerDoor Instance;

    void Awake()
    {
        Instance = this;
    }

    public string GetPromptText()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation)
            return "Store is already open";

        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null && carry.TotalCarried() > 0)
            return "Put away your bugs before opening the store!";

        return "Press E to open store for customers";
    }

    public void Interact()
    {
        if (GameManager.Instance.currentPhase != GamePhase.Preparation) return;

        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null && carry.TotalCarried() > 0)
        {
            UIManager.Instance.ShowTimedPrompt("Put your bugs away before opening the store!");
            return;
        }

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
        if (doorTrigger != null)
            doorTrigger.ResetTrigger();
    }
}