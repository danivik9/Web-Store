using UnityEngine;

public class StorageRoomTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.GetComponent<SpiderMovement>()) return;
        triggered = true;
        TutorialManager.Instance?.OnStorageEntered();
    }
}