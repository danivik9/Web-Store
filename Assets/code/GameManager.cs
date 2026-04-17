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
        onBreakdownPhase?.Invoke();
    }

    // ── Round Control ──────────────────────────────

    void EndRound()
    {
        if (currentRound >= maxRounds)
        {
            EndGame();
            return;
        }

        currentRound++;
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);
        StartPhase(GamePhase.Preparation);
    }

    // ── Money ──────────────────────────────────────

    public void SpendMoney(float amount)
    {
        currentMoney -= amount;
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        if (currentMoney < 0f) EndGame();
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
        if (currentMoney < 0f) EndGame();
    }

    // ── Game End ───────────────────────────────────

    public bool IsGameEnded() => gameEnded;

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log($"Game over! Final money: ${currentMoney:F2}");

        // Show end screen while screen is still black, then fade in to reveal it.
        // FadeFromBlack snaps to alpha=1 before lerping, so this also works
        // correctly if called mid-game outside a fade (e.g. bankruptcy).
        EndScreenUI.Instance.ShowEnding(currentMoney);
        FadeManager.Instance.FadeFromBlack();
    }
}