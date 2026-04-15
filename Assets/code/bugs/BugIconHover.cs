using UnityEngine;
using UnityEngine.EventSystems;

public class BugIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BugToken bugToken;

    // For 3D world icons (store shelves)
    void OnMouseEnter()
    {
        ShowTooltip();
    }

    void OnMouseExit()
    {
        UIManager.Instance.HideTooltip();
    }

    // For UI elements (storage grid)
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideTooltip();
    }

    void ShowTooltip()
    {
        if (bugToken == null) return;

        int currentRound = GameManager.Instance.currentRound;
        int daysLeft = bugToken.DaysUntilExpiry(currentRound);

        string expiryText = daysLeft == 99
            ? "Never expires"
            : $"Expires in {daysLeft} day(s)";

        UIManager.Instance.ShowTooltip(
            bugToken.bugType.bugName,
            expiryText
        );
    }
}