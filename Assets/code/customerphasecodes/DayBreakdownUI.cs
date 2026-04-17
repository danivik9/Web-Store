using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("Spider Reset")]
    public Transform spiderResetPosition;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

    private float expiredPenalty = 0f;
    private float totalEarnings = 0f;
    private float totalPenalties = 0f;
    private bool isTyping = false;
    private int lastTotalExpired = 0;

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

        totalPenalties = GameManager.Instance.GetPendingPenalties();
        totalEarnings = earnings;
        expiredPenalty = 0f;

        float net = totalEarnings - totalPenalties;

        continueButton.interactable = false;

        customersServedText.text = "";
        customersFailedText.text = "";
        roundEarningsText.text = "";
        expiredItemsText.text = "";
        expiredPenaltyText.text = "";
        netResultText.text = "";
        dayLogText.text = "";

        string logText = "--- LOG ---\n";
        foreach (string entry in log)
            logText += entry + "\n";

        StartCoroutine(TypewriterSequence(
            served, failed, earnings,
            net, logText
        ));
    }

    IEnumerator TypewriterSequence(
        int served, int failed, float earnings,
        float net, string logText)
    {
        isTyping = true;

        yield return StartCoroutine(TypeText(customersServedText, $"Customers Served: {served}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(customersFailedText, $"Customers Failed: {failed}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(roundEarningsText, $"Sales Earnings: ${earnings:F2}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(expiredItemsText, "Expired items calculated at end of day..."));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(netResultText, $"Customer Earnings: ${net:F2}"));
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(TypeText(dayLogText, logText));

        isTyping = false;
        continueButton.interactable = true;
    }

    IEnumerator TypeText(TextMeshProUGUI textField, string fullText)
    {
        textField.text = "";
        foreach (char c in fullText)
        {
            textField.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
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
        if (isTyping) return;

        breakdownPanel.SetActive(false);
        InteractionManager.IsLocked = false;

        CustomerSpawner.Instance.WalkAllOut();

        FadeManager.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.ApplyPendingMoney();
            CustomerSpawner.Instance.DespawnAll();
            GameManager.Instance.AdvancePhase();

            // Only do normal flow if game hasn't ended
            if (!GameManager.Instance.IsGameEnded())
            {
                int shelfExpired = ProcessShelfWaste();
                int storageExpired = StorageInventory.Instance.ProcessWaste(GameManager.Instance.currentRound);
                lastTotalExpired = shelfExpired + storageExpired;
                expiredPenalty = lastTotalExpired * 1f;

                if (expiredPenalty > 0)
                    GameManager.Instance.SpendMoney(expiredPenalty);

                ResetSpider();

                FadeManager.Instance.FadeFromBlack(() =>
                {
                    if (GameManager.Instance.currentPhase == GamePhase.Preparation)
                        MorningReportUI.Instance.ShowMorningReport(
                            lastTotalExpired,
                            expiredPenalty
                        );
                });
            }
            // If game ended GameManager handles fade and end screen
        });
    }

    // ── Spider Reset ───────────────────────────────

    void ResetSpider()
    {
        SpiderMovement spider = FindObjectOfType<SpiderMovement>();
        if (spider == null) return;

        spider.gameObject.SetActive(true);
        spider.enabled = true;

        if (spiderResetPosition != null)
            spider.transform.position = spiderResetPosition.position;

        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null) carry.ClearAll();
    }
}