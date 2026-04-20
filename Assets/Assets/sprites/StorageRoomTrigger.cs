using UnityEngine;

public class StorageRoomTrigger : MonoBehaviour
{
    // ── No 'triggered' guard — fires every entry so tutorial step
    //    is never missed if the player wanders in early. TryAdvance
    //    in TutorialManager only reacts when the correct step is active.

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<SpiderMovement>()) return;
        TutorialManager.Instance?.OnStorageEntered();
    }
}