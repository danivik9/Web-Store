using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CarryCameraFollow : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    void LateUpdate()
    {
        if (mainCamera == null) return;
        transform.SetPositionAndRotation(
            mainCamera.transform.position,
            mainCamera.transform.rotation
        );
    }
}