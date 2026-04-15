using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public BugToken bugToken;
    public bool isOccupied => bugToken != null;

    [Header("Visuals")]
    public GameObject iconObject;
    public float hoverHeight = 0.3f;

    private SpriteRenderer spriteRenderer;

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

        iconObject.AddComponent<BoxCollider>();
        iconObject.AddComponent<FaceCamera>();

        // Use BugToken directly now
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