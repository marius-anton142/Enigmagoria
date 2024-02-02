using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float tileSize = 1.0f;
    public Tilemap tilemapFloor;

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

    private void HandleMovement()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalking", true);
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f) // Check for movement completion
            {
                transform.position = targetPosition;
                isMoving = false;
                animator.SetBool("IsWalking", false);
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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    // Movement logic is handled on touch end
                    break;

                case TouchPhase.Ended:
                    if (isSwiping)
                    {
                        Vector2 touchEndPos = touch.position;
                        Vector2 swipeDirection = touchEndPos - touchStartPos;
                        isSwiping = false; // Reset isSwiping

                        if (swipeDirection.magnitude > 50f) // Minimum swipe distance
                        {
                            swipeDirection.Normalize();
                            if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                            {
                                if (swipeDirection.x > 0) Move(Vector3.right);
                                else Move(Vector3.left);
                            }
                            else
                            {
                                if (swipeDirection.y > 0) Move(Vector3.up);
                                else Move(Vector3.down);
                            }
                        }
                    }
                    break;
            }
        }
    }

    private void Move(Vector3 direction)
    {
        Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position + direction * tileSize);
        if (tilemapFloor.GetTile(cellPosition) != null && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position + direction * tileSize))
        {
            targetPosition = transform.position + direction * tileSize; // Set new target position
            isMoving = true;
            // Flip sprite if moving left/right
            spriteRenderer.flipX = direction == Vector3.left;
        }
    }
}
