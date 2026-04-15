using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public float followSpeed = 8f;
    public float returnSpeed = 3f;
    public Vector3 offset;

    private bool isPanned = false;
    private Vector3 pannedPosition;
    private Quaternion pannedRotation;
    private Quaternion defaultRotation;
    private float originalY;
    private float originalZ;

    void Start()
    {
        defaultRotation = transform.rotation;
        originalY = 12.67f;
        originalZ = -4.59f;
    }

    void LateUpdate()
    {
        if (isPanned)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                pannedPosition,
                followSpeed * Time.deltaTime
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                pannedRotation,
                followSpeed * Time.deltaTime
            );
        }
        else
        {
            if (target == null) return;

            Vector3 targetPosition = new Vector3(
                target.position.x + offset.x,
                originalY,
                originalZ
            );

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                returnSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                defaultRotation,
                returnSpeed * Time.deltaTime
            );
        }
    }

    public void PanToPosition(Vector3 position, Quaternion rotation)
    {
        pannedPosition = position;
        pannedRotation = rotation;
        isPanned = true;
    }

    public void ReturnToFollow()
    {
        isPanned = false;
    }
}