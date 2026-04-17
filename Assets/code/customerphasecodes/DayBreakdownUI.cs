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
    public float typewriterSpeed = 0.03f; // ← lower = faster

    private float expiredPenalty = 0f;
    private float totalEarnings = 0f;
    private float totalPenalties = 0f;
    private bool isTyping = false;

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

        int expiredCount = StorageInventory.Instance.ProcessWaste(GameManager.Instance.currentRound);
        int shelfExpiredCount = ProcessShelfWaste();
        int totalExpired = expiredCount + shelfExpiredCount;
        expiredPenalty = totalExpired * 1f;

        totalPenalties = GameManager.Instance.GetPendingPenalties() + expiredPenalty;
        totalEarnings = earnings;

        float net = totalEarnings - totalPenalties;

        // Disable continue button until typing is done
        continueButton.interactable = false;

        // Clear all text first
        customersServedText.text = "";
        customersFailedText.text = "";
        roundEarningsText.text = "";
        expiredItemsText.text = "";
        expiredPenaltyText.text = "";
        netResultText.text = "";
        dayLogText.text = "";

        // Build log string
        string logText = "--- LOG ---\n";
        foreach (string entry in log)
            logText += entry + "\n";

        // Start typewriter sequence
        StartCoroutine(TypewriterSequence(
            served, failed, earnings,
            totalExpired, expiredPenalty,
            net, logText
        ));
    }

    IEnumerator TypewriterSequence(
        int served, int failed, float earnings,
        int totalExpired, float expPenalty,
        float net, string logText)
    {
        isTyping = true;

        yield return StartCoroutine(TypeText(customersServedText, $"Customers Served: {served}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(customersFailedText, $"Customers Failed: {failed}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(roundEarningsText, $"Sales Earnings: ${earnings:F2}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(expiredItemsText, $"Expired Items: {totalExpired}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(expiredPenaltyText, $"Expiry Penalty: -${expPenalty:F2}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(netResultText, $"Net This Round: ${net:F2}"));
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(TypeText(dayLogText, logText));

        isTyping = false;
        continueButton.interactable = true; // ← enable continue only when done
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
        if (isTyping) return; // safety check

        if (expiredPenalty > 0)
            GameManager.Instance.AddPendingPenalty(expiredPenalty);

        breakdownPanel.SetActive(false);
        InteractionManager.IsLocked = false;

        CustomerSpawner.Instance.WalkAllOut();

        FadeManager.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.ApplyPendingMoney();
            CustomerSpawner.Instance.DespawnAll();
            GameManager.Instance.AdvancePhase();
            ResetSpider();
            FadeManager.Instance.FadeFromBlack();
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