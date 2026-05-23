using UnityEngine;
using System.Collections;

public class DoorTrigger : MonoBehaviour
{
    public DoorAnimator doorAnimator;
    public bool reactToPlayer = true;
    public bool reactToCustomers = true;

    [Header("Mode")]
    public bool useStayOpen = true;
    public float autoCloseDelay = 1.5f;

    private int occupantCount = 0;
    private Coroutine closeCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (!IsRelevant(other)) return;

        if (useStayOpen)
        {
            occupantCount++;
            if (doorAnimator != null)
                doorAnimator.Open();
        }
        else
        {
            if (closeCoroutine != null)
                StopCoroutine(closeCoroutine);

            if (doorAnimator != null)
                doorAnimator.Open();

            closeCoroutine = StartCoroutine(CloseAfterDelay());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!useStayOpen) return;
        if (!IsRelevant(other)) return;

        occupantCount--;
        if (occupantCount <= 0)
        {
            occupantCount = 0;
            if (doorAnimator != null)
                doorAnimator.Close();
        }
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        if (doorAnimator != null)
            doorAnimator.Close();
    }

    bool IsRelevant(Collider other)
    {
        if (reactToPlayer && other.GetComponent<SpiderMovement>() != null)
            return true;
        if (reactToCustomers && other.GetComponent<Customer>() != null)
            return true;
        return false;
    }

    public void ResetTrigger()
    {
        occupantCount = 0;
        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);
    }
}