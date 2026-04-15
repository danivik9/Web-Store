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

    // ── Day tracking for breakdown ──
    private List<string> dayLog = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    // ── Phase Start ────────────────────────────────

    public void StartCustomerPhase()
    {
        hasRestockedThisRound = false;
        roundEarnings = 0f;
        customersServed = 0;
        customersFailed = 0;
        dayLog.Clear();
        itemsPlacedForCurrentCustomer.Clear();
        activeCustomer = null;
        storeOpen = false;

        // Shuffle and pick 5 cards
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

        Debug.Log($"Customer phase started — {customerQueue.Count} customers queued");
    }

    // ── Store Door ─────────────────────────────────

    public void OpenStore()
    {
        storeOpen = true;
        Debug.Log("Store is open!");
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

        // Remove a random customer from queue
        int removeIndex = Random.Range(0, customerQueue.Count);
        string removedName = customerQueue[removeIndex].customerName;
        customerQueue.RemoveAt(removeIndex);
        dayLog.Add($"Restocked — lost {removedName}");

        hasRestockedThisRound = true;

        // Move all storage bugs to shelves automatically
        // Player still needs to physically stock via carry system
        // so just flag it and let StorageShelfUI handle the actual stocking
        Debug.Log("Restock triggered!");
        CustomerUI.Instance.ShowQueue(customerQueue);
        return true;
    }

    // ── Serving a Customer ─────────────────────────

    public void SelectCustomer(CustomerCard card)
    {
        activeCustomer = card;
        itemsPlacedForCurrentCustomer.Clear();
        CustomerUI.Instance.OpenCustomerOrder(card);
    }

    // Called when player clicks a guaranteed item slot
    public bool PlaceItem(BugType bugType)
    {
        // Find the shelf with this bug type
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
        {
            if (shelf.acceptedBugType == bugType)
            {
                // Find an occupied slot and take from it
                foreach (ShelfSlot slot in shelf.slots)
                {
                    if (slot.isOccupied)
                    {
                        slot.RemoveBug();
                        itemsPlacedForCurrentCustomer.Add(bugType);
                        roundEarnings += bugType.sellPrice;
                        return true;
                    }
                }
            }
        }

        // Not found on any shelf
        Debug.Log($"No {bugType.bugName} available on shelves!");
        return false;
    }

    // Called by dice roll result
    public BugType RollForRandomItem()
    {
        int roll = Random.Range(1, 7) + Random.Range(1, 7); // 2d6
        BugType result = activeCustomer.GetBugForRoll(roll);
        Debug.Log($"Rolled {roll} → {(result != null ? result.bugName : "nothing")}");
        return result;
    }

    // Called when all slots filled successfully
    public void CompleteCustomer()
    {
        customerQueue.Remove(activeCustomer);
        customersServed++;
        dayLog.Add($"✓ Served {activeCustomer.customerName} — earned ${CalculateCustomerEarnings()}");
        GameManager.Instance.EarnMoney(CalculateCustomerEarnings());
        itemsPlacedForCurrentCustomer.Clear();
        activeCustomer = null;

        CustomerUI.Instance.CloseCustomerOrder();
        CustomerUI.Instance.ShowQueue(customerQueue);

        if (customerQueue.Count == 0)
            EndCustomerPhase();
    }

    // Called when player hits Can't Serve
    public void FailCustomer()
    {
        customerQueue.Remove(activeCustomer);
        customersFailed++;

        // Lose already placed items (already removed from shelves)
        // Lose earnings already counted for this customer
        float earned = CalculateCustomerEarnings();
        roundEarnings -= earned;

        // Apply penalty
        GameManager.Instance.SpendMoney(activeCustomer.penalty);
        dayLog.Add($"✗ Failed {activeCustomer.customerName} — penalty -${activeCustomer.penalty}");

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
        Debug.Log("Customer phase complete!");
        CustomerUI.Instance.CloseQueue();
        DayBreakdownUI.Instance.ShowBreakdown(
            customersServed,
            customersFailed,
            roundEarnings,
            dayLog
        );
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