using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CustomerUI : MonoBehaviour
{
    public static CustomerUI Instance;

    [Header("Camera Pan")]
    public Vector3 registerCameraPosition;
    public Vector3 registerCameraRotation;

    [Header("Queue Panel")]
    public GameObject queuePanel;
    public Transform queueContainer;
    public GameObject customerQueueSlotPrefab;
    public Button restockButton;
    public TextMeshProUGUI restockButtonText;

    [Header("Order Panel")]
    public GameObject orderPanel;
    public TextMeshProUGUI customerNameText;
    public Transform guaranteedSlotsContainer;
    public Transform randomSlotsContainer;
    public GameObject itemSlotPrefab;
    public GameObject hiddenSlotPrefab;
    public Button rollDiceButton;
    public Button cantServeButton;
    public TextMeshProUGUI diceResultText;

    [Header("Stock Sticky Note")]
    public GameObject stockStickyNote;
    public TextMeshProUGUI stockStickyNoteText;

    [Header("Dice Settings")]
    public float diceSpinDuration = 1.5f;

    private CameraFollow cameraFollow;
    private SpiderMovement spiderMovement;
    private GameObject spiderObject;

    private CustomerCard currentCard;
    private List<GameObject> guaranteedSlotObjects = new List<GameObject>();
    private List<GameObject> randomSlotObjects = new List<GameObject>();
    private List<BugType> revealedRandomBugs = new List<BugType>();

    private int guaranteedFilled = 0;
    private int randomFilled = 0;
    private bool randomRevealed = false;
    private bool isRolling = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
        spiderMovement = FindObjectOfType<SpiderMovement>();
        spiderObject = spiderMovement.gameObject;

        queuePanel.SetActive(false);
        orderPanel.SetActive(false);
        stockStickyNote.SetActive(false);

        restockButton.onClick.AddListener(OnRestock);
        rollDiceButton.onClick.AddListener(() => StartCoroutine(RollDice()));
        cantServeButton.onClick.AddListener(OnCantServe);
    }

    // ── Camera ─────────────────────────────────────

    public void PanToRegister()
    {
        InteractionManager.IsLocked = true;
        UIManager.Instance.HidePrompt();
        spiderMovement.enabled = false;
        spiderObject.SetActive(false);

        cameraFollow.PanToPosition(
            registerCameraPosition,
            Quaternion.Euler(registerCameraRotation)
        );
    }

    public void ReturnFromRegister()
    {
        InteractionManager.IsLocked = false;
        spiderObject.SetActive(true);
        spiderMovement.enabled = true;
        cameraFollow.ReturnToFollow();
    }

    // ── Queue ──────────────────────────────────────

    public void ShowQueue(List<CustomerCard> queue)
    {
        queuePanel.SetActive(true);
        stockStickyNote.SetActive(true);
        UpdateStockStickyNote();

        // Clear old slots
        for (int i = queueContainer.childCount - 1; i >= 0; i--)
            Destroy(queueContainer.GetChild(i).gameObject);

        foreach (CustomerCard card in queue)
        {
            GameObject slot = Instantiate(customerQueueSlotPrefab, queueContainer);
            CustomerCard captured = card;

            // Name label
            var nameText = slot.transform.Find("CustomerName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = card.customerName;

            // Guaranteed item preview
            var guaranteedText = slot.transform.Find("GuaranteedText")?.GetComponent<TextMeshProUGUI>();
            if (guaranteedText != null)
                guaranteedText.text = $"{card.guaranteedAmount}× {card.guaranteedBugType.bugName}";

            // Guaranteed item icon
            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
            if (icon != null) icon.sprite = card.guaranteedBugType.icon;

            // Click to serve
            var btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => CustomerPhaseManager.Instance.SelectCustomer(captured));
        }

        // Restock button
        bool canRestock = !CustomerPhaseManager.Instance.hasRestockedThisRound
            && queue.Count > 0
            && StorageInventory.Instance.GetCount() > 0;
        restockButton.interactable = canRestock;
        restockButtonText.text = CustomerPhaseManager.Instance.hasRestockedThisRound
            ? "Restocked"
            : "Restock (lose 1 customer)";
    }

    public void CloseQueue()
    {
        queuePanel.SetActive(false);
        stockStickyNote.SetActive(false);
        ReturnFromRegister();
    }

    // ── Order Panel ────────────────────────────────

    public void OpenCustomerOrder(CustomerCard card)
    {
        currentCard = card;
        guaranteedFilled = 0;
        randomFilled = 0;
        randomRevealed = false;
        revealedRandomBugs.Clear();

        orderPanel.SetActive(true);
        customerNameText.text = card.customerName;

        rollDiceButton.gameObject.SetActive(false);
        cantServeButton.gameObject.SetActive(true);
        diceResultText.text = "";

        BuildGuaranteedSlots(card);
        BuildHiddenSlots(card.randomRollCount);
        UpdateStockStickyNote();
    }

    public void CloseCustomerOrder()
    {
        orderPanel.SetActive(false);
        ClearSlots();
    }

    // ── Slots ──────────────────────────────────────

    void BuildGuaranteedSlots(CustomerCard card)
    {
        ClearSlots();

        for (int i = 0; i < card.guaranteedAmount; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, guaranteedSlotsContainer);

            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
            if (icon != null) icon.sprite = card.guaranteedBugType.icon;

            var label = slot.transform.Find("BugName")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = card.guaranteedBugType.bugName;

            // Highlight as unfilled
            var bg = slot.GetComponent<Image>();
            if (bg != null) bg.color = new Color(1f, 0.9f, 0.6f);

            BugType captured = card.guaranteedBugType;
            GameObject capturedSlot = slot;
            var btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnClickGuaranteedSlot(captured, capturedSlot));

            guaranteedSlotObjects.Add(slot);
        }
    }

    void BuildHiddenSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject slot = Instantiate(hiddenSlotPrefab, randomSlotsContainer);
            randomSlotObjects.Add(slot);
        }
    }

    void ClearSlots()
    {
        foreach (GameObject s in guaranteedSlotObjects) Destroy(s);
        foreach (GameObject s in randomSlotObjects) Destroy(s);
        guaranteedSlotObjects.Clear();
        randomSlotObjects.Clear();
    }

    // ── Slot Clicks ────────────────────────────────

    void OnClickGuaranteedSlot(BugType bugType, GameObject slot)
    {
        bool success = CustomerPhaseManager.Instance.PlaceItem(bugType);

        if (success)
        {
            // Mark slot as filled
            var bg = slot.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.6f, 1f, 0.6f);

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            guaranteedFilled++;
            UpdateStockStickyNote();

            // All guaranteed filled → show roll button
            if (guaranteedFilled >= currentCard.guaranteedAmount)
            {
                rollDiceButton.gameObject.SetActive(true);
                diceResultText.text = "Roll for mystery items!";
            }
        }
        else
        {
            // Can't fulfill — flash red
            StartCoroutine(FlashSlotRed(slot));
        }
    }

    void OnClickRandomSlot(BugType bugType, GameObject slot)
    {
        bool success = CustomerPhaseManager.Instance.PlaceItem(bugType);

        if (success)
        {
            var bg = slot.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.6f, 1f, 0.6f);

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            randomFilled++;
            UpdateStockStickyNote();

            // All random filled → complete customer
            if (randomFilled >= currentCard.randomRollCount)
                CustomerPhaseManager.Instance.CompleteCustomer();
        }
        else
        {
            StartCoroutine(FlashSlotRed(slot));
        }
    }

    // ── Dice Roll ──────────────────────────────────

    IEnumerator RollDice()
    {
        if (isRolling) yield break;
        isRolling = true;
        rollDiceButton.interactable = false;

        // Spin animation — cycle through random numbers
        float elapsed = 0f;
        while (elapsed < diceSpinDuration)
        {
            int fakeRoll = Random.Range(2, 13);
            diceResultText.text = $"🎲 {fakeRoll}";
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Reveal random items one by one
        revealedRandomBugs.Clear();
        for (int i = 0; i < currentCard.randomRollCount; i++)
        {
            BugType result = CustomerPhaseManager.Instance.RollForRandomItem();
            revealedRandomBugs.Add(result);

            int roll = Random.Range(2, 13); // visual only — real roll is in manager
            diceResultText.text = $"🎲 Rolled {roll}!";

            // Replace hidden slot with revealed slot
            if (i < randomSlotObjects.Count)
            {
                GameObject oldSlot = randomSlotObjects[i];
                Vector2 position = oldSlot.GetComponent<RectTransform>().anchoredPosition;
                Transform parent = oldSlot.transform.parent;
                Destroy(oldSlot);

                GameObject newSlot = Instantiate(itemSlotPrefab, parent);
                randomSlotObjects[i] = newSlot;

                if (result != null)
                {
                    var icon = newSlot.transform.Find("BugIcon")?.GetComponent<Image>();
                    if (icon != null) icon.sprite = result.icon;

                    var label = newSlot.transform.Find("BugName")?.GetComponent<TextMeshProUGUI>();
                    if (label != null) label.text = result.bugName;

                    var bg = newSlot.GetComponent<Image>();
                    if (bg != null) bg.color = new Color(1f, 0.9f, 0.6f);

                    BugType capturedBug = result;
                    GameObject capturedSlot = newSlot;
                    var btn = newSlot.GetComponent<Button>();
                    if (btn != null)
                        btn.onClick.AddListener(() => OnClickRandomSlot(capturedBug, capturedSlot));
                }

                yield return new WaitForSeconds(0.4f);
            }
        }

        rollDiceButton.gameObject.SetActive(false);
        isRolling = false;

        // If randomRollCount is 0 complete immediately
        if (currentCard.randomRollCount == 0)
            CustomerPhaseManager.Instance.CompleteCustomer();
    }

    // ── Can't Serve ────────────────────────────────

    void OnCantServe()
    {
        CustomerPhaseManager.Instance.FailCustomer();
    }

    // ── Restock ────────────────────────────────────

    void OnRestock()
    {
        bool success = CustomerPhaseManager.Instance.TryRestock();
        if (success)
        {
            restockButton.interactable = false;
            restockButtonText.text = "Restocked";
        }
    }

    // ── Stock Sticky Note ──────────────────────────

    void UpdateStockStickyNote()
    {
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        string text = "ON SHELVES\n";

        foreach (Shelf shelf in shelves)
        {
            if (shelf.acceptedBugType == null) continue;
            int count = shelf.GetOccupiedCount();
            text += $"{shelf.acceptedBugType.bugName}: {count}\n";
        }

        stockStickyNoteText.text = text;
    }

    // ── Helpers ────────────────────────────────────

    IEnumerator FlashSlotRed(GameObject slot)
    {
        var bg = slot.GetComponent<Image>();
        if (bg == null) yield break;
        Color original = bg.color;
        bg.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        bg.color = original;
    }
}