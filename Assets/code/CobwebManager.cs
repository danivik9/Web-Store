using UnityEngine;
using System.Collections.Generic;

public class CobwebManager : MonoBehaviour, IInteractable
{
    public static CobwebManager Instance;

    [Header("Cobweb Cards")]
    public CobwebCard[] allCards;

    [Header("Bug Types")]
    public BugType fruitFly;
    public BugType ant;
    public BugType mosquito;
    public BugType maggot;
    public BugType moth;

    private Queue<CobwebCard> deck = new Queue<CobwebCard>();
    private CobwebCard currentCard;
    private List<BugType> pendingOrder = new List<BugType>();

    void Awake()
    {
        Instance = this;
    }

    // ── Deck ───────────────────────────────────────

    public void ShuffleDeck()
    {
        List<CobwebCard> temp = new List<CobwebCard>(allCards);

        for (int i = temp.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            CobwebCard swap = temp[i];
            temp[i] = temp[rand];
            temp[rand] = swap;
        }

        deck.Clear();
        foreach (CobwebCard card in temp)
            deck.Enqueue(card);

        Debug.Log("Cobweb deck shuffled!");
    }

    public void DrawNextCard()
    {
        if (deck.Count == 0)
        {
            Debug.Log("Deck empty!");
            return;
        }
        currentCard = deck.Dequeue();
        Debug.Log("New cobweb card drawn!");
    }

    // ── Interactable ───────────────────────────────

    public string GetPromptText() => "Press E to open Cobweb Shop";

    public void Interact()
    {
        pendingOrder.Clear();
        CobwebUI.Instance.OpenShop(currentCard, this);
    }

    // ── Order Management ───────────────────────────

    public bool AddToPendingOrder(BugType bugType)
    {
        int currentStorage = StorageInventory.Instance.GetCount();
        int pendingCount = pendingOrder.Count;

        if (currentStorage + pendingCount >= StorageInventory.MAX_STORAGE)
        {
            Debug.Log("Storage would be full!");
            return false;
        }

        // Check if player can afford total order so far
        float totalCost = GetTotalCost() + currentCard.GetPrice(bugType);
        if (totalCost > GameManager.Instance.currentMoney)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        pendingOrder.Add(bugType);
        CobwebUI.Instance.UpdateWebDisplay(pendingOrder);
        CobwebUI.Instance.UpdateStickyNote();
        CobwebUI.Instance.UpdateTotalCost(GetTotalCost());
        return true;
    }

    public void RemoveFromPendingOrder(int index)
    {
        if (index < 0 || index >= pendingOrder.Count) return;

        pendingOrder.RemoveAt(index);
        CobwebUI.Instance.UpdateWebDisplay(pendingOrder);
        CobwebUI.Instance.UpdateStickyNote();
        CobwebUI.Instance.UpdateTotalCost(GetTotalCost());
    }

    // Only deducts money when player confirms
    public void CollectOrder()
    {
        float totalCost = GetTotalCost();

        if (totalCost > GameManager.Instance.currentMoney)
        {
            Debug.Log("Not enough money to collect!");
            return;
        }

        GameManager.Instance.SpendMoney(totalCost);

        int currentRound = GameManager.Instance.currentRound;
        foreach (BugType bugType in pendingOrder)
        {
            BugToken token = new BugToken(bugType, currentRound);
            StorageInventory.Instance.AddItem(token);
        }

        pendingOrder.Clear();
        CobwebUI.Instance.OnCollect();
    }

    public float GetTotalCost()
    {
        float total = 0f;
        foreach (BugType bugType in pendingOrder)
            total += currentCard.GetPrice(bugType);
        return total;
    }

    public List<BugType> GetPendingOrder() => pendingOrder;
    public CobwebCard GetCurrentCard() => currentCard;
    public BugType[] GetAllBugTypes() => new BugType[]
        { fruitFly, ant, mosquito, maggot, moth };
}