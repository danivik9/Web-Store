using UnityEngine;
using UnityEngine.Audio;

// Persistent singleton — add to a GO in your MainMenu scene.
// Survives scene loads so volume settings carry into the game.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer (optional)")]
    // Assign an AudioMixer asset if you have one.
    // Expose three float params named exactly: "MasterVol", "MusicVol", "SFXVol"
    public AudioMixer audioMixer;

    // ── Volume properties (0 – 1) ──────────────────

    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }

    // PlayerPrefs keys
    const string KEY_MASTER = "Vol_Master";
    const string KEY_MUSIC = "Vol_Music";
    const string KEY_SFX = "Vol_SFX";

    // ── Lifecycle ──────────────────────────────────

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAndApply();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ── Public setters (called by SettingsUI sliders) ──

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        PlayerPrefs.SetFloat(KEY_MASTER, value);
        ApplyMaster();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = value;
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
        ApplyMixer("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        PlayerPrefs.SetFloat(KEY_SFX, value);
        ApplyMixer("SFXVol", value);
    }

    // ── Internal ───────────────────────────────────

    void LoadAndApply()
    {
        MasterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f);
        SFXVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);

        ApplyMaster();
        ApplyMixer("MusicVol", MusicVolume);
        ApplyMixer("SFXVol", SFXVolume);
    }

    void ApplyMaster()
    {
        // AudioListener.volume controls everything in the scene
        AudioListener.volume = MasterVolume;
        ApplyMixer("MasterVol", MasterVolume);
    }

    // Converts 0-1 linear to dB for AudioMixer params
    void ApplyMixer(string param, float linear)
    {
        if (audioMixer == null) return;
        float db = Mathf.Log10(Mathf.Max(linear, 0.0001f)) * 20f;
        audioMixer.SetFloat(param, db);
    }
}