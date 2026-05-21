using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Interaction Prompt")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;

    [Header("HUD")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI roundText;

    [Header("Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI bugNameText;
    public TextMeshProUGUI expiryText;

    private Coroutine timedPromptCoroutine;

    void Awake()
    {
        Instance = this;
        promptPanel.SetActive(false);
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            tooltipPanel.transform.position = Input.mousePosition + new Vector3(75f, 50f, 0f);
        }
    }

    public void ShowPrompt(string text)
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            return;

        promptText.text = text;
        promptPanel.SetActive(true);
    }

    public void ShowTimedPrompt(string text, float duration = 2f)
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            return;

        promptText.text = text;
        promptPanel.SetActive(true);
        promptPanel.transform.SetAsLastSibling();

        if (timedPromptCoroutine != null)
            StopCoroutine(timedPromptCoroutine);
        timedPromptCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        promptPanel.SetActive(false);
        timedPromptCoroutine = null;
    }

    public void HidePrompt()
    {
        if (timedPromptCoroutine != null)
        {
            StopCoroutine(timedPromptCoroutine);
            timedPromptCoroutine = null;
        }
        promptPanel.SetActive(false);
    }

    public void UpdateMoneyDisplay(float amount)
    {
        moneyText.text = $"${amount:F2}";
    }

    public void UpdateRoundDisplay(int round, int maxRounds)
    {
        roundText.text = $"Round {round}/{maxRounds}";
    }

    public void ShowTooltip(string bugName, string expiry)
    {
        bugNameText.text = bugName;
        expiryText.text = expiry;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}