using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Fade")]
    public Image fadePanel;
    public float fadeDuration = 1f;

    void Awake()
    {
        Instance = this;
        fadePanel.color = new Color(0, 0, 0, 0);
    }

    public void FadeToBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(0f, 1f, onComplete));
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(1f, 0f, onComplete));
    }

    IEnumerator FadeCoroutine(float from, float to, System.Action onComplete)
    {
        float elapsed = 0f;
        fadePanel.color = new Color(0, 0, 0, from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, to);
        onComplete?.Invoke();
    }
}