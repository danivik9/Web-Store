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

    [Header("References")]
    public SpiderMovement spiderMovement;

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
    private GameObject spiderObject;
    private CustomerCard currentCard;
    private List<GameObject> guaranteedSlotObjects = new List<GameObject>();
    private List<GameObject> randomSlotObjects = new List<GameObject>();
    private List<BugType> revealedRandomBugs = new List<BugType>();

    private int guaranteedFilled = 0;
    private int randomFilled = 0;
    private int currentRandomRollIndex = 0;
    private bool isRolling = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cameraFollow = FindObjectOfType<CameraFollow>();
        spiderObject = spiderMovement.gameObject;

        queuePanel.SetActive(false);
        orderPanel.SetActive(false);
        stockStickyNote.SetActive(false);
        restockButton.gameObject.SetActive(false);

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

        stockStickyNote.SetActive(true);
        restockButton.gameObject.SetActive(true);
        UpdateStockStickyNote();

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

        stockStickyNote.SetActive(false);
        restockButton.gameObject.SetActive(false);
    }

    // ── Queue ──────────────────────────────────────

    public void ShowQueue(List<CustomerCard> queue)
    {
        queuePanel.SetActive(true);
        UpdateStockStickyNote();

        for (int i = queueContainer.childCount - 1; i >= 0; i--)
            Destroy(queueContainer.GetChild(i).gameObject);

        foreach (CustomerCard card in queue)
        {
            GameObject slot = Instantiate(customerQueueSlotPrefab, queueContainer);
            CustomerCard captured = card;

            var nameText = slot.transform.Find("CustomerName")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = card.customerName;

            var guaranteedText = slot.transform.Find("GuaranteedText")?.GetComponent<TextMeshProUGUI>();
            if (guaranteedText != null)
                guaranteedText.text = $"{card.guaranteedAmount}x {card.guaranteedBugType.bugName}";

            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
            Debug.Log($"BugIcon found: {icon != null}"); // ← add this line
            if (icon != null) icon.sprite = card.guaranteedBugType.icon;

            var btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => CustomerPhaseManager.Instance.SelectCustomer(captured));
        }

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
        ReturnFromRegister();
    }

    // ── Order Panel ────────────────────────────────

    public void OpenCustomerOrder(CustomerCard card)
    {
        currentCard = card;
        guaranteedFilled = 0;
        randomFilled = 0;
        currentRandomRollIndex = 0;
        isRolling = false;
        revealedRandomBugs.Clear();

        orderPanel.SetActive(true);
        customerNameText.text = card.customerName;

        rollDiceButton.gameObject.SetActive(false);
        rollDiceButton.interactable = true;
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
            if (icon != null)
            {
                icon.sprite = card.guaranteedBugType.icon;
                icon.color = new Color(1f, 1f, 1f, 0.35f); // ← semi-transparent until filled
            }

            var label = slot.transform.Find("BugName")?.GetComponent<TextMeshProUGUI>();
            if (label != null) label.text = card.guaranteedBugType.bugName;

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
            var bg = slot.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.6f, 1f, 0.6f);

            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
            if (icon != null) icon.color = new Color(1f, 1f, 1f, 1f); // ← fully opaque

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            guaranteedFilled++;
            UpdateStockStickyNote();

            if (guaranteedFilled >= currentCard.guaranteedAmount)
            {
                if (currentCard.randomRollCount > 0)
                {
                    rollDiceButton.gameObject.SetActive(true);
                    rollDiceButton.interactable = true;
                    diceResultText.text = "Roll for mystery items!";
                }
                else
                {
                    CustomerPhaseManager.Instance.CompleteCustomer();
                }
            }
        }
        else
        {
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

            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
            if (icon != null) icon.color = new Color(1f, 1f, 1f, 1f); // ← fully opaque

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            randomFilled++;
            UpdateStockStickyNote();

            if (randomFilled >= currentCard.randomRollCount)
            {
                CustomerPhaseManager.Instance.CompleteCustomer();
            }
            else
            {
                rollDiceButton.gameObject.SetActive(true);
                rollDiceButton.interactable = true;
                diceResultText.text = "Roll for next item!";
            }
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

        float elapsed = 0f;
        while (elapsed < diceSpinDuration)
        {
            diceResultText.text = $"Rolling... {Random.Range(2, 13)}";
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        BugType result = CustomerPhaseManager.Instance.RollForRandomItem();
        revealedRandomBugs.Add(result);

        int index = currentRandomRollIndex;

        if (index < randomSlotObjects.Count)
        {
            GameObject oldSlot = randomSlotObjects[index];
            Transform parent = oldSlot.transform.parent;
            Destroy(oldSlot);

            GameObject newSlot = Instantiate(itemSlotPrefab, parent);
            randomSlotObjects[index] = newSlot;

            if (result != null)
            {
                var icon = newSlot.transform.Find("BugIcon")?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = result.icon;
                    icon.color = new Color(1f, 1f, 1f, 0.35f); // ← semi-transparent until clicked
                }

                var label = newSlot.transform.Find("BugName")?.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = result.bugName;

                var bg = newSlot.GetComponent<Image>();
                if (bg != null) bg.color = new Color(1f, 0.9f, 0.6f);

                BugType capturedBug = result;
                GameObject capturedSlot = newSlot;
                var btn = newSlot.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnClickRandomSlot(capturedBug, capturedSlot));

                diceResultText.text = $"Rolled {result.bugName}!";
            }
        }

        currentRandomRollIndex++;
        isRolling = false;
        rollDiceButton.gameObject.SetActive(false);
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
            CloseQueue();
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