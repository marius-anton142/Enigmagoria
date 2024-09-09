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
    public float leapForce = 10.0f;     // Force of the leap attack
    public float knockbackForce = 10.0f;
    public float knockTime = 1f;
    public float knockResistance = 1f;
    public GameObject DijkstraMap;
    public Tilemap tilemapFloor;
    public GameObject player;
    public bool isLeaping = false;      // Track if the enemy is leaping

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Collider2D mainCollider;   // Main collider for environment interactions
    private Collider2D triggerCollider;

    private HashSet<Collider2D> hitUnits;
    private Coroutine leapCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        // Get Player's Collider and set IgnoreCollision at the start
        playerCollider = player.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>(), true);
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
            SetStateToPreparing();
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
        hitUnits = new HashSet<Collider2D>();

        // Prepare for 1 second before leaping
        yield return new WaitForSeconds(1.0f);

        // Launch towards the player
        SetStateToAttacking();
        Vector2 leapDirection = (player.transform.position - transform.position).normalized;
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
        if (isLeaping && !hitUnits.Contains(other) && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            Vector2 knockbackDirection = rb.velocity.normalized;

            PlayerScript player = other.GetComponent<PlayerScript>();
            EnemyAI enemy = other.GetComponent<EnemyAI>();

            if (player != null)
            {
                player.ApplyKnockback(knockbackDirection, knockbackForce, knockTime);
            }
            else if (enemy != null)
            {
                enemy.ApplyKnockback(knockbackDirection, knockbackForce, knockTime);  // Assuming you have ApplyKnockback in EnemyAI
            }

            hitUnits.Add(other);
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

    void SetStateToPreparing()
    {
        state = "preparing";
    }

    void SetStateToAttacking()
    {
        state = "attacking";
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
}
