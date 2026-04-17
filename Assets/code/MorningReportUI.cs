using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MorningReportUI : MonoBehaviour
{
    public static MorningReportUI Instance;

    [Header("Panel")]
    public GameObject morningPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI daysRemainingText;
    public TextMeshProUGUI currentMoneyText;
    public TextMeshProUGUI expiredItemsText;
    public TextMeshProUGUI cleanupCostText;
    public TextMeshProUGUI stickyNoteText;

    [Header("Button")]
    public Button continueButton;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.03f;

    private bool isTyping = false;

    // ── Spider sticky notes ────────────────────────
    private string[] stickyNotes = new string[]
    {
        "Note to self: moths never expire. Buy more moths.",
        "Why do the frogs always want mosquitoes??",
        "Ran out of fruit flies again. The birds are NOT happy.",
        "Today feels like a maggot kind of day.",
        "Business is booming! ...I think.",
        "The ants keep escaping the shelf. Again.",
        "Must remember to restock before opening. MUST.",
        "A customer left without buying anything. Rude.",
        "I should really label these shelves better.",
        "Day started well. Optimistic. Cautiously.",
        "The cobweb supplier raised prices again. Classic.",
        "Spiders running stores. What a time to be alive.",
        "If I sell enough moths I can retire early. Maybe.",
        "Note: fruit flies expire in ONE day. ONE.",
        "Today will be different. Today will be great."
    };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        morningPanel.SetActive(false);
        continueButton.onClick.AddListener(OnContinue);
    }

    public void ShowMorningReport(int expiredCount, float cleanupCost)
    {
        morningPanel.SetActive(true);
        InteractionManager.IsLocked = true;
        continueButton.interactable = false;

        // Clear text
        roundText.text = "";
        daysRemainingText.text = "";
        currentMoneyText.text = "";
        expiredItemsText.text = "";
        cleanupCostText.text = "";
        stickyNoteText.text = "";

        StartCoroutine(TypewriterSequence(expiredCount, cleanupCost));
    }

    IEnumerator TypewriterSequence(int expiredCount, float cleanupCost)
    {
        isTyping = true;

        int currentRound = GameManager.Instance.currentRound;
        int daysRemaining = GameManager.Instance.maxRounds - currentRound + 1;
        float money = GameManager.Instance.currentMoney;
        string note = stickyNotes[Random.Range(0, stickyNotes.Length)];

        yield return StartCoroutine(TypeText(roundText, $"Day {currentRound} of {GameManager.Instance.maxRounds}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(daysRemainingText, $"Days Remaining: {daysRemaining}"));
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(TypeText(currentMoneyText, $"Current Balance: ${money:F2}"));
        yield return new WaitForSeconds(0.1f);

        if (expiredCount > 0)
        {
            yield return StartCoroutine(TypeText(expiredItemsText, $"Expired Overnight: {expiredCount} item(s)"));
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(TypeText(cleanupCostText, $"Cleanup Cost: -${cleanupCost:F2}"));
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            yield return StartCoroutine(TypeText(expiredItemsText, "No items expired overnight!"));
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(TypeText(stickyNoteText, $"\"{note}\""));

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

    void OnContinue()
    {
        if (isTyping) return;
        morningPanel.SetActive(false);
        InteractionManager.IsLocked = false;
    }
}