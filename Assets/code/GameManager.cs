using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Test")]
    public BugType testBugType;
    public Shelf testShelf;

    [Header("Game State")]
    public int currentRound = 1;
    public int maxRounds = 6;
    public float currentMoney = 15f;
    public GamePhase currentPhase;

    [Header("Events")]
    public UnityEvent onPreparationPhase;
    public UnityEvent onCustomerPhase;
    public UnityEvent onBreakdownPhase;
    public UnityEvent onGameOver;
    public UnityEvent onGameWin;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);

        CobwebManager.Instance.ShuffleDeck();
        CobwebManager.Instance.DrawNextCard();

        StartPhase(GamePhase.Preparation);

        BugToken testToken = new BugToken(testBugType, currentRound);
        testShelf.AddBug(testToken);
    }

    // ── Phase Control ──────────────────────────────

    public void StartPhase(GamePhase phase)
    {
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
        Debug.Log($"Round {currentRound} - Preparation Phase");

        if (currentRound > 1)
            CobwebManager.Instance.DrawNextCard();

        onPreparationPhase?.Invoke();
    }

    void StartCustomer()
    {
        Debug.Log($"Round {currentRound} - Customer Phase");
        CustomerPhaseManager.Instance.StartCustomerPhase();
        onCustomerPhase?.Invoke();
    }

    void StartBreakdown()
    {
        Debug.Log($"Round {currentRound} - Breakdown Phase");
        onBreakdownPhase?.Invoke();
    }

    // ── Round Control ──────────────────────────────

    void EndRound()
    {
        Debug.Log($"Round {currentRound} complete");

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

        if (currentMoney < 0)
        {
            Debug.Log("Broke! Game Over");
            onGameOver?.Invoke();
        }
    }

    public void EarnMoney(float amount)
    {
        currentMoney += amount;
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
    }

    // ── Game End ───────────────────────────────────

    void EndGame()
    {
        if (currentMoney >= 0)
        {
            Debug.Log($"Game Won! Final money: ${currentMoney}");
            onGameWin?.Invoke();
        }
        else
        {
            Debug.Log("Game Over!");
            onGameOver?.Invoke();
        }
    }
}