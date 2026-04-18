using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [Header("Panel")]
    public GameObject pausePanel;

    [Header("Pause Button (always visible)")]
    public Button pauseButton;

    [Header("Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pausePanel.SetActive(false);

        pauseButton.onClick.AddListener(OnPause);
        resumeButton.onClick.AddListener(OnResume);
        restartButton.onClick.AddListener(OnRestart);
        quitButton.onClick.AddListener(OnQuit);
    }

    void OnPause()
    {
        pausePanel.SetActive(true);
        InteractionManager.IsLocked = true;
    }

    public void OnResume()
    {
        pausePanel.SetActive(false);
        InteractionManager.IsLocked = false;
    }

    void OnRestart()
    {
        InteractionManager.IsLocked = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnQuit()
    {
        InteractionManager.IsLocked = false;
        SceneManager.LoadScene("MainMenu");
    }
}