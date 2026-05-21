using UnityEngine;
using System.Collections;

public class DoorAnimator : MonoBehaviour
{
    [Header("Rotation")]
    public float closedAngle = 0f;
    public float openAngle = 90f;
    public float speed = 5f;
    public float autoCloseDelay = 1.5f;

    private bool isOpen = false;
    private Coroutine currentRoutine;
    private Coroutine autoCloseRoutine;

    public void Open(bool autoClose = true)
    {
        // Reset auto-close timer even if already open
        if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);

        if (!isOpen)
        {
            isOpen = true;
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(RotateTo(openAngle));
        }

        if (autoClose)
            autoCloseRoutine = StartCoroutine(AutoClose());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);

        currentRoutine = StartCoroutine(RotateTo(closedAngle));
    }

    public void ForceClose()
    {
        isOpen = false;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);
        transform.localRotation = Quaternion.Euler(0f, closedAngle, 0f);
    }

    public void OpenForDuration(float duration)
    {
        if (autoCloseRoutine != null) StopCoroutine(autoCloseRoutine);

        if (!isOpen)
        {
            isOpen = true;
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(RotateTo(openAngle));
        }

        autoCloseRoutine = StartCoroutine(CloseAfter(duration));
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

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        Close();
    }

    IEnumerator CloseAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    public bool IsOpen() => isOpen;
}