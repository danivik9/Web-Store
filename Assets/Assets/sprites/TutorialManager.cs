using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    const string PREFS_KEY = "TutorialComplete";
    const int CANT_SERVE_STEP = 26;

    // ── UI ─────────────────────────────────────────
    [Header("UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public GameObject clickToContinueButton;
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

    [Header("Arrow Bob")]
    public float arrowBobSpeed = 4f;
    public float arrowBobAmount = 12f;

    private int currentStep = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForClick = false;
    private bool waitingForAction = false;
    private Camera mainCamera;
    private float arrowBobTimer = 0f;
    private Vector3 arrowBasePosition;
    private int bugAddedCount = 0;
    private HashSet<TutorialTrigger> preCompletedTriggers = new HashSet<TutorialTrigger>();

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
        RestockUsed,
        GuaranteedFilled
    }

    struct TutorialStep
    {
        public string text;
        public TutorialTrigger trigger;
        public Transform arrowTarget;
        public float arrowRotation;
        public Vector2 arrowOffset;
        public bool useFixedPosition;
        public Vector2 fixedScreenPosition;

        public TutorialStep(string text, TutorialTrigger trigger,
                            Transform arrowTarget,
                            float arrowRotation,
                            float offsetX, float offsetY)
        {
            this.text = text;
            this.trigger = trigger;
            this.arrowTarget = arrowTarget;
            this.arrowRotation = arrowRotation;
            this.arrowOffset = new Vector2(offsetX, offsetY);
            this.useFixedPosition = false;
            this.fixedScreenPosition = Vector2.zero;
        }

        public TutorialStep(string text, TutorialTrigger trigger,
                            float arrowRotation,
                            float screenX, float screenY)
        {
            this.text = text;
            this.trigger = trigger;
            this.arrowTarget = null;
            this.arrowRotation = arrowRotation;
            this.arrowOffset = Vector2.zero;
            this.useFixedPosition = true;
            this.fixedScreenPosition = new Vector2(screenX, screenY);
        }
    }

    private TutorialStep[] steps;

    void Awake() { Instance = this; }

    void Start()
    {
        mainCamera = Camera.main;
        tutorialPanel.SetActive(false);
        arrowImage.gameObject.SetActive(false);
        clickToContinueButton.SetActive(false);
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
                "Phase 1: Preparation. Buy bugs and stock your shelves.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
            new TutorialStep(
                "Phase 2: Customer. Open the store and serve customers.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
            new TutorialStep(
                "Phase 3: Breakdown. See how the day went.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Storage Room ──────────────────────── 6-7
            new TutorialStep(
                "Let's start! Head to the Storage Room through the door on the left.",
                TutorialTrigger.StorageEntered, storageDoorTarget, 180f, 80f, 0f),
            new TutorialStep(
                "This is the Storage Room! Press E on the cobweb to open the shop.",
                TutorialTrigger.CobwebOpened, cobwebTarget, -90f, 0f, 80f),

            // ── Cobweb Shop ───────────────────────── 8-10
            new TutorialStep(
                "Welcome to the Cobweb Shop! Click a bug button to add it to your order.",
                TutorialTrigger.BugAddedToCart, bugButtonsContainerTarget, 0f, -120f, 0f),
            new TutorialStep(
                "Your bug appeared on the web! Click a bug on the web to remove it.",
                TutorialTrigger.Click, webItemsContainerTarget, -90f, 0f, 80f),
            new TutorialStep(
                "Happy with your order? Hit Collect to buy everything!",
                TutorialTrigger.CobwebBought, collectButtonTarget, -90f, 0f, 80f),

            // ── Storage Shelf ─────────────────────── 11-13
            new TutorialStep(
                "Great purchase! Head to the Storage Shelf on the left and press E.",
                TutorialTrigger.StorageOpened, storageShelfTarget, 0f, -100f, 0f),
            new TutorialStep(
                "This is your Storage Shelf! Hover over any bug to see its expiry date.",
                TutorialTrigger.Click, 90f, 960f, 400f),
            new TutorialStep(
                "Click bugs to select them, up to 5 at a time. Then hit Carry!",
                TutorialTrigger.BugsCarried, carryButtonTarget, -90f, 0f, 80f),

            // ── Stocking Shelves ──────────────────── 14-17
            new TutorialStep(
                "Bugs are above your head! Press E on a shelf to place them.",
                TutorialTrigger.BugsPlaced, storeShelfTarget, -90f, 0f, 80f),
            new TutorialStep(
                "Nice stocking! Wrong bugs? Press E on the Storage Shelf to return them.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
            new TutorialStep(
                "Watch expiry dates! Expired bugs cost $1 each. Fruit Flies last 1 day!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
            new TutorialStep(
                "The Cobweb Shop closes once the store opens. Buy everything first!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Opening the Store ─────────────────── 18
            new TutorialStep(
                "Shelves stocked? Walk to the Customer Door and press E to open!",
                TutorialTrigger.DoorOpened, storeDoorTarget, -90f, 0f, 80f),

            // ── Customer Phase ────────────────────── 19-22
            new TutorialStep(
                "Customers are coming in! Walk to the Register and press E.",
                TutorialTrigger.RegisterOpened, registerTarget, -90f, 0f, 80f),
            new TutorialStep(
                "This is the queue! Click a customer card to call them up.",
                TutorialTrigger.CustomerCalled, queueContainerTarget, 90f, 0f, -110f),
            new TutorialStep(
                "Click the guaranteed slots to place bugs from your shelves!",
                TutorialTrigger.Click, guaranteedSlotsContainerTarget, -90f, 0f, 80f),
            new TutorialStep(
                "All guaranteed slots filled! Roll the dice to reveal mystery items!",
                TutorialTrigger.CustomerServed, rollDiceButtonTarget, 0f, -150f, 0f),

            // ── Second Customer ───────────────────── 23
            new TutorialStep(
                "Great job! Now try serving the next customer yourself.",
                TutorialTrigger.CustomerServed, null, -90f, 0f, 80f),

            // ── Call Can't Serve Customer ─────────── 24
            new TutorialStep(
                "Now click the next customer card to call them to the register.",
                TutorialTrigger.CustomerCalled, queueContainerTarget, 90f, 0f, -110f),

            // ── Fill Guaranteed First ─────────────── 25
            new TutorialStep(
                "Fill the guaranteed slots for this customer first!",
                TutorialTrigger.GuaranteedFilled, guaranteedSlotsContainerTarget, -90f, 0f, 80f),

            // ── Can't Serve ───────────────────────── 26
            new TutorialStep(
                "Hmm, seems like you can't finish this order. Hit Can't Serve!",
                TutorialTrigger.CantServeUsed, cantServeButtonTarget, 180f, 150f, 0f),

            // ── After Can't Serve ─────────────────── 27
            new TutorialStep(
                "Bugs you placed are lost and you take a penalty! Plan ahead next time.",
                TutorialTrigger.Click, null, -90f, 0f, 80f),

            // ── Restock ───────────────────────────── 28
            new TutorialStep(
                "Low on stock? Restock from storage, but it costs you one customer!",
                TutorialTrigger.RestockUsed, restockButtonTarget, -90f, 0f, 110f),

            // ── Final Customer ────────────────────── 29
            new TutorialStep(
                "Good thinking! Now serve the remaining customer to finish up.",
                TutorialTrigger.CustomerServed, null, -90f, 0f, 80f),

            // ── End ───────────────────────────────── 30
            new TutorialStep(
                "Amazing! You're a natural shopkeeper. The real game starts now. Good luck!",
                TutorialTrigger.Click, null, -90f, 0f, 80f),
        };
    }

    // ── Tutorial Control ───────────────────────────

    public void StartTutorial()
    {
        isActive = true;
        currentStep = 0;
        preCompletedTriggers.Clear();
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

        if (index == CANT_SERVE_STEP || index == 28)
            CustomerUI.Instance?.RefreshButtonStates();

        TutorialStep step = steps[index];
        InteractionManager.IsLocked = false;
        arrowBobTimer = 0f;

        if (step.arrowTarget != null || step.useFixedPosition)
        {
            arrowImage.gameObject.SetActive(true);
            if (step.useFixedPosition)
                SetArrowFixed(step.fixedScreenPosition, step.arrowRotation);
            else
                UpdateArrowPosition(step.arrowTarget, step.arrowOffset, step.arrowRotation);
        }
        else
        {
            arrowImage.gameObject.SetActive(false);
        }

        waitingForClick = step.trigger == TutorialTrigger.Click;
        waitingForAction = !waitingForClick;

        skipButton.gameObject.SetActive(waitingForClick);

        if (waitingForAction && preCompletedTriggers.Contains(step.trigger))
        {
            preCompletedTriggers.Remove(step.trigger);
            AdvanceStep();
            return;
        }

        clickToContinueButton.SetActive(false);
        StopAllCoroutines();
        StartCoroutine(TypeText(step.text));
    }

    void Update()
    {
        if (!isActive) return;

        if (currentStep < steps.Length)
        {
            TutorialStep step = steps[currentStep];
            bool hasArrow = step.arrowTarget != null || step.useFixedPosition;

            if (hasArrow)
            {
                if (step.useFixedPosition)
                    SetArrowFixed(step.fixedScreenPosition, step.arrowRotation);
                else
                    UpdateArrowPosition(step.arrowTarget, step.arrowOffset, step.arrowRotation);

                arrowBasePosition = arrowImage.rectTransform.position;
                arrowBobTimer += Time.deltaTime * arrowBobSpeed;

                float rotation = step.arrowRotation;
                Vector3 bobDir;

                if (Mathf.Abs(Mathf.DeltaAngle(rotation, -90f)) < 45f)
                    bobDir = Vector3.up;
                else if (Mathf.Abs(Mathf.DeltaAngle(rotation, 90f)) < 45f)
                    bobDir = Vector3.down;
                else if (Mathf.Abs(Mathf.DeltaAngle(rotation, 180f)) < 45f)
                    bobDir = Vector3.left;
                else
                    bobDir = Vector3.right;

                arrowImage.rectTransform.position = arrowBasePosition +
                    bobDir * (Mathf.Sin(arrowBobTimer) * arrowBobAmount);
            }
        }

        if (!waitingForClick) return;

        if (Input.GetMouseButtonDown(0) && isTyping)
        {
            StopAllCoroutines();
            tutorialText.text = steps[currentStep].text;
            isTyping = false;
            InteractionManager.IsLocked = false;
            clickToContinueButton.SetActive(true);
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

    // ── Public Helpers ─────────────────────────────
    // ── Public Helpers ─────────────────────────────

    public bool IsTutorialActive() => isActive;
    public bool IsDoorStepReached() => !isActive || currentStep >= 18;
    public bool IsCantServeAllowed() => !isActive || currentStep >= CANT_SERVE_STEP;
    public bool IsRestockAllowed() => !isActive || currentStep >= 28;
    public bool IsCustomerSelectionAllowed()
    {
        if (!isActive) return true;
        if (currentStep >= steps.Length) return true;
        if (steps[currentStep].trigger == TutorialTrigger.Click) return false;
        if (steps[currentStep].trigger == TutorialTrigger.RestockUsed) return false;
        return true;
    }

    // ── Trigger Hooks ──────────────────────────────

    public void OnStorageEntered() => TryAdvance(TutorialTrigger.StorageEntered);
    public void OnCobwebOpened() => TryAdvance(TutorialTrigger.CobwebOpened);
    public void OnStorageOpened() => TryAdvance(TutorialTrigger.StorageOpened);
    public void OnBugsCarried() => TryAdvance(TutorialTrigger.BugsCarried);
    public void OnBugsPlaced() => TryAdvance(TutorialTrigger.BugsPlaced);
    public void OnDoorOpened() => TryAdvance(TutorialTrigger.DoorOpened);
    public void OnRegisterOpened() => TryAdvance(TutorialTrigger.RegisterOpened);
    public void OnCustomerCalled() => TryAdvance(TutorialTrigger.CustomerCalled);
    public void OnCustomerServed() => TryAdvance(TutorialTrigger.CustomerServed);
    public void OnCantServeUsed() => TryAdvance(TutorialTrigger.CantServeUsed);
    public void OnRestockUsed() => TryAdvance(TutorialTrigger.RestockUsed);
    public void OnCobwebBought() => TryAdvance(TutorialTrigger.CobwebBought);
    public void OnGuaranteedFilled() => TryAdvance(TutorialTrigger.GuaranteedFilled);

    public void OnBugAddedToCart()
    {
        if (!isActive) return;
        if (steps[currentStep].trigger != TutorialTrigger.BugAddedToCart) return;
        bugAddedCount++;
        if (bugAddedCount >= 3)
        {
            bugAddedCount = 0;
            TryAdvance(TutorialTrigger.BugAddedToCart);
        }
    }

    void TryAdvance(TutorialTrigger trigger)
    {
        if (!isActive) return;

        if (!waitingForAction)
        {
            preCompletedTriggers.Add(trigger);
            return;
        }

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
        clickToContinueButton.SetActive(false);
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

    void SetArrowFixed(Vector2 screenPosition, float rotation)
    {
        arrowImage.rectTransform.position = new Vector3(screenPosition.x, screenPosition.y, 0f);
        arrowImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, rotation);
    }

    // ── Typewriter ─────────────────────────────────

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        tutorialText.text = "";
        clickToContinueButton.SetActive(false);

        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        InteractionManager.IsLocked = false;
        if (waitingForClick)
            clickToContinueButton.SetActive(true);
    }
}