using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    public string state = "alive";
    public float moveSpeed = 5.0f;
    public float hp = 100;
    public bool immune = false;
    public float immunityDuration = 1f;
    public float tileSize = 1.0f;
    public Tilemap tilemapFloor;

    public bool allowContinuousMove = false;
    public float continuousMoveCooldown = 0.2f;
    private bool canMove = true;

    public GameObject DungeonManager, DijkstraMap;
    public Vector3 targetPosition;
    private bool isMoving = false;

    private Vector2 touchStartPos;
    private bool isSwiping = false;

    private Animator animator;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position; // Initialize targetPosition
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

    public void TakeDamage(float damage)
    {
        immune = true;
        hp -= damage;

        if (hp <= 0)
        {
            SetStateToDead();
        }
        else
        {
            StartCoroutine(ResetImmunityAfterDelay());
        }
    }

    void SetStateToDead()
    {
        state = "dead";

        DungeonManager.GetComponent<DungeonGenerationScript>().RestartScene();
    }

    IEnumerator ResetImmunityAfterDelay()
    {
        yield return new WaitForSeconds(immunityDuration);
        immune = false;
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalking", true);
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
                animator.SetBool("IsWalking", false);

                // Generate Dijkstra map after moving
                Vector2Int targetPosition2D = new Vector2Int(
                    Mathf.FloorToInt(targetPosition.x - 0.5f),
                    Mathf.FloorToInt(targetPosition.y - 0.5f)
                );
                DijkstraMap.GetComponent<DijkstraMap>().GenerateDijkstraMap(targetPosition2D);
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
        if (allowContinuousMove && !canMove) return;

        Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position + direction * tileSize);
        if (!isMoving && tilemapFloor.GetTile(cellPosition) != null/* && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position + direction * tileSize)*/)
        {
            targetPosition = transform.position; // Set new target position

            // Snap x and y to the grid using SnapToGrid
            Vector2 snappedPosition = SnapToGrid(new Vector2(transform.position.x, transform.position.y));

            // Apply the snapped X or Y based on movement direction
            if (direction == Vector3.up)
            {
                targetPosition.x = snappedPosition.x; // Snap X to the grid
                targetPosition.y = Mathf.Floor(targetPosition.y - 0.5f) + 1.5f; // Move up
            }
            else if (direction == Vector3.down)
            {
                targetPosition.x = snappedPosition.x; // Snap X to the grid
                targetPosition.y = Mathf.Ceil(targetPosition.y + 0.5f) - 1.5f; // Move down
            }
            else if (direction == Vector3.right)
            {
                targetPosition.y = snappedPosition.y; // Snap Y to the grid
                targetPosition.x = Mathf.Floor(targetPosition.x - 0.5f) + 1.5f; // Move right
            }
            else if (direction == Vector3.left)
            {
                targetPosition.y = snappedPosition.y; // Snap Y to the grid
                targetPosition.x = Mathf.Ceil(targetPosition.x + 0.5f) - 1.5f; // Move left
            }

            isMoving = true;
            spriteRenderer.flipX = direction == Vector3.left;
            animator.SetBool("IsWalking", true);

            if (allowContinuousMove)
            {
                StartCoroutine(MoveCooldown());
            }
        }
    }

    private IEnumerator MoveCooldown()
    {
        canMove = false;
        yield return new WaitForSeconds(continuousMoveCooldown);
        canMove = true;
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX = Mathf.Round(position.x * 2) / 2;
        float snappedY = Mathf.Round(position.y * 2) / 2;

        return new Vector2(snappedX, snappedY);
    }

    float GetDecimalPart(float value)
    {
        return value - Mathf.Floor(value);
    }
}
