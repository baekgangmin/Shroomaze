using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -4.5f);
    public float followSpeed = 8f;
    public float lookAtHeightOffset = 1f;

    Vector3 shakeOffset;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime) + shakeOffset;
        transform.LookAt(target.position + Vector3.up * lookAtHeightOffset);
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float falloff = 1f - (t / duration);
            shakeOffset = Random.insideUnitSphere * magnitude * falloff;
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
