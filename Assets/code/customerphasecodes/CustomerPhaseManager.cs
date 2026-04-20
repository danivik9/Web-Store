using UnityEngine;
using System.Collections.Generic;

public class CustomerPhaseManager : MonoBehaviour
{
    public static CustomerPhaseManager Instance;

    [Header("Customer Cards")]
    public CustomerCard[] allCards;

    [Header("State")]
    public bool storeOpen = false;
    public bool hasRestockedThisRound = false;

    private List<CustomerCard> customerQueue = new List<CustomerCard>();
    private CustomerCard activeCustomer;
    private List<BugType> itemsPlacedForCurrentCustomer = new List<BugType>();
    private float roundEarnings = 0f;
    private int customersServed = 0;
    private int customersFailed = 0;
    private List<string> dayLog = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    // ── Phase Start ────────────────────────────────

    public void StartCustomerPhase()
    {
        CustomerUI.Instance.ClearAssignedSprites();
        hasRestockedThisRound = false;
        roundEarnings = 0f;
        customersServed = 0;
        customersFailed = 0;
        dayLog.Clear();
        itemsPlacedForCurrentCustomer.Clear();
        activeCustomer = null;
        storeOpen = false;

        // Fisher-Yates shuffle
        List<CustomerCard> shuffled = new List<CustomerCard>(allCards);
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            CustomerCard temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        customerQueue.Clear();
        int count = Mathf.Min(5, shuffled.Count);
        for (int i = 0; i < count; i++)
            customerQueue.Add(shuffled[i]);

        // ── Round 0: sort so servable customers come first ──
        if (GameManager.Instance.isRound0)
            SortQueueForTutorial();

        Debug.Log($"Customer phase started — {customerQueue.Count} customers queued");
    }

    // ── Store Door ─────────────────────────────────

    public void OpenStore()
    {
        storeOpen = true;
        CustomerSpawner.Instance.SpawnCustomers(customerQueue);
        CustomerUI.Instance.ShowQueue(customerQueue);
    }

    // ── Restock ────────────────────────────────────

    public bool TryRestock()
    {
        if (hasRestockedThisRound)
        {
            Debug.Log("Already restocked this round!");
            return false;
        }
        if (customerQueue.Count == 0)
        {
            Debug.Log("No customers to lose!");
            return false;
        }
        if (StorageInventory.Instance.GetCount() == 0)
        {
            Debug.Log("Nothing in storage to restock with!");
            return false;
        }

        int removeIndex = Random.Range(0, customerQueue.Count);
        CustomerCard removed = customerQueue[removeIndex];
        string removedName = removed.customerName;
        customerQueue.RemoveAt(removeIndex);
        dayLog.Add($"Restocked — lost {removedName}");

        hasRestockedThisRound = true;
        CustomerSpawner.Instance.DespawnCustomer(removed);
        CustomerUI.Instance.ShowQueue(customerQueue);

        // ── Tutorial hook ──────────────────────────
        TutorialManager.Instance?.OnRestockUsed();
        return true;
    }

    // ── Serving a Customer ─────────────────────────

    public void SelectCustomer(CustomerCard card)
    {
        activeCustomer = card;
        itemsPlacedForCurrentCustomer.Clear();
        CustomerSpawner.Instance.MoveCustomerToRegister(card);
        CustomerUI.Instance.OpenCustomerOrder(card);

        // ── Tutorial hook ──────────────────────────
        TutorialManager.Instance?.OnCustomerCalled();
    }

    public bool PlaceItem(BugType bugType)
    {
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
        {
            if (shelf.acceptedBugType == bugType)
            {
                foreach (ShelfSlot slot in shelf.slots)
                {
                    if (slot.isOccupied)
                    {
                        slot.RemoveBug();
                        itemsPlacedForCurrentCustomer.Add(bugType);
                        return true;
                    }
                }
            }
        }

        Debug.Log($"No {bugType.bugName} available on shelves!");
        return false;
    }

    public BugType RollForRandomItem()
    {
        int roll = Random.Range(1, 7) + Random.Range(1, 7);
        BugType result = activeCustomer.GetBugForRoll(roll);
        Debug.Log($"Rolled {roll} → {(result != null ? result.bugName : "nothing")}");
        return result;
    }

    public void CompleteCustomer()
    {
        float earned = CalculateCustomerEarnings();

        customerQueue.Remove(activeCustomer);
        customersServed++;
        roundEarnings += earned;

        dayLog.Add($"[SERVED] {activeCustomer.customerName} — earned ${earned:F2}");
        GameManager.Instance.AddPendingEarnings(earned);
        TutorialManager.Instance?.OnCustomerServed();

        CustomerSpawner.Instance.DespawnCustomer(activeCustomer);
        itemsPlacedForCurrentCustomer.Clear();
        activeCustomer = null;

        CustomerUI.Instance.CloseCustomerOrder();
        CustomerUI.Instance.ShowQueue(customerQueue);

        if (customerQueue.Count == 0)
            EndCustomerPhase();
    }

