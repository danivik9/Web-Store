using UnityEngine;

[CreateAssetMenu(fileName = "NewCobwebCard", menuName = "WebStore/Cobweb Card")]
public class CobwebCard : ScriptableObject
{
    public float fruitFlyPrice;
    public float antPrice;
    public float mosquitoPrice;
    public float maggotPrice;
    public float mothPrice;

    public float GetPrice(BugType bugType)
    {
        if (bugType.bugName == "FruitFly") return fruitFlyPrice;
        if (bugType.bugName == "Ant") return antPrice;
        if (bugType.bugName == "Mosquito") return mosquitoPrice;
        if (bugType.bugName == "Maggot") return maggotPrice;
        if (bugType.bugName == "Moth") return mothPrice;
        return 0f;
    }
}