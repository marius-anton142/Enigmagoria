using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float moveIntervalMin = 0.4f;
    public float moveIntervalMax = 0.6f;
    public GameObject DijkstraMap;
    public Tilemap tilemapFloor;
    public GameObject player;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        targetPosition = transform.position; // Initialize target position
        InvokeRepeating(nameof(NextStep), Random.Range(moveIntervalMin, moveIntervalMax), Random.Range(moveIntervalMin, moveIntervalMax));
    }

    private void Update()
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
            }
        }
    }

    // Determine the next step based on the Dijkstra map and move the enemy
    private void NextStep()
    {
        if (isMoving) return; // Don't process if already moving

        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );

        // Get neighboring cells (up, down, left, right) and shuffle them for random selection in case of a tie
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        ShuffleArray(directions); // Shuffle the directions to randomize the order

        Vector2Int bestMove = currentCell;
        int lowestCost = DijkstraMap.GetComponent<DijkstraMap>().GetCost(currentCell);

        // Check each neighboring cell to find the one with the lowest Dijkstra value
        foreach (var direction in directions)
        {
            Vector2Int neighbor = currentCell + direction;
            int neighborCost = DijkstraMap.GetComponent<DijkstraMap>().GetCost(neighbor);

            // Move towards the tile with the lowest Dijkstra value
            if (neighborCost < lowestCost)
            {
                bestMove = neighbor;
                lowestCost = neighborCost;
            }
        }

        // If a valid move is found, update the target position and move
        if (bestMove != currentCell)
        {
            MoveToPosition(bestMove);
        }
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
        // Snap the target position to the center of the tile (grid alignment)
        targetPosition = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0);  // Align to grid

        // Update the sprite orientation based on the movement direction
        if (targetCell.x < transform.position.x)
            spriteRenderer.flipX = true;  // Face left
        else if (targetCell.x > transform.position.x)
            spriteRenderer.flipX = false; // Face right

        isMoving = true; // Start movement
    }
}
