using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    public static bool IsLocked = false; // ← add this

    private IInteractable currentInteractable;

    void Update()
    {
        if (IsLocked) return; // ← stop here when UI is open

        if (currentInteractable != null)
        {
            UIManager.Instance.ShowPrompt(currentInteractable.GetPromptText());

            if (Input.GetKeyDown(interactKey))
                currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            UIManager.Instance.ShowPrompt(interactable.GetPromptText());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == currentInteractable)
        {
            currentInteractable = null;
            UIManager.Instance.HidePrompt();
        }
    }
}