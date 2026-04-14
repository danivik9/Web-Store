using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CarryDisplay : MonoBehaviour
{
    public static CarryDisplay Instance;

    [Header("Settings")]
    public float stackSpacing = 0.4f;
    public float heightAboveSpider = 1.2f;

    [Header("Prefab")]
    public GameObject carryRowPrefab;

    private List<GameObject> activeRows = new List<GameObject>();
    private Camera mainCamera;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = mainCamera.transform.rotation;
    }

    public void UpdateDisplay(Dictionary<BugType, List<BugToken>> carriedBugs)
    {
        foreach (GameObject row in activeRows)
            Destroy(row);
        activeRows.Clear();

        if (carriedBugs.Count == 0) return;

        int rowIndex = 0;
        foreach (var kvp in carriedBugs)
        {
            BugType bugType = kvp.Key;
            int count = kvp.Value.Count;

            GameObject row = Instantiate(carryRowPrefab, transform);
            row.transform.localPosition = new Vector3(0,
                heightAboveSpider + (rowIndex * stackSpacing), 0);

            var img = row.transform.Find("BugSprite")?.GetComponent<SpriteRenderer>();
            if (img != null)
            {
                img.sprite = bugType.icon;
                img.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            }

            var txt = row.transform.Find("CountText")?.GetComponent<TextMeshPro>();
            if (txt != null)
            {
                txt.text = $"{count}x";
                txt.transform.localPosition = new Vector3(-0.9f, 0f, 0f);
            }

            activeRows.Add(row);
            rowIndex++;
        }
    }
}