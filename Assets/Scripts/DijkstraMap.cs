using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class DijkstraMap : MonoBehaviour
{
    private Dictionary<Vector2Int, int> dijkstraMap; // Store cost for each tile position
    private HashSet<Vector2Int> walkableTiles; // Store walkable tiles
    public Tilemap tilemapFloor; // Reference to your floor tilemap

    // Cache mechanism
    private Dictionary<Vector2Int, Dictionary<Vector2Int, int>> cachedDijkstraMaps = new Dictionary<Vector2Int, Dictionary<Vector2Int, int>>();
    private List<Vector2Int> cacheAccessOrder = new List<Vector2Int>(); // To track least-recently used
    private int cacheLimit = 9; // Limit to 9 cached maps

    public void GenerateDijkstraMap(Vector2Int targetPosition)
    {
        // Check if the map for this position is already cached
        if (cachedDijkstraMaps.ContainsKey(targetPosition))
        {
            // Move the accessed map to the top of the access order (most recent)
            cacheAccessOrder.Remove(targetPosition);
            cacheAccessOrder.Add(targetPosition);

            // Use the cached map, no recalculation needed
            dijkstraMap = cachedDijkstraMaps[targetPosition];
            return;
        }

        // If not cached, generate a new Dijkstra map
        dijkstraMap = new Dictionary<Vector2Int, int>();
        walkableTiles = GetWalkableTiles(); // Get all walkable tiles from the floor tilemap

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(targetPosition);
        dijkstraMap[targetPosition] = 0; // Start with the player's position, cost = 0

        // BFS to calculate the cost for every other tile
        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();
            int currentCost = dijkstraMap[current];

            // Get all adjacent positions (up, down, left, right)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(1, 0), new Vector2Int(-1, 0)
            };

            foreach (var direction in directions)
            {
                Vector2Int neighbor = current + direction;

                // If the neighbor is walkable and not yet visited
                if (walkableTiles.Contains(neighbor) && !dijkstraMap.ContainsKey(neighbor))
                {
                    dijkstraMap[neighbor] = currentCost + 1; // Set cost to be one higher than the current tile
                    frontier.Enqueue(neighbor);
                }
            }
        }

        // Cache the newly calculated Dijkstra map
        CacheDijkstraMap(targetPosition, dijkstraMap);
    }

    // Cache the map and manage the cache size
    private void CacheDijkstraMap(Vector2Int position, Dictionary<Vector2Int, int> map)
    {
        // Add to cache
        cachedDijkstraMaps[position] = map;
        cacheAccessOrder.Add(position);

        // Check cache size and remove the least recently used if needed
        if (cacheAccessOrder.Count > cacheLimit)
        {
            Vector2Int leastUsed = cacheAccessOrder[0]; // Get the oldest map (least recently used)
            cachedDijkstraMaps.Remove(leastUsed); // Remove it from the cache
            cacheAccessOrder.RemoveAt(0); // Remove it from the access order
        }
    }

    // Get all walkable tiles from the dungeon's tilemap
    private HashSet<Vector2Int> GetWalkableTiles()
    {
        HashSet<Vector2Int> walkable = new HashSet<Vector2Int>();

        // Loop through all tiles in the tilemap and find walkable tiles
        foreach (var pos in tilemapFloor.cellBounds.allPositionsWithin)
        {
            Vector3Int tilePosition = new Vector3Int(pos.x, pos.y, pos.z);
            if (tilemapFloor.GetTile(tilePosition) != null) // If there's a floor tile, it's walkable
            {
                walkable.Add(new Vector2Int(pos.x, pos.y)); // Store the integer position
            }
        }

        return walkable;
    }

    // Get the Dijkstra cost for a specific position
    public int GetCost(Vector2Int position)
    {
        return dijkstraMap.ContainsKey(position) ? dijkstraMap[position] : int.MaxValue; // Return max value if not in the map
    }
}
