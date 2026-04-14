using UnityEngine;

public class BugIconHover : MonoBehaviour
{
    public ShelfSlot slot;

    void OnMouseEnter()
    {
        if (slot == null || slot.bugToken == null) return;

        int currentRound = GameManager.Instance.currentRound;
        int daysLeft = slot.bugToken.DaysUntilExpiry(currentRound);

        string expiryText = daysLeft == 99
            ? "Never expires"
            : $"Expires in {daysLeft} day(s)";

        UIManager.Instance.ShowTooltip(
            slot.bugToken.bugType.bugName,
            expiryText
        );
    }

    void OnMouseExit()
    {
        UIManager.Instance.HideTooltip();
    }
}