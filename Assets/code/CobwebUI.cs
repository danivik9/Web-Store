using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CobwebUI : MonoBehaviour
{
    public static CobwebUI Instance;

    [Header("Panels")]
    public GameObject cobwebPanel;

    [Header("Web Display")]
    public Transform webItemsContainer;
    public GameObject webItemPrefab;

    [Header("Shop Buttons")]
    public Button fruitFlyButton;
    public Button antButton;
    public Button mosquitoButton;
    public Button maggotButton;
    public Button mothButton;

    [Header("Price Labels")]
    public TextMeshProUGUI fruitFlyPriceText;
    public TextMeshProUGUI antPriceText;
    public TextMeshProUGUI mosquitoPriceText;
    public TextMeshProUGUI maggotPriceText;
    public TextMeshProUGUI mothPriceText;

    [Header("Sticky Note")]
    public TextMeshProUGUI stickyNoteText;

    [Header("Buttons")]
    public Button collectButton;
    public TextMeshProUGUI collectButtonText;
    public Button exitButton;

    [Header("Total Cost")]
    public TextMeshProUGUI totalCostText;

    private CobwebManager cobwebManager;
    private CobwebCard currentCard;
    private List<Vector2> storedPositions = new List<Vector2>();

    private const float ITEM_HALF_SIZE = 12.5f;

    void Awake()
    {
        Instance = this;
    }

    // ── Open / Close ───────────────────────────────

    public void OpenShop(CobwebCard card, CobwebManager manager)
    {
        InteractionManager.IsLocked = true; // ← added
        currentCard = card;
        cobwebManager = manager;

        cobwebPanel.SetActive(true);
        storedPositions.Clear();
        FindObjectOfType<SpiderMovement>().enabled = false;

        UpdatePriceLabels();
        UpdateWebDisplay(new List<BugType>());
        UpdateStickyNote();
        UpdateTotalCost(0f);
        collectButtonText.text = "Collect";

        fruitFlyButton.onClick.RemoveAllListeners();
        antButton.onClick.RemoveAllListeners();
        mosquitoButton.onClick.RemoveAllListeners();
        maggotButton.onClick.RemoveAllListeners();
        mothButton.onClick.RemoveAllListeners();

        fruitFlyButton.onClick.AddListener(() =>
            cobwebManager.AddToPendingOrder(CobwebManager.Instance.GetAllBugTypes()[0]));
        antButton.onClick.AddListener(() =>
            cobwebManager.AddToPendingOrder(CobwebManager.Instance.GetAllBugTypes()[1]));
        mosquitoButton.onClick.AddListener(() =>
            cobwebManager.AddToPendingOrder(CobwebManager.Instance.GetAllBugTypes()[2]));
        maggotButton.onClick.AddListener(() =>
            cobwebManager.AddToPendingOrder(CobwebManager.Instance.GetAllBugTypes()[3]));
        mothButton.onClick.AddListener(() =>
            cobwebManager.AddToPendingOrder(CobwebManager.Instance.GetAllBugTypes()[4]));

        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(() => cobwebManager.CollectOrder());

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => CloseShop());
    }

    public void CloseShop()
    {
        InteractionManager.IsLocked = false; // ← added
        cobwebManager.GetPendingOrder().Clear();
        storedPositions.Clear();
        UpdateWebDisplay(new List<BugType>());
        UpdateTotalCost(0f);

        cobwebPanel.SetActive(false);
        FindObjectOfType<SpiderMovement>().enabled = true;
    }

    // ── Updates ────────────────────────────────────

    void UpdatePriceLabels()
    {
        fruitFlyPriceText.text = $"${currentCard.fruitFlyPrice}";
        antPriceText.text = $"${currentCard.antPrice}";
        mosquitoPriceText.text = $"${currentCard.mosquitoPrice}";
        maggotPriceText.text = $"${currentCard.maggotPrice}";
        mothPriceText.text = $"${currentCard.mothPrice}";
    }

    public void UpdateWebDisplay(List<BugType> pendingOrder)
    {
        for (int i = webItemsContainer.childCount - 1; i >= 0; i--)
            Destroy(webItemsContainer.GetChild(i).gameObject);

        RectTransform containerRect = webItemsContainer.GetComponent<RectTransform>();
        float width = containerRect.rect.width;
        float height = containerRect.rect.height;

        while (storedPositions.Count < pendingOrder.Count)
        {
            storedPositions.Add(new Vector2(
                Random.Range(-width / 2 + ITEM_HALF_SIZE, width / 2 - ITEM_HALF_SIZE),
                Random.Range(-height / 2 + ITEM_HALF_SIZE, height / 2 - ITEM_HALF_SIZE)
            ));
        }

        for (int i = 0; i < pendingOrder.Count; i++)
        {
            int index = i;
            GameObject item = Instantiate(webItemPrefab, webItemsContainer);

            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemRect.sizeDelta = new Vector2(25f, 25f);
            itemRect.anchoredPosition = storedPositions[i];

            var img = item.transform.Find("BugIcon")?.GetComponent<Image>();
            if (img != null) img.sprite = pendingOrder[i].icon;

            var btn = item.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() =>
                {
                    storedPositions.RemoveAt(index);
                    cobwebManager.RemoveFromPendingOrder(index);
                });
        }
    }

    public void UpdateStickyNote()
    {
        stickyNoteText.text = StorageInventory.Instance.GetStickyNoteText();
    }

    public void UpdateTotalCost(float total)
    {
        if (totalCostText != null)
            totalCostText.text = $"Total: ${total}";
    }

    public void OnCollect()
    {
        collectButtonText.text = "Go check the shelf!";
        UpdateStickyNote();
        UpdateTotalCost(0f);

        for (int i = webItemsContainer.childCount - 1; i >= 0; i--)
            Destroy(webItemsContainer.GetChild(i).gameObject);

        Invoke(nameof(CloseShop), 2f);
    }
}