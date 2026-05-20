using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Intro Sequence")]
    public Animator spiderAnimator;
    public Transform spiderTransform;
    public Transform walkTarget;           // empty GO inside the shop
    public float walkSpeed = 2f;
    public Transform door;                 // the door pivot
    public float doorClosedAngle = -90f;   // Y rotation when closed
    public float doorCloseSpeed = 3f;

    [Header("Fade")]
    public Image fadePanel;                // full-screen black Image, alpha 0
    public float fadeDuration = 1f;

    private bool isPlaying = false;

    void Start()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (fadePanel != null)
            fadePanel.color = new Color(0, 0, 0, 0);

        playButton.onClick.AddListener(OnPlay);
        settingsButton.onClick.AddListener(OnSettings);
        quitButton.onClick.AddListener(OnQuit);
    }

    // ── Play ───────────────────────────────────────

    void OnPlay()
    {
        if (isPlaying) return;
        isPlaying = true;

        // Disable buttons so player can't double-click
        playButton.interactable = false;
        settingsButton.interactable = false;
        quitButton.interactable = false;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 1. Hide UI
        mainPanel.SetActive(false);

        // 2. Trigger walk animation
        if (spiderAnimator != null)
            spiderAnimator.SetTrigger("EnterShop");

        // 3. Walk spider toward the inside target
        if (spiderTransform != null && walkTarget != null)
        {
            Vector3 destination = walkTarget.position;
            destination.y = spiderTransform.position.y; // keep same height

            // Rotate spider to face walk direction
            Vector3 dir = (destination - spiderTransform.position).normalized;
            if (dir != Vector3.zero)
                spiderTransform.rotation = Quaternion.LookRotation(dir);

            while (Vector3.Distance(spiderTransform.position, destination) > 0.1f)
            {
                spiderTransform.position = Vector3.MoveTowards(
                    spiderTransform.position,
                    destination,
                    walkSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        // 4. Close the door
        if (door != null)
        {
            Quaternion targetRot = Quaternion.Euler(0f, doorClosedAngle, 0f);
            while (Quaternion.Angle(door.rotation, targetRot) > 1f)
            {
                door.rotation = Quaternion.Slerp(
                    door.rotation,
                    targetRot,
                    doorCloseSpeed * Time.deltaTime
                );
                yield return null;
            }
            door.rotation = targetRot;
        }

        // 5. Short pause after door closes
        yield return new WaitForSeconds(0.3f);

        // 6. Fade to black
        if (fadePanel != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.color = new Color(0, 0, 0, 1);
        }

        // 7. Load game scene
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(gameSceneName);
    }

    // ── Settings ───────────────────────────────────

    void OnSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // ── Quit ───────────────────────────────────────

    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}