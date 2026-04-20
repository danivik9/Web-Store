using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int currentRound = 1;
    public int maxRounds = 6;
    public float currentMoney = 15f;
    public GamePhase currentPhase;
    public bool isRound0 = false;

    [Header("Events")]
    public UnityEvent onPreparationPhase;
    public UnityEvent onCustomerPhase;
    public UnityEvent onBreakdownPhase;

    private float pendingEarnings = 0f;
    private float pendingPenalties = 0f;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PlayerPrefs.DeleteKey("TutorialComplete"); // ← remove before final build

        bool tutorialDone = PlayerPrefs.GetInt("TutorialComplete", 0) == 1;

        if (!tutorialDone)
        {
            isRound0 = true;
            currentRound = 0;
            currentMoney = 50f;
        }

        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);
        CobwebManager.Instance.ShuffleDeck();
        CobwebManager.Instance.DrawNextCard();
        StartPhase(GamePhase.Preparation);
    }

    // ── Phase Control ──────────────────────────────

    public void StartPhase(GamePhase phase)
    {
        if (gameEnded) return;
        currentPhase = phase;

        switch (phase)
        {
            case GamePhase.Preparation: StartPreparation(); break;
            case GamePhase.Customer: StartCustomer(); break;
            case GamePhase.Breakdown: StartBreakdown(); break;
        }
    }

    public void AdvancePhase()
    {
        if (gameEnded) return;
        switch (currentPhase)
        {
            case GamePhase.Preparation: StartPhase(GamePhase.Customer); break;
            case GamePhase.Customer: StartPhase(GamePhase.Breakdown); break;
            case GamePhase.Breakdown: EndRound(); break;
        }
    }

    // ── Phases ─────────────────────────────────────

    void StartPreparation()
    {
        Debug.Log($"Round {currentRound} — Preparation Phase");
        if (currentRound > 1)
            CobwebManager.Instance.DrawNextCard();
        onPreparationPhase?.Invoke();
    }

    void StartCustomer()
    {
        Debug.Log($"Round {currentRound} — Customer Phase");
        CustomerPhaseManager.Instance.StartCustomerPhase();
        onCustomerPhase?.Invoke();
    }

    void StartBreakdown()
    {
        Debug.Log($"Round {currentRound} — Breakdown Phase");
        if (isRound0) return;
        onBreakdownPhase?.Invoke();
    }

    // ── Round Control ──────────────────────────────

    void EndRound()
    {
        if (isRound0) return;

        if (currentRound >= maxRounds)
        {
            EndGame();
            return;
        }

        currentRound++;
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);
        StartPhase(GamePhase.Preparation);
    }

    public void EndRound0()
    {
        if (!isRound0) return;
        isRound0 = false;

        // Clear all shelves
        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
            foreach (ShelfSlot slot in shelf.slots)
                slot.ClearSlot();

        // Clear storage and carry
        StorageInventory.Instance.ClearAll();
        CarrySystem carry = FindObjectOfType<CarrySystem>();
        if (carry != null) carry.ClearAll();

        // ── Reset spider to starting position ──────
        DayBreakdownUI.Instance.ResetSpiderPublic();

        // Re-enable spider
        SpiderMovement spider = FindObjectOfType<SpiderMovement>();
        if (spider != null) spider.enabled = true;

        // Reset to real game state
        currentRound = 1;
        currentMoney = 15f;
        pendingEarnings = 0f;
        pendingPenalties = 0f;
        gameEnded = false;

        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);

        CobwebManager.Instance.ShuffleDeck();
        CobwebManager.Instance.DrawNextCard();
        StartPhase(GamePhase.Preparation);
    }

    // ── Money ──────────────────────────────────────

    public void SpendMoney(float amount)
    {
        currentMoney -= amount;
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        if (currentMoney < 0f && !isRound0) EndGame();
    }

    public void EarnMoney(float amount)
    {
        currentMoney += amount;
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
    }

    // ── Pending Money ──────────────────────────────

    public void AddPendingEarnings(float amount) => pendingEarnings += amount;
    public void AddPendingPenalty(float amount) => pendingPenalties += amount;
    public float GetPendingEarnings() => pendingEarnings;
    public float GetPendingPenalties() => pendingPenalties;

    public void ApplyPendingMoney()
    {
        currentMoney += pendingEarnings - pendingPenalties;
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        pendingEarnings = 0f;
        pendingPenalties = 0f;
        if (currentMoney < 0f && !isRound0) EndGame();
    }

    // ── Game End ───────────────────────────────────

    public bool IsGameEnded() => gameEnded;

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log($"Game over! Final money: ${currentMoney:F2}");

        EndScreenUI.Instance.ShowEnding(currentMoney);
        FadeManager.Instance.FadeFromBlack();
    }
}