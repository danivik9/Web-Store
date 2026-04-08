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
    public UnityEvent onDeliveryPhase;
    public UnityEvent onStockingPhase;
    public UnityEvent onCustomerPhase;
    public UnityEvent onWastePhase;
    public UnityEvent onGameOver;
    public UnityEvent onGameWin;

    void Awake()
    {
        // Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Initialize UI displays
        UIManager.Instance.UpdateMoneyDisplay(currentMoney);
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds);

        StartPhase(GamePhase.Preparation);
    }


    // ── Phase Control ──────────────────────────────

    public void StartPhase(GamePhase phase)
    {
        currentPhase = phase;

        switch (phase)
        {
            case GamePhase.Preparation: StartPreparation(); break;
            case GamePhase.Delivery: StartDelivery(); break;
            case GamePhase.Stocking: StartStocking(); break;
            case GamePhase.Customer: StartCustomer(); break;
            case GamePhase.Waste: StartWaste(); break;
        }
    }

    public void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Preparation: StartPhase(GamePhase.Delivery); break;
            case GamePhase.Delivery: StartPhase(GamePhase.Stocking); break;
            case GamePhase.Stocking: StartPhase(GamePhase.Customer); break;
            case GamePhase.Customer: StartPhase(GamePhase.Waste); break;
            case GamePhase.Waste: EndRound(); break;
        }
    }

    // ── Phases ─────────────────────────────────────

    void StartPreparation()
    {
        Debug.Log($"Round {currentRound} — Preparation Phase");
        onPreparationPhase?.Invoke();
    }

    void StartDelivery()
    {
        Debug.Log($"Round {currentRound} — Delivery Phase");
        onDeliveryPhase?.Invoke();
    }

    void StartStocking()
    {
        Debug.Log($"Round {currentRound} — Stocking Phase");
        onStockingPhase?.Invoke();
    }

    void StartCustomer()
    {
        Debug.Log($"Round {currentRound} — Customer Phase");
        onCustomerPhase?.Invoke();
    }

    void StartWaste()
    {
        Debug.Log($"Round {currentRound} — Waste Phase");
        onWastePhase?.Invoke();
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
        UIManager.Instance.UpdateRoundDisplay(currentRound, maxRounds); // add this
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
