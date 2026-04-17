using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    public float moveSpeed = 2f;

    private NavMeshAgent agent;
    private CustomerCard assignedCard;
    private int waitingSpotIndex;

    public CustomerCard AssignedCard => assignedCard;
    public int WaitingSpotIndex => waitingSpotIndex;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = moveSpeed;
    }

    public void Setup(CustomerCard card, int spotIndex)
    {
        assignedCard = card;
        waitingSpotIndex = spotIndex;
    }

    public void MoveTo(Vector3 destination, System.Action onArrived = null)
    {
        StopAllCoroutines(); // ← stop any existing movement first
        StartCoroutine(MoveCoroutine(destination, onArrived));
    }

    IEnumerator MoveCoroutine(Vector3 destination, System.Action onArrived)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
            yield return new WaitForSeconds(0.1f);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.1f)
                yield return null;
        }
        else
        {
            destination.y = transform.position.y;
            while (Vector3.Distance(transform.position, destination) > 0.1f)
            {
                Vector3 dir = (destination - transform.position).normalized;
                if (dir != Vector3.zero)
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(dir),
                        10f * Time.deltaTime
                    );
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        transform.position = destination;
        onArrived?.Invoke();
    }
}