using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float followSpeed = 8f;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,  // follows spider on X
            transform.position.y,           // locked, never moves
            transform.position.z            // locked, never moves
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}