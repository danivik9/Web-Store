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
        Debug.Log($"ShowEnding called! Money: {finalMoney}");
        endPanel.SetActive(true);
        InteractionManager.IsLocked = true;
        UIManager.TooltipsEnabled = false;

        finalMoneyText.text = $"Final Balance: ${finalMoney:F2}";
        backgroundImage.color = Color.white;

        if (finalMoney < 200f)
        {
            titleText.text = "Store Bankrupt!";
            messageText.text = "You couldn't pay off the debt. The bank has taken over your store.";
            if (failedBackground != null) backgroundImage.sprite = failedBackground;
        }
        else if (finalMoney < 300f)
        {
            titleText.text = "Debt Paid!";
            messageText.text = "You paid off the debt — just barely. The store survives!";
            if (basicWinBackground != null) backgroundImage.sprite = basicWinBackground;
        }
        else if (finalMoney < 400f)
        {
            titleText.text = "Thriving Business!";
            messageText.text = "Great work! The store is doing well and the debt is long gone.";
            if (goodWinBackground != null) backgroundImage.sprite = goodWinBackground;
        }
        else
        {
            titleText.text = "Bug Empire!";
            messageText.text = "Incredible! You built a bug empire. The bank is impressed.";
            if (greatWinBackground != null) backgroundImage.sprite = greatWinBackground;
        }
    }

    void OnRestart()
    {
        InteractionManager.IsLocked = false;
        UIManager.TooltipsEnabled = true;
        SceneManager.LoadScene("MainMenu");
    }
}