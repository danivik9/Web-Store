using UnityEngine;
using System.Collections.Generic;

public class StorageShelf : MonoBehaviour, IInteractable
{
    public string GetPromptText()
    {
        CarrySystem carry = FindObjectOfType<CarrySystem>();

        if (carry != null && carry.TotalCarried() > 0)
            return "Press E to return bugs to storage";

        return "Press E to open Storage";
    }

    public void Interact()
    {
        CarrySystem carry = FindObjectOfType<CarrySystem>();

        if (carry != null && carry.TotalCarried() > 0)
        {
            // Deposit all carried bugs back to storage
            var carriedBugs = carry.GetCarriedBugs();
            foreach (var kvp in new Dictionary<BugType, List<BugToken>>(carriedBugs))
            {
                foreach (BugToken token in kvp.Value)
                    StorageInventory.Instance.AddItem(token);
            }
            carry.ClearAll();
            UIManager.Instance.ShowPrompt("Bugs returned to storage!");
            return;
        }

        StorageShelfUI.Instance.OpenShelf();
    }
}