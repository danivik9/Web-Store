using UnityEngine;

public class ShelfSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public BugToken bugToken;
    private bool occupied = false;
    public bool isOccupied => occupied;

    [Header("Visuals")]
    public GameObject iconObject;
    public float hoverHeight = 0.3f;

    private SpriteRenderer spriteRenderer;

    public bool PlaceBug(BugToken token)
    {
        if (isOccupied) return false;
        bugToken = token;
        occupied = true;
        SpawnIcon();
        return true;
    }

    public BugToken RemoveBug()
    {
        if (!isOccupied) return null;
        BugToken removed = bugToken;
        bugToken = null;
        occupied = false;
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
        iconObject.transform.localScale = Vector3.one * 0.15f; // add this line

        spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = bugToken.bugType.icon;
        spriteRenderer.sortingOrder = 1;

        iconObject.AddComponent<BoxCollider>();
        iconObject.AddComponent<FaceCamera>();

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