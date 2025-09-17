using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    public string type = "Critter";
    public string state = "idle";
    public float hp = 100;
    public float damage = 50;
    public float moveSpeed = 5.0f;
    public float moveIntervalMin = 0.4f;
    public float moveIntervalMax = 0.6f;
    public float leapForce = 10.0f;
    public float knockbackForce = 10.0f;
    public float prepareTime = 1f;
    public float leapTime = 1f;
    public float cooldownTime = 1f;
    public float knockTime = 1f;
    public float knockResistance = 1f;
    public Tilemap tilemapFloor;
    public GameObject player;
    public bool isLeaping = false;
    public GameObject DungeonManager;

    public AudioPlayer audioPlayer;

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
    private HashSet<Vector2Int> walkableTiles;
    private int bumpsStuck = 0;

    public Material whiteFlashMaterial;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    private bool hasPlayedSlideSound = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        //DijkstraMap = GameObject.FindGameObjectWithTag("DijkstraMap");
        tilemapFloor = GameObject.FindGameObjectWithTag("TilemapFloor").GetComponent<Tilemap>();
        DungeonManager = GameObject.FindGameObjectWithTag("DungeonManager");

        audioPlayer = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioPlayer>();
        originalMaterial = spriteRenderer.material;
    }

    private void Start()
    {
        targetPosition = transform.position; // Initialize target position
        Invoke(nameof(NextStep), Random.Range(moveIntervalMin, moveIntervalMax));

        walkableTiles = GetWalkableTiles();

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
                hasPlayedSlideSound = false;
            }
        }
    }

    private void NextStep()
    {
        Invoke(nameof(NextStep), Random.Range(moveIntervalMin, moveIntervalMax));

        if (!CanMove() || isMoving || isLeaping) return; // Skip if already moving or leaping

        //walkableTiles = GetWalkableTiles();

        Vector2Int currentCell = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y)
        );

        Vector2Int targetCell = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x),
            Mathf.FloorToInt(player.transform.position.y)
        );

        List<Vector2Int> path = AStarPathfinding(currentCell, targetCell);

        if (type == "Critter")
        {
            if (currentCell == targetCell)
            {
                leapCoroutine = StartCoroutine(PrepareAndLeap(Vector2Int.zero));
                SetStateToPrepare();
                return;
            }

            if (path.Count > 7)
            {
                SetStateToIdle();
                return;
            }
            else if (path.Count > 1 && path.Count <= 3 && !isLeaping)
            {
                leapCoroutine = StartCoroutine(PrepareAndLeap(Vector2Int.zero));
                SetStateToPrepare();
                return;
            }

            if (path.Count > 1)
            {
                TryPathMovesInOrder(path); // 🔄 New smarter path selection
                if (state != "chase") SetStateToChase();
            }
            else if (state != "idle")
            {
                SetStateToIdle();
            }
        }
        else if (type == "Knight")
        {
            if (currentCell == targetCell)
            {
                if (state == "idle")
                {
                    SetStateToChase();
                }
                else if (state == "chase")
                {
                    List<Vector2Int> neighbors = GetNeighbors(currentCell);
                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor != targetCell)
                        {
                            MoveToPosition(neighbor); // ✅ simple L-shape jump
                            return;
                        }
                    }
                }
                return;
            }

            if (path.Count > 1 && path[1] == targetCell && !isLeaping)
            {
                leapCoroutine = StartCoroutine(PrepareAndLeap(targetCell));
                SetStateToPrepare();
                return;
            }

            if (path.Count > 7)
            {
                SetStateToIdle();
                return;
            }
            else if (path.Count > 1)
            {
                MoveToPosition(path[1]); // ✅ direct, no checks
                if (state != "chase") SetStateToChase();
            }
            else if (state != "idle")
            {
                SetStateToIdle();
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
        Invoke(nameof(NextStep), Random.Range(moveIntervalMin, moveIntervalMax));
    }

    // Coroutine to handle the leap attack
    private IEnumerator PrepareAndLeap(Vector2Int targetCell)
    {
        if (IsStuck())
        {
            Vector3 bumpDir = (player.transform.position - transform.position).normalized;
            if (bumpDir == Vector3.zero) bumpDir = Vector3.right;

            Vector3 bumpPos = transform.position + bumpDir * 0.23f;
            transform.position = bumpPos; // <-- ✅ actually move visually
            StartCoroutine(ReturnFromBump(transform.position - bumpDir * 0.23f));
            yield break;
        }

        isLeaping = true;

        // Prepare for 1 second before leaping
        yield return new WaitForSeconds(prepareTime);

        // Launch towards the player
        SetStateToAttack();

        Vector3 initialPosition = transform.position;

        if (type == "Critter")
        {
            Vector2 leapDirection = (player.transform.position - transform.position).normalized;
            if (leapDirection == Vector2.zero)
            {
                leapDirection = GetRandomDirection();
            }
            ApplyKnockbackToOverlappingEntities(leapDirection);

            rb.AddForce(leapDirection * leapForce, ForceMode2D.Impulse);
        }
        else if (type == "Knight")
        {
            targetPosition = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0);

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            Collider2D[] entitiesInCell = Physics2D.OverlapBoxAll(
                targetPosition, new Vector2(1, 1), 0f, LayerMask.GetMask("Player", "Enemy")
            );

            foreach (var entity in entitiesInCell)
            {
                // Ensure the entity is either the player or another enemy, and apply knockback
                if (entity.gameObject != gameObject && !hitEntities.Contains(entity.gameObject) && (entity.CompareTag("Player") || entity.CompareTag("Enemy")))
                {
                    hitEntities.Add(entity.gameObject);
                    Vector2 knockbackDirection = (transform.position - initialPosition).normalized;
                    ApplyKnockbackToOther(entity, knockbackDirection);
                }
            }
        }
        // After a short leap, stop and wait before invoking NextStep again
        yield return new WaitForSeconds(leapTime);

        isLeaping = false; // Reset leap state
        SetStateToCooldown();
        yield return new WaitForSeconds(cooldownTime); // Pause for 1 second before resuming regular behavior
        SetStateToIdle(); // Resume regular movement
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

    private void MoveToPosition(Vector2Int targetCell)
    {
        mainCollider.enabled = false;
        if (IsStuck())
        {
            Vector3 bumpDir = (player.transform.position - transform.position).normalized;
            if (bumpDir == Vector3.zero) bumpDir = Vector3.right;

            Vector3 bumpPos = transform.position + bumpDir * 0.23f;
            transform.position = bumpPos; // <-- ✅ actually move visually
            StartCoroutine(ReturnFromBump(transform.position - bumpDir * 0.23f));

            return;
        }

        if (isLeaping) return;

        if (type == "Knight")
        {
            // Simple movement for Knight — no sliding
            targetPosition = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0);
            isMoving = true;

            if (targetCell.x < transform.position.x - 0.5f)
                spriteRenderer.flipX = true;
            else if (targetCell.x > transform.position.x - 0.5f)
                spriteRenderer.flipX = false;

            audioPlayer.PlayWalkKnightSound();
            return;
        }

        // Sliding behavior for Critters
        Vector2Int direction = targetCell - new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        direction = new Vector2Int(Mathf.Clamp(direction.x, -1, 1), Mathf.Clamp(direction.y, -1, 1));

        Vector2Int currentCell = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        Vector3Int nextCell = new Vector3Int(currentCell.x + direction.x, currentCell.y + direction.y, 0);

        float slideSpeed = moveSpeed * 1.2f;
        Vector3 finalTarget = transform.position;

        for (int i = 0; i < 10; i++)
        {
            if (tilemapFloor.GetTile(nextCell) == null) break;

            bool blocked =
                DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable2x2AtPositionAny(nextCell) ||
                DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable1x2AtPositionAny(nextCell);

            if (blocked)
            {
                nextCell += new Vector3Int(direction.x, direction.y, 0);
                continue;
            }
            else if (!DungeonManager.GetComponent<DungeonGenerationScript01>().IsSolidAtPosition(nextCell))
            {
                finalTarget = tilemapFloor.CellToWorld(nextCell) + new Vector3(0.5f, 0.5f, 0);
                break;
            }
        }

        bool isSliding = false;

        Vector3Int slideCheckCell = new Vector3Int(currentCell.x + direction.x, currentCell.y + direction.y, 0);
        if (DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable2x2AtPositionAny(slideCheckCell) ||
            DungeonManager.GetComponent<DungeonGenerationScript01>().IsTable1x2AtPositionAny(slideCheckCell))
        {
            isSliding = true;
        }

        if (Vector3.Distance(finalTarget, transform.position) > 0.1f)
        {
            targetPosition = finalTarget;
            isMoving = true;
            spriteRenderer.flipX = direction.x < 0;

            if (isSliding && !hasPlayedSlideSound)
            {
                audioPlayer.PlaySlideSound();
                hasPlayedSlideSound = true;
            } else
            {
                audioPlayer.PlayWalkCritterSound();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            SetStateToDead();
        } else
        {
            FindObjectOfType<AudioPlayer>().PlayHitSound();
        }
    }

    void SetStateToDead()
    {
        state = "dead";
        FindObjectOfType<AudioPlayer>().PlayKillSound();
        RDG.Vibration.Vibrate(15);

        Destroy(gameObject);
    }

    public void ApplyKnockback(Vector2 direction, float force, float knockTime, float damageOther)
    {
        TakeDamage(damageOther);

        if (IsStuck())
        {
            Vector3 bumpDir = (player.transform.position - transform.position).normalized;
            if (bumpDir == Vector3.zero) bumpDir = Vector3.right;

            Vector3 bumpPos = transform.position + bumpDir * 0.23f;
            transform.position = bumpPos; // <-- ✅ actually move visually
            StartCoroutine(ReturnFromBump(transform.position - bumpDir * 0.23f));
            return;
        }

        // Stop the leap coroutine if it is running
        if (leapCoroutine != null)
        {
            StopCoroutine(leapCoroutine);
            leapCoroutine = null;
        }

        isLeaping = false; // Ensure leaping state is reset
        isMoving = false;
        hasPlayedSlideSound = false;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direction * force * knockResistance, ForceMode2D.Impulse);
        SetStateToKnocked(knockTime);
        float distance = (force / rb.mass) / (1 + rb.drag);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhite(0.15f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (type == "Critter")
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
    }

    void ApplyKnockbackToOther(Collider2D other, Vector2 knockbackDirection)
    {
        if (knockbackDirection == Vector2.zero)
        {
            if (type == "Critter")
            {
                knockbackDirection = rb.velocity.normalized;
            }
            else if (type == "Knight")
            {
                knockbackDirection = (other.transform.position - transform.position).normalized;
            }
        }

        PlayerScript player = other.GetComponent<PlayerScript>();
        EnemyAI enemy = other.GetComponent<EnemyAI>();

        if (player != null)
        {
            player.ApplyKnockback(knockbackDirection, knockbackForce, knockTime, damage);
        }
        else if (enemy != null)
        {
            Physics2D.IgnoreCollision(mainCollider, other.GetComponent<Collider2D>(), true);
            enemy.ApplyKnockback(knockbackDirection, knockbackForce, knockTime, damage);
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
        if (state != "knocked" && state != "leaping" && state != "prepare" && state != "dead" && state != "cooldown")
        {
            return true;
        }
        return false;
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
        state = "idle";
    }

    void SetStateToPrepare()
    {
        state = "prepare";
    }

    void SetStateToAttack()
    {
        state = "attack";
        hitEntities = new HashSet<GameObject>();

        if (type != "Knight")
        {
            mainCollider.enabled = true;
        }
    }

    void SetStateToCooldown()
    {
        state = "cooldown";
        mainCollider.enabled = false;
    }

    void SetStateToIdle()
    {
        state = "idle";
    }

    void SetStateToChase()
    {
        state = "chase";
    }

    public void SetStuck(int bumpsStuck)
    {
        this.bumpsStuck = bumpsStuck;
        FindObjectOfType<AudioPlayer>().PlayCobwebStuckSound();
    }

    Vector2 GetRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f); // Random angle in degrees
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        return direction;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector2Int GetNodeWithLowestFScore(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, int> fScore)
    {
        Vector2Int lowestNode = default;
        int lowestFScore = int.MaxValue;

        foreach (var node in openSet)
        {
            int score = fScore.GetValueOrDefault(node, int.MaxValue);
            if (score < lowestFScore)
            {
                lowestFScore = score;
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions;

        if (type == "Critter")
        {
            directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        }
        else // Knight
        {
            directions = new Vector2Int[] {
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(1, -2),
            new Vector2Int(-1, 2), new Vector2Int(-1, -2)
        };
        }

        ShuffleArray(directions); // <--- shuffle directions before evaluating

        foreach (var direction in directions)
        {
            Vector2Int neighbor = cell + direction;
            if (walkableTiles.Contains(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private bool CanMoveToCell(Vector2Int cell)
    {
        Vector3Int cell3D = new Vector3Int(cell.x, cell.y, 0);
        //Debug.Log(cell.x);
        //Debug.Log(DungeonManager.GetComponent<DungeonGenerationScript01>().IsSolidAtPosition(cell3D));
        return tilemapFloor.GetTile(cell3D) != null;
    }

    private void TryPathMovesInOrder(List<Vector2Int> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int step = path[i];

            if (CanMoveToCell(step))
            {
                MoveToPosition(step); // will slide if needed
                return;
            }
        }

        // No valid moves found — stay idle or slide bump if desired
    }

    private HashSet<Vector2Int> GetWalkableTiles()
    {
        HashSet<Vector2Int> walkable = new HashSet<Vector2Int>();

        foreach (var pos in tilemapFloor.cellBounds.allPositionsWithin)
        {
            Vector3Int tilePosition = new Vector3Int(pos.x, pos.y, 0);

            // Only consider it walkable if it's a tile and not a tree
            if (tilemapFloor.GetTile(tilePosition) != null &&
                !DungeonManager.GetComponent<DungeonGenerationScript01>().IsSolidAtPosition(tilePosition))
            {
                walkable.Add((Vector2Int)tilePosition);
            }
        }

        return walkable;
    }


    private List<Vector2Int> AStarPathfinding(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { start };
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        Dictionary<Vector2Int, int> fScore = new Dictionary<Vector2Int, int> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            Vector2Int current = GetNodeWithLowestFScore(openSet, fScore);

            if (current == goal)
            {
                while (cameFrom.ContainsKey(current))
                {
                    path.Insert(0, current);
                    current = cameFrom[current];
                }
                path.Insert(0, start);
                return path;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                int tentativeGScore = gScore[current] + 1;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, int.MaxValue))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
            }
        }

        return path;
    }

    private bool IsStuck()
    {
        Vector3Int pos = new Vector3Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y),
            Mathf.FloorToInt(transform.position.z)
        );

        if (!(DungeonManager.GetComponent<DungeonGenerationScript01>().IsCobwebAtPosition(pos)))
        {
            return false;
        }

        if (bumpsStuck == 1)
        {
            DungeonManager.GetComponent<DungeonGenerationScript01>().RemoveCobwebAtPosition(pos);
            bumpsStuck = 0;
        }

        return bumpsStuck > 1;
    }

    private IEnumerator ReturnFromBump(Vector3 originalPosition)
    {
        FindObjectOfType<AudioPlayer>().PlayCobwebSound();

        yield return new WaitForSeconds(0.062f);
        transform.position = originalPosition;

        if (bumpsStuck > 1)
        {
            bumpsStuck--;
        }
        else if (bumpsStuck == 1)
        {
            Vector3Int pos = new Vector3Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y),
                Mathf.FloorToInt(transform.position.z)
            );

            FindObjectOfType<AudioPlayer>().PlayCobwebBreakSound();
            DungeonManager.GetComponent<DungeonGenerationScript01>().RemoveCobwebAtPosition(pos);
            bumpsStuck = 0;
        }

        isLeaping = false;
        SetStateToIdle();
    }

    private IEnumerator FlashWhite(float duration)
    {
        spriteRenderer.material = whiteFlashMaterial;
        yield return new WaitForSeconds(duration);
        spriteRenderer.material = originalMaterial;
    }
}