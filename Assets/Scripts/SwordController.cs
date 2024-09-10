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
    public float attackRange = 2f; // The range of the sword attack
    public float damage = 50;
    public float attackSize = 1.5f;
    public float knockbackForce = 10.0f;
    public float knockTime = 1f;

    public float baseAngle;
    public float adjustedAngle;
    public float additionalAngleValue = 20f;

    public LayerMask enemyLayer;
    private float targetAngleOffset; // Target angle for the current attack phase
    public float additionalAngle = 0f; // Additional angle to be applied during an attack

    public bool attacked;
    private float additionalAngleLatest = 0f;

    private float lastAttackTime = -1f; // Track the last attack time
    private bool isAttacking = false; // Is the sword currently attacking
    private Vector3 direction = new Vector3(0, 0, 0);

    private bool isResetting = false;
    private float resetStartTime; // Time when resetting starts
    private HashSet<GameObject> hitEntities;

    void Update()
    {
        HandleInput();

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
        }
        else
        {
            HandleResetting();
        }

        FollowInput();
    }

    void HandleInput()
    {
        // Handle mouse input for non-touch devices
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject() && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttack();
            hitEntities = new HashSet<GameObject>();
        }

        // Handle touch input for touch-enabled devices
        if (Input.touchCount > 0 && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && !IsPointerOverUIObject(touch))
                {
                    StartAttack();
                    break; // Exit the loop once a suitable touch is found and used to start an attack
                }
            }
        }
    }

    void StartAttack()
    {
        lastAttackTime = Time.time;
        isAttacking = true;
        isResetting = false;

        // Initialize additional logic for starting the attack here if necessary
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
            attacked = false;
        } else if (!attacked && percentageComplete >= 0.2f)
        {
            Vector3 attackPoint = player.transform.position + direction * attackRange;
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange, enemyLayer);
            attacked = true;

            foreach (Collider2D collider in hitEnemies)
            {
                GameObject hitObject = collider?.gameObject;

                if (collider == null || hitEntities.Contains(hitObject) || hitObject == gameObject)
                    continue;

                hitEntities.Add(hitObject);
                EnemyAI enemy = collider.GetComponent<EnemyAI>();
                Vector2 knockbackDirection = (hitObject.transform.position - player.transform.position).normalized;

                if (enemy != null)
                {
                    enemy.ApplyKnockback(knockbackDirection, knockbackForce, knockTime, damage);
                }

                //Debug.Log("We hit " + enemy.name);
            }
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

    void FollowInput()
    {
        Vector3 inputPosition = Vector3.zero;

        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began) && !IsPointerOverUIObject(touch))
                {
                    inputPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    inputPosition.z = 0;
                    direction = (inputPosition - player.transform.position).normalized;
                    break;
                }
            }
        }
        else if (!IsPointerOverUIObject())
        {
            inputPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputPosition.z = 0;
            direction = (inputPosition - player.transform.position).normalized;
        }

        if (direction != Vector3.zero)
        {
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
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(touch.position.x, touch.position.y)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            // Calculate the attack point based on the player's position and attack direction
            Vector3 attackPoint = player.transform.position + direction * attackRange;

            // Set the color for the Gizmo
            Gizmos.color = Color.red;

            // Draw a wireframe sphere at the attack point to visualize the attack range
            Gizmos.DrawWireSphere(attackPoint, attackSize);
        }
    }
}
