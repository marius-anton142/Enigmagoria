using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    public string state = "alive";
    public float hp = 100;
    public float moveSpeed = 5.0f;
    public float knockResistance = 1f;

    public float tileSize = 1.0f;
    public Tilemap tilemapFloor;

    public bool allowContinuousMove = false;
    public float continuousMoveCooldown = 0.2f;
    public bool canMove = true;

    public GameObject DungeonManager;
    public Vector3 targetPosition;
    private bool isMoving = false;
    //public GameObject DijkstraMap;

    private Vector2 touchStartPos;
    private bool isSwiping = false;

    private Animator animator;
    SpriteRenderer spriteRenderer;
    private float normalMoveSpeed;
    public bool isSliding = false;
    public int bumpsStuck = 0;

    private Collider2D mainCollider;
    private Collider2D triggerCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position; // Initialize targetPosition
        normalMoveSpeed = moveSpeed;
    }

    private void Start()
    {
        Collider2D[] playerColliders = GetComponents<Collider2D>();

        foreach (var col in playerColliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
            }
            else
            {
                mainCollider = col;
            }
        }
    }

    private void Update()
    {
        HandleMovement();
        if (!isMoving) // Only check for input if not currently moving
        {
            CheckForInput();
            HandleSwipeInput(); // Handle swipe inputs
        }
    }

    public bool CanMove()
    {
        if (state != "knocked")
        {
            return true;
        }
        return false;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            SetStateToDead();
        }
    }

    void SetStateToDead()
    {
        state = "dead";

        DungeonManager.GetComponent<DungeonGenerationScript01>().RestartScene();
    }

    void SetStateToKnocked(float knockTime)
    {
        state = "knocked";
        StartCoroutine(ResetStateAfterKnock(knockTime * knockResistance));
        mainCollider.enabled = true;
    }

    private IEnumerator ResetStateAfterKnock(float knockTime)
    {
        yield return new WaitForSeconds(knockTime);
        state = "alive";
        mainCollider.enabled = false;
    }

    public void SetStuck(int bumpsStuck)
    {
        this.bumpsStuck = bumpsStuck;
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            if (isSliding)
            {
                // Use linear movement (Lerp) when sliding
                transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime / Vector3.Distance(transform.position, targetPosition));
            }
            else
            {
                // Default smooth movement
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }

            animator.SetBool("IsWalking", true);

            // Check if the player has reached the target position
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                // Stop movement and reset flags
                transform.position = targetPosition;
                isMoving = false;
                isSliding = false; // Reset isSliding to false
                moveSpeed = normalMoveSpeed; // Reset speed to normal

                animator.SetBool("IsWalking", false);

                Vector2Int targetPosition2D = new Vector2Int(
                    Mathf.FloorToInt(targetPosition.x - 0.5f),
                    Mathf.FloorToInt(targetPosition.y - 0.5f)
                );
                // DijkstraMap.GetComponent<DijkstraMap>().GenerateDijkstraMap(targetPosition2D);
            }
        }
    }

    private void CheckForInput()
    {
        // Use GetKeyDown to ensure movement is triggered only once per key press
        if (Input.GetKeyDown(KeyCode.W)) Move(Vector3.up);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector3.down);
        if (Input.GetKeyDown(KeyCode.D)) Move(Vector3.right);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector3.left);
    }

    private void HandleSwipeInput()
    {
        // Swipe input handling logic from the original script
    }

    // Public methods to be called by UI buttons or other input methods
    public void MoveUp() { Move(Vector3.up); }
    public void MoveDown() { Move(Vector3.down); }
    public void MoveLeft() { Move(Vector3.left); }
    public void MoveRight() { Move(Vector3.right); }

    public void Move(Vector3 direction)
    {
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );

        if (bumpsStuck > 1 && DungeonManager.GetComponent<DungeonGenerationScript01>().IsCobwebAtPosition(pos))
        {
            if (direction.x != 0)
            {
                spriteRenderer.flipX = (direction.x < 0);
            }

            Vector3 bumpPosition = transform.position + direction * 0.23f;
            StartCoroutine(ReturnFromBump(transform.position));
            transform.position = bumpPosition;
            isSliding = false;
            return;
        }

        if (allowContinuousMove && (!canMove || !CanMove()))
        {
            return;
        }
        Vector3 currentPosition = transform.position;
        Vector3Int cellPosition = tilemapFloor.WorldToCell(currentPosition + direction * tileSize);
        bool positionFound = false; // Track if a valid position is found

        // Set initial target position to current position in case no valid move is found
        targetPosition = currentPosition;

        // Check if there's a table and try to find a clear position
        while (!positionFound)
        {
            // Check if the target cell contains a floor tile and no table
            if (tilemapFloor.GetTile(cellPosition) != null &&
                !DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable2x2AtPositionAny(cellPosition) &&
                !DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable1x2AtPositionAny(cellPosition))
            {
                // Valid position found
                positionFound = true;
                targetPosition = tilemapFloor.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0); // Center on cell
            }
            else
            {
                // If a table is in the way, keep moving one more unit in the same direction
                cellPosition += Vector3Int.RoundToInt(direction);
                moveSpeed = normalMoveSpeed / 2.5f;
                isSliding = true;

                // If the cell does not have a floor tile, break the loop (no valid move)
                if (tilemapFloor.GetTile(cellPosition) == null)
                {
                    moveSpeed = normalMoveSpeed;
                    break;
                }
            }
        }

        // Only proceed with the movement if a valid target position was found
        if (positionFound && !isMoving && !DungeonManager.GetComponent<DungeonGenerationScript01>().IsSolidAtPosition(tilemapFloor.WorldToCell(targetPosition)))
        {
            // Proceed with setting movement flags and initiating movement
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            isMoving = true;
            if (direction.x != 0)
            {
                spriteRenderer.flipX = (direction.x < 0);
            }
            animator.SetBool("IsWalking", true);

            if (allowContinuousMove)
            {
                StartCoroutine(MoveCooldown());
            }

            if (bumpsStuck == 1)
            {
                Vector3Int flooredPosition = new Vector3Int(
                    Mathf.FloorToInt(transform.position.x),
                    Mathf.FloorToInt(transform.position.y),
                    Mathf.FloorToInt(transform.position.z)
                );
                DungeonManager.GetComponent<DungeonGenerationScript01>().RemoveCobwebAtPosition(flooredPosition);
                bumpsStuck = 0;
            }
        }
        else if (!positionFound || (DungeonManager.GetComponent<DungeonGenerationScript01>().IsSolidAtPosition(tilemapFloor.WorldToCell(targetPosition))))
        {
            if (direction.x != 0)
            {
                spriteRenderer.flipX = (direction.x < 0);
            }

            Vector3 bumpPosition = currentPosition + direction * 0.23f;
            transform.position = bumpPosition;
            StartCoroutine(ReturnFromBump(currentPosition));
            isSliding = false;
            canMove = false;
        }
    }

    private IEnumerator ReturnFromBump(Vector3 originalPosition)
    {
        yield return new WaitForSeconds(0.062f); // Brief delay
        transform.position = originalPosition;

        if (bumpsStuck <= 1)
        {
            canMove = true;

            if (allowContinuousMove)
            {
                StartCoroutine(MoveCooldown());
            }
        }
        else if (bumpsStuck > 1)
        {
            --bumpsStuck;
        }
    }

    private IEnumerator MoveCooldown()
    {
        canMove = false;
        yield return new WaitForSeconds(continuousMoveCooldown);
        canMove = true;
    }

    public void ApplyKnockback(Vector2 direction, float force, float knockTime, float damageOther)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        SetStateToKnocked(knockTime);
        TakeDamage(damageOther);
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX = Mathf.Floor(position.x) + 0.5f;
        float snappedY = Mathf.Floor(position.y) + 0.5f;

        return new Vector2(snappedX, snappedY);
    }

    private Vector2Int SnapToGridInt(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    float GetDecimalPart(float value)
    {
        return value - Mathf.Floor(value);
    }
}
