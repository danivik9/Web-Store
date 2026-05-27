using UnityEngine;
using UnityEngine.EventSystems;

public class BugIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BugToken bugToken;

    void OnMouseEnter()
    {
        ShowTooltip();
    }

    void OnMouseExit()
    {
        UIManager.Instance.HideTooltip();
    }

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
        if (!UIManager.TooltipsEnabled) return;
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