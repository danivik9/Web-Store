using UnityEngine;
using UnityEngine.EventSystems;

public class ShelfSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public BugToken bugToken;       // null = empty slot
    public bool isOccupied => bugToken != null;

    [Header("Visuals")]
    public GameObject iconObject;   // spawned at runtime
    public float hoverHeight = 0.3f;

    private SpriteRenderer spriteRenderer;

    // Place a bug token in this slot
    public bool PlaceBug(BugToken token)
    {
        if (isOccupied) return false;

        bugToken = token;
        SpawnIcon();
        return true;
    }

    // Remove the bug token from this slot
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

        // Create a new GameObject with a SpriteRenderer
        iconObject = new GameObject("BugIcon");
        iconObject.transform.SetParent(transform);
        iconObject.transform.localPosition = Vector3.up * hoverHeight;

        // Add sprite renderer
        spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = bugToken.bugType.icon;
        spriteRenderer.sortingOrder = 1;

        // Face the camera
        iconObject.AddComponent<FaceCamera>();

        // Add hover detection
        var hover = iconObject.AddComponent<BugIconHover>();
        hover.slot = this;
    }

    void ClearIcon()
    {
        if (iconObject != null)
            Destroy(iconObject);
        iconObject = null;
    }
}
