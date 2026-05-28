using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Slider")]
    public Slider musicSlider;

    [Header("Value Label (optional)")]
    public TextMeshProUGUI musicValueText;

    [Header("Back Button")]
    public Button backButton;

    [Header("Main Menu (leave empty if used in-game)")]
    public MainMenuUI mainMenuUI;

    void Start()
    {
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;

        if (AudioManager.Instance != null)
            musicSlider.value = AudioManager.Instance.MusicVolume;

        UpdateLabel();

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        backButton.onClick.AddListener(OnBack);
    }

    void OnMusicChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        UpdateLabel();
    }

    void OnBack()
    {
        if (mainMenuUI != null)
            mainMenuUI.CloseSettings();
        else
            gameObject.SetActive(false);
    }

    void UpdateLabel()
    {
        if (musicValueText != null)
            musicValueText.text = $"{Mathf.RoundToInt(musicSlider.value * 100)}%";
    }

    public void Open()
    {
        if (AudioManager.Instance != null)
            musicSlider.value = AudioManager.Instance.MusicVolume;
        UpdateLabel();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}