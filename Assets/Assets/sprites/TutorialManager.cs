using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    const string PREFS_KEY = "TutorialComplete";

    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI clickToContinueText;
    public Image arrowImage;
    public Button skipButton;

    [Header("Arrow Targets")]
    public Transform storageDoorTarget;
    public Transform cobwebTarget;
    public Transform storageShelfTarget;
    public Transform storeDoorTarget;
    public Transform registerTarget;
    public Transform storeShelfTarget;

    [Header("Settings")]
    public float typewriterSpeed = 0.025f;

    private int currentStep = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForClick = false;
    private bool waitingForAction = false;
    private Camera mainCamera;

    enum TutorialTrigger
    {
        Click,
        StorageEntered,
        CobwebBought,
        StorageOpened,
        BugsCarried,
        BugsPlaced,
        DoorOpened,
        RegisterOpened,
        CustomerServed
    }

    struct TutorialStep
    {
        public string text;
        public Transform arrowTarget;
        public TutorialTrigger trigger;

        public TutorialStep(string t, TutorialTrigger trigger, Transform arrow = null)
        {
            text = t;
            this.trigger = trigger;
            arrowTarget = arrow;
        }
    }

    private TutorialStep[] steps;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PlayerPrefs.DeleteKey("TutorialComplete");
        mainCamera = Camera.main;
        tutorialPanel.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        skipButton.onClick.AddListener(Skip);

        BuildSteps();

        Debug.Log($"TutorialManager Start — round: {GameManager.Instance.currentRound}, prefs: {PlayerPrefs.GetInt(PREFS_KEY, 0)}");
        Debug.Log($"Should show tutorial: {GameManager.Instance.currentRound == 1 && PlayerPrefs.GetInt(PREFS_KEY, 0) == 0}");

        if (GameManager.Instance.currentRound == 1 &&
            PlayerPrefs.GetInt(PREFS_KEY, 0) == 0)
        {
            StartTutorial();
        }
    }

    void BuildSteps()
    {
        steps = new TutorialStep[]
        {
            new TutorialStep(
                "Welcome to Web-Store! You're a spider running a bug grocery store.",
                TutorialTrigger.Click),

            new TutorialStep(
                "You have 6 days to earn $200 to pay off your bank loan. Good luck!",
                TutorialTrigger.Click),

            new TutorialStep(
                "Each day has 3 phases:\n1. Preparation — buy and stock bugs\n2. Customer — serve customers\n3. Breakdown — see how you did",
                TutorialTrigger.Click),

            new TutorialStep(
                "First, head through the Storage Door on the left to see your stock.",
                TutorialTrigger.StorageEntered,
                storageDoorTarget),

            new TutorialStep(
                "That's your Storage Shelf — bugs you buy will land here. Now head back to the store and visit the Cobweb Shop to buy some bugs!",
                TutorialTrigger.CobwebBought,
                cobwebTarget),

            new TutorialStep(
                "Nice purchase! Now open the Storage Shelf and pick up your bugs to carry them.",
                TutorialTrigger.BugsCarried,
                storageShelfTarget),

            new TutorialStep(
                "Bugs are floating above your head! Walk to a store shelf and press E to place them. Each shelf only accepts one bug type!",
                TutorialTrigger.BugsPlaced,
                storeShelfTarget),

            new TutorialStep(
                "Great! Remember — carrying the wrong bugs? Walk back to the Storage Shelf and press E to return them.",
                TutorialTrigger.Click),

            new TutorialStep(
                "Watch out for expiry! Fruit Flies last only 1 day. Expired bugs cost $1 each at end of day.",
                TutorialTrigger.Click),

            new TutorialStep(
                "Shelves stocked? Walk to the Customer Door on the right and press E to open the store!",
                TutorialTrigger.DoorOpened,
                storeDoorTarget),

            new TutorialStep(
                "Customers are coming in! Walk to the Register and press E to start serving.",
                TutorialTrigger.RegisterOpened,
                registerTarget),

            new TutorialStep(
                "Click a customer in the queue to serve them. Fill their guaranteed slots, then roll dice for mystery items!",
                TutorialTrigger.CustomerServed),

            new TutorialStep(
                "Amazing! You're a natural shopkeeper. Run your store well and pay off that debt! 🕷️",
                TutorialTrigger.Click),
        };
    }

    // ── Core ───────────────────────────────────────

    public void StartTutorial()
    {
        isActive = true;
        currentStep = 0;
        tutorialPanel.SetActive(true);
        ShowStep(0);
    }

    void ShowStep(int index)
    {
        if (index >= steps.Length)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = steps[index];

        InteractionManager.IsLocked = step.trigger == TutorialTrigger.Click;

        if (step.arrowTarget != null)
        {
            arrowImage.gameObject.SetActive(true);
            UpdateArrowPosition(step.arrowTarget);
        }
        else
        {
            arrowImage.gameObject.SetActive(false);
        }

        waitingForClick = step.trigger == TutorialTrigger.Click;
        waitingForAction = !waitingForClick;

        clickToContinueText.gameObject.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(TypeText(step.text));
    }

    void Update()
    {
        if (!isActive) return;

        if (currentStep < steps.Length && steps[currentStep].arrowTarget != null)
            UpdateArrowPosition(steps[currentStep].arrowTarget);

        if (!waitingForClick) return;

        if (Input.GetMouseButtonDown(0) && isTyping)
        {
            StopAllCoroutines();
            tutorialText.text = steps[currentStep].text;
            isTyping = false;
            clickToContinueText.gameObject.SetActive(true);
            return;
        }

        if (Input.GetMouseButtonDown(0) && !isTyping)
            AdvanceStep();
    }

    void AdvanceStep()
    {
        currentStep++;
        ShowStep(currentStep);
    }

    // ── Action Hooks ───────────────────────────────

    public void OnStorageEntered() => TryAdvance(TutorialTrigger.StorageEntered);
    public void OnCobwebBought() => TryAdvance(TutorialTrigger.CobwebBought);
    public void OnStorageOpened() => TryAdvance(TutorialTrigger.StorageOpened);
    public void OnBugsCarried() => TryAdvance(TutorialTrigger.BugsCarried);
    public void OnBugsPlaced() => TryAdvance(TutorialTrigger.BugsPlaced);
    public void OnDoorOpened() => TryAdvance(TutorialTrigger.DoorOpened);
    public void OnRegisterOpened() => TryAdvance(TutorialTrigger.RegisterOpened);
    public void OnCustomerServed() => TryAdvance(TutorialTrigger.CustomerServed);

    void TryAdvance(TutorialTrigger trigger)
    {
        if (!isActive) return;
        if (!waitingForAction) return;
        if (steps[currentStep].trigger != trigger) return;
        AdvanceStep();
    }

    // ── End ────────────────────────────────────────

    void EndTutorial()
    {
        isActive = false;
        tutorialPanel.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        InteractionManager.IsLocked = false;
        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();
    }

    public void Skip()
    {
        StopAllCoroutines();
        EndTutorial();
    }

    // ── Helpers ────────────────────────────────────

    void UpdateArrowPosition(Transform target)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
        arrowImage.rectTransform.position = screenPos + new Vector3(0, 80f, 0);
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        tutorialText.text = "";
        clickToContinueText.gameObject.SetActive(false);

        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        if (waitingForClick)
            clickToContinueText.gameObject.SetActive(true);
    }
}