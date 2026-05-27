using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StorageShelfUI : MonoBehaviour
{
    public static StorageShelfUI Instance;

    [Header("Panel")]
    public GameObject storagePanel;

    [Header("Grid")]
    public Transform gridContainer;
    public GameObject storageSlotPrefab;

    [Header("Selection Frame")]
    public Sprite selectionFrameSprite;
    public Color selectionFrameColor = Color.white;

    [Header("Buttons")]
    public Button carryButton;
    public Button backButton;
    public TextMeshProUGUI selectedCountText;

    [Header("Camera Pan")]
    public Vector3 shelfCameraPosition;
    public Vector3 shelfCameraRotation;

    [Header("Grid Animation")]
    public float animDelay = 0.5f;
    public float animDuration = 0.2f;
    public float animStagger = 0.05f;

    private List<BugToken> selectedTokens = new List<BugToken>();
    private List<GameObject> spawnedSlots = new List<GameObject>();
    private Dictionary<GameObject, GameObject> slotFrames = new Dictionary<GameObject, GameObject>();
    private CameraFollow cameraFollow;
    private SpiderMovement spiderMovement;
    private GameObject spiderObject;
    private CarrySystem carrySystem;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
        spiderMovement = FindObjectOfType<SpiderMovement>();
        spiderObject = spiderMovement.gameObject;

        storagePanel.SetActive(false);

        carryButton.onClick.AddListener(OnCarry);
        backButton.onClick.AddListener(CloseShelf);
    }

    void Update()
    {
        if (storagePanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            CloseShelf();
    }

    // ── Open / Close ───────────────────────────────

    public void OpenShelf()
    {
        InteractionManager.IsLocked = true;
        UIManager.Instance.HidePrompt();
        selectedTokens.Clear();
        storagePanel.SetActive(true);

        carrySystem = FindObjectOfType<CarrySystem>();
        spiderMovement.enabled = false;
        spiderObject.SetActive(false);

        cameraFollow.PanToPosition(
            shelfCameraPosition,
            Quaternion.Euler(shelfCameraRotation)
        );

        UpdateGrid();
        StartCoroutine(AnimateGridIn(animDelay, animDuration));
        UpdateSelectedCount();
        TutorialManager.Instance?.OnStorageOpened();
    }

    public void CloseShelf()
    {
        StopAllCoroutines();
        InteractionManager.IsLocked = false;
        storagePanel.SetActive(false);

        spiderObject.SetActive(true);
        spiderMovement.enabled = true;

        cameraFollow.ReturnToFollow();
        selectedTokens.Clear();
        slotFrames.Clear();
    }

    // ── Grid ───────────────────────────────────────

    void UpdateGrid()
    {
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);

        spawnedSlots.Clear();
        slotFrames.Clear();

        List<BugToken> items = StorageInventory.Instance.GetItems();

        for (int i = 0; i < 25; i++)
        {
            GameObject slot = Instantiate(storageSlotPrefab, gridContainer);
            spawnedSlots.Add(slot);

            if (i < items.Count)
            {
                BugToken token = items[i];
                int currentRound = GameManager.Instance.currentRound;
                int daysLeft = token.expiryRound == 99
                    ? 99
                    : token.expiryRound - currentRound;

                var img = slot.transform.Find("BugIcon")?.GetComponent<Image>();
                if (img != null) img.sprite = token.bugType.icon;

                var txt = slot.transform.Find("ExpiryText")?.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = daysLeft == 99 ? "∞" : $"{daysLeft}d";

                var hover = slot.AddComponent<BugIconHover>();
                hover.bugToken = token;

                var btn = slot.GetComponent<Button>();
                if (btn != null)
                {
                    BugToken captured = token;
                    GameObject capturedSlot = slot;
                    btn.onClick.AddListener(() => ToggleSelect(captured, capturedSlot));
                }
            }
            else
            {
                var btn = slot.GetComponent<Button>();
                if (btn != null) btn.interactable = false;

                var img = slot.transform.Find("BugIcon")?.GetComponent<Image>();
                if (img != null) img.color = new Color(1, 1, 1, 0);

                var txt = slot.transform.Find("ExpiryText")?.GetComponent<TextMeshProUGUI>();
                if (txt != null) txt.text = "";
            }
        }
    }

    // ── Grid Animation ─────────────────────────────

    IEnumerator AnimateGridIn(float delay, float duration)
    {
        foreach (GameObject slot in spawnedSlots)
        {
            if (slot != null)
                slot.transform.localScale = Vector3.zero;
        }

        yield return new WaitForSeconds(delay);

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] == null) continue;
            StartCoroutine(ScaleIn(spawnedSlots[i].transform, duration));
            yield return new WaitForSeconds(animStagger);
        }
    }

    IEnumerator ScaleIn(Transform target, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = Vector3.one * ease;
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    // ── Selection ──────────────────────────────────

    void ToggleSelect(BugToken token, GameObject slot)
    {
        if (selectedTokens.Contains(token))
        {
            selectedTokens.Remove(token);
            RemoveFrame(slot);
        }
        else
        {
            if (selectedTokens.Count >= CarrySystem.MAX_CARRY)
            {
                Debug.Log("Can only carry 5 at a time!");
                return;
            }
            selectedTokens.Add(token);
            AddFrame(slot);
        }

        UpdateSelectedCount();
    }

    void AddFrame(GameObject slot)
    {
        if (selectionFrameSprite == null) return;
        if (slotFrames.ContainsKey(slot)) return;

        GameObject frame = new GameObject("SelectionFrame");
        frame.transform.SetParent(slot.transform, false);

        RectTransform rt = frame.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = frame.AddComponent<Image>();
        img.sprite = selectionFrameSprite;
        img.type = Image.Type.Simple;
        img.color = selectionFrameColor;
        img.raycastTarget = false;

        frame.transform.SetAsLastSibling();
        slotFrames[slot] = frame;
    }

    void RemoveFrame(GameObject slot)
    {
        if (slotFrames.TryGetValue(slot, out GameObject frame))
        {
            Destroy(frame);
            slotFrames.Remove(slot);
        }
    }

    void UpdateSelectedCount()
    {
        selectedCountText.text = $"Selected: {selectedTokens.Count}/5";
        carryButton.interactable = selectedTokens.Count > 0;
    }

    // ── Carry ──────────────────────────────────────

    void OnCarry()
    {
        foreach (BugToken token in selectedTokens)
        {
            StorageInventory.Instance.RemoveItem(token);
            carrySystem.PickUp(token);
        }

        CloseShelf();
        TutorialManager.Instance?.OnBugsCarried();
    }
}