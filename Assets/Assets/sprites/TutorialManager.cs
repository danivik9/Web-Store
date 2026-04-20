using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    const string PREFS_KEY = "TutorialComplete";
    const int CANT_SERVE_STEP = 24;

    // ── UI ─────────────────────────────────────────
    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI clickToContinueText;
    public Image arrowImage;
    public Button skipButton;

    // ── World Arrow Targets ────────────────────────
    [Header("World Targets")]
    public Transform storageDoorTarget;
    public Transform cobwebTarget;
    public Transform storageShelfTarget;
    public Transform storeDoorTarget;
    public Transform registerTarget;
    public Transform storeShelfTarget;

    // ── UI Arrow Targets ───────────────────────────
    [Header("UI Targets")]
    public Transform bugButtonsContainerTarget;
    public Transform webItemsContainerTarget;
    public Transform collectButtonTarget;
    public Transform gridContainerTarget;
    public Transform carryButtonTarget;
    public Transform queueContainerTarget;
    public Transform guaranteedSlotsContainerTarget;
    public Transform rollDiceButtonTarget;
    public Transform cantServeButtonTarget;
    public Transform restockButtonTarget;

    [Header("Settings")]
    public float typewriterSpeed = 0.025f;

    private int currentStep = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForClick = false;
    private bool waitingForAction = false;
    private Camera mainCamera;

    // ── Step Definition ────────────────────────────

    enum TutorialTrigger
    {
        Click,
        StorageEntered,
        CobwebOpened,
        BugAddedToCart,
        CobwebBought,
        StorageOpened,
        BugsCarried,
        BugsPlaced,
        DoorOpened,
        RegisterOpened,
        CustomerCalled,
        CustomerServed,
        CantServeUsed,
        RestockUsed
    }

    struct TutorialStep
    {
        public string text;
        public TutorialTrigger trigger;
        public Transform arrowTarget;
        public float arrowRotation;
        public Vector2 arrowOffset;

        public TutorialStep(string text, TutorialTrigger trigger,
                            Transform arrowTarget,
                            float arrowRotation,
                            float offsetX,
                            float offsetY)
        {
            this.text = text;
            this.trigger = trigger;
            this.arrowTarget = arrowTarget;
            this.arrowRotation = arrowRotation;
            this.arrowOffset = new Vector2(offsetX, offsetY);
        }
    }

    private TutorialStep[] steps;

    // ── Lifecycle ──────────────────────────────────

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        tutorialPanel.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        skipButton.onClick.AddListener(Skip);

        BuildSteps();

        if (GameManager.Instance.isRound0)
        {
            SetupRound0();
            StartTutorial();
        }
    }

    // ── Round 0 Inventory Setup ────────────────────

    void SetupRound0()
    {
        BugType[] bugs = CobwebManager.Instance.GetAllBugTypes();
        // 0=FruitFly  1=Ant  2=Mosquito  3=Maggot  4=Moth

        int[] shelfAmounts = { 2, 3, 3, 3, 2 };
        int storageAmount = 2;

        Shelf[] shelves = FindObjectsOfType<Shelf>();
        foreach (Shelf shelf in shelves)
        {
            if (shelf.acceptedBugType == null) continue;
            for (int i = 0; i < bugs.Length; i++)
            {
                if (shelf.acceptedBugType == bugs[i])
                {
                    for (int j = 0; j < shelfAmounts[i]; j++)
                        shelf.AddBug(new BugToken(bugs[i], 0));
                    break;
                }
            }
        }

        foreach (BugType bug in bugs)
            for (int i = 0; i < storageAmount; i++)
                StorageInventory.Instance.AddItem(new BugToken(bug, 0));

        Debug.Log("Round 0 inventory set up.");
    }

    // ── Step Building ──────────────────────────────

    void BuildSteps()
    {
        // arrowRotation: -90 = points down, 90 = points up, 180 = points left, 0 = points right
        // offsetX/Y: pixels offset from target centre that the arrow is placed at

        steps = new TutorialStep[]
        {
            // ── Introduction ──────────────────────── 0-5
            new TutorialStep(
                "Welcome to Web-Store! You're a spider running a bug grocery store.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "You have 6 days to earn $200 to pay off your bank loan. Good luck!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Each day has 3 phases. Let's go through them!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Phase 1: Preparation - buy bugs from the Cobweb Shop and stock your shelves.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Phase 2: Customer - open the store and serve customers at the register.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Phase 3: Breakdown - see how the day went and what expired overnight.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Storage Room ──────────────────────── 6-7
            new TutorialStep(
                "Let's start! Head to the Storage Room through the door on the left - that's where you buy and store your inventory!",
                TutorialTrigger.StorageEntered, storageDoorTarget, 180f, 80f, 0f),

            new TutorialStep(
                "This is the Storage Room! The Cobweb Shop is in the top right corner. Press E on the cobweb to open the shop!",
                TutorialTrigger.CobwebOpened, cobwebTarget, -90f, 0f, 80f),

            // ── Cobweb Shop ───────────────────────── 8-10
            new TutorialStep(
                "Welcome to the Cobweb Shop! Click a bug type button on the right to add it to your order.",
                TutorialTrigger.BugAddedToCart, bugButtonsContainerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Your bug appeared on the web! Added too many? Click a bug on the web to remove it.",
                TutorialTrigger.Click, webItemsContainerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Happy with your order? Hit Collect to buy everything on the web!",
                TutorialTrigger.CobwebBought, collectButtonTarget, -90f, 0f, 80f),

            // ── Storage Shelf ─────────────────────── 11-13
            new TutorialStep(
                "Great purchase! Now head to the Storage Shelf on the left wall and press E to open it.",
                TutorialTrigger.StorageOpened, storageShelfTarget, -90f, 0f, 80f),

            new TutorialStep(
                "This is your Storage Shelf! Each bug shows how many days until it expires. Hover over any bug to see full details!",
                TutorialTrigger.Click, gridContainerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Click bugs to select them - up to 5 at a time. Then hit Carry to pick them up!",
                TutorialTrigger.BugsCarried, carryButtonTarget, -90f, 0f, 80f),

            // ── Stocking Shelves ──────────────────── 14-17
            new TutorialStep(
                "Bugs are floating above your head! Head back to the store and press E on a shelf to place them. Each shelf only accepts one bug type!",
                TutorialTrigger.BugsPlaced, storeShelfTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Nice stocking! Carrying the wrong bugs? Walk back to the Storage Shelf and press E to return them.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Each bug has its own expiry date. Hover your cursor over any bug to check when it expires. Expired bugs cost $1 each at end of day - Fruit Flies only last 1 day!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            new TutorialStep(
                "Important: once the store is open the Cobweb Shop closes! Make sure you buy everything you need before opening the doors.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Opening the Store ─────────────────── 18
            new TutorialStep(
                "Shelves stocked? Walk to the Customer Door on the right and press E to open the store!",
                TutorialTrigger.DoorOpened, storeDoorTarget, -90f, 0f, 80f),

            // ── Customer Phase ────────────────────── 19-22
            new TutorialStep(
                "Customers are coming in! Walk to the Register and press E to start serving.",
                TutorialTrigger.RegisterOpened, registerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "This is the customer queue! Each card shows the customer and what they want. Click a card to call them to the register!",
                TutorialTrigger.CustomerCalled, queueContainerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "See the guaranteed item slots? Click them to place that bug from your shelves onto the counter!",
                TutorialTrigger.Click, guaranteedSlotsContainerTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Once all guaranteed slots are filled, roll the dice to reveal mystery items - then click the revealed slot to fill it!",
                TutorialTrigger.CustomerServed, rollDiceButtonTarget, -90f, 0f, 80f),

            // ── Second Customer ───────────────────── 23
            new TutorialStep(
                "Great job! Now try serving the next customer yourself.",
                TutorialTrigger.CustomerServed, null, -90f, 0f, 80f),

            // ── Can't Serve ───────────────────────── 24-25
            new TutorialStep(
                "Hmm, you might not have all the bugs for this next customer. Fill what you can - if you can't complete the order, hit Can't Serve!",
                TutorialTrigger.CantServeUsed, cantServeButtonTarget, -90f, 0f, 80f),

            new TutorialStep(
                "Any bugs you already placed are lost and you take a penalty! Plan ahead and make sure you have enough stock before opening.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Restock ───────────────────────────── 26
            new TutorialStep(
                "Running low on shelf stock? You can restock mid-round from storage - but it costs you one customer from the queue! Hit Restock when ready.",
                TutorialTrigger.RestockUsed, restockButtonTarget, -90f, 0f, 80f),

            // ── Final Customer ────────────────────── 27
            new TutorialStep(
                "Good thinking! Now serve the remaining customer to wrap up the tutorial.",
                TutorialTrigger.CustomerServed, null, -90f, 0f, 80f),

            // ── End ───────────────────────────────── 28
            new TutorialStep(
                "Amazing! You're a natural shopkeeper. The real game starts now - 6 days, $200 goal, and the bank is watching. Run your store well!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
        };
    }

    // ── Tutorial Control ───────────────────────────

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

        if (index == CANT_SERVE_STEP)
            CustomerPhaseManager.Instance?.MoveUnservableCustomerToFront();

        TutorialStep step = steps[index];
        InteractionManager.IsLocked = false;

        if (step.arrowTarget != null)
        {
            arrowImage.gameObject.SetActive(true);
            UpdateArrowPosition(step.arrowTarget, step.arrowOffset, step.arrowRotation);
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
            UpdateArrowPosition(
                steps[currentStep].arrowTarget,
                steps[currentStep].arrowOffset,
                steps[currentStep].arrowRotation);

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

    // ── Trigger Hooks ──────────────────────────────

    public void OnStorageEntered() => TryAdvance(TutorialTrigger.StorageEntered);
    public void OnCobwebOpened() => TryAdvance(TutorialTrigger.CobwebOpened);
    public void OnBugAddedToCart() => TryAdvance(TutorialTrigger.BugAddedToCart);
    public void OnCobwebBought() => TryAdvance(TutorialTrigger.CobwebBought);
    public void OnStorageOpened() => TryAdvance(TutorialTrigger.StorageOpened);
    public void OnBugsCarried() => TryAdvance(TutorialTrigger.BugsCarried);
    public void OnBugsPlaced() => TryAdvance(TutorialTrigger.BugsPlaced);
    public void OnDoorOpened() => TryAdvance(TutorialTrigger.DoorOpened);
    public void OnRegisterOpened() => TryAdvance(TutorialTrigger.RegisterOpened);
    public void OnCustomerCalled() => TryAdvance(TutorialTrigger.CustomerCalled);
    public void OnCustomerServed() => TryAdvance(TutorialTrigger.CustomerServed);
    public void OnCantServeUsed() => TryAdvance(TutorialTrigger.CantServeUsed);
    public void OnRestockUsed() => TryAdvance(TutorialTrigger.RestockUsed);

    void TryAdvance(TutorialTrigger trigger)
    {
        if (!isActive) return;
        if (!waitingForAction) return;
        if (steps[currentStep].trigger != trigger) return;
        InteractionManager.IsLocked = false;
        AdvanceStep();
    }

    // ── End Tutorial ───────────────────────────────

    void EndTutorial()
    {
        isActive = false;
        tutorialPanel.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        InteractionManager.IsLocked = false;

        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();

        FadeManager.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.EndRound0();
            FadeManager.Instance.FadeFromBlack();
        });
    }

    public void Skip()
    {
        if (!isActive) return;
        StopAllCoroutines();
        EndTutorial();
    }

    // ── Arrow Positioning ──────────────────────────

    void UpdateArrowPosition(Transform target, Vector2 offset, float rotation)
    {
        if (target == null) return;

        Vector3 screenPos;

        if (target.GetComponent<RectTransform>() != null)
            screenPos = target.position;
        else
            screenPos = mainCamera.WorldToScreenPoint(target.position);

        arrowImage.rectTransform.position = screenPos + new Vector3(offset.x, offset.y, 0f);
        arrowImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    // ── Typewriter ─────────────────────────────────

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