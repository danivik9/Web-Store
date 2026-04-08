using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;

    private IInteractable currentInteractable;

    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
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