    public void FailCustomer()
    {
        customerQueue.Remove(activeCustomer);
        customersFailed++;

        dayLog.Add($"[FAILED] {activeCustomer.customerName} — penalty -${activeCustomer.penalty:F2}");
        GameManager.Instance.AddPendingPenalty(activeCustomer.penalty);

        // ── Tutorial hook ──────────────────────────
        TutorialManager.Instance?.OnCantServeUsed();

        CustomerSpawner.Instance.DespawnCustomer(activeCustomer);
        itemsPlacedForCurrentCustomer.Clear();
        activeCustomer = null;

        CustomerUI.Instance.CloseCustomerOrder();
        CustomerUI.Instance.ShowQueue(customerQueue);

        if (customerQueue.Count == 0)
            EndCustomerPhase();
    }

    // ── End of Phase ───────────────────────────────

    void EndCustomerPhase()
    {
        storeOpen = false;
        CustomerUI.Instance.CloseQueue();

        // ── Round 0: skip breakdown entirely ──────
        if (GameManager.Instance.isRound0)
            return;

        // Do NOT DespawnAll here — customers still need to be visible during the
        // Day Breakdown screen. DayBreakdownUI.OnContinue handles WalkAllOut
        // (customers walk to door during fade) then DespawnAll (force-clears).
        GameManager.Instance.StartPhase(GamePhase.Breakdown);
        DayBreakdownUI.Instance.ShowBreakdown(
            customersServed,
            customersFailed,
            roundEarnings,
            dayLog
        );
    }

    // ── Tutorial Queue Helpers ─────────────────────

    /// <summary>
    /// Called by TutorialManager when the can't-serve step is shown.
    /// Moves the most appropriate unservable customer to the front of the queue.
    /// </summary>
    public void MoveUnservableCustomerToFront()
    {
        if (customerQueue.Count == 0) return;

        CustomerCard target = null;

        // Priority 1: partial stock (has SOME but not ENOUGH)
        foreach (CustomerCard card in customerQueue)
        {
            int shelfCount = GetShelfCount(card.guaranteedBugType);
            if (shelfCount > 0 && shelfCount < card.guaranteedAmount)
            {
                target = card;
                break;
            }
        }

        // Priority 2: completely out of stock for guaranteed bug
        if (target == null)
        {
            foreach (CustomerCard card in customerQueue)
            {
                if (GetShelfCount(card.guaranteedBugType) < card.guaranteedAmount)
                {
                    target = card;
                    break;
                }
            }
        }

        // Priority 3: customer with highest guaranteed amount
        if (target == null)
        {
            target = customerQueue[0];
            foreach (CustomerCard card in customerQueue)
                if (card.guaranteedAmount > target.guaranteedAmount)
                    target = card;
        }

        if (target == null) return;

        customerQueue.Remove(target);
        customerQueue.Insert(0, target);

        CustomerUI.Instance.ShowQueue(customerQueue);
        Debug.Log($"Tutorial: moved {target.customerName} to front as unservable customer.");
    }

    /// <summary>
    /// Called at start of Round 0 customer phase.
    /// Puts servable customers first so tutorial can guarantee first 2 serves succeed.
    /// </summary>
    void SortQueueForTutorial()
    {
        List<CustomerCard> servable = new List<CustomerCard>();
        List<CustomerCard> unservable = new List<CustomerCard>();

        foreach (CustomerCard card in customerQueue)
        {
            if (GetShelfCount(card.guaranteedBugType) >= card.guaranteedAmount)
                servable.Add(card);
            else
                unservable.Add(card);
        }

        customerQueue.Clear();
        customerQueue.AddRange(servable);
        customerQueue.AddRange(unservable);

        Debug.Log($"Tutorial queue sorted: {servable.Count} servable, {unservable.Count} unservable.");
    }

    int GetShelfCount(BugType bugType)
    {
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
            if (shelf.acceptedBugType == bugType)
                return shelf.GetOccupiedCount();
        return 0;
    }

    // ── Helpers ────────────────────────────────────

    float CalculateCustomerEarnings()
    {
        float total = 0f;
        foreach (BugType bug in itemsPlacedForCurrentCustomer)
            total += bug.sellPrice;
        return total;
    }

    public List<CustomerCard> GetQueue() => customerQueue;
    public CustomerCard GetActiveCustomer() => activeCustomer;
    public bool HasActiveCustomer() => activeCustomer != null;
}