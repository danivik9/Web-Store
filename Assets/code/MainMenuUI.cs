using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Scene")]
    public string gameSceneName = "WebstoreMainGame";

    void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);
    }

    void OnPlay()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    void OnSettings()
    {
        // Main menu stays visible underneath — settings overlays on top
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}