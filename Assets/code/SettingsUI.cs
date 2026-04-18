using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Can live on the SettingsPanel GO itself.
// Works from BOTH the main menu and a future in-game pause menu —
// just call Open() / Close() from wherever you need it.
//
// Expected children inside the panel:
//   MasterSlider   (Slider)
//   MusicSlider    (Slider)
//   SFXSlider      (Slider)
//   MasterValueText, MusicValueText, SFXValueText  (TextMeshProUGUI, optional)
//   BackButton     (Button)

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Value Labels (optional)")]
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI sfxValueText;

    [Header("Back Button")]
    public Button backButton;

    // Optional — assign if this settings panel is inside the main menu
    // so the Back button knows how to return to the main panel.
    [Header("Main Menu (leave empty if used in-game)")]
    public MainMenuUI mainMenuUI;

    void Start()
    {
        // Sliders go 0 → 1
        masterSlider.minValue = 0f;
        masterSlider.maxValue = 1f;
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // Load saved values
        if (AudioManager.Instance != null)
        {
            masterSlider.value = AudioManager.Instance.MasterVolume;
            musicSlider.value = AudioManager.Instance.MusicVolume;
            sfxSlider.value = AudioManager.Instance.SFXVolume;
        }

        UpdateLabels();

        // Wire sliders
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        backButton.onClick.AddListener(OnBack);
    }

    // ── Slider callbacks ───────────────────────────

    void OnMasterChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
        UpdateLabels();
    }

    void OnMusicChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        UpdateLabels();
    }

    void OnSFXChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        UpdateLabels();
    }

    // ── Back ───────────────────────────────────────

    void OnBack()
    {
        if (mainMenuUI != null)
            mainMenuUI.CloseSettings();   // back to main menu panel
        else
            gameObject.SetActive(false);  // in-game: just close the panel
    }

    // ── Helpers ────────────────────────────────────

    void UpdateLabels()
    {
        if (masterValueText != null)
            masterValueText.text = $"{Mathf.RoundToInt(masterSlider.value * 100)}%";
        if (musicValueText != null)
            musicValueText.text = $"{Mathf.RoundToInt(musicSlider.value * 100)}%";
        if (sfxValueText != null)
            sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";
    }

    // ── Public API for pause menu use ─────────────

    public void Open()
    {
        // Refresh sliders to current saved values when reopened
        if (AudioManager.Instance != null)
        {
            masterSlider.value = AudioManager.Instance.MasterVolume;
            musicSlider.value = AudioManager.Instance.MusicVolume;
            sfxSlider.value = AudioManager.Instance.SFXVolume;
        }
        UpdateLabels();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}