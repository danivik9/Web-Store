using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StorageShelfUI : MonoBehaviour
{
    public static StorageShelfUI Instance;

    [Header("Panel")]
    public GameObject storagePanel;

    [Header("Grid")]
    public Transform gridContainer;
    public GameObject storageSlotPrefab;

    [Header("Buttons")]
    public Button carryButton;
    public Button backButton;
    public TextMeshProUGUI selectedCountText;

    [Header("Camera Pan")]
    public Vector3 shelfCameraPosition;
    public Vector3 shelfCameraRotation;

    private List<BugToken> selectedTokens = new List<BugToken>();
    private List<GameObject> spawnedSlots = new List<GameObject>();
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
        UIManager.Instance.HidePrompt(); // ← added
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
        UpdateSelectedCount();
    }

    public void CloseShelf()
    {
        InteractionManager.IsLocked = false;
        storagePanel.SetActive(false);

        spiderObject.SetActive(true);
        spiderMovement.enabled = true;

        cameraFollow.ReturnToFollow();
        selectedTokens.Clear();
    }

    // ── Grid ───────────────────────────────────────

    void UpdateGrid()
    {
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);

        spawnedSlots.Clear();

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

    void ToggleSelect(BugToken token, GameObject slot)
    {
        if (selectedTokens.Contains(token))
        {
            selectedTokens.Remove(token);
            var img = slot.GetComponent<Image>();
            if (img != null) img.color = Color.white;
        }
        else
        {
            if (selectedTokens.Count >= CarrySystem.MAX_CARRY)
            {
                Debug.Log("Can only carry 5 at a time!");
                return;
            }
            selectedTokens.Add(token);
            var img = slot.GetComponent<Image>();
            if (img != null) img.color = new Color(0.5f, 1f, 0.5f);
        }

        UpdateSelectedCount();
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
    }
}