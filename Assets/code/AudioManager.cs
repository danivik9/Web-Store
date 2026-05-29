using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Source")]
    public AudioSource musicSource;

    public float MusicVolume { get; private set; }

    const string KEY_MUSIC = "Vol_Music";

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

    public void SetMusicVolume(float value)
    {
        Debug.Log($"SetMusicVolume called with value: {value}");
        MusicVolume = value;
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
        ApplyVolume();
    }

    void LoadAndApply()
    {
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f);
        ApplyVolume();
    }

    void ApplyVolume()
    {
        Debug.Log($"ApplyVolume — musicSource null? {musicSource == null}, volume: {MusicVolume}");
        if (musicSource != null)
        {
            musicSource.volume = MusicVolume;
            Debug.Log($"musicSource isPlaying? {musicSource.isPlaying}, gameObject: {musicSource.gameObject.name}");
        }
    }
}