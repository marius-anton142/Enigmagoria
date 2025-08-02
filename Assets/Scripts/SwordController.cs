using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class SwordController : MonoBehaviour
{
    public WeaponState weaponState = WeaponState.Free;
    public GameObject inventory;
    public GameObject player; // Assign the player GameObject in the inspector
    public float distanceFromPlayer; // Distance between player and sword
    public float angleOffset; // Initial angle offset
    private bool isAttackPhaseOne = true; // Tracks the attack phase

    public float attackDuration = 0.5f; // Duration of the attack animation
    public float resetDuration = 0.5f;
    public float attackCooldown = 1f; // Cooldown time between attacks
    public float attackRange = 2f; // The range of the sword attack
    public float attackSize = 1.5f;
    public float damage = 50;
    public float knockbackForce = 10.0f;
    public float knockTime = 1f;
    [SerializeField] private float cameraShakeDuration = 0.07f;
    [SerializeField] private float cameraShakeMagnitude = 0.06f;

    public float baseAngle;
    public float adjustedAngle;
    public float additionalAngleValue = 20f;

    [SerializeField] private GameObject slashPrefab;
    public List<Sprite> slashFrames;

    public LayerMask enemyLayer;
    private float targetAngleOffset; // Target angle for the current attack phase
    public float additionalAngle = 0f; // Additional angle to be applied during an attack

    public bool attacked;
    private float additionalAngleLatest = 0f;

    private float lastAttackTime = -1f; // Track the last attack time
    private bool isAttacking = false; // Is the sword currently attacking
    private Vector3 direction = new Vector3(0, 0, 0);

    private bool attackStarted = false;
    private bool isResetting = false;
    private float resetStartTime; // Time when resetting starts
    private HashSet<GameObject> hitEntities;
    public RectTransform canvasRect;
    public Vector3 bottomRightReferencePoint = new Vector3();
    public GameObject WeaponDot;

    private GameObject attackCircleVisual;

    void Update()
    {
        if (weaponState == WeaponState.Equipped)
        {
            HandleInput();
            FollowInput();

            if (isAttacking)
            {
                float timeSinceStarted = Time.time - lastAttackTime;
                float percentageComplete = timeSinceStarted / attackDuration;

                if (!attackStarted)
                {
                    additionalAngleLatest = additionalAngle;
                    attackStarted = true;
                }

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
        }
    }

    public void SetState(WeaponState newState)
    {
        weaponState = newState;
        switch (newState)
        {
            case WeaponState.Free:
            case WeaponState.Dropped:
                transform.SetParent(null);
                gameObject.SetActive(true); // Active in the world
                break;

            case WeaponState.Equipped:
                transform.SetParent(player.transform);
                gameObject.SetActive(true);

                // Default direction right
                direction = Vector3.right;
                baseAngle = 0f;
                additionalAngle = 0f;
                adjustedAngle = baseAngle;

                // Apply initial position and rotation
                Vector3 adjustedDirection = new Vector3(Mathf.Cos(adjustedAngle * Mathf.Deg2Rad), Mathf.Sin(adjustedAngle * Mathf.Deg2Rad), 0);
                transform.localPosition = adjustedDirection * distanceFromPlayer;
                float angleForRotation = adjustedAngle + angleOffset;
                transform.localRotation = Quaternion.AngleAxis(angleForRotation, Vector3.forward);
                break;

            case WeaponState.Inventory:
                transform.SetParent(player.transform);
                transform.localPosition = Vector3.zero;
                gameObject.SetActive(false);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && weaponState == WeaponState.Free)
        {
            if (inventory != null)
            {
                bool added = inventory.GetComponent<WeaponDotScript>().AddWeapon(gameObject, gameObject.GetComponent<SpriteRenderer>().sprite, new Vector2(16f, 24f));
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (weaponState == WeaponState.Dropped)
        {
            SetState(WeaponState.Free);
        }
    }

    void HandleInput()
    {
        // Handle mouse input for non-touch devices
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject() && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartAttack();
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
        hitEntities = new HashSet<GameObject>();

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
            attackStarted = false;
            additionalAngleLatest = additionalAngle;
            isAttacking = false;
            isResetting = true; // Start resetting after the attack
            resetStartTime = Time.time; // Mark the start time for the reset
            attacked = false;
        }
        else if (!attacked && percentageComplete >= 0.2f)
        {
            Vector3 attackPoint = player.transform.position + direction * attackRange;
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackSize, enemyLayer);
            attacked = true;

            float attackAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            SpawnSlash(attackPoint, attackAngle, attackSize);

            //DrawAttackVisual(attackPoint, attackSize);

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
                    Camera.main.GetComponent<FollowScript>()?.Shake(duration: cameraShakeDuration, magnitude: cameraShakeMagnitude);
                }

                //Debug.Log("We hit " + enemy.name);
            }
        }
    }

    private void SpawnSlash(Vector3 position, float angle, float size)
    {
        GameObject slash = Instantiate(slashPrefab, position, Quaternion.identity);
        slash.GetComponent<SlashEffect>().Initialize(slashFrames, angle, size, !isAttackPhaseOne);
    }

    void HandleResetting()
    {
        if (isResetting)
        {
            float timeSinceStarted = Time.time - resetStartTime;
            float percentageComplete = timeSinceStarted / resetDuration;

            if (percentageComplete >= 1.0f)
            {
                isResetting = false; // Stop resetting once done
                additionalAngle = 0; // Ensure it's precisely 0
            } else if (percentageComplete >= 0f)
            {
                additionalAngle = Mathf.SmoothStep(additionalAngleLatest, 0, percentageComplete);
            }
        }
    }

    void FollowInput()
    {
        Vector3 inputPosition = Vector3.zero;

        // Use viewport coordinates instead of canvas width/height to get the bottom-right reference point
        Vector2 viewportReferencePoint = new Vector2(6.5f / 8f, 3f / 8f); // Normalized values (0 to 1) for 6.5/8 and 3/8

        // Convert viewport reference point directly to world space
        Vector3 worldReferencePoint = Camera.main.ViewportToWorldPoint(new Vector3(viewportReferencePoint.x, viewportReferencePoint.y, Camera.main.nearClipPlane));
        worldReferencePoint.z = 0; // Ensure it stays on the 2D plane

        // Handle input (touch or mouse)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began) && !IsPointerOverUIObject(touch))
                {
                    inputPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    inputPosition.z = 0;
                    direction = (inputPosition - worldReferencePoint).normalized;  // Use the new reference point
                    break;
                }
            }
        }
        else if (!IsPointerOverUIObject())
        {
            inputPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputPosition.z = 0;
            direction = (inputPosition - worldReferencePoint).normalized;
        }

        // Proceed with direction logic
        if (direction != Vector3.zero)
        {
            // Calculate the base angle from the direction
            baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Add the additionalAngle to the base angle for position and rotation adjustment
            adjustedAngle = baseAngle + additionalAngle;

            // Calculate direction based on adjusted angle
            float adjustedAngleRadians = adjustedAngle * Mathf.Deg2Rad;
            Vector3 adjustedDirection = new Vector3(Mathf.Cos(adjustedAngleRadians), Mathf.Sin(adjustedAngleRadians), 0);

            // Move the object in the new direction (local to player)
            transform.localPosition = adjustedDirection * distanceFromPlayer;

            // Rotate the object relative to the player
            float angleForRotation = adjustedAngle + angleOffset;
            transform.localRotation = Quaternion.AngleAxis(angleForRotation, Vector3.forward);
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

        // Check if any of the raycast results include your specific UI dot
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.name == "WeaponDot") // Replace with your dot's name or tag
            {
                return true; // Pointer is over the dot
            }
        }

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

        // Check if any of the raycast results include your specific UI dot
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.name == "WeaponDot") // Replace with your dot's name or tag
            {
                return true; // Pointer is over the dot
            }
        }

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

    /*
    void DrawAttackVisual(Vector3 position, float radius)
    {
        if (attackCircleVisual != null)
            Destroy(attackCircleVisual);

        attackCircleVisual = new GameObject("AttackCircle");
        attackCircleVisual.transform.position = position;

        LineRenderer line = attackCircleVisual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.widthMultiplier = 0.05f;

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.red;
        line.endColor = Color.red;

        line.sortingLayerName = "UI";
        line.sortingOrder = 100;

        int segments = 40;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            line.SetPosition(i, position + point);
        }

        Destroy(attackCircleVisual, 0.5f);
    }
    */
}
