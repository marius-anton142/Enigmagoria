using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public string state = "idle";
    public float moveSpeed = 5.0f;
    public float moveIntervalMin = 0.4f;
    public float moveIntervalMax = 0.6f;
    public float leapForce = 10.0f;
    public float knockbackForce = 10.0f;
    public float knockTime = 1f;
    public float knockResistance = 1f;
    public GameObject DijkstraMap;
    public Tilemap tilemapFloor;
    public GameObject player;
    public bool isLeaping = false;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D mainCollider;
    private Collider2D triggerCollider;

    private HashSet<GameObject> hitEntities;
    private Coroutine leapCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        targetPosition = transform.position; // Initialize target position
        InvokeRepeating(nameof(NextStep), Random.Range(moveIntervalMin, moveIntervalMax), Random.Range(moveIntervalMin, moveIntervalMax));

        // Find both colliders attached to the enemy
        Collider2D[] enemyColliders = GetComponents<Collider2D>();

        // Loop through colliders to assign mainCollider and triggerCollider
        foreach (var col in enemyColliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col; // Assign the trigger collider
            }
            else
            {
                mainCollider = col; // Assign the non-trigger collider (main collider)
            }
        }

        // Ignore collision between player's main collider and enemy's main collider
        Physics2D.IgnoreCollision(mainCollider, player.GetComponent<Collider2D>(), true);
    }

    private void Update()
    {
        // Only move if not leaping
        if (CanMove() && isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalking", true);

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
                animator.SetBool("IsWalking", false);
            }
        }
    }

    // Determine the next step based on the Dijkstra map and move the enemy
    private void NextStep()
    {
        if (!CanMove() || isMoving || isLeaping) return; // Don't process if already moving or leaping

        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );

        int currentCost = DijkstraMap.GetComponent<DijkstraMap>().GetCost(currentCell);

        if (currentCost < 3 && !isLeaping)
        {
            // Prepare to leap towards the player
            StopInvokeNextStep(); // Stop the NextStep coroutine
            leapCoroutine = StartCoroutine(PrepareAndLeap());
            SetStateToPrepare();
            return;
        }

        // Regular movement logic using Dijkstra map
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        ShuffleArray(directions); // Shuffle directions for randomness

        Vector2Int bestMove = currentCell;
        int lowestCost = currentCost;

        // Check each neighboring cell to find the one with the lowest Dijkstra value
        foreach (var direction in directions)
        {
            Vector2Int neighbor = currentCell + direction;
            int neighborCost = DijkstraMap.GetComponent<DijkstraMap>().GetCost(neighbor);

            if (neighborCost < lowestCost)
            {
                bestMove = neighbor;
                lowestCost = neighborCost;
            }
        }

        if (bestMove == currentCell && state != "idle")
        {
            SetStateToIdle();
        }
        else if (bestMove != currentCell)
        {
            MoveToPosition(bestMove);
            if (state != "chase")
            {
                SetStateToChase();
            }
        }
    }

    // Stop invoking NextStep
    private void StopInvokeNextStep()
    {
        CancelInvoke(nameof(NextStep));
    }

    // Resume invoking NextStep after a delay
    private void ResumeInvokeNextStep()
    {
        InvokeRepeating(nameof(NextStep), moveIntervalMin, moveIntervalMax);
    }

    // Coroutine to handle the leap attack
    private IEnumerator PrepareAndLeap()
    {
        isLeaping = true;

        // Prepare for 1 second before leaping
        yield return new WaitForSeconds(1.0f);

        // Launch towards the player
        SetStateToAttack();
        Vector2 leapDirection = (player.transform.position - transform.position).normalized;
        if (leapDirection == Vector2.zero)
        {
            leapDirection = GetRandomDirection();
        }

        ApplyKnockbackToOverlappingEntities(leapDirection);
        rb.AddForce(leapDirection * leapForce, ForceMode2D.Impulse);

        // After a short leap, stop and wait before invoking NextStep again
        yield return new WaitForSeconds(0.5f);

        isLeaping = false; // Reset leap state
        SetStateToCooldown();
        yield return new WaitForSeconds(1.0f); // Pause for 1 second before resuming regular behavior
        ResumeInvokeNextStep(); // Resume regular movement
    }

    // Shuffle array to randomize the order of directions
    private void ShuffleArray(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            Vector2Int temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    // Move the enemy to the specified grid position
    private void MoveToPosition(Vector2Int targetCell)
    {
        // Prevent moving if leaping
        if (isLeaping) return;

        targetPosition = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0); // Align to grid

        if (targetCell.x < transform.position.x)
            spriteRenderer.flipX = true;
        else if (targetCell.x > transform.position.x)
            spriteRenderer.flipX = false;

        isMoving = true;
    }

    public void ApplyKnockback(Vector2 direction, float force, float knockTime)
    {
        // Stop the leap coroutine if it is running
        if (leapCoroutine != null)
        {
            StopCoroutine(leapCoroutine);
            leapCoroutine = null;
            isLeaping = false; // Ensure leaping state is reset
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        SetStateToKnocked(knockTime);

        float distance = (force / rb.mass) / (1 + rb.drag);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we collided with the player or another enemy while leaping
        if (state == "attack" && !hitEntities.Contains(other.gameObject) && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            ApplyKnockbackToOther(other, Vector2.zero);
            hitEntities.Add(other.gameObject); // Track the object instead of just the collider
        }

        if (other.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(mainCollider, other.GetComponent<Collider2D>(), true);
        }
    }

    void ApplyKnockbackToOther(Collider2D other, Vector2 knockbackDirection)
    {
        if (knockbackDirection == Vector2.zero) 
        {
            knockbackDirection = rb.velocity.normalized;
        }

        PlayerScript player = other.GetComponent<PlayerScript>();
        EnemyAI enemy = other.GetComponent<EnemyAI>();

        if (player != null)
        {
            player.ApplyKnockback(knockbackDirection, knockbackForce, knockTime);
        }
        else if (enemy != null)
        {
            Physics2D.IgnoreCollision(mainCollider, other.GetComponent<Collider2D>(), true);
            enemy.ApplyKnockback(knockbackDirection, knockbackForce, knockTime);  // Assuming you have ApplyKnockback in EnemyAI
        }
    }

    void ApplyKnockbackToOverlappingEntities(Vector2 knockbackDirection)
    {
        // Create an array to hold the overlapping colliders
        Collider2D[] overlappingColliders = new Collider2D[10]; // Max 10 colliders can overlap
        ContactFilter2D contactFilter = new ContactFilter2D().NoFilter(); // No specific filter

        // Get the number of overlapping colliders
        int colliderCount = Physics2D.OverlapCollider(triggerCollider, contactFilter, overlappingColliders);

        // Loop through the overlapping colliders
        for (int i = 0; i < colliderCount; i++)
        {
            Collider2D collider = overlappingColliders[i];

            // Get the GameObject from the collider
            GameObject hitObject = collider?.gameObject;

            if (collider == null || hitEntities.Contains(hitObject) || hitObject == gameObject)
                continue; // Skip if no collider or object has already been hit

            if (collider.CompareTag("Player") || collider.CompareTag("Enemy"))
            {
                ApplyKnockbackToOther(collider, knockbackDirection); // Apply knockback to the overlapping entity
                hitEntities.Add(hitObject); // Track the object that was hit
            }
        }
    }

    private bool CanMove()
    {
        if (state != "knocked" && state != "leaping")
        {
            return true;
        }
        return false;
    }

    void SetStateToKnocked(float knockTime)
    {
        state = "knocked";
        StartCoroutine(ResetStateAfterKnock(knockTime * knockResistance));
    }

    private IEnumerator ResetStateAfterKnock(float knockTime)
    {
        yield return new WaitForSeconds(knockTime);
        state = "idle";
        ResumeInvokeNextStep();
    }

    void SetStateToPrepare()
    {
        state = "prepare";
    }

    void SetStateToAttack()
    {
        state = "attack";
        hitEntities = new HashSet<GameObject>();
    }

    void SetStateToCooldown()
    {
        state = "cooldown";
    }

    void SetStateToIdle()
    {
        state = "idle";
    }

    void SetStateToChase()
    {
        state = "chase";
    }

    Vector2 GetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f); // Random angle in degrees
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        return direction;
    }
}
