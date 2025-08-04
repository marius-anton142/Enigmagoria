using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
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
            Vector3 baseLocalPos = Vector3.zero; // we want to shake around local (0,0)

            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = baseLocalPos + new Vector3(offsetX, offsetY, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }
}
