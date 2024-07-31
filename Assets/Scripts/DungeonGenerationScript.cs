using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine.SceneManagement;
using System.Linq;

public class DungeonGenerationScript : MonoBehaviour
{
    public int numRooms;
    public float mainRoomsPercentage = 0.08f;
    public float extraEdgesPercentage = 0.15f;
    public float aspectRatioWeight = 0f;
    public float chanceTileFloor01, chanceTileFloor02, chanceTileFloor03, chanceTileFloor04;
    public int startingRoom;
    public int meanWidth, stdDevWidth, minWidth, maxWidth;
    public int meanHeight, stdDevHeight, minHeight, maxHeight;
    public int minWidthFinal, minHeightFinal;

    public float cellSize;
    public Color color = Color.white;
    public Grid Map;
    private GameObject[] rectangles;
    private Rigidbody2D[] rectanglesRigidBodies;
    private bool dungeonBuilt = false;
    List<GameObject> sortedRectangles;

    public Player Player;
    public GameObject playerGO;

    public int numLines;
    public Vector2[] startPoints;
    public Vector2[] endPoints;
    public Color lineColor = Color.blue;
    public float lineWidth = .3f;
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;
    private LineRenderer[] lineRenderers;
    private Delaunator delaunator;
    Vector3 tileSize = new Vector3(1,1,1);

    public Tilemap tilemapFloor, tilemapFloorWalls, tilemapDecoratives, tilemapWalls;
    public Tile tileFloor01, tileBricks01, tileWallUpper, tileWallUpperLeft, tileWallUpperRight, tileWallLowerLeft, tileWallLowerRight;
    public Tile tileFloor02, tileFloor03, tileFloor04;
    public Tile tileCornerUpperLeft, tileCornerUpperRight, tileCornerLowerLeft, tileCornerLowerRight, tileCorner02LowerLeft, tileCorner02LowerRight;
    public Tile tileBricks02, tileBricks03;
    public Tile tileCarpet01Upper, tileCarpet01UpperLeft, tileCarpet01UpperRight, tileCarpet01Lower, tileCarpet01LowerLeft, tileCarpet01LowerRight, tileCarpet01Left, tileCarpet01Right, tileCarpet01Center;
    public Tile tileCarpet02Upper, tileCarpet02UpperLeft, tileCarpet02UpperRight, tileCarpet02Lower, tileCarpet02LowerLeft, tileCarpet02LowerRight, tileCarpet02Left, tileCarpet02Right, tileCarpet02Center;

    [Header("Wall Tiles")]
    [SerializeField] private Tile tileWallHorizontal;
    [SerializeField] private Tile tileWallHorizontalUpLeft, tileWallHorizontalUpRight, tileWallHorizontalDownLeft, tileWallHorizontalDownRight, tileWallLeft, tileWallRight, tileWallCornerUpLeft, tileWallCornerUpRight, tileWallBase, tileWallBaseCornerLeft, tileWallBaseCornerRight, tileWallBaseDownLeft, tileWallBaseDownRight, tileWallBaseUpLeft, tileWallBaseUpRight;

    [Header("Others")]
    //Prefabs
    [SerializeField] private GameObject ChestPrefab;
    [SerializeField] private GameObject[] Enemies;
    public int enemiesCount;

    [SerializeField] private GameObject boxPrefab01;
    [SerializeField] private GameObject spikePrefab01;
    public GameObject camera0;

    Dictionary<int, HashSet<int>> graphFinal = new Dictionary<int, HashSet<int>>();
    public List<Vector3> objectPositions = new List<Vector3>();
    public List<Vector3> objectPositionsSolid = new List<Vector3>();
    public List<Vector3> objectPositionsCarpet = new List<Vector3>();
    public List<Vector3> objectPositionsDoor = new List<Vector3>();
    public List<Vector3> objectPositionsDoorLeft = new List<Vector3>();
    public List<Vector3> objectPositionsDoorRight = new List<Vector3>();
    public List<Vector3> objectPositionsDoorLower = new List<Vector3>();
    public List<Vector3> objectPositionsDoorUpper = new List<Vector3>();
    public GameObject DungeonManager, DijkstraMap;

    public GameObject EnemyNEW;

    Sprite CreateRectangleSprite(float width, float height)
    {
        Texture2D texture = new Texture2D((int)width, (int)height);
        Color[] colors = new Color[(int)(width * height)];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        texture.SetPixels(colors);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);

