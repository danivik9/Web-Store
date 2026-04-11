using UnityEngine;

public class BugIconHover : MonoBehaviour
{
    public ShelfSlot slot;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Raycast from mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                ShowTooltip();
            }
            else
            {
                HideTooltip();
            }
        }
        else
        {
            HideTooltip();
        }
    }

    void ShowTooltip()
    {
        if (slot.bugToken == null) return;

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

    void HideTooltip()
    {
        UIManager.Instance.HideTooltip();
    }
}
