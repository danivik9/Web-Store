using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public BugToken bugToken;
    public bool isOccupied => bugToken != null && bugToken.bugType != null;

    [Header("Visuals")]
    public GameObject iconObject;
    public float hoverHeight = 0.3f;
    public float targetSize = 0.8f;

    private SpriteRenderer spriteRenderer;

    public bool IsEmpty() => !isOccupied;

    public void ClearSlot()
    {
        bugToken = null;
        ClearIcon();
    }

    public bool PlaceBug(BugToken token)
    {
        if (isOccupied) return false;
        bugToken = token;
        SpawnIcon();
        return true;
    }

    public BugToken RemoveBug()
    {
        if (!isOccupied) return null;
        BugToken removed = bugToken;
        bugToken = null;
        ClearIcon();
        return removed;
    }

    void SpawnIcon()
    {
        if (iconObject != null)
            Destroy(iconObject);

        iconObject = new GameObject("BugIcon");
        iconObject.transform.SetParent(transform);
        iconObject.transform.localPosition = Vector3.up * hoverHeight;

        spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = bugToken.bugType.icon;
        spriteRenderer.sortingOrder = 1;

        // ── Normalize size so all bugs look the same ──
        Bounds bounds = spriteRenderer.bounds;
        float largestSide = Mathf.Max(bounds.size.x, bounds.size.y);
        if (largestSide > 0f)
        {
            float uniformScale = targetSize / largestSide;
            iconObject.transform.localScale = Vector3.one * uniformScale;
        }

        // ── Fixed-size collider — thick enough for camera raycast ──
        BoxCollider col = iconObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
        float scale = iconObject.transform.localScale.x;
        float colSize = targetSize / scale;
        col.size = new Vector3(colSize * 2f, colSize * 2f, colSize * 2f);

        iconObject.AddComponent<FaceCamera>();

        var hover = iconObject.AddComponent<BugIconHover>();
        hover.bugToken = bugToken;
    }

    void ClearIcon()
    {
        if (iconObject != null)
            Destroy(iconObject);
        iconObject = null;
    }
}