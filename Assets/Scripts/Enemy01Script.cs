using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy01Script : MonoBehaviour
{
    public GameObject DijkstraMap, DungeonManager;
    public float moveSpeed = 5.0f;
    public int rangeMovement = 10;
    public float tileSize = 1.0f;  // Adjust this to match your tile size
    private Rigidbody2D rb;
    public Tilemap tilemapFloor;

    public Vector3 targetPosition;
    private bool isMoving = false;

    private Animator animator;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating("NextStep", 5.0f, Random.Range(0.4f, 0.6f));
        targetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            animator.SetBool("IsWalking", true);

            // Check if we're close enough to the target position to stop lerping
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                Vector2Int targetPosition2D = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));
                isMoving = false;

                animator.SetBool("IsWalking", false);
                //Debug.Log(DijkstraMap.GetComponent<DijkstraMapGenerator>().GetDijkstraValue(new Vector2Int(a, b)));
                //Debug.Log(targetPosition2D);
            }
        }
    }

    public int getCurrentDistance(Vector2Int targetPosition2D)
    {
        //Vector2Int targetPosition2D = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));
        return DijkstraMap.GetComponent<DijkstraMapGenerator>().GetDijkstraValue(targetPosition2D);
    }

    private void ShuffleArray<T>(T[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            int randomIndex = Random.Range(i, n);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    public void NextStep()
    {
        Vector2Int currentCell = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));
        Vector2Int[] neighbors = GetNeighbors(currentCell);
        ShuffleArray(neighbors);

        int smallestDistance = int.MaxValue;
        Vector2Int nextCell = currentCell;

        foreach (Vector2Int neighbor in neighbors)
        {
            int distance = getCurrentDistance(neighbor);
            if (distance < smallestDistance && distance < rangeMovement)
            {
                smallestDistance = distance;
                nextCell = neighbor;
            }
        }

        // Determine the movement direction based on the next cell's position
        Vector2Int direction = nextCell - currentCell;

        if (direction == Vector2Int.up)
        {
            MoveUp();
        }
        else if (direction == Vector2Int.down)
        {
            MoveDown();
        }
        else if (direction == Vector2Int.right)
        {
            MoveRight();
        }
        else if (direction == Vector2Int.left)
        {
            MoveLeft();
        }
    }

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

    Vector2Int[] GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        // Define neighboring offsets based on your grid's layout
        Vector2Int[] neighborOffsets = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // Above
        new Vector2Int(0, -1),  // Below
        new Vector2Int(1, 0),   // Right
        new Vector2Int(-1, 0),  // Left
        };

        foreach (Vector2Int offset in neighborOffsets)
        {
            Vector2Int neighbor = cell + offset;

            // Check if the neighbor is within the bounds of your grid and if it's a floor cell
            if (IsFloor(neighbor) && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors.ToArray();
    }

    bool IsFloor(Vector2Int cell)
    {
        Vector3Int cellPosition = new Vector3Int(cell.x, cell.y, 0);
        return tilemapFloor.GetTile(cellPosition) != null;
    }
}
