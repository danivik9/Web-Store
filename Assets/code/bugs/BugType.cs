using UnityEngine;

[CreateAssetMenu(fileName = "NewBugType", menuName = "WebStore/Bug Type")]
public class BugType : ScriptableObject
{
    public string bugName;
    public int expiryDays;      // 1, 2, 3, 4, 99 (moths)
    public float buyPrice;
    public float sellPrice;
    public Sprite icon;         // your 2D icon
}
