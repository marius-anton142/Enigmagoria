using System.Collections;
using UnityEngine;

public class CameraRigController : MonoBehaviour
{
    public GameObject objectToFollow;

    public float speed = 2.0f;
    public float lerpDuration = 1f;
    public bool spawnedPlayer = false;
    public float cameraShiftAmount = 3f;
    public float shiftDelay = 0.1f;
    public float releaseDelay = 0.5f;
    public float verticalShiftMultiplier = 1.5f;

    private Vector3 additionalOffset = Vector3.zero;
    private bool isShiftingCamera = false;
    private Vector3 holdDirection = Vector3.zero;
    private Coroutine shiftCoroutine;
    private Coroutine delayCoroutine;
    private Coroutine releaseCoroutine;
    private bool isDelayed = false;

    private void Start()
    {
        Application.targetFrameRate = 120;
    }

    void FixedUpdate()
    {
        float interpolation = speed * Time.deltaTime;

        Vector3 position = transform.position;
        position.x = Mathf.Lerp(transform.position.x, objectToFollow.transform.position.x + additionalOffset.x, interpolation);
        position.y = Mathf.Lerp(transform.position.y, objectToFollow.transform.position.y + additionalOffset.y, interpolation);
        transform.position = position;
    }

    public void OnArrowHeld(Vector3 direction)
    {
        if (holdDirection == direction)
            return;

        if (delayCoroutine != null) StopCoroutine(delayCoroutine);
        if (releaseCoroutine != null) StopCoroutine(releaseCoroutine);

        holdDirection = direction;
        delayCoroutine = StartCoroutine(DelayedShift(direction));
    }

    public void OnArrowReleased()
    {
        holdDirection = Vector3.zero;

        if (delayCoroutine != null) StopCoroutine(delayCoroutine);
        if (shiftCoroutine != null) StopCoroutine(shiftCoroutine);

        releaseCoroutine = StartCoroutine(DelayedReset());
    }

    private IEnumerator DelayedShift(Vector3 direction)
    {
        isDelayed = true;
        yield return new WaitForSeconds(shiftDelay);
        isDelayed = false;

        Vector3 targetOffset = direction.normalized * cameraShiftAmount;
        if (Mathf.Abs(direction.y) > 0) targetOffset.y *= verticalShiftMultiplier;

        if (shiftCoroutine != null) StopCoroutine(shiftCoroutine);
        shiftCoroutine = StartCoroutine(SmoothShiftCamera(targetOffset));
    }

    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(releaseDelay);

        if (shiftCoroutine != null) StopCoroutine(shiftCoroutine);
        shiftCoroutine = StartCoroutine(SmoothShiftCamera(Vector3.zero));
    }

    private IEnumerator SmoothShiftCamera(Vector3 targetOffset)
    {
        isShiftingCamera = true;
        Vector3 initialOffset = additionalOffset;
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / lerpDuration);
            additionalOffset = Vector3.Lerp(initialOffset, targetOffset, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        additionalOffset = targetOffset;
        isShiftingCamera = false;
    }
}
