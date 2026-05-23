using UnityEngine;
using System.Collections;

public class DoorAnimator : MonoBehaviour
{
    [Header("Rotation")]
    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float speed = 5f;

    private bool isOpen = false;
    private Coroutine currentRoutine;

    public void Open(bool unused = true)
    {
        if (isOpen) return;
        isOpen = true;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(RotateTo(openAngle));
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(RotateTo(closedAngle));
    }

    public void ForceClose()
    {
        isOpen = false;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        transform.localRotation = Quaternion.Euler(0f, closedAngle, 0f);
    }

    IEnumerator RotateTo(float targetAngle)
    {
        Quaternion target = Quaternion.Euler(0f, targetAngle, 0f);
        while (Quaternion.Angle(transform.localRotation, target) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }
        transform.localRotation = target;
    }

    public bool IsOpen() => isOpen;
}