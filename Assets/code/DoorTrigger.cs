using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public DoorAnimator doorAnimator;
    public bool reactToPlayer = true;
    public bool reactToCustomers = true;

    void OnTriggerEnter(Collider other)
    {
        if (reactToPlayer && other.GetComponent<SpiderMovement>() != null)
        {
            if (doorAnimator != null) doorAnimator.Open();
            return;
        }

        if (reactToCustomers && other.GetComponent<Customer>() != null)
        {
            if (doorAnimator != null) doorAnimator.Open();
        }
    }
}