        return sprite;
    }

    public List<(int, int)> GetAllEdges(Dictionary<int, HashSet<int>> graph)
    {
        List<(int, int)> edges = new List<(int, int)>();
        foreach (var kvp in graph)
        {
            int source = kvp.Key;
            foreach (int destination in kvp.Value)
            {
                edges.Add((source, destination));
            }
        }
        return edges;
    }

    public int Find(int[] parent, int node)
    {
        if (parent[node] != node)
        {
            parent[node] = Find(parent, parent[node]);
        }
        return parent[node];
    }

    public void Union(int[] parent, int[] rank, int node1, int node2)
    {
        //Union operation
        int root1 = Find(parent, node1);
        int root2 = Find(parent, node2);

        if (root1 == root2)
        {
            return;
        }

        if (rank[root1] > rank[root2])
        {
            parent[root2] = root1;
        }
        else if (rank[root2] > rank[root1])
        {
            parent[root1] = root2;
        }
        else
        {
            parent[root2] = root1;
            rank[root1]++;
        }
    }

    int GetRandomIntExcluding(int min, int max, List<int> exclusions)
    {
        //Select a random index for special rooms excluding already selected ones
        int randomInt = UnityEngine.Random.Range(min, max);
        int k = 0;

        while (exclusions.Contains(randomInt))
        {
            if (k == 1000) return -1;
            randomInt = UnityEngine.Random.Range(min, max);
            k++;
        }

        return randomInt;
    }

    int roomDistance(int fromNode, int toNode)
    {
        Vector2 startPoint = new Vector2(sortedRectangles[fromNode].transform.position.x + sortedRectangles[fromNode].GetComponent<SpriteRenderer>().bounds.size.x / 2, sortedRectangles[fromNode].transform.position.y + sortedRectangles[fromNode].GetComponent<SpriteRenderer>().bounds.size.y / 2);
        Vector2 endPoint = new Vector2(sortedRectangles[toNode].transform.position.x + sortedRectangles[toNode].GetComponent<SpriteRenderer>().bounds.size.x / 2, sortedRectangles[toNode].transform.position.y + sortedRectangles[toNode].GetComponent<SpriteRenderer>().bounds.size.y / 2);

        float distance = Vector2.Distance(startPoint, endPoint);
        int distanceAsInt = Mathf.RoundToInt(distance);

        return distanceAsInt;
    }

    public Dictionary<int, HashSet<int>> GetMinimalSpanningTree(Dictionary<int, HashSet<int>> graph)
    {
        //Create minimal spanning tree
        Dictionary<int, HashSet<int>> minimalSpanningTree = new Dictionary<int, HashSet<int>>();

        List<(int, int)> edges = GetAllEdges(graph);

        edges.Sort((x, y) => roomDistance(x.Item1, x.Item2) - roomDistance(y.Item1, y.Item2));

        int[] parent = new int[graph.Count];
        int[] rank = new int[graph.Count];
        for (int i = 0; i < parent.Length; i++)
        {
            parent[i] = i;
            rank[i] = 0;
        }

        foreach ((int, int) edge in edges)
        {
            int source = edge.Item1;
            int destination = edge.Item2;

            if (Find(parent, source) != Find(parent, destination))
            {
                if (!minimalSpanningTree.ContainsKey(source))
                {
                    minimalSpanningTree[source] = new HashSet<int>();
                }
                if (!minimalSpanningTree.ContainsKey(destination))
                {
                    minimalSpanningTree[destination] = new HashSet<int>();
                }

                minimalSpanningTree[source].Add(destination);
                minimalSpanningTree[destination].Add(source);

                Union(parent, rank, source, destination);
            }
        }

        return minimalSpanningTree;
    }

    public void PlaceSpikes(Vector3 cornerLowerLeft, Vector3 cornerLowerRight, Vector3 cornerUpperLeft, Vector3 cornerUpperRight, int width, int height)
    {
        if (Random.Range(0f, 1f) < .6 && height < 16)
        {
            List<float> possibleXCoordinates = new List<float>();
            for (float x = cornerLowerLeft.x + 1; x <= cornerLowerRight.x - 1; x++)
            {
                possibleXCoordinates.Add(x);
            }
            // Remove x-coordinates where upper doors are
            foreach (Vector3 doorPos in objectPositionsDoorUpper)
            {
                possibleXCoordinates.Remove(doorPos.x);
            }
            // Remove x-coordinates where lower doors are
            foreach (Vector3 doorPos in objectPositionsDoorLower)
            {
                possibleXCoordinates.Remove(doorPos.x);
            }
            if (possibleXCoordinates.Count > 0)
            {
                // If there are possible x-coordinates, select one at random
                int randomIndex = Random.Range(0, possibleXCoordinates.Count);
                float chosenX = possibleXCoordinates[randomIndex];
                for (Vector3 j = cornerUpperLeft; j.y >= cornerLowerLeft.y; j += new Vector3(0, -1, 0))
                {
                    Vector3 jNew = new Vector3(chosenX, j.y, j.z);
                    Vector2Int jInt = new Vector2Int(Mathf.FloorToInt(chosenX - 0.5f), Mathf.FloorToInt(j.y - 0.5f));
                    if (!IsPositionCarpet(jInt))
                    {
                        GameObject spikeN = Instantiate(spikePrefab01, jNew, Quaternion.identity);
                    }
                }
            }
        }

        if (Random.Range(0f, 1f) < .6 && width < 16)
        {
            List<float> possibleYCoordinates = new List<float>();
            for (float y = cornerLowerLeft.y + 1; y <= cornerUpperLeft.y - 1; y++)
            {
                possibleYCoordinates.Add(y);
            }
            // Remove x-coordinates where upper doors are
            foreach (Vector3 doorPos in objectPositionsDoorLeft)
            {
                possibleYCoordinates.Remove(doorPos.y);
            }
            // Remove x-coordinates where lower doors are
            foreach (Vector3 doorPos in objectPositionsDoorRight)
            {
                possibleYCoordinates.Remove(doorPos.y);
            }
            if (possibleYCoordinates.Count > 0)
            {
                // If there are possible x-coordinates, select one at random
                int randomIndex = Random.Range(0, possibleYCoordinates.Count);
                float chosenY = possibleYCoordinates[randomIndex];

                for (Vector3 j = cornerUpperLeft; j.x <= cornerUpperRight.x; j += new Vector3(1, 0, 0))
                {
                    Vector3 jNew = new Vector3(j.x, chosenY, j.z);
                    Vector2Int jInt = new Vector2Int(Mathf.FloorToInt(j.x - 0.5f), Mathf.FloorToInt(chosenY - 0.5f));
                    if (!IsPositionCarpet(jInt))
                    {
                        GameObject spikeN = Instantiate(spikePrefab01, jNew, Quaternion.identity);
                    }
                }
            }
        }
    }

    void PlaceCarpet(Vector3 cornerLowerLeft, Vector3 cornerLowerRight, Vector3 cornerUpperLeft, Vector3 cornerUpperRight, TileBase tileCarpet01Center, TileBase tileCarpet01Lower, TileBase tileCarpet01Upper, TileBase tileCarpet01Left, TileBase tileCarpet01Right, TileBase tileCarpet01LowerLeft, TileBase tileCarpet01LowerRight, TileBase tileCarpet01UpperLeft, TileBase tileCarpet01UpperRight)
    {
        if (Random.Range(0f, 1f) < .5) {
            int randomLeft = Mathf.FloorToInt(Random.Range(cornerLowerLeft.x, cornerLowerRight.x - 2));
            int randomLower = Mathf.FloorToInt(Random.Range(cornerLowerLeft.y, cornerUpperLeft.y - 2));

            Vector3Int jInt01 = new Vector3Int(Mathf.FloorToInt(randomLeft + 1), Mathf.FloorToInt(randomLower + 1), Mathf.FloorToInt(cornerLowerLeft.z));
            tilemapFloor.SetTile(jInt01, tileCarpet01LowerLeft);

            Vector3Int jInt02 = new Vector3Int(Mathf.FloorToInt(randomLeft + 2), Mathf.FloorToInt(randomLower + 1), Mathf.FloorToInt(cornerLowerRight.z));
            tilemapFloor.SetTile(jInt02, tileCarpet01LowerRight);

            Vector3Int jInt03 = new Vector3Int(Mathf.FloorToInt(randomLeft + 1), Mathf.FloorToInt(randomLower + 2), Mathf.FloorToInt(cornerUpperLeft.z));
            tilemapFloor.SetTile(jInt03, tileCarpet01UpperLeft);

            Vector3Int jInt04 = new Vector3Int(Mathf.FloorToInt(randomLeft + 2), Mathf.FloorToInt(randomLower + 2), Mathf.FloorToInt(cornerUpperRight.z));
            tilemapFloor.SetTile(jInt04, tileCarpet01UpperRight);

            objectPositionsCarpet.Add(jInt01);
            objectPositionsCarpet.Add(jInt02);
            objectPositionsCarpet.Add(jInt03);
            objectPositionsCarpet.Add(jInt04);

        } else if (Random.Range(0f, 1f) < .8f) //Fill carpet no edges
        {
            for (Vector3 j = cornerLowerLeft; j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
            {
                if (j == cornerLowerLeft || j == cornerLowerRight) continue;

                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y + 1), Mathf.FloorToInt(j.z));

                for (Vector3 k = cornerUpperLeft + new Vector3(0, -1, 0); k.y >= cornerLowerLeft.y; k += new Vector3(0, -1, 0))
                {
                    if (k == cornerUpperLeft + new Vector3(0, -1, 0) || k == cornerLowerLeft) continue;
                    Vector3Int kInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(k.y), Mathf.FloorToInt(k.z));
                    tilemapFloor.SetTile(kInt, tileCarpet01Center);
                    objectPositionsCarpet.Add(kInt);
                }
            }

            for (Vector3 j = cornerLowerLeft; j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
            {
                if (j == cornerLowerLeft || j == cornerLowerRight) continue;

                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y + 1), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Lower);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperLeft; j.x <= cornerUpperRight.x; j += new Vector3(1, 0, 0))
            {
                if (j == cornerUpperLeft || j == cornerUpperRight) continue;
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y - 1), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Upper);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperLeft + new Vector3(0, -1, 0); j.y >= cornerLowerLeft.y; j += new Vector3(0, -1, 0))
            {
                if (j == cornerUpperLeft + new Vector3(0, -1, 0) || j == cornerLowerLeft) continue;
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x + 1), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Left);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperRight + new Vector3(0, -1, 0); j.y >= cornerLowerRight.y; j += new Vector3(0, -1, 0))
            {
                if (j == cornerUpperRight + new Vector3(0, -1, 0) || j == cornerLowerRight) continue;
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x - 1), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Right);
                objectPositionsCarpet.Add(jInt);
            }

            Vector3Int jInt01 = new Vector3Int(Mathf.FloorToInt(cornerLowerLeft.x + 1), Mathf.FloorToInt(cornerLowerLeft.y + 1), Mathf.FloorToInt(cornerLowerLeft.z));
            tilemapFloor.SetTile(jInt01, tileCarpet01LowerLeft);

            Vector3Int jInt02 = new Vector3Int(Mathf.FloorToInt(cornerLowerRight.x - 1), Mathf.FloorToInt(cornerLowerRight.y + 1), Mathf.FloorToInt(cornerLowerRight.z));
            tilemapFloor.SetTile(jInt02, tileCarpet01LowerRight);

            Vector3Int jInt03 = new Vector3Int(Mathf.FloorToInt(cornerUpperLeft.x + 1), Mathf.FloorToInt(cornerUpperLeft.y - 1), Mathf.FloorToInt(cornerUpperLeft.z));
            tilemapFloor.SetTile(jInt03, tileCarpet01UpperLeft);

            Vector3Int jInt04 = new Vector3Int(Mathf.FloorToInt(cornerUpperRight.x - 1), Mathf.FloorToInt(cornerUpperRight.y - 1), Mathf.FloorToInt(cornerUpperRight.z));
            tilemapFloor.SetTile(jInt04, tileCarpet01UpperRight);

            objectPositionsCarpet.Add(jInt01);
            objectPositionsCarpet.Add(jInt02);
            objectPositionsCarpet.Add(jInt03);
            objectPositionsCarpet.Add(jInt04);
        }
        else //Fill carpet
        {
            for (Vector3 j = cornerLowerLeft; j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
            {
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y + 1), Mathf.FloorToInt(j.z));

                for (Vector3 k = cornerUpperLeft + new Vector3(0, -1, 0); k.y >= cornerLowerLeft.y; k += new Vector3(0, -1, 0))
                {
                    Vector3Int kInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(k.y), Mathf.FloorToInt(k.z));
                    tilemapFloor.SetTile(kInt, tileCarpet01Center);
                    objectPositionsCarpet.Add(kInt);
                }
            }

            for (Vector3 j = cornerLowerLeft; j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
            {
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Lower);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperLeft; j.x <= cornerUpperRight.x; j += new Vector3(1, 0, 0))
            {
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Upper);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperLeft + new Vector3(0, -1, 0); j.y >= cornerLowerLeft.y; j += new Vector3(0, -1, 0))
            {
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Left);
                objectPositionsCarpet.Add(jInt);
            }

            for (Vector3 j = cornerUpperRight + new Vector3(0, -1, 0); j.y >= cornerLowerRight.y; j += new Vector3(0, -1, 0))
            {
                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                tilemapFloor.SetTile(jInt, tileCarpet01Right);
                objectPositionsCarpet.Add(jInt);
            }

            Vector3Int jInt01 = new Vector3Int(Mathf.FloorToInt(cornerLowerLeft.x), Mathf.FloorToInt(cornerLowerLeft.y), Mathf.FloorToInt(cornerLowerLeft.z));
            tilemapFloor.SetTile(jInt01, tileCarpet01LowerLeft);

            Vector3Int jInt02 = new Vector3Int(Mathf.FloorToInt(cornerLowerRight.x), Mathf.FloorToInt(cornerLowerRight.y), Mathf.FloorToInt(cornerLowerRight.z));
            tilemapFloor.SetTile(jInt02, tileCarpet01LowerRight);

            Vector3Int jInt03 = new Vector3Int(Mathf.FloorToInt(cornerUpperLeft.x), Mathf.FloorToInt(cornerUpperLeft.y), Mathf.FloorToInt(cornerUpperLeft.z));
            tilemapFloor.SetTile(jInt03, tileCarpet01UpperLeft);

            Vector3Int jInt04 = new Vector3Int(Mathf.FloorToInt(cornerUpperRight.x), Mathf.FloorToInt(cornerUpperRight.y), Mathf.FloorToInt(cornerUpperRight.z));
            tilemapFloor.SetTile(jInt04, tileCarpet01UpperRight);

            objectPositionsCarpet.Add(jInt01);
            objectPositionsCarpet.Add(jInt02);
            objectPositionsCarpet.Add(jInt03);
            objectPositionsCarpet.Add(jInt04);
        }
    }

    TileBase getRandomTileFloor()
    {
        //Get a random tile based on a chance
        TileBase tileToPaint;
        float randomValue = Random.value;

        if (randomValue < chanceTileFloor01)
        {
            tileToPaint = tileFloor01;
        }
        else if (randomValue < chanceTileFloor02)
        {
            tileToPaint = tileFloor02;
        }
        else if (randomValue < chanceTileFloor03)
        {
            tileToPaint = tileFloor03;
        }
        else
        {
            tileToPaint = tileFloor04;
        }
        return tileToPaint;
    }

    private IEnumerator MakeDynamicAfterDelay(float delay, BoxCollider2D boxCollider1)
    {
        yield return new WaitForSeconds(delay);
        boxCollider1.enabled = true;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("SceneMap1");
    }

    void Start()
    {
        cellSize = Map.cellSize[0] * 100;
        rectangles = new GameObject[numRooms];

        rectanglesRigidBodies = new Rigidbody2D[numRooms];

        sortedRectangles = new List<GameObject>();

        for (int i = 0; i < numRooms; i += 2)
        {
            //Create a list of rectangles for rooms
            int width1 = Mathf.RoundToInt(Mathf.Clamp(UnityEngine.Random.Range(meanWidth - stdDevWidth, meanWidth + stdDevWidth + 1), minWidth, maxWidth));
            int height1 = Mathf.RoundToInt(Mathf.Clamp(UnityEngine.Random.Range(meanHeight - stdDevHeight, meanHeight + stdDevHeight + 1), minHeight, maxHeight));

            if (i == 0)
            {
                int baseWidth = Random.Range(11, 15);
                int baseHeight = baseWidth * 2;

                if (Random.value < 0.5f)
                {
                    width1 = baseHeight;
                    height1 = baseWidth;
                }
                else
                {
                    width1 = baseWidth;
                    height1 = baseHeight;
                }
            }

            GameObject rectangle1 = new GameObject("Rectangle" + i);
            SpriteRenderer spriteRenderer1 = rectangle1.AddComponent<SpriteRenderer>();
            spriteRenderer1.color = color;
            spriteRenderer1.sprite = CreateRectangleSprite(cellSize * width1, cellSize * height1);
            rectangle1.transform.position = new Vector2(UnityEngine.Random.Range(-4, 4), UnityEngine.Random.Range(-4, 4));
            rectangles[i] = rectangle1;

            //Add box colliders and rigid bodies to use the physics engine
            BoxCollider2D boxCollider1 = rectangles[i].AddComponent<BoxCollider2D>();
            boxCollider1.size = new Vector2(width1, height1);

            Rigidbody2D rigidbody1 = rectangle1.AddComponent<Rigidbody2D>();
            rectanglesRigidBodies[i] = rigidbody1;
            rigidbody1.mass = 1.0f;
            rigidbody1.gravityScale = 0.0f;
            rigidbody1.drag = 2f;
            rigidbody1.angularDrag = 100f;
            rectangle1.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            Vector2 force1 = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            if (i != 0) rigidbody1.AddForce(force1 * UnityEngine.Random.Range(500f, 1200f));
            if (i != 0) rectangles[i].AddComponent<RoomScript>();

            if (i == 0)
            {
                boxCollider1.enabled = false;
                StartCoroutine(MakeDynamicAfterDelay(1f, boxCollider1));
            }






            int width2 = Mathf.RoundToInt(Mathf.Clamp(UnityEngine.Random.Range(meanWidth - stdDevWidth, meanWidth + stdDevWidth + 1), minWidth, maxWidth));
            int height2 = Mathf.RoundToInt(Mathf.Clamp(UnityEngine.Random.Range(meanHeight - stdDevHeight, meanHeight + stdDevHeight + 1), minHeight, maxHeight));

            GameObject rectangle2 = new GameObject("Rectangle" + i);
            SpriteRenderer spriteRenderer2 = rectangle2.AddComponent<SpriteRenderer>();
            spriteRenderer2.color = color;
            spriteRenderer2.sprite = CreateRectangleSprite(cellSize * width2, cellSize * height2);
            rectangle2.transform.position = new Vector2(rectangle1.transform.position.x, rectangle1.transform.position.y);
            rectangles[i+1] = rectangle2;

            //Add box colliders and rigid bodies to use the physics engine
            BoxCollider2D boxCollider2 = rectangles[i+1].AddComponent<BoxCollider2D>();
            boxCollider2.size = new Vector2(width2, height2);

            Rigidbody2D rigidbody2 = rectangle2.AddComponent<Rigidbody2D>();
            rectanglesRigidBodies[i+1] = rigidbody2;
            rigidbody2.mass = 1.0f;
            rigidbody2.gravityScale = 0.0f;
            rigidbody2.drag = 2f;
            rigidbody2.angularDrag = 100f;
            rectangle2.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            Vector2 force2 = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
            rigidbody2.AddForce(force2 * UnityEngine.Random.Range(500f, 1200f));

            rectangles[i+1].AddComponent<RoomScript>();
        }
    }

    public Dictionary<int, HashSet<int>> AddExtraEdges(Dictionary<int, HashSet<int>> graph, Dictionary<int, HashSet<int>> minimalSpanningTree, float extraEdgesPercentage)
    {
        //Add some edges removed when creating the minimal spanning tree
        Dictionary<int, HashSet<int>> thirdGraph = new Dictionary<int, HashSet<int>>();

        foreach (KeyValuePair<int, HashSet<int>> kvp in minimalSpanningTree)
        {
            thirdGraph[kvp.Key] = new HashSet<int>(kvp.Value);
        }

        int numAdditionalEdges = Mathf.RoundToInt(graph.Count * extraEdgesPercentage);

        List<(int, int)> edgesToAdd = new List<(int, int)>();
        foreach (KeyValuePair<int, HashSet<int>> kvp in graph)
        {
            int source = kvp.Key;
            foreach (int destination in kvp.Value)
            {
                if (!minimalSpanningTree.ContainsKey(source) || !minimalSpanningTree[source].Contains(destination))
                {
                    if (edgesToAdd.Count < numAdditionalEdges)
                    {
                        edgesToAdd.Add((source, destination));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        foreach ((int, int) edge in edgesToAdd)
        {
            int source = edge.Item1;
            int destination = edge.Item2;

            if (!thirdGraph.ContainsKey(source))
            {
                thirdGraph[source] = new HashSet<int>();
            }
            if (!thirdGraph.ContainsKey(destination))
            {
                thirdGraph[destination] = new HashSet<int>();
            }

            thirdGraph[source].Add(destination);
            thirdGraph[destination].Add(source);
        }

        return thirdGraph;
    }

    void DFS(int currentNode, Dictionary<int, HashSet<int>> graph, HashSet<int> visited, HashSet<int> component)
    {
        visited.Add(currentNode);
        component.Add(currentNode);

        foreach (int neighborNode in graph[currentNode])
        {
            if (!visited.Contains(neighborNode))
            {
                DFS(neighborNode, graph, visited, component);
            }
        }
    }

    public bool IsPositionOccupied(Vector3 position)
    {
        return objectPositions.Contains(position);
    }

    public bool IsPositionOccupiedSolid(Vector3 position)
    {
        return objectPositionsSolid.Contains(position);
    }

    public bool IsPositionOccupiedSolid(Vector2Int position)
    {
        return objectPositionsSolid.Contains(new Vector3(position.x + 0.5f, position.y + 0.5f, 0));
    }

    public bool IsPositionCarpet(Vector3 position)
    {
        return objectPositionsCarpet.Contains(position);
    }

    public bool IsPositionCarpet(Vector2Int position)
    {
        return objectPositionsCarpet.Contains(new Vector3(position.x, position.y, 0));
    }

    public bool IsPositionDoor(Vector3 position)
    {
        return objectPositionsDoor.Contains(position);
    }

    public bool IsPositionIsPositionDoor(Vector2Int position)
    {
        return objectPositionsDoor.Contains(new Vector3(position.x + 0.5f, position.y + 0.5f, 0));
    }

    bool IsWall(Vector3 position)
    {
        TileBase wallTile = tilemapWalls.GetTile(tilemapWalls.WorldToCell(position));
        TileBase floorTile = tilemapFloorWalls.GetTile(tilemapFloorWalls.WorldToCell(position));
        
        return (wallTile == null && floorTile == null);
    }

    void generateHallways(Dictionary<int, HashSet<int>> extraEdgesGraph, HashSet<string> nodePairs, bool initialBuild, int buildStep = 0)
    {
        //Build the hallways either first time or try to connect new rooms
        List<HashSet<int>> connectedComponents = new List<HashSet<int>>();
        if (!initialBuild)
        {
            HashSet<int> visitedNodes = new HashSet<int>();

            foreach (var nodePair2 in graphFinal)
            {
                int fromNode2 = nodePair2.Key;

                if (visitedNodes.Contains(fromNode2))
                    continue;

                HashSet<int> currentComponent = new HashSet<int>();
                DFS(fromNode2, graphFinal, visitedNodes, currentComponent);
                connectedComponents.Add(currentComponent);
            }

            connectedComponents.Sort((a, b) => b.Count.CompareTo(a.Count));
            foreach (HashSet<int> component in connectedComponents)
            {
                string componentString = "Connected Component: ";
                foreach (int node in component)
                {
                    componentString += node.ToString() + " ";
                }
                Debug.Log(componentString);
            }
            if (buildStep == connectedComponents.Count) return;
        }

        foreach (var nodePair in extraEdgesGraph)
        {
            int fromNode = nodePair.Key;
            foreach (int toNode in (initialBuild ? nodePair.Value: connectedComponents[buildStep]))
            {
                Debug.Log("Try: " + fromNode + " " + toNode);
                if (nodePairs.Contains($"{toNode},{fromNode}") || nodePairs.Contains($"{fromNode},{toNode}"))
                {
                    continue;
                }

                Rigidbody2D rb1 = sortedRectangles[fromNode].GetComponent<Rigidbody2D>();
                Vector3Int startPos1 = tilemapFloor.WorldToCell(rb1.position);
                Rigidbody2D rb2 = sortedRectangles[toNode].GetComponent<Rigidbody2D>();
                Vector3Int startPos2 = tilemapFloor.WorldToCell(rb2.position);

                Vector3Int sizeInTiles1 = new Vector3Int(
                    Mathf.RoundToInt(rb1.GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                    Mathf.RoundToInt(rb1.GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                    1);
                Vector3Int sizeInTiles2 = new Vector3Int(
                    Mathf.RoundToInt(rb2.GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                    Mathf.RoundToInt(rb2.GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                    1);

                bool builtL = false;
                bool builtN = false;

                if (startPos1.x < startPos2.x)
                {
                    bool case_orientation = true;
                    if (startPos1.y > startPos2.y)
                    {
                        case_orientation = false;
                    }

                    if ((case_orientation && startPos1.y <= startPos2.y && startPos2.y < startPos1.y + sizeInTiles1.y - 6) || (!case_orientation && startPos1.y > startPos2.y && startPos1.y < startPos2.y + sizeInTiles2.y - 6))
                    {
                        Vector3Int startPosOffset = case_orientation ? startPos2 : startPos1;
                        int startPosOffsetExtra = case_orientation ? 0 : startPos2.x - startPos1.x;

                        bool intersectionN = false;
                        for (int i = 0; i <= startPos2.x - startPos1.x - sizeInTiles1.x + 1; ++i)
                        {
                            int offset = case_orientation ? i * -1 : i + sizeInTiles1.x - 1;

                            if (tilemapFloor.GetTile(startPosOffset + new Vector3Int(offset, 3, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloor.GetTile(startPosOffset + new Vector3Int(offset, 4, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }

                            if (i == 0 || i == startPos2.x - startPos1.x - sizeInTiles1.x + 1) continue;
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(offset, 2, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(offset, 2, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(offset, 3, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(offset, 3, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(offset, 5, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(offset, 5, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(offset, 6, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(offset, 6, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                        }

                        if (!intersectionN)
                        {
                            for (int i = 0; i <= startPos2.x - startPos1.x - sizeInTiles1.x + 1; ++i)
                            {
                                int offset = case_orientation ? i * -1 : i + sizeInTiles1.x - 1;

                                Vector3Int tilePos = startPosOffset + new Vector3Int(offset, 3, 0);
                                TileBase tileToPaint = getRandomTileFloor();
                                tilemapFloor.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(offset, 4, 0);
                                tileToPaint = getRandomTileFloor();
                                tilemapFloor.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(offset, 3, 0);
                                tileToPaint = tileWallUpper;
                                tilemapWalls.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(offset, 6, 0);
                                tileToPaint = tileWallUpper;
                                tilemapWalls.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(offset, 5, 0);
                                tileToPaint = tileBricks01;
                                tilemapFloorWalls.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(offset, 2, 0);
                                tileToPaint = tileBricks01;
                                tilemapWalls.SetTile(tilePos, tileToPaint);
                            }

                            Vector3Int tilePos2 = startPosOffset + new Vector3Int(0, 6, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            TileBase tileToPaint2 = tileCornerUpperLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 6, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileCornerUpperRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(0, 5, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(0, 4, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tilemapWalls.SetTile(tilePos2, null);

                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 5, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 4, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tilemapWalls.SetTile(tilePos2, null);

                            tilePos2 = startPosOffset + new Vector3Int(0, 5, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileBricks03;
                            tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 5, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileBricks02;
                            tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(0, 2, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileCornerLowerRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 2, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileCornerLowerLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(0, 3, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileCorner02LowerRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(startPos1.x - startPos2.x + sizeInTiles1.x - 1, 3, 0) + new Vector3Int(startPosOffsetExtra, 0, 0);
                            tileToPaint2 = tileCorner02LowerLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            builtN = true;
                            nodePairs.Add($"{fromNode},{toNode}");
                            if (!graphFinal.ContainsKey(fromNode))
                            {
                                graphFinal[fromNode] = new HashSet<int>();
                            }
                            if (!graphFinal.ContainsKey(toNode))
                            {
                                graphFinal[toNode] = new HashSet<int>();
                            }
                            graphFinal[fromNode].Add(toNode);
                            graphFinal[toNode].Add(fromNode);

                            if (!extraEdgesGraph.ContainsKey(fromNode))
                            {
                                extraEdgesGraph[fromNode] = new HashSet<int>();
                            }
                            if (!extraEdgesGraph.ContainsKey(toNode))
                            {
                                extraEdgesGraph[toNode] = new HashSet<int>();
                            }
                            extraEdgesGraph[fromNode].Add(toNode);
                            extraEdgesGraph[toNode].Add(fromNode);

                            if (!initialBuild)
                            {
                                Debug.Log("Changed graph.");
                                generateHallways(extraEdgesGraph, nodePairs, false, 0);
                                return;
                            }
                            continue;
                        }
                    }

                    if (builtN) continue;

                    //if ((case_orientation && startPos1.x <= startPos2.x && startPos2.x < startPos1.x + sizeInTiles1.x - 5) || (!case_orientation && startPos1.x > startPos2.x && startPos1.x < startPos2.x + sizeInTiles2.x - 5))
                    if (case_orientation && startPos1.y <= startPos2.y) //L Hallway
                    {
                        float randomNumber = Random.value;
                        for (int j = 0; j < 2; ++j)
                        {
                            if (randomNumber > 0.5f)
                            {
                                for (int startL = startPos2.y + 4; startL < startPos2.y + sizeInTiles2.y - 3; ++startL)
                                {
                                    for (int endL = startPos1.x + sizeInTiles1.x - 4; endL > startPos1.x + 1; --endL)
                                    {
                                        bool intersectionL = false;

                                        if (endL > startPos2.x) //Fix impossible L hallways bug #1
                                        {
                                            intersectionL = true;   //  -
                                            continue;               // I
                                        }

                                        for (int i = startPos2.x; i >= endL; --i)
                                        {
                                            if (tilemapFloor.GetTile(new Vector3Int(i, startL, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloor.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }

                                            if (i == startPos2.x) continue;

                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL + 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL + 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL + 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL + 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL - 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL - 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                        }

                                        if (intersectionL) continue;
                                        for (int i = startPos1.y + sizeInTiles1.y - 1; i <= startL; ++i)
                                        {
                                            if (tilemapFloor.GetTile(new Vector3Int(endL, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloor.GetTile(new Vector3Int(endL + 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }

                                            if (i == startPos1.y + sizeInTiles1.y || i == startPos1.y + sizeInTiles1.y - 1) continue;

                                            if (tilemapWalls.GetTile(new Vector3Int(endL - 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(endL - 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(endL + 2, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(endL + 2, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                        }
                                        if (intersectionL) continue;

                                        for (int i = startPos2.x; i >= endL; --i)
                                        {
                                            Vector3Int tilePos = new Vector3Int(i, startL, 0);
                                            TileBase tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL - 1, 0);
                                            tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL + 2, 0);
                                            tileToPaint = tileWallUpper;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL + 1, 0);
                                            tileToPaint = tileBricks01;
                                            tilemapFloorWalls.SetTile(tilePos, tileToPaint);

                                            if (i == endL + 1 || i == endL) continue;
                                            tilePos = new Vector3Int(i, startL - 1, 0);
                                            tileToPaint = tileWallUpper;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL - 2, 0);
                                            tileToPaint = tileBricks01;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);
                                        }

                                        for (int i = startPos1.y + sizeInTiles1.y - 1; i <= startL; ++i)
                                        {
                                            Vector3Int tilePos = new Vector3Int(endL, i, 0);
                                            TileBase tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(endL + 1, i, 0);
                                            tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            if (i != startPos1.y + sizeInTiles1.y - 1)
                                            {
                                                tilePos = new Vector3Int(endL - 1, i, 0);
                                                tileToPaint = tileWallLeft;
                                                tilemapWalls.SetTile(tilePos, tileToPaint);

                                                if (i == startL - 1 || i == startL || i == startL + 1) continue;
                                                tilePos = new Vector3Int(endL + 2, i, 0);
                                                tileToPaint = tileWallRight;
                                                tilemapWalls.SetTile(tilePos, tileToPaint);
                                            }
                                        }

                                        Vector3Int tilePos2 = new Vector3Int(endL - 1, startL + 2, 0);
                                        TileBase tileToPaint2 = tileWallUpperLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startL + 1, 0);
                                        tileToPaint2 = tileWallLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startPos1.y + sizeInTiles1.y, 0);
                                        tileToPaint2 = tileCornerUpperRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startPos1.y + sizeInTiles1.y - 1, 0);
                                        tileToPaint2 = tileBricks02;
                                        tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startPos1.y + sizeInTiles1.y, 0);
                                        tileToPaint2 = tileCornerUpperLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startPos1.y + sizeInTiles1.y - 1, 0);
                                        tileToPaint2 = tileBricks03;
                                        tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startL - 1, 0);
                                        tileToPaint2 = tileCorner02LowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startL - 2, 0);
                                        tileToPaint2 = tileCornerLowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos2.x, startL - 1, 0);
                                        tileToPaint2 = tileCorner02LowerRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos2.x, startL - 2, 0);
                                        tileToPaint2 = tileCornerLowerRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos2.x, startL + 1, 0);
                                        tileToPaint2 = tileBricks03;
                                        tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos2.x, startL + 2, 0);
                                        tileToPaint2 = tileCornerUpperLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL, startPos1.y + sizeInTiles1.y, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL + 1, startPos1.y + sizeInTiles1.y, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL, startPos1.y + sizeInTiles1.y - 1, 0);
                                        tilemapFloorWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL + 1, startPos1.y + sizeInTiles1.y - 1, 0);
                                        tilemapFloorWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(startPos2.x, startL, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(startPos2.x, startL + 1, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        nodePairs.Add($"{fromNode},{toNode}");
                                        if (!graphFinal.ContainsKey(fromNode))
                                        {
                                            graphFinal[fromNode] = new HashSet<int>();
                                        }
                                        if (!graphFinal.ContainsKey(toNode))
                                        {
                                            graphFinal[toNode] = new HashSet<int>();
                                        }
                                        graphFinal[fromNode].Add(toNode);
                                        graphFinal[toNode].Add(fromNode);

                                        if (!extraEdgesGraph.ContainsKey(fromNode))
                                        {
                                            extraEdgesGraph[fromNode] = new HashSet<int>();
                                        }
                                        if (!extraEdgesGraph.ContainsKey(toNode))
                                        {
                                            extraEdgesGraph[toNode] = new HashSet<int>();
                                        }
                                        extraEdgesGraph[fromNode].Add(toNode);
                                        extraEdgesGraph[toNode].Add(fromNode);

                                        Debug.Log("TWO");
                                        builtL = true;
                                        if (!initialBuild)
                                        {
                                            Debug.Log("Changed graph.");
                                            generateHallways(extraEdgesGraph, nodePairs, false, 0);
                                            return;
                                        }
                                        break;
                                    }
                                    if (builtL) break;
                                }
                            }
                            else
                            {
                                if (builtL) break;
                                for (int startL = startPos1.y + sizeInTiles1.y - 3; startL > startPos1.y + 5; --startL)
                                {
                                    for (int endL = startPos2.x + 2; endL < startPos2.x + sizeInTiles2.x - 3; ++endL)
                                    {
                                        bool intersectionL = false;

                                        if (startL > startPos2.y + sizeInTiles2.y) //Fix impossible L hallways bug #2
                                        {
                                            intersectionL = true;
                                            continue;
                                        }

                                        for (int i = startPos1.x + sizeInTiles1.x - 1; i <= endL + 2; ++i)
                                        {
                                            if (tilemapFloor.GetTile(new Vector3Int(i, startL, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloor.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }

                                            if (i == startPos1.x + sizeInTiles1.x - 1 || i == startPos1.x + sizeInTiles1.x - 2) continue;

                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL + 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL + 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL - 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL + 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL + 1, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(i, startL - 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(i, startL - 2, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                        }
                                        if (intersectionL) continue;

                                        for (int i = startPos2.y; i >= startL - 1; --i)
                                        {
                                            if (tilemapFloor.GetTile(new Vector3Int(endL, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloor.GetTile(new Vector3Int(endL + 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }

                                            if (i == startPos2.y) continue;

                                            if (tilemapWalls.GetTile(new Vector3Int(endL - 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(endL - 1, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapWalls.GetTile(new Vector3Int(endL + 2, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                            if (tilemapFloorWalls.GetTile(new Vector3Int(endL + 2, i, 0)) != null)
                                            {
                                                intersectionL = true;
                                                break;
                                            }
                                        }
                                        if (intersectionL) continue;

                                        for (int i = startPos1.x + sizeInTiles1.x - 1; i <= endL + 1; ++i)
                                        {
                                            Vector3Int tilePos = new Vector3Int(i, startL, 0);
                                            TileBase tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL - 1, 0);
                                            tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL - 1, 0);
                                            tileToPaint = tileWallUpper;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL - 2, 0);
                                            tileToPaint = tileBricks01;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            if (i == endL + 1 || i == endL) continue;
                                            tilePos = new Vector3Int(i, startL + 2, 0);
                                            tileToPaint = tileWallUpper;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(i, startL + 1, 0);
                                            tileToPaint = tileBricks01;
                                            tilemapFloorWalls.SetTile(tilePos, tileToPaint);
                                        }

                                        for (int i = startPos2.y + 1; i >= startL - 1; --i)
                                        {
                                            Vector3Int tilePos = new Vector3Int(endL, i, 0);
                                            TileBase tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(endL + 1, i, 0);
                                            tileToPaint = getRandomTileFloor();
                                            tilemapFloor.SetTile(tilePos, tileToPaint);

                                            tilePos = new Vector3Int(endL + 2, i, 0);
                                            tileToPaint = tileWallRight;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);

                                            if (i == startL - 1 || i == startL || i == startL + 1) continue;
                                            tilePos = new Vector3Int(endL - 1, i, 0);
                                            tileToPaint = tileWallLeft;
                                            tilemapWalls.SetTile(tilePos, tileToPaint);
                                        }

                                        Vector3Int tilePos2 = new Vector3Int(endL - 1, startL + 2, 0);
                                        TileBase tileToPaint2 = tileCornerUpperLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startL + 1, 0);
                                        tileToPaint2 = tileBricks03;
                                        tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL - 1, 0);
                                        tileToPaint2 = tileCorner02LowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL + 2, 0);
                                        tileToPaint2 = tileCornerUpperRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL + 1, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL + 1, 0);
                                        tileToPaint2 = tileBricks02;
                                        tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(startPos1.x + sizeInTiles1.x - 1, startL - 2, 0);
                                        tileToPaint2 = tileCornerLowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startL - 2, 0);
                                        tileToPaint2 = tileWallLowerRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startPos2.y + 2, 0);
                                        tileToPaint2 = tileCorner02LowerRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL - 1, startPos2.y + 1, 0);
                                        tileToPaint2 = tileCornerLowerRight;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startPos2.y + 2, 0);
                                        tileToPaint2 = tileCorner02LowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL + 2, startPos2.y + 1, 0);
                                        tileToPaint2 = tileCornerLowerLeft;
                                        tilemapWalls.SetTile(tilePos2, tileToPaint2);

                                        tilePos2 = new Vector3Int(endL, startPos2.y + 2, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL, startPos2.y + 1, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL + 1, startPos2.y + 2, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        tilePos2 = new Vector3Int(endL + 1, startPos2.y + 1, 0);
                                        tilemapWalls.SetTile(tilePos2, null);

                                        nodePairs.Add($"{fromNode},{toNode}");
                                        if (!graphFinal.ContainsKey(fromNode))
                                        {
                                            graphFinal[fromNode] = new HashSet<int>();
                                        }
                                        if (!graphFinal.ContainsKey(toNode))
                                        {
                                            graphFinal[toNode] = new HashSet<int>();
                                        }
                                        graphFinal[fromNode].Add(toNode);
                                        graphFinal[toNode].Add(fromNode);

                                        if (!extraEdgesGraph.ContainsKey(fromNode))
                                        {
                                            extraEdgesGraph[fromNode] = new HashSet<int>();
                                        }
                                        if (!extraEdgesGraph.ContainsKey(toNode))
                                        {
                                            extraEdgesGraph[toNode] = new HashSet<int>();
                                        }
                                        extraEdgesGraph[fromNode].Add(toNode);
                                        extraEdgesGraph[toNode].Add(fromNode);

                                        Debug.Log("ONE");
                                        builtL = true;
                                        if (!initialBuild)
                                        {
                                            Debug.Log("Changed graph.");
                                            generateHallways(extraEdgesGraph, nodePairs, false, 0);
                                            return;
                                        }
                                        break;
                                    }
                                    if (builtL) break;
                                }
                                if (builtL) break;
                            }
                            randomNumber = 1 - randomNumber;
                        }
                    }

                    if (builtL) continue;
                    if (builtN) continue;
                }

                if (builtL) continue;
                if (builtN) continue;
                if (startPos1.y < startPos2.y)
                {
                    bool case_orientation = true;
                    if (startPos1.x > startPos2.x)
                    {
                        case_orientation = false;
                    }

                    if ((case_orientation && startPos1.x <= startPos2.x && startPos2.x < startPos1.x + sizeInTiles1.x - 5) || (!case_orientation && startPos1.x > startPos2.x && startPos1.x < startPos2.x + sizeInTiles2.x - 5))
                    {
                        Vector3Int startPosOffset = case_orientation ? startPos2 : startPos1;
                        int startPosOffsetExtra = case_orientation ? 0 : startPos2.y - startPos1.y;

                        bool intersectionN = false;
                        for (int i = -1; i <= startPos2.y - startPos1.y - sizeInTiles1.y + 1; ++i)
                        {
                            int offset = case_orientation ? i * -1 : i + sizeInTiles1.y;

                            if (tilemapFloor.GetTile(startPosOffset + new Vector3Int(2, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloor.GetTile(startPosOffset + new Vector3Int(3, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }

                            if (i == -1 || i == 0 || i == startPos2.y - startPos1.y - sizeInTiles1.y + 1 || i == startPos2.y - startPos1.y - sizeInTiles1.y) continue;
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(1, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(1, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapWalls.GetTile(startPosOffset + new Vector3Int(4, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                            if (tilemapFloorWalls.GetTile(startPosOffset + new Vector3Int(4, offset, 0)) != null)
                            {
                                intersectionN = true;
                                break;
                            }
                        }

                        if (!intersectionN)
                        {
                            for (int i = -1; i <= startPos2.y - startPos1.y - sizeInTiles1.y + 1; ++i)
                            {
                                int offset = case_orientation ? i * -1 : i + sizeInTiles1.y;

                                Vector3Int tilePos = startPosOffset + new Vector3Int(2, offset, 0);
                                TileBase tileToPaint = getRandomTileFloor();
                                tilemapFloor.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(3, offset, 0);
                                tileToPaint = getRandomTileFloor();
                                tilemapFloor.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(1, offset, 0);
                                tileToPaint = tileWallLeft;
                                tilemapWalls.SetTile(tilePos, tileToPaint);

                                tilePos = startPosOffset + new Vector3Int(4, offset, 0);
                                tileToPaint = tileWallRight;
                                tilemapWalls.SetTile(tilePos, tileToPaint);
                            }

                            Vector3Int tilePos2 = startPosOffset + new Vector3Int(1, 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            TileBase tileToPaint2 = tileCornerLowerRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(4, 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileCornerLowerLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(1, 2, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileCorner02LowerRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(4, 2, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileCorner02LowerLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(2, 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(2, 2, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(3, 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(3, 2, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);

                            tilePos2 = startPosOffset + new Vector3Int(1, startPos1.y - startPos2.y + sizeInTiles1.y, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileCornerUpperLeft;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(4, startPos1.y - startPos2.y + sizeInTiles1.y, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileCornerUpperRight;
                            tilemapWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(2, startPos1.y - startPos2.y + sizeInTiles1.y, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(2, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapFloorWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(3, startPos1.y - startPos2.y + sizeInTiles1.y, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(3, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapFloorWalls.SetTile(tilePos2, null);

                            tilePos2 = startPosOffset + new Vector3Int(1, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);
                            tilePos2 = startPosOffset + new Vector3Int(4, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tilemapWalls.SetTile(tilePos2, null);

                            tilePos2 = startPosOffset + new Vector3Int(1, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileBricks03;
                            tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                            tilePos2 = startPosOffset + new Vector3Int(4, startPos1.y - startPos2.y + sizeInTiles1.y - 1, 0) + new Vector3Int(0, startPosOffsetExtra, 0);
                            tileToPaint2 = tileBricks02;
                            tilemapFloorWalls.SetTile(tilePos2, tileToPaint2);

                            nodePairs.Add($"{fromNode},{toNode}");
                            if (!graphFinal.ContainsKey(fromNode))
                            {
                                graphFinal[fromNode] = new HashSet<int>();
                            }
                            if (!graphFinal.ContainsKey(toNode))
                            {
                                graphFinal[toNode] = new HashSet<int>();
                            }
                            graphFinal[fromNode].Add(toNode);
                            graphFinal[toNode].Add(fromNode);

                            if (!extraEdgesGraph.ContainsKey(fromNode))
                            {
                                extraEdgesGraph[fromNode] = new HashSet<int>();
                            }
                            if (!extraEdgesGraph.ContainsKey(toNode))
                            {
                                extraEdgesGraph[toNode] = new HashSet<int>();
                            }
                            extraEdgesGraph[fromNode].Add(toNode);
                            extraEdgesGraph[toNode].Add(fromNode);

                            if (!initialBuild)
                            {
                                Debug.Log("Changed graph.");
                                generateHallways(extraEdgesGraph, nodePairs, false, 0);
                                return;
                            }
                            continue;
                        }
                    }
                }
            }
        }
        generateHallways(extraEdgesGraph, nodePairs, false, buildStep + 1);
        return;
    }

    private void ClearWalls(Vector3Int position)
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {
        new Vector3Int(-1,  2, 0), new Vector3Int(0,  2, 0), new Vector3Int(1,  2, 0),
        new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),
        new Vector3Int(-1,  0, 0),                          new Vector3Int(1,  0, 0),
        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0)
        };

        tilemapWalls.SetTile(position, null);
        foreach (var neighbor in neighbors)
        {
            tilemapWalls.SetTile(position + neighbor, null);
        }
    }

    private void BuildWall(Vector3Int position)
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {
        new Vector3Int(-1,  2, 0), new Vector3Int(0,  2, 0), new Vector3Int(1,  2, 0),
        new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),
        new Vector3Int(-1,  0, 0),                          new Vector3Int(1,  0, 0),
        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
        new Vector3Int(-1, -2, 0), new Vector3Int(0, -2, 0), new Vector3Int(1, -2, 0)
        };

        foreach (var neighbor in neighbors)
        {
            Vector3Int neighborPos = position + neighbor;

            // Conditions
            bool center = tilemapFloor.GetTile(neighborPos) != null;
            bool right = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, 0, 0)) != null;
            bool left = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, 0, 0)) != null;
            bool up = tilemapFloor.GetTile(neighborPos + new Vector3Int(0, 1, 0)) != null;
            bool down = tilemapFloor.GetTile(neighborPos + new Vector3Int(0, -1, 0)) != null;
            bool upleft = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, 1, 0)) != null;
            bool upright = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, 1, 0)) != null;
            bool downleft = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, -1, 0)) != null;
            bool downright = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, -1, 0)) != null;

            PlaceWallTile(neighborPos, center, right, left, up, down, upleft, upright, downleft, downright);
        }
    }

    private void PlaceWallTile(Vector3Int pos, bool center, bool right, bool left, bool up, bool down, bool upleft, bool upright, bool downleft, bool downright)
    {
        if (downright && !center && !right && !left && !up && !down && !upleft && !upright && !downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallCornerUpLeft);
            tilemapWalls.SetTile(pos, tileWallLeft);
        }
        else if (downleft && !center && !right && !left && !up && !down && !upleft && !upright && !downright)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallCornerUpRight);
            tilemapWalls.SetTile(pos, tileWallRight);
        }
        else if (upright && !center && !right && !left && !up && !down && !upleft && !downleft && !downright)
        {
            tilemapWalls.SetTile(pos, tileWallBaseCornerLeft);
        }
        else if (upleft && !center && !right && !left && !up && !down && !upright && !downleft && !downright)
        {
            tilemapWalls.SetTile(pos, tileWallBaseCornerRight);
        }
        else if (right && !center && !left && !up && !down && !upleft && !downleft)
        {
            tilemapWalls.SetTile(pos, tileWallLeft);
        }
        else if (left && !center && !right && !up && !down && !upright && !downright)
        {
            tilemapWalls.SetTile(pos, tileWallRight);
        }
        else if (down && !center && !right && !left && !up && !upleft && !upright)
        {
            tilemapWalls.SetTile(pos, tileWallBase);
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontal);
        }
        else if (up && !center && !right && !left && !down && !downleft && !downright)
        {
            tilemapWalls.SetTile(pos, tileWallBase);
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontal);
        }
    }

    private void BuildFloor(Vector3Int position)
    {
        TileBase tileToPaint = getRandomTileFloor();
        tilemapFloor.SetTile(position, tileToPaint);
        ClearWalls(position);
        BuildWall(position);
    }

    private void BuildSquare(Vector3Int startPos, int width, int height)
    {
        for (int i = 1; i < width; ++i)
        {
            for (int j = 1; j < height; ++j)
            {
                Vector3Int tilePos = startPos + new Vector3Int(i, j, 0);
                BuildFloor(tilePos);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Restart the scene by pressing R
            RestartScene();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Zoom in and out
            camera0.GetComponent<FollowScript>().lerpDuration = .4f;
            if (camera0.GetComponent<FollowScript>().newSize == 5) camera0.GetComponent<FollowScript>().newSize = 15;
            else camera0.GetComponent<FollowScript>().newSize = 5;
            camera0.GetComponent<FollowScript>().spawnedPlayer = true;
        }

        bool allSleeping = true;
        if (!dungeonBuilt)
        {
            foreach (Rigidbody2D rb in rectanglesRigidBodies)
            {
                if (!rb.IsSleeping())
                {
                    allSleeping = false;
                    break;
                }
            }
            //Check when room separating using physics engine is finished
            if (allSleeping)
            {
                Debug.Log("All rigidbodies have gone to sleep");
                dungeonBuilt = true;
                List<GameObject> rectanglesToDelete = new List<GameObject>();

                foreach (GameObject rectangle in rectangles)
                {
                    Rigidbody2D rb = rectangle.GetComponent<Rigidbody2D>();
                    if (rb.GetComponent<SpriteRenderer>().bounds.size.x < minWidthFinal || rb.GetComponent<SpriteRenderer>().bounds.size.y < minHeightFinal)
                    {
                        rectanglesToDelete.Add(rectangle);
                    } 
                    else 
                    {
                        Vector2 roundedPos = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
                        rb.isKinematic = true;
                        rb.position = roundedPos;
                        rb.gameObject.transform.position = roundedPos;
                        sortedRectangles.Add(rb.gameObject);
                    }
                }

                foreach (GameObject rectangleToDelete in rectanglesToDelete)
                {
                    Destroy(rectangleToDelete);
                }

                sortedRectangles.Sort((r1, r2) =>
                {
                    Rect rect1 = r1.GetComponent<SpriteRenderer>().sprite.textureRect;
                    Rect rect2 = r2.GetComponent<SpriteRenderer>().sprite.textureRect;

                    float weightedArea1 = (rect1.width * rect1.height) + aspectRatioWeight * Mathf.Abs(rect1.width - rect1.height);
                    float weightedArea2 = (rect2.width * rect2.height) + aspectRatioWeight * Mathf.Abs(rect2.width - rect2.height);

                    return weightedArea2.CompareTo(weightedArea1);
                });

                int numRectanglesToChange = Mathf.CeilToInt(sortedRectangles.Count * mainRoomsPercentage);

                List<IPoint> points = new List<IPoint>()
                {

                };

                for (int i = 0; i < numRectanglesToChange; i++)
                {
                    SpriteRenderer spriteRenderer = sortedRectangles[i].GetComponent<SpriteRenderer>();
                    spriteRenderer.color = Color.red;

                    points.Insert(i, new Point(sortedRectangles[i].transform.position.x, sortedRectangles[i].transform.position.y));
                }
                //Delaunay triangulation for the rooms
                delaunator = new Delaunator(points.ToArray());
                int[] triangles = delaunator.Triangles;

                numLines = triangles.Length * 3;
                lineRenderers = new LineRenderer[numLines];
                startPoints = new Vector2[numLines];
                endPoints = new Vector2[numLines];

                Dictionary<int, HashSet<int>> graph = new Dictionary<int, HashSet<int>>();

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int v1 = triangles[i];
                    int v2 = triangles[i + 1];
                    int v3 = triangles[i + 2];

                    if (!graph.ContainsKey(v1))
                    {
                        graph[v1] = new HashSet<int>();
                    }
                    if (!graph.ContainsKey(v2))
                    {
                        graph[v2] = new HashSet<int>();
                    }
                    if (!graph.ContainsKey(v3))
                    {
                        graph[v3] = new HashSet<int>();
                    }

                    graph[v1].Add(v2);
                    graph[v1].Add(v3);
                    graph[v2].Add(v1);
                    graph[v2].Add(v3);
                    graph[v3].Add(v1);
                    graph[v3].Add(v2);
                }
                //Create minimal spanning tree
                Dictionary<int, HashSet<int>> minimalSpanningTree = GetMinimalSpanningTree(graph);
                //Add extra edges to the tree to make the dungeon more more circular
                Dictionary<int, HashSet<int>> extraEdgesGraph = AddExtraEdges(graph, minimalSpanningTree, extraEdgesPercentage);

                int lineIndex = 0;
                foreach (var nodePair in extraEdgesGraph)
                {
                    int fromNode = nodePair.Key;
                    foreach (int toNode in nodePair.Value)
                    {
                        Vector2 startPoint = new Vector2(sortedRectangles[fromNode].transform.position.x + sortedRectangles[fromNode].GetComponent<SpriteRenderer>().bounds.size.x / 2, sortedRectangles[fromNode].transform.position.y + sortedRectangles[fromNode].GetComponent<SpriteRenderer>().bounds.size.y / 2);
                        Vector2 endPoint = new Vector2(sortedRectangles[toNode].transform.position.x + sortedRectangles[toNode].GetComponent<SpriteRenderer>().bounds.size.x / 2, sortedRectangles[toNode].transform.position.y + sortedRectangles[toNode].GetComponent<SpriteRenderer>().bounds.size.y / 2);

                        startPoints[lineIndex] = startPoint;
                        endPoints[lineIndex] = endPoint;
                        ++lineIndex;
                    }
                }

                //PAINT TILES
                int roomsToDraw = numRectanglesToChange;
                foreach (GameObject rectangle in sortedRectangles)
                {
                    if (roomsToDraw <= 0) break;
                    --roomsToDraw;

                    Rigidbody2D rb = rectangle.GetComponent<Rigidbody2D>();
                    Vector3Int startPos = tilemapFloor.WorldToCell(rb.position) + new Vector3Int(1, 2, 0);

                    Vector3 tileSize = tilemapFloor.cellSize;
                    Vector3Int sizeInTiles = new Vector3Int(
                        Mathf.RoundToInt(rb.GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                        Mathf.RoundToInt(rb.GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                        1);

                    //Build rooms

                    BuildSquare(startPos, sizeInTiles.x - 2, sizeInTiles.y - 2);

                    /*
                    //Floor
                    for (int i = 1; i < sizeInTiles.x - 1; ++i)
                    {
                        for (int j = 2; j < sizeInTiles.y - 1; ++j)
                        {
                            Vector3Int tilePos = startPos + new Vector3Int(i, j, 0);
                            TileBase tileToPaint = getRandomTileFloor();
                            tilemapFloor.SetTile(tilePos, tileToPaint);
                        }
                    }

                    //FloorWalls
                    for (int i = 1; i < sizeInTiles.x - 1; ++i)
                    {
                        Vector3Int tilePos = startPos + new Vector3Int(i, sizeInTiles.y - 1, 0);
                        tilemapFloorWalls.SetTile(tilePos, tileBricks01);

                        tilePos = startPos + new Vector3Int(i, sizeInTiles.y, 0);
                        tilemapWalls.SetTile(tilePos, tileWallUpper);
                    }
                    Vector3Int tilePos2 = startPos + new Vector3Int(0, sizeInTiles.y - 1, 0);

                    //Walls
                    for (int j = 2; j < sizeInTiles.y; ++j)
                    {
                        Vector3Int tilePos = startPos + new Vector3Int(0, j, 0);
                        tilemapWalls.SetTile(tilePos, tileWallLeft);

                        tilePos = startPos + new Vector3Int(sizeInTiles.x - 1, j, 0);
                        tilemapWalls.SetTile(tilePos, tileWallRight);
                    }
                    for (int i = 1; i < sizeInTiles.x - 1; ++i)
                    {
                        Vector3Int tilePos = startPos + new Vector3Int(i, 1, 0);
                        tilemapWalls.SetTile(tilePos, tileBricks01);

                        tilePos = startPos + new Vector3Int(i, 2, 0);
                        tilemapWalls.SetTile(tilePos, tileWallUpper);
                    }

                    tilePos2 = startPos + new Vector3Int(0, sizeInTiles.y, 0);
                    tilemapWalls.SetTile(tilePos2, tileWallUpperLeft);
                    tilePos2 = startPos + new Vector3Int(sizeInTiles.x - 1, sizeInTiles.y, 0);
                    tilemapWalls.SetTile(tilePos2, tileWallUpperRight);

                    tilePos2 = startPos + new Vector3Int(0, 1, 0);
                    tilemapWalls.SetTile(tilePos2, tileWallLowerLeft);
                    tilePos2 = startPos + new Vector3Int(sizeInTiles.x - 1, 1, 0);
                    tilemapWalls.SetTile(tilePos2, tileWallLowerRight);
                    */
                }

                /*
                //GENERATE HALLWAYS
                HashSet<string> nodePairs = new HashSet<string>();

                generateHallways(extraEdgesGraph, nodePairs, true);
                generateHallways(extraEdgesGraph, nodePairs, false, 0);

                List<HashSet<int>> connectedComponents = new List<HashSet<int>>();
                HashSet<int> visitedNodes = new HashSet<int>();

                foreach (var nodePair2 in graphFinal)
                {
                    int fromNode2 = nodePair2.Key;

                    if (visitedNodes.Contains(fromNode2))
                        continue;

                    HashSet<int> currentComponent = new HashSet<int>();
                    DFS(fromNode2, graphFinal, visitedNodes, currentComponent);
                    connectedComponents.Add(currentComponent);
                }

                connectedComponents.Sort((a, b) => b.Count.CompareTo(a.Count));
                foreach (HashSet<int> component in connectedComponents)
                {
                    string componentString = "Connected Component: ";
                    foreach (int node in component)
                    {
                        componentString += node.ToString() + " ";
                    }
                    Debug.Log(componentString);
                }
                Debug.Log("Finished");

                //SELECT MAIN ROOMS
                if (connectedComponents.Count > 0)
                {
                    //Starting Room
                    List<int> firstConnectedComponent = new List<int>(connectedComponents[0]);
                    List<int> excludedValues = new List<int>() { };

                    int connectedRoomsCount = connectedComponents[0].Count;
                    int[] firstIndexElements = new int[connectedRoomsCount];
                    connectedComponents[0].CopyTo(firstIndexElements);
                    int startingRoom = GetRandomIntExcluding(0, connectedComponents[0].Count, excludedValues);
                    excludedValues.Add(startingRoom);
                    Debug.Log("Starting Room done: " + startingRoom);

                    int treasureRoom = GetRandomIntExcluding(0, connectedComponents[0].Count, excludedValues);
                    excludedValues.Add(treasureRoom);

                    int enemyRoom01 = GetRandomIntExcluding(0, connectedComponents[0].Count, excludedValues);
                    excludedValues.Add(enemyRoom01);

                    int enemyRoom02 = GetRandomIntExcluding(0, connectedComponents[0].Count, excludedValues);
                    excludedValues.Add(enemyRoom02);

                    int enemyRoom03 = GetRandomIntExcluding(0, connectedComponents[0].Count, excludedValues);
                    excludedValues.Add(enemyRoom03);

                    Debug.Log("Treasure Room: " + treasureRoom);
                    Debug.Log("Enemy Room 1: " + enemyRoom01);
                    Debug.Log("Enemy Room 2: " + enemyRoom02);
                    Debug.Log("Enemy Room 3: " + enemyRoom03);

                    //Populating with treasure

                    Vector3Int sizeInTilesTreasure01 = new Vector3Int(
                    Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[treasureRoom]].GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                    Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[treasureRoom]].GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                    1);

                    Vector3 chest01Position = new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.x + sizeInTilesTreasure01.x / 2) + 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.y + sizeInTilesTreasure01.y / 2) + 0.8f, 0);
                    Vector3 chest02Position = new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.x + sizeInTilesTreasure01.x / 2) - 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.y + sizeInTilesTreasure01.y / 2) + 0.8f, 0);

                    Instantiate(ChestPrefab, chest01Position, Quaternion.identity);
                    Instantiate(ChestPrefab, chest02Position, Quaternion.identity);

                    objectPositionsSolid.Add(chest01Position);
                    objectPositionsSolid.Add(chest02Position);
                    objectPositionsSolid.Add(new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.x + sizeInTilesTreasure01.x / 2) + 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.y + sizeInTilesTreasure01.y / 2) + 0.5f, 0));
                    objectPositionsSolid.Add(new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.x + sizeInTilesTreasure01.x / 2) - 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[treasureRoom]].transform.position.y + sizeInTilesTreasure01.y / 2) + 0.5f, 0));


                    Debug.Log("Treasure Room done: " + treasureRoom);


                    //Starting room
                    Vector3 startingPosition = new Vector3(sortedRectangles[firstConnectedComponent[startingRoom]].transform.position.x + 1.5f, sortedRectangles[firstConnectedComponent[startingRoom]].transform.position.y + 2.5f, 0);
                    Player.transform.position = startingPosition;
                    playerGO.transform.position = startingPosition;
                    playerGO.SetActive(true);
                    playerGO.GetComponent<PlayerScript>().targetPosition = new Vector3(startingPosition.x, startingPosition.y, startingPosition.z);
                    DijkstraMap.GetComponent<DijkstraMapGenerator>().GenerateDijkstraMap(new Vector2Int(Mathf.FloorToInt(startingPosition.x), Mathf.FloorToInt(startingPosition.y)));

                    camera0.GetComponent<FollowScript>().spawnedPlayer = true;
                    objectPositions.Add(startingPosition);

                    //Populating with enemies
                    for (int i = 0; i < connectedRoomsCount; ++i)
                    {
                        Debug.Log("i = " + i);

                        if (i != treasureRoom)
                        {
                            var room = sortedRectangles[firstConnectedComponent[i]];
                            Vector3Int sizeInTilesRoom = new Vector3Int(
                            Mathf.RoundToInt(room.GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                            Mathf.RoundToInt(room.GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                            1);

                            Vector3 cornerLowerLeft = new Vector3(room.transform.position.x + 1.5f, room.transform.position.y + 2.5f, 0);
                            Vector3 cornerLowerRight = new Vector3(room.transform.position.x + room.GetComponent<SpriteRenderer>().bounds.size.x - 1.5f, room.transform.position.y + 2.5f, 0);
                            Vector3 cornerUpperLeft = new Vector3(room.transform.position.x + 1.5f, room.transform.position.y + room.GetComponent<SpriteRenderer>().bounds.size.y - 1.5f, 0);
                            Vector3 cornerUpperRight = new Vector3(room.transform.position.x + room.GetComponent<SpriteRenderer>().bounds.size.x - 1.5f, room.transform.position.y + room.GetComponent<SpriteRenderer>().bounds.size.y - 1.5f, 0);

                            int width = (int)room.GetComponent<SpriteRenderer>().bounds.size.x - 2;
                            int height = (int)room.GetComponent<SpriteRenderer>().bounds.size.y - 3;

                            objectPositionsDoor = new List<Vector3>();
                            objectPositionsDoorLeft = new List<Vector3>();
                            objectPositionsDoorRight = new List<Vector3>();
                            objectPositionsDoorLower = new List<Vector3>();
                            objectPositionsDoorUpper = new List<Vector3>();
                            for (Vector3 j = cornerUpperLeft + new Vector3(-1, -1, 0); j.y >= cornerLowerLeft.y; j += new Vector3(0, -1, 0))
                            {
                                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                                if (tilemapFloor.GetTile(jInt) != null)
                                {
                                    //GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                    objectPositionsDoor.Add(j);
                                    objectPositionsDoorLeft.Add(j);
                                }
                            }

                            for (Vector3 j = cornerUpperRight + new Vector3(1, -1, 0); j.y >= cornerLowerRight.y; j += new Vector3(0, -1, 0))
                            {
                                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                                if (tilemapFloor.GetTile(jInt) != null)
                                {
                                    //GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                    objectPositionsDoor.Add(j);
                                    objectPositionsDoorRight.Add(j);
                                }
                            }

                            for (Vector3 j = cornerLowerLeft + new Vector3(0, -1, 0); j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
                            {
                                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                                if (tilemapFloor.GetTile(jInt) != null)
                                {
                                    //GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                    objectPositionsDoor.Add(j);
                                    objectPositionsDoorLower.Add(j);
                                }
                            }

                            for (Vector3 j = cornerUpperLeft + new Vector3(0, 1, 0); j.x <= cornerUpperRight.x; j += new Vector3(1, 0, 0))
                            {
                                Vector3Int jInt = new Vector3Int(Mathf.FloorToInt(j.x), Mathf.FloorToInt(j.y), Mathf.FloorToInt(j.z));
                                if (tilemapFloor.GetTile(jInt) != null)
                                {
                                    //GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                    objectPositionsDoor.Add(j);
                                    objectPositionsDoorUpper.Add(j);
                                    Debug.Log("HEEEEE");
                                    Debug.Log(j);
                                }
                            }

                            //Carpets
                            if (Random.Range(0f, 1f) < .75f) 
                            {
                                //int randomLeft = Mathf.FloorToInt(Random.Range(cornerLowerLeft.x, cornerLowerRight.x - 3));
                                //int randomRight = Mathf.FloorToInt(Random.Range(randomLeft + 3, cornerLowerRight.x));
                                //int randomLower = Mathf.FloorToInt(Random.Range(cornerLowerLeft.y, cornerUpperLeft.y - 3));
                                //int randomUpper = Mathf.FloorToInt(Random.Range(randomLower + 3, cornerUpperLeft.y));

                                //Debug.Log("START");
                                //Debug.Log(randomLeft);
                                //Debug.Log(randomRight);
                                //Debug.Log(randomLower);
                                //Debug.Log(randomUpper);


                                //Vector3 cornerLowerLeftRandom = new Vector3(randomLeft, randomLower, cornerLowerLeft.z);
                                //Vector3 cornerLowerRightRandom = new Vector3(randomRight, randomLower, cornerLowerRight.z);
                                //Vector3 cornerUpperLeftRandom = new Vector3(randomLeft, randomUpper, cornerUpperLeft.z);
                                //Vector3 cornerUpperRightRandom = new Vector3(randomRight, randomUpper, cornerUpperRight.z);

                                if (Random.Range(0f, 1f) < 0.5f)
                                {
                                    PlaceCarpet(cornerLowerLeft, cornerLowerRight, cornerUpperLeft, cornerUpperRight, tileCarpet02Center, tileCarpet02Lower, tileCarpet02Upper, tileCarpet02Left, tileCarpet02Right, tileCarpet02LowerLeft, tileCarpet02LowerRight, tileCarpet02UpperLeft, tileCarpet02UpperRight);
                                    //PlaceCarpet(cornerLowerLeftRandom, cornerLowerRightRandom, cornerUpperLeftRandom, cornerUpperRightRandom, tileCarpet02Center, tileCarpet02Lower, tileCarpet02Upper, tileCarpet02Left, tileCarpet02Right, tileCarpet02LowerLeft, tileCarpet02LowerRight, tileCarpet02UpperLeft, tileCarpet02UpperRight);
                                }
                                else
                                {
                                    PlaceCarpet(cornerLowerLeft, cornerLowerRight, cornerUpperLeft, cornerUpperRight, tileCarpet01Center, tileCarpet01Lower, tileCarpet01Upper, tileCarpet01Left, tileCarpet01Right, tileCarpet01LowerLeft, tileCarpet01LowerRight, tileCarpet01UpperLeft, tileCarpet01UpperRight);
                                    //PlaceCarpet(cornerLowerLeftRandom, cornerLowerRightRandom, cornerUpperLeftRandom, cornerUpperRightRandom, tileCarpet01Center, tileCarpet01Lower, tileCarpet01Upper, tileCarpet01Left, tileCarpet01Right, tileCarpet01LowerLeft, tileCarpet01LowerRight, tileCarpet01UpperLeft, tileCarpet01UpperRight);
                                }
                            }

                            //Spikes
                            
                            if (Random.Range(0f, 1f) < 1f)
                            {
                                PlaceSpikes(cornerLowerLeft, cornerLowerRight, cornerUpperLeft, cornerUpperRight, width, height);
                            }

                            //Boxes
                            if (Random.Range(0f, 1f) < .45f)
                            {
                                if (Random.Range(0f, 1f) < .3f) //Boxes at random positions
                                {
                                    int boxCount = Random.Range(1, 8);
                                    for (int j = 0; j < boxCount; ++j)
                                    {
                                        Vector3 boxPosition = new Vector3(Mathf.Round(Random.Range(Mathf.Round(room.transform.position.x + 2), Mathf.Round(room.transform.position.x + sizeInTilesRoom.x - 4))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(room.transform.position.y + 5), Mathf.Round(room.transform.position.y + sizeInTilesRoom.y - 1))) - 1.5f, 0);

                                        if (!IsPositionOccupied(boxPosition) && !IsPositionOccupiedSolid(boxPosition))
                                        {
                                            GameObject boxN = Instantiate(boxPrefab01, boxPosition, Quaternion.identity);
                                            objectPositions.Add(boxPosition);
                                        }
                                    }
                                }
                                else if (Random.Range(0f, 1f) < 0.05f) //Boxes in corners
                                {
                                    GameObject box1 = Instantiate(boxPrefab01, cornerLowerLeft, Quaternion.identity);
                                    GameObject box2 = Instantiate(boxPrefab01, cornerLowerRight, Quaternion.identity);
                                    GameObject box3 = Instantiate(boxPrefab01, cornerUpperLeft, Quaternion.identity);
                                    GameObject box4 = Instantiate(boxPrefab01, cornerUpperRight, Quaternion.identity);
                                }
                                else
                                {
                                    if (Random.Range(0f, 1f) < 0.3f)
                                    {
                                        for (Vector3 j = cornerLowerLeft; j.x <= cornerLowerRight.x; j += new Vector3(1, 0, 0))
                                        {
                                            if (!IsPositionOccupied(j) && !IsWall(j + new Vector3(0, -1, 0)) && !IsPositionOccupiedSolid(j))
                                            {
                                                GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                                objectPositions.Add(j);
                                            }
                                        }
                                    }
                                    if (Random.Range(0f, 1f) < 0.3f)
                                    {
                                        for (Vector3 j = cornerUpperLeft; j.x <= cornerUpperRight.x; j += new Vector3(1, 0, 0))
                                        {
                                            if (!IsPositionOccupied(j) && !IsWall(j + new Vector3(0, 1, 0)) && !IsPositionOccupiedSolid(j))
                                            {
                                                GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                                objectPositions.Add(j);
                                            }
                                        }
                                    }
                                    if (Random.Range(0f, 1f) < 0.3f)
                                    {
                                        if (!IsPositionOccupied(cornerUpperLeft) && !IsPositionOccupiedSolid(cornerUpperLeft))
                                        {
                                            GameObject boxN = Instantiate(boxPrefab01, cornerUpperLeft, Quaternion.identity);
                                            objectPositions.Add(cornerUpperLeft);
                                        }
                                        for (Vector3 j = cornerUpperLeft + new Vector3(0, -1, 0); j.y >= cornerLowerLeft.y; j += new Vector3(0, -1, 0))
                                        {
                                            if (!IsPositionOccupied(j) && !IsWall(j + new Vector3(-1, 0, 0)) && !IsPositionOccupiedSolid(j))
                                            {
                                                GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                                objectPositions.Add(j);
                                            }
                                            else
                                            {
                                                j += new Vector3(0, -1, 0); //Skip position if previous was occupied or wall
                                            }
                                        }
                                    }
                                    if (Random.Range(0f, 1f) < 0.3f)
                                    {
                                        if (!IsPositionOccupied(cornerUpperRight) && !IsPositionOccupiedSolid(cornerUpperRight))
                                        {
                                            GameObject boxN = Instantiate(boxPrefab01, cornerUpperRight, Quaternion.identity);
                                            objectPositions.Add(cornerUpperRight);
                                        }
                                        for (Vector3 j = cornerUpperRight + new Vector3(0, -1, 0); j.y >= cornerLowerRight.y; j += new Vector3(0, -1, 0))
                                        {
                                            if (!IsPositionOccupied(j) && !IsWall(j + new Vector3(1, 0, 0)) && !IsPositionOccupiedSolid(j))
                                            {
                                                GameObject boxN = Instantiate(boxPrefab01, j, Quaternion.identity);
                                                objectPositions.Add(j);
                                            }
                                            else
                                            {
                                                j += new Vector3(0, -1, 0); //Skip position if previous was occupied or wall
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (i != treasureRoom && i != startingRoom && i != enemyRoom01 && i != enemyRoom02 && i != enemyRoom03)
                        {
                            Vector3Int sizeInTilesRoom = new Vector3Int(
                            Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                            Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                            1);

                            Vector3 enemy01Position = new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x / 2) + 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y / 2) + 0.5f, 0);
                            Vector3 enemy02Position = new Vector3(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x / 2) - 1.5f, Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y / 2) + 0.5f, 0);

                            int randomEnemy01 = UnityEngine.Random.Range(0, enemiesCount);
                            int randomEnemy02 = UnityEngine.Random.Range(0, enemiesCount);

                            GameObject e01 = Instantiate(Enemies[randomEnemy01], enemy01Position, Quaternion.identity);
                            GameObject e02 = Instantiate(Enemies[randomEnemy02], enemy02Position, Quaternion.identity);

                            e01.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                            e01.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                            e01.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;

                            e02.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                            e02.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                            e02.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;

                        } else if (i != treasureRoom)
                        {
                            //Check for each room if it's a special enemy or treasure room
                            if (i == enemyRoom01)
                            {
                                Vector3Int sizeInTilesRoom = new Vector3Int(
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                                1);

                                Vector3 enemy01Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);
                                Vector3 enemy02Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);

                                int randomEnemy01 = UnityEngine.Random.Range(0, enemiesCount);
                                int randomEnemy02 = UnityEngine.Random.Range(0, enemiesCount);

                                GameObject e01 = Instantiate(Enemies[randomEnemy01], enemy01Position, Quaternion.identity);
                                GameObject e02 = Instantiate(Enemies[randomEnemy02], enemy02Position, Quaternion.identity);

                                e01.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e01.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e01.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;

                                e02.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e02.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e02.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;
                            } else if (i == enemyRoom02)
                            {
                                Vector3Int sizeInTilesRoom = new Vector3Int(
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                                1);

                                Vector3 enemy01Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);
                                Vector3 enemy02Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);

                                int randomEnemy01 = UnityEngine.Random.Range(0, enemiesCount);
                                int randomEnemy02 = UnityEngine.Random.Range(0, enemiesCount);

                                GameObject e01 = Instantiate(Enemies[randomEnemy01], enemy01Position, Quaternion.identity);
                                GameObject e02 = Instantiate(Enemies[randomEnemy02], enemy02Position, Quaternion.identity);

                                e01.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e01.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e01.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;

                                e02.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e02.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e02.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;
                            } else if (i == enemyRoom03)
                            {
                                Vector3Int sizeInTilesRoom = new Vector3Int(
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.x / tileSize.x),
                                Mathf.RoundToInt(sortedRectangles[firstConnectedComponent[i]].GetComponent<SpriteRenderer>().bounds.size.y / tileSize.y),
                                1);

                                Vector3 enemy01Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);
                                Vector3 enemy02Position = new Vector3(Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + 1), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.x + sizeInTilesRoom.x - 3))) + 0.5f, Mathf.Round(Random.Range(Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + 3), Mathf.Round(sortedRectangles[firstConnectedComponent[i]].transform.position.y + sizeInTilesRoom.y - 1))) - 0.5f, 0);

                                int randomEnemy01 = UnityEngine.Random.Range(0, enemiesCount);
                                int randomEnemy02 = UnityEngine.Random.Range(0, enemiesCount);

                                GameObject e01 = Instantiate(Enemies[randomEnemy01], enemy01Position, Quaternion.identity);
                                GameObject e02 = Instantiate(Enemies[randomEnemy02], enemy02Position, Quaternion.identity);

                                e01.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e01.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e01.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;

                                e02.GetComponent<Enemy01Script>().DungeonManager = DungeonManager;
                                e02.GetComponent<Enemy01Script>().DijkstraMap = DijkstraMap;
                                e02.GetComponent<Enemy01Script>().tilemapFloor = tilemapFloor;
                            }
                        }
                    }
                }
                */

                
                //DEACTIVATE ROOM RECTANGLES
                foreach (GameObject rectangle in rectangles)
                {
                    rectangle.SetActive(false);
                }
            }
        }
    }

}
