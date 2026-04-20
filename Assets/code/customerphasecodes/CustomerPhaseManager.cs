using UnityEngine;
using System.Collections.Generic;

public class CustomerPhaseManager : MonoBehaviour
{
    public static CustomerPhaseManager Instance;

    [Header("Customer Cards")]
    public CustomerCard[] allCards;

    [Header("Tutorial Cards (Round 0 only)")]
    public CustomerCard[] tutorialCards;

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

        customerQueue.Clear();

        // ── Round 0 uses fixed tutorial cards in order ──
        if (GameManager.Instance.isRound0)
        {
            foreach (CustomerCard card in tutorialCards)
                customerQueue.Add(card);

            Debug.Log($"Tutorial customer phase started — {customerQueue.Count} customers queued");
        }
        else
        {
            // Fisher-Yates shuffle for normal rounds
            List<CustomerCard> shuffled = new List<CustomerCard>(allCards);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int rand = Random.Range(0, i + 1);
                CustomerCard temp = shuffled[i];
                shuffled[i] = shuffled[rand];
                shuffled[rand] = temp;
            }

            int count = Mathf.Min(5, shuffled.Count);
            for (int i = 0; i < count; i++)
                customerQueue.Add(shuffled[i]);

            Debug.Log($"Customer phase started — {customerQueue.Count} customers queued");
        }
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

        // ── Round 0: tutorial handles ending ──────
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

    // ── Tutorial Queue Helper ──────────────────────

    public void MoveUnservableCustomerToFront()
    {
        if (customerQueue.Count == 0) return;

        // During Round 0 Rex is always index 2 — no need to search
        if (GameManager.Instance.isRound0)
        {
            Debug.Log("Tutorial: Rex is already in position, no reordering needed.");
            return;
        }

        CustomerCard target = null;

        // Priority 1: partial stock
        foreach (CustomerCard card in customerQueue)
        {
            int shelfCount = GetShelfCount(card.guaranteedBugType);
            if (shelfCount > 0 && shelfCount < card.guaranteedAmount)
            {
                target = card;
                break;
            }
        }

        // Priority 2: completely out of stock
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

        if (target == null) return;

        customerQueue.Remove(target);
        customerQueue.Insert(0, target);
        CustomerUI.Instance.ShowQueue(customerQueue);
        Debug.Log($"Tutorial: moved {target.customerName} to front as unservable customer.");
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