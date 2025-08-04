using System.Collections;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    public GameObject objectToFollow;

    public float speed = 2.0f;
    public float newSize = 3.5f;
    public float lerpDuration = 1f;
    public bool spawnedPlayer = false;
    public float cameraShiftAmount = 3f; // Amount to shift camera
    public float shiftDelay = 0.5f; // Delay before camera starts shifting when holding
    public float releaseDelay = 0.5f; // Delay before camera lerps back after releasing
    public float verticalShiftMultiplier = 1.5f; // Multiplier for vertical movement

    private Camera mainCamera;
    private Vector3 cameraOriginalOffset;
    private Vector3 additionalOffset = Vector3.zero;
    private bool isShiftingCamera = false;
    private Vector3 holdDirection = Vector3.zero;
    private Coroutine shiftCoroutine; // Track the currently running shift coroutine
    private Coroutine delayCoroutine; // Track the delay coroutine
    private Coroutine releaseCoroutine; // Track the release delay coroutine
    private bool isDelayed = false; // Indicates if a shift delay is in progress

    private void Start()
    {
        mainCamera = Camera.main;
        cameraOriginalOffset = transform.position - objectToFollow.transform.position;

        Application.targetFrameRate = 120;
    }

    void FixedUpdate()
    {
        float interpolation = speed * Time.deltaTime;

        // Handle camera movement towards the player with any additional offset
        Vector3 position = this.transform.position;
        position.y = Mathf.Lerp(this.transform.position.y, objectToFollow.transform.position.y + additionalOffset.y, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, objectToFollow.transform.position.x + additionalOffset.x, interpolation);
        this.transform.position = position;

        // Handle camera size lerping when player is spawned
        if (spawnedPlayer && !isShiftingCamera)
        {
            StartCoroutine(LerpCameraSize());
            spawnedPlayer = false;
        }
    }

    public void OnArrowHeld(Vector3 direction)
    {
        if (holdDirection == direction)
            return;

        // Cancel any ongoing delay or release coroutine
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        if (releaseCoroutine != null)
        {
            StopCoroutine(releaseCoroutine);
            releaseCoroutine = null;
        }

        holdDirection = direction;
        delayCoroutine = StartCoroutine(DelayedShift(direction));
    }

    public void OnArrowReleased()
    {
        holdDirection = Vector3.zero;

        // Cancel any ongoing delay or shift coroutine
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
            delayCoroutine = null;
        }
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
            shiftCoroutine = null;
        }

        // Start a coroutine to delay the camera reset
        releaseCoroutine = StartCoroutine(DelayedReset());
    }

    private IEnumerator DelayedShift(Vector3 direction)
    {
        isDelayed = true;

        // Wait for the specified delay
        yield return new WaitForSeconds(shiftDelay);

        isDelayed = false;

        // Adjust the shift amount based on the direction
        Vector3 targetOffset = direction.normalized * cameraShiftAmount;

        // Apply vertical multiplier if moving up or down
        if (Mathf.Abs(direction.y) > 0)
        {
            targetOffset.y *= verticalShiftMultiplier;
        }

        // Cancel any running shift coroutine before starting a new one
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
        }

        shiftCoroutine = StartCoroutine(SmoothShiftCamera(targetOffset));
    }

    private IEnumerator DelayedReset()
    {
        // Wait for the specified delay before resetting
        yield return new WaitForSeconds(releaseDelay);

        // Start lerping back to the original position
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
        }

        shiftCoroutine = StartCoroutine(SmoothShiftCamera(Vector3.zero));
    }

    private IEnumerator SmoothShiftCamera(Vector3 targetOffset)
    {
        isShiftingCamera = true;

        Vector3 initialOffset = additionalOffset;
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            additionalOffset = Vector3.Lerp(initialOffset, targetOffset, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        additionalOffset = targetOffset;
        isShiftingCamera = false;
    }

    private IEnumerator LerpCameraSize()
    {
        isShiftingCamera = true;

        float elapsedTime = 0f;
        float startSize = mainCamera.orthographicSize;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.orthographicSize = Mathf.Lerp(startSize, newSize, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = newSize;
        isShiftingCamera = false;
    }

    //Camera effects
    private Coroutine shakeCoroutine;

    public void Shake(float duration = 0.07f, float magnitude = 0.07f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float interpolation = speed * Time.deltaTime;

            // Get latest follow position every frame
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(position.y, objectToFollow.transform.position.y + additionalOffset.y, interpolation);
            position.x = Mathf.Lerp(position.x, objectToFollow.transform.position.x + additionalOffset.x, interpolation);

            Vector3 originalPos = position;

            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
