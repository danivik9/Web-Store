using UnityEngine;
using System.Collections.Generic;

public class ShelfHighlighter : MonoBehaviour
{
    public static ShelfHighlighter Instance;

    [Header("Arrow Settings")]
    public Sprite arrowSprite;
    public Color arrowColor = Color.white;
    public float arrowHeight = 1.5f;
    public float arrowScale = 0.4f;
    public float bobSpeed = 4f;
    public float bobAmount = 0.15f;

    private List<GameObject> activeArrows = new List<GameObject>();
    private List<Vector3> basePositions = new List<Vector3>();
    private float bobTimer = 0f;
    private Camera mainCamera;
    private Shelf[] cachedShelves;

    void Awake() { Instance = this; }

    void Start() { mainCamera = Camera.main; }

    void LateUpdate()
    {
        if (activeArrows.Count == 0) return;

        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobAmount;

        for (int i = 0; i < activeArrows.Count; i++)
        {
            if (activeArrows[i] == null) continue;
            activeArrows[i].transform.position = basePositions[i] + Vector3.up * bobOffset;
            activeArrows[i].transform.rotation = mainCamera.transform.rotation
                * Quaternion.Euler(0f, 0f, -90f);
        }
    }

    public void UpdateHighlights(Dictionary<BugType, List<BugToken>> carriedBugs)
    {
        ClearArrows();
        if (carriedBugs.Count == 0) return;

        if (cachedShelves == null)
            cachedShelves = FindObjectsOfType<Shelf>();

        foreach (var kvp in carriedBugs)
        {
            BugType bugType = kvp.Key;
            int carriedCount = kvp.Value.Count;

            foreach (Shelf shelf in cachedShelves)
            {
                if (shelf.acceptedBugType != bugType) continue;

                int arrowsPlaced = 0;
                foreach (ShelfSlot slot in shelf.slots)
                {
                    if (arrowsPlaced >= carriedCount) break;
                    if (slot.IsEmpty())
                    {
                        Vector3 pos = slot.transform.position + Vector3.up * arrowHeight;
                        SpawnArrow(pos);
                        arrowsPlaced++;
                    }
                }
                break;
            }
        }
    }

    void SpawnArrow(Vector3 position)
    {
        GameObject arrow = new GameObject("ShelfArrow");
        arrow.transform.position = position;

        SpriteRenderer sr = arrow.AddComponent<SpriteRenderer>();
        sr.sprite = arrowSprite;
        sr.color = arrowColor;
        arrow.transform.localScale = Vector3.one * arrowScale;

        activeArrows.Add(arrow);
        basePositions.Add(position);
    }

    public void ClearArrows()
    {
        foreach (GameObject arrow in activeArrows)
            if (arrow != null) Destroy(arrow);
        activeArrows.Clear();
        basePositions.Clear();
        bobTimer = 0f;
    }
}