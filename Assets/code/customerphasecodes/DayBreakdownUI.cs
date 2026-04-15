using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DayBreakdownUI : MonoBehaviour
{
    public static DayBreakdownUI Instance;

    [Header("Panel")]
    public GameObject breakdownPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI customersServedText;
    public TextMeshProUGUI customersFailedText;
    public TextMeshProUGUI roundEarningsText;
    public TextMeshProUGUI expiredItemsText;
    public TextMeshProUGUI expiredPenaltyText;
    public TextMeshProUGUI netResultText;
    public TextMeshProUGUI dayLogText;

    [Header("Button")]
    public Button continueButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        breakdownPanel.SetActive(false);
        continueButton.onClick.AddListener(OnContinue);
    }

    public void ShowBreakdown(int served, int failed, float earnings, List<string> log)
    {
        breakdownPanel.SetActive(true);
        InteractionManager.IsLocked = true;

        // Process waste and get expired count + penalty
        int expiredCount = StorageInventory.Instance.ProcessWaste(GameManager.Instance.currentRound);

        // Also check store shelves for expired items
        int shelfExpiredCount = ProcessShelfWaste();

        int totalExpired = expiredCount + shelfExpiredCount;
        float expiredPenalty = totalExpired * 1f;

        // Apply expired penalty
        if (expiredPenalty > 0)
            GameManager.Instance.SpendMoney(expiredPenalty);

        float net = earnings - expiredPenalty;

        // Fill UI
        customersServedText.text = $"Customers Served: {served}";
        customersFailedText.text = $"Customers Failed: {failed}";
        roundEarningsText.text = $"Sales Earnings: ${earnings:F2}";
        expiredItemsText.text = $"Expired Items: {totalExpired}";
        expiredPenaltyText.text = $"Expiry Penalty: -${expiredPenalty:F2}";
        netResultText.text = $"Net This Round: ${net:F2}";

        // Day log
        string logText = "--- LOG ---\n";
        foreach (string entry in log)
            logText += entry + "\n";
        dayLogText.text = logText;
    }

    int ProcessShelfWaste()
    {
        int count = 0;
        int currentRound = GameManager.Instance.currentRound;
        Shelf[] shelves = FindObjectsOfType<Shelf>();

        foreach (Shelf shelf in shelves)
        {
            foreach (ShelfSlot slot in shelf.slots)
            {
                if (slot.isOccupied &&
                    slot.bugToken.expiryRound != 99 &&
                    slot.bugToken.expiryRound <= currentRound)
                {
                    slot.ClearSlot();
                    count++;
                }
            }
        }

        return count;
    }

    void OnContinue()
    {
        breakdownPanel.SetActive(false);
        InteractionManager.IsLocked = false;
        GameManager.Instance.AdvancePhase();
    }
}