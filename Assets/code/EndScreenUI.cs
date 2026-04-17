using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance;

    [Header("Panel")]
    public GameObject endPanel;

    [Header("UI Elements")]
    public Image backgroundImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI finalMoneyText;
    public Button restartButton;

    [Header("Ending Images (assign later)")]
    public Sprite failedBackground;
    public Sprite basicWinBackground;
    public Sprite goodWinBackground;
    public Sprite greatWinBackground;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        endPanel.SetActive(false);
        restartButton.onClick.AddListener(OnRestart);
    }

    public void ShowEnding(float finalMoney)
    {
        Debug.Log($"ShowEnding called! Money: {finalMoney}"); // ← debug
        endPanel.SetActive(true);
        InteractionManager.IsLocked = true;

        finalMoneyText.text = $"Final Balance: ${finalMoney:F2}";

        if (finalMoney < 200f)
        {
            titleText.text = "Store Bankrupt!";
            messageText.text = "You couldn't pay off the debt. The bank has taken over your store.";
            backgroundImage.color = new Color(0.3f, 0.1f, 0.1f);
            if (failedBackground != null) backgroundImage.sprite = failedBackground;
        }
        else if (finalMoney < 300f)
        {
            titleText.text = "Debt Paid!";
            messageText.text = "You paid off the debt — just barely. The store survives!";
            backgroundImage.color = new Color(0.8f, 0.8f, 0.8f);
            if (basicWinBackground != null) backgroundImage.sprite = basicWinBackground;
        }
        else if (finalMoney < 400f)
        {
            titleText.text = "Thriving Business!";
            messageText.text = "Great work! The store is doing well and the debt is long gone.";
            backgroundImage.color = new Color(0.6f, 0.9f, 0.6f);
            if (goodWinBackground != null) backgroundImage.sprite = goodWinBackground;
        }
        else
        {
            titleText.text = "Bug Empire!";
            messageText.text = "Incredible! You built a bug empire. The bank is impressed.";
            backgroundImage.color = new Color(1f, 0.85f, 0.2f);
            if (greatWinBackground != null) backgroundImage.sprite = greatWinBackground;
        }
    }

    void OnRestart()
    {
        InteractionManager.IsLocked = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}