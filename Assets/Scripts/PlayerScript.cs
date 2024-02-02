using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Adjust this for movement speed
    public float tileSize = 1.0f;  // Adjust this to match your tile size
    private Rigidbody2D rb;
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
    }

    private void Update()
    {
        HandleSwipeInput();
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalking", true);

            // Check if we're close enough to the target position to stop lerping
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                Vector2Int targetPosition2D = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));
                DijkstraMap.GetComponent<DijkstraMapGenerator>().GenerateDijkstraMap(targetPosition2D);
                isMoving = false;

                animator.SetBool("IsWalking", false);

                //Debug.Log(DijkstraMap.GetComponent<DijkstraMapGenerator>().GetDijkstraValue(new Vector2Int(a, b)));
                //Debug.Log(targetPosition2D);
            }
        }

        // Check for input and call movement functions
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                MoveDown();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveRight();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                MoveLeft();
            }
        }
    }

    // Move the player up by one tile
    public void MoveUp()
    {
        if (!isMoving)
        {
            Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position + Vector3.up * tileSize);
            if (tilemapFloor.GetTile(cellPosition) != null && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position + Vector3.up * tileSize))
            {
                targetPosition = transform.position + Vector3.up * tileSize;
                isMoving = true;
            }
        }
    }

    // Move the player down by one tile
    public void MoveDown()
    {
        if (!isMoving)
        {
            Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position - Vector3.up * tileSize);
            if (tilemapFloor.GetTile(cellPosition) != null && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position - Vector3.up * tileSize))
            {
                targetPosition = transform.position - Vector3.up * tileSize;
                isMoving = true;
            }
        }
    }

    // Move the player right by one tile
    public void MoveRight()
    {
        if (!isMoving)
        {
            Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position + Vector3.right * tileSize);
            if (tilemapFloor.GetTile(cellPosition) != null && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position + Vector3.right * tileSize))
            {
                targetPosition = transform.position + Vector3.right * tileSize;
                isMoving = true;
                spriteRenderer.flipX = false;
            }
        }
    }

    // Move the player left by one tile
    public void MoveLeft()
    {
        if (!isMoving)
        {
            Vector3Int cellPosition = tilemapFloor.WorldToCell(transform.position - Vector3.right * tileSize);
            if (tilemapFloor.GetTile(cellPosition) != null && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(transform.position - Vector3.right * tileSize))
            {
                targetPosition = transform.position - Vector3.right * tileSize;
                isMoving = true;
                spriteRenderer.flipX = true;
            }
        }
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
                    break;

                case TouchPhase.Ended:
                    Vector2 touchEndPos = touch.position;
                    Vector2 swipeDirection = touchEndPos - touchStartPos;

                    if (isSwiping && swipeDirection.magnitude > 50f)
                    {
                        swipeDirection.Normalize();

                        if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
                        {
                            if (swipeDirection.x > 0)
                            {
                                MoveRight();
                            }
                            else
                            {
                                MoveLeft();
                            }
                        }
                        else
                        {
                            if (swipeDirection.y > 0)
                            {
                                MoveUp();
                            }
                            else
                            {
                                MoveDown();
                            }
                        }
                    }
                    else
                    {
                        // If the swipe was not detected, treat it as a tap
                        MoveUp();
                    }

                    isSwiping = false;
                    break;
            }
        }
    }
}
