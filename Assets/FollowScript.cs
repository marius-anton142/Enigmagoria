using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    public GameObject objectToFollow;

    public float speed = 2.0f;
    public float newSize = 3.5f;
    public float lerpDuration = 1f;
    public bool spawnedPlayer = false;

    private Camera mainCamera;
    private float initialSize;
    private bool isLerping = false;

    private void Start()
    {
        mainCamera = Camera.main;
        initialSize = mainCamera.orthographicSize;

        Application.targetFrameRate = 120;
    }

    void FixedUpdate()
    {
        float interpolation = speed * Time.deltaTime;

        if (spawnedPlayer && !isLerping)
        {
            StartCoroutine(LerpCameraSize());
            spawnedPlayer = false;
        }

        Vector3 position = this.transform.position;
        position.y = Mathf.Lerp(this.transform.position.y, objectToFollow.transform.position.y, interpolation);
        position.x = Mathf.Lerp(this.transform.position.x, objectToFollow.transform.position.x, interpolation);

        this.transform.position = position;
    }

    private System.Collections.IEnumerator LerpCameraSize()
    {
        isLerping = true;

        float elapsedTime = 0f;
        float startSize = mainCamera.orthographicSize;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // Apply easing function

            mainCamera.orthographicSize = Mathf.Lerp(startSize, newSize, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = newSize;
        isLerping = false;
    }
}
