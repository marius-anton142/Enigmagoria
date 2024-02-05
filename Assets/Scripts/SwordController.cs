using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SwordController : MonoBehaviour
{
    public GameObject player; // Assign the player GameObject in the inspector
    public float distanceFromPlayer; // Distance between player and sword
    public float angleOffset; // Initial angle offset
    private bool isAttackPhaseOne = true; // Tracks the attack phase

    public float attackDuration = 0.5f; // Duration of the attack animation
    public float attackCooldown = 1f; // Cooldown time between attacks

    public float baseAngle;
    public float adjustedAngle;
    public float additionalAngleValue = 20f;

    private float lastAttackTime = -1f; // Track the last attack time
    private bool isAttacking = false; // Is the sword currently attacking
    private float targetAngleOffset; // Target angle for the current attack phase
    public float additionalAngle = 0f; // Additional angle to be applied during an attack
    private float additionalAngleLatest = 0f;

    private bool isResetting = false;
    private float resetStartTime; // Time when resetting starts
    private Vector3 direction = new Vector3(0, 0, 0);

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown && !isAttacking && !IsPointerOverUIObject())
        {
            isResetting = false;
            StartAttack();
        }

        if (isAttacking)
        {
            float timeSinceStarted = Time.time - lastAttackTime;
            float percentageComplete = timeSinceStarted / attackDuration;

            // Use SmoothStep for a smoother transition of additionalAngle
            if (isAttackPhaseOne)
            {
                additionalAngle = Mathf.SmoothStep(additionalAngleLatest, additionalAngleValue, percentageComplete);
            }
            else
            {
                additionalAngle = Mathf.SmoothStep(additionalAngleLatest, additionalAngleValue * -1, percentageComplete);
            }

            PerformAttack();
        } else
        {
            HandleResetting();
        }

        FollowMouse();
    }

    void StartAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        isResetting = false;

        // Determine the target angle offset based on the attack phase
        if (isAttackPhaseOne)
        {
            targetAngleOffset = angleOffset - 240; // Adjust values as needed
        }
        else
        {
            targetAngleOffset = angleOffset + 240; // Adjust values as needed
        }

        // Toggle the attack phase for the next attack
        isAttackPhaseOne = !isAttackPhaseOne;
    }

    void PerformAttack()
    {
        float percentageComplete = (Time.time - lastAttackTime) / attackDuration;

        // Smoothly transition to the target angle offset
        angleOffset = Mathf.SmoothStep(angleOffset, targetAngleOffset, percentageComplete);

        // Complete the attack once the duration is finished
        if (percentageComplete >= 1.0f)
        {
            isAttacking = false;
            isResetting = true; // Start resetting after the attack
            resetStartTime = Time.time; // Mark the start time for the reset
        }
    }

    void HandleResetting()
    {
        if (isResetting)
        {
            float timeSinceStarted = Time.time - resetStartTime;
            // Use the same duration for resetting as the attack for consistency
            float percentageComplete = timeSinceStarted / attackDuration;

            additionalAngle = Mathf.SmoothStep(additionalAngle, 0, percentageComplete);
            additionalAngleLatest = additionalAngle;

            if (percentageComplete >= 1.0f)
            {
                isResetting = false; // Stop resetting once done
                additionalAngle = 0; // Ensure it's precisely 0
            }
        }
    }

    void FollowMouse()
    {
        if (!IsPointerOverUIObject())
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;

            direction = (mousePosition - player.transform.position).normalized;
        }
        // Calculate the base angle from the direction
        baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Add the additionalAngle to the base angle for both position and rotation adjustment
        adjustedAngle = baseAngle + additionalAngle;

        // Convert the adjusted angle back to radians for direction calculation
        float adjustedAngleRadians = adjustedAngle * Mathf.Deg2Rad;

        // Create a new direction vector based on the adjusted angle
        Vector3 adjustedDirection = new Vector3(Mathf.Cos(adjustedAngleRadians), Mathf.Sin(adjustedAngleRadians), 0);

        // Apply the additional angle for both position and rotation
        transform.position = player.transform.position + adjustedDirection * distanceFromPlayer;

        // Adjust the rotation to include the additional angle as well
        float angleForRotation = adjustedAngle + angleOffset; // Include additionalAngle in rotation
        transform.rotation = Quaternion.AngleAxis(angleForRotation, Vector3.forward);
    }

    private bool IsPointerOverUIObject()
    {
        // Check for current touch (mobile) or mouse input position
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        return results.Count > 0; // Return true if any UI element was hit
    }
}
