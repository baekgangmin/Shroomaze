using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;
    public float openDistance = 1.4f;
    public float slideDuration = 0.6f;

    Vector3 leftClosedLocalPos;
    Vector3 rightClosedLocalPos;
    Coroutine activeRoutine;

    void Awake()
    {
        if (doorLeft != null) leftClosedLocalPos = doorLeft.localPosition;
        if (doorRight != null) rightClosedLocalPos = doorRight.localPosition;
    }

    public void SnapOpen()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        if (doorLeft != null) doorLeft.localPosition = leftClosedLocalPos + Vector3.right * openDistance;
        if (doorRight != null) doorRight.localPosition = rightClosedLocalPos - Vector3.right * openDistance;
    }

    public void SnapClosed()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        if (doorLeft != null) doorLeft.localPosition = leftClosedLocalPos;
        if (doorRight != null) doorRight.localPosition = rightClosedLocalPos;
    }

    public void Open()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Slide(leftClosedLocalPos + Vector3.right * openDistance,
                                             rightClosedLocalPos - Vector3.right * openDistance));
    }

    public void Close()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(Slide(leftClosedLocalPos, rightClosedLocalPos));
    }

    IEnumerator Slide(Vector3 leftTarget, Vector3 rightTarget)
    {
        Vector3 leftStart = doorLeft != null ? doorLeft.localPosition : Vector3.zero;
        Vector3 rightStart = doorRight != null ? doorRight.localPosition : Vector3.zero;
        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / slideDuration);
            float eased = Mathf.SmoothStep(0f, 1f, frac);
            if (doorLeft != null) doorLeft.localPosition = Vector3.Lerp(leftStart, leftTarget, eased);
            if (doorRight != null) doorRight.localPosition = Vector3.Lerp(rightStart, rightTarget, eased);
            yield return null;
        }
        if (doorLeft != null) doorLeft.localPosition = leftTarget;
        if (doorRight != null) doorRight.localPosition = rightTarget;
    }
}
