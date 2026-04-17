using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Fade")]
    public Image fadePanel;
    public float fadeDuration = 1f;
    public float holdDuration = 0.8f; // ← hold on black before fading back in

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

        // Hold on black to give time for scene to reset
        if (to == 1f && holdDuration > 0)
            yield return new WaitForSeconds(holdDuration);

        onComplete?.Invoke();
    }
}