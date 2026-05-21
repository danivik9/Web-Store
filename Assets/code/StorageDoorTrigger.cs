using UnityEngine;

public class StorageDoorTrigger : MonoBehaviour
{
    public DoorAnimator doorAnimator;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SpiderMovement>() == null) return;
        if (doorAnimator != null)
            doorAnimator.Open();
    }
}