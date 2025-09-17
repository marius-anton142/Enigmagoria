using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private Coroutine shakeCoroutine;

    [Header("Zoom Settings")]
    public float sizeA = 5f;
    public float sizeB = 8f;
    public float zoomDuration = 0.5f;

    private Camera cam;
    private bool zoomToggle = false;
    private Coroutine zoomCoroutine;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            zoomToggle = !zoomToggle;
            float targetSize = zoomToggle ? sizeB : sizeA;

            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(SmoothZoom(targetSize));
        }
    }

    private IEnumerator SmoothZoom(float targetSize)
    {
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cam.orthographicSize = Mathf.SmoothStep(startSize, targetSize, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }

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
            Vector3 baseLocalPos = Vector3.zero;

            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = baseLocalPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }
}
