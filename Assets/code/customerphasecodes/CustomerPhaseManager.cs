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
        string removedName = customerQueue[removeIndex].customerName;
        customerQueue.RemoveAt(removeIndex);
        dayLog.Add($"Restocked — lost {removedName}");

        hasRestockedThisRound = true;
        CustomerUI.Instance.ShowQueue(customerQueue);
        return true;
    }

    // ── Serving a Customer ─────────────────────────

    public void SelectCustomer(CustomerCard card)
    {
        activeCustomer = card;
        itemsPlacedForCurrentCustomer.Clear();
        CustomerSpawner.Instance.MoveCustomerToRegister(card);
        CustomerUI.Instance.OpenCustomerOrder(card);
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
        // Do NOT DespawnAll here — customers still need to be visible during the
        // Day Breakdown screen. DayBreakdownUI.OnContinue handles WalkAllOut
        // (customers walk to door during fade) then DespawnAll (force-clears).
        CustomerUI.Instance.CloseQueue();
        GameManager.Instance.StartPhase(GamePhase.Breakdown);
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