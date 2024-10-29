using UnityEngine;
using System.Collections.Generic;

public class PlantScript : MonoBehaviour
{
    public float rotationAngleTouch = 20f; // The angle to rotate to when touched by the player
    public float idleRotationAngle = 10f; // The angle to rotate back and forth when idle
    public float idleRotationSpeed = 2f; // The speed of the idle rotation
    public float touchRotationSpeed = 4f; // The speed of the rotation when touched
    public float smoothTime = 0.3f; // The time it takes to reach the target angle smoothly

    private float targetAngle;
    private float currentAngle;
    private float velocity = 0f;
    private bool isReturningToZero = false;

    [SerializeField] private Transform playerTransform;
    private bool isTouchingPlayer;
    private bool isIdleIncreasing = true;

    void Update()
    {
        if (isTouchingPlayer && !isReturningToZero)
        {
            // Check if the player has stopped moving
            if (playerTransform.hasChanged)
            {
                // Rotate to the target angle determined at the time of the player entering the trigger
                if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
                {
                    isReturningToZero = true;
                    targetAngle = 0f;
                    playerTransform.hasChanged = false; // Reset the transform change detection
                }
            }
            else
            {
                // If the player stops moving, go back to idle rotation
                isReturningToZero = false;
                isTouchingPlayer = false;
                HandleIdleRotation();
            }
        }
        else
        {
            HandleIdleRotation();
        }

        // Smoothly rotate towards the target angle using SmoothDamp
        float speed = isTouchingPlayer ? touchRotationSpeed : idleRotationSpeed;
        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref velocity, smoothTime, speed);

        // Apply the rotation to the plant
        transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

        // Return to idle state once the return to zero is complete
        if (isReturningToZero && Mathf.Abs(currentAngle) < 0.1f)
        {
            isReturningToZero = false;
        }
    }

    private void HandleIdleRotation()
    {
        if (isIdleIncreasing)
        {
            targetAngle = idleRotationAngle;
            if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
            {
                isIdleIncreasing = false;
            }
        }
        else
        {
            targetAngle = 0;
            if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
            {
                isIdleIncreasing = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            isTouchingPlayer = true;
            isReturningToZero = false;

            // Determine the rotation direction based on the player's position relative to the plant
            targetAngle = playerTransform.position.x > transform.position.x ? rotationAngleTouch : -rotationAngleTouch;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isTouchingPlayer = false;

            // Ensure the plant returns to 0 when the player leaves
            isReturningToZero = true;
            targetAngle = 0f;
        }
    }
}