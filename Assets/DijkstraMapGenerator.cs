using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DijkstraMapGenerator : MonoBehaviour
{
    public Dictionary<Vector2Int, int> dijkstraMap;
    public Tilemap tilemap;
    public GameObject DungeonManager;

    public void GenerateDijkstraMap(Vector2Int playerPosition)
    {
        dijkstraMap = new Dictionary<Vector2Int, int>();
        dijkstraMap[playerPosition] = 0;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(playerPosition);

        while (queue.Count > 0)
        {
            Vector2Int currentCell = queue.Dequeue();
            int currentValue = dijkstraMap[currentCell];

            Vector2Int[] neighbors = GetNeighbors(currentCell);
            foreach (Vector2Int neighbor in neighbors)
            {
                if (!dijkstraMap.ContainsKey(neighbor) && IsFloor(neighbor))
                {
                    dijkstraMap[neighbor] = currentValue + 1;
                    queue.Enqueue(neighbor);
                }
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
            if (IsFloor(neighbor)/* && !DungeonManager.GetComponent<DungeonGenerationScript>().IsPositionOccupiedSolid(neighbor)*/)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors.ToArray();
    }

    bool IsFloor(Vector2Int cell)
    {
        Vector3Int cellPosition = new Vector3Int(cell.x, cell.y, 0);
        return tilemap.GetTile(cellPosition) != null;
    }

    public int GetDijkstraValue(Vector2Int cell)
    {
        int value;
        if (dijkstraMap.TryGetValue(cell, out value))
        {
            return value;
        }
        else
        {
            return int.MaxValue;
        }
    }
}
