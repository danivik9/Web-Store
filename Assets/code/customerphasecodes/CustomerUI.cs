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
    public CameraFollow cameraFollow;
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

    private GameObject spiderObject;
    private CustomerCard currentCard;
    private List<GameObject> guaranteedSlotObjects = new List<GameObject>();
    private List<GameObject> randomSlotObjects = new List<GameObject>();
    private List<BugType> revealedRandomBugs = new List<BugType>();

    private int guaranteedFilled = 0;
    private int randomFilled = 0;
    private bool isRolling = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
                guaranteedText.text = $"{card.guaranteedAmount}× {card.guaranteedBugType.bugName}";

            var icon = slot.transform.Find("BugIcon")?.GetComponent<Image>();
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
        stockStickyNote.SetActive(false);
        ReturnFromRegister();
    }

    // ── Order Panel ────────────────────────────────

    public void OpenCustomerOrder(CustomerCard card)
    {
        currentCard = card;
        guaranteedFilled = 0;
        randomFilled = 0;
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

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            guaranteedFilled++;
            UpdateStockStickyNote();

            if (guaranteedFilled >= currentCard.guaranteedAmount)
            {
                rollDiceButton.gameObject.SetActive(true);
                diceResultText.text = "Roll for mystery items!";
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

            var btn = slot.GetComponent<Button>();
            if (btn != null) btn.interactable = false;

            randomFilled++;
            UpdateStockStickyNote();

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

        float elapsed = 0f;
        while (elapsed < diceSpinDuration)
        {
            int fakeRoll = Random.Range(2, 13);
            diceResultText.text = $"🎲 {fakeRoll}";
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        revealedRandomBugs.Clear();
        for (int i = 0; i < currentCard.randomRollCount; i++)
        {
            BugType result = CustomerPhaseManager.Instance.RollForRandomItem();
            revealedRandomBugs.Add(result);

            if (i < randomSlotObjects.Count)
            {
                GameObject oldSlot = randomSlotObjects[i];
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