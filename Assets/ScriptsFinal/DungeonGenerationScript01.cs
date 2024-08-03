using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine.SceneManagement;
using System.Linq;

public class DungeonGenerationScript01 : MonoBehaviour
{
    [SerializeField] private int numRooms;
    [SerializeField] private float[] chanceTileFloors = new float[5];

    [Header("Tile Maps")]
    [SerializeField] private Tilemap tilemapFloor;
    [SerializeField] private Tilemap tilemapWalls, tilemapWallColliders;

    [Header("Fall Tiles")]
    [SerializeField] private Tile tileFloor01;
    [SerializeField] private Tile tileFloor02, tileFloor03, tileFloor04, tileFloor05;

    [Header("Wall Tiles")]
    [SerializeField] private Tile tileWallHorizontal;
    [SerializeField] private Tile tileWallHorizontalUpLeft, tileWallHorizontalUpRight, tileWallHorizontalDownLeft, tileWallHorizontalDownRight, tileWallLeft, tileWallRight, tileWallCornerUpLeft, tileWallCornerUpRight, tileWallBase, tileWallBaseCornerLeft, tileWallBaseCornerRight, tileWallBaseDownLeft, tileWallBaseDownRight, tileWallBaseUpLeft, tileWallBaseUpRight;

    private List<Room> rooms = new List<Room>();

    TileBase getRandomTileFloor()
    {
        TileBase tileToPaint;

        if (Random.value > chanceTileFloors[4])
        {
            tileToPaint = tileFloor05;
        }
        else if (Random.value > chanceTileFloors[3])
        {
            tileToPaint = tileFloor04;
        }
        else if (Random.value > chanceTileFloors[2])
        {
            tileToPaint = tileFloor03;
        }
        else if (Random.value > chanceTileFloors[1])
        {
            tileToPaint = tileFloor02;
        }
        else
        {
            tileToPaint = tileFloor01;
        }
        return tileToPaint;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("SceneMap01");
    }

    private void ClearWalls(Vector3Int position)
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {
        new Vector3Int(-1,  2, 0), new Vector3Int(0,  2, 0), new Vector3Int(1,  2, 0),
        new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),
        new Vector3Int(-1,  0, 0),                          new Vector3Int(1,  0, 0),
        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
        };

        tilemapWalls.SetTile(position, null);
        foreach (var neighbor in neighbors)
        {
            tilemapWalls.SetTile(position + neighbor, null);
        }
    }

    private bool CheckBuildWall(Vector3Int position, string type = "default")
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {
        new Vector3Int(-1,  2, 0), new Vector3Int(0,  2, 0), new Vector3Int(1,  2, 0),
        new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),
        new Vector3Int(-1,  0, 0), new Vector3Int(0,  0, 0), new Vector3Int(1,  0, 0),
        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
        };

        if (type == "horizontal")
        {
            neighbors = new Vector3Int[]
            {
            new Vector3Int(0,  2, 0),
            new Vector3Int(0,  1, 0),
            new Vector3Int(0,  0, 0),
            new Vector3Int(0, -1, 0),
            };
        }
        else if (type == "vertical")
        {
            neighbors = new Vector3Int[]
            {
            new Vector3Int(-1,  0, 0), new Vector3Int(0,  0, 0), new Vector3Int(1,  0, 0),
            };
        }

        foreach (var neighbor in neighbors)
        {
            Vector3Int neighborPos = position + neighbor;

            bool overlapsWalls = tilemapWalls.GetTile(neighborPos) != null;
            
            //if (type == "vertical") tilemapFloor.SetTile(neighborPos, tileFloor05);
            if (overlapsWalls) return false;
        }
        return true;
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

            bool center = tilemapFloor.GetTile(neighborPos) != null;
            bool right = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, 0, 0)) != null;
            bool left = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, 0, 0)) != null;
            bool up = tilemapFloor.GetTile(neighborPos + new Vector3Int(0, 1, 0)) != null;
            bool down = tilemapFloor.GetTile(neighborPos + new Vector3Int(0, -1, 0)) != null;
            bool upleft = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, 1, 0)) != null;
            bool upright = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, 1, 0)) != null;
            bool downleft = tilemapFloor.GetTile(neighborPos + new Vector3Int(-1, -1, 0)) != null;
            bool downright = tilemapFloor.GetTile(neighborPos + new Vector3Int(1, -1, 0)) != null;
            bool down_2 = tilemapFloor.GetTile(neighborPos + new Vector3Int(0, -2, 0)) != null;

            PlaceWallTile(neighborPos, center, right, left, up, down, upleft, upright, downleft, downright, down_2);
        }
    }

    private void PlaceWallTile(Vector3Int pos, bool center, bool right, bool left, bool up, bool down, bool upleft, bool upright, bool downleft, bool downright, bool down_2)
    {
        if (downright && !center && !right && !left && !up && !down && !upleft && !upright && !downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallCornerUpLeft);
            tilemapWalls.SetTile(pos, tileWallLeft);
        }
        else if (!downright && !center && !right && !left && !up && !down && !upleft && !upright && downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallCornerUpRight);
            tilemapWalls.SetTile(pos, tileWallRight);
        }
        else if (!downright && !center && !right && !left && !up && !down && !upleft && upright && !downleft)
        {
            tilemapWalls.SetTile(pos, tileWallBaseCornerLeft);
        }
        else if (!downright && !center && !right && !left && !up && !down && upleft && !upright && !downleft)
        {
            tilemapWalls.SetTile(pos, tileWallBaseCornerRight);
        }
        else if (!center && right && !left && !up && down && !upleft && downright)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalDownRight);
            tilemapWalls.SetTile(pos, tileWallBaseDownRight);
        }
        else if (!center && !right && left && !up && down && !upright && downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalDownLeft);
            tilemapWalls.SetTile(pos, tileWallBaseDownLeft);
        }
        else if (!center && right && !left && up && !down && !downleft && upright)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalUpRight);
            tilemapWalls.SetTile(pos, tileWallBaseUpRight);
        }
        else if (!center && !right && left && up && !down && !downright && upleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalUpLeft);
            tilemapWalls.SetTile(pos, tileWallBaseUpLeft);
        }
        else if (!center && right && !left && !up && !down && !upleft && !downleft && !down_2)
        {
            tilemapWalls.SetTile(pos, tileWallLeft);
        }
        else if (!downright && !center && !right && left && !up && !down && !upright && !down_2)
        {
            tilemapWalls.SetTile(pos, tileWallRight);
        }
        else if (!center && !right && !left && !up && down && !upleft && !upright)
        {
            tilemapWalls.SetTile(pos, tileWallBase);
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontal);
        }
        else if (!downright && !center && !right && !left && up && !down && !downleft)
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

    private bool CheckBuildFloor(Vector3Int position, string type = "default")
    {
        return CheckBuildWall(position, type);
    }

    private void BuildSquare(Vector3Int startPos, int width, int height)
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                Vector3Int tilePos = startPos + new Vector3Int(i, j, 0);
                BuildFloor(tilePos);
            }
        }
    }

    private bool CheckBuildSquare(Vector3Int startPos, int width, int height, string type = "default")
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                Vector3Int tilePos = startPos + new Vector3Int(i, j, 0);
                if (!CheckBuildFloor(tilePos, type))
                {
                    return false;
                };
            }
        }
        return true;
    }

    private bool CheckBuildHallwayVertical(Vector3Int startPos, int width, int height)
    {
        return CheckBuildSquare(startPos + new Vector3Int(0, 2, 0), width, height - 3, "vertical");
    }

    private bool CheckBuildHallwayHorizontal(Vector3Int startPos, int width, int height)
    {
        return CheckBuildSquare(startPos + new Vector3Int(1, 0, 0), width - 2, height, "horizontal");
    }

    public List<Vector3Int>[] GetConnectionPoints(Room room, int width)
    {
        List<Vector3Int> upConnections = new List<Vector3Int>();
        List<Vector3Int> downConnections = new List<Vector3Int>();
        List<Vector3Int> leftConnections = new List<Vector3Int>();
        List<Vector3Int> rightConnections = new List<Vector3Int>();

        // Get all floor tiles in the room
        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            // Check for left connections
            if (!floorTiles.Contains(tile + Vector3Int.left) && // Must be an edge tile
                !floorTiles.Contains(tile + Vector3Int.left + Vector3Int.down) && // Below must also be an edge
                floorTiles.Contains(tile + Vector3Int.down)) // Has a neighbor below
            {
                bool validConnection = true;
                for (int i = 0; i < width; i++)
                {
                    Vector3Int aboveTile = tile + new Vector3Int(0, i + 1, 0);
                    if (!floorTiles.Contains(aboveTile) || floorTiles.Contains(aboveTile + Vector3Int.left))
                    {
                        validConnection = false;
                        break;
                    }
                }
                if (validConnection)
                {
                    leftConnections.Add(tile);
                }
            }

            // Check for right connections
            if (!floorTiles.Contains(tile + Vector3Int.right) && // Must be an edge tile
                !floorTiles.Contains(tile + Vector3Int.right + Vector3Int.down) && // Below must also be an edge
                floorTiles.Contains(tile + Vector3Int.down)) // Has a neighbor below
            {
                bool validConnection = true;
                for (int i = 0; i < width; i++)
                {
                    Vector3Int aboveTile = tile + new Vector3Int(0, i + 1, 0);
                    if (!floorTiles.Contains(aboveTile) || floorTiles.Contains(aboveTile + Vector3Int.right))
                    {
                        validConnection = false;
                        break;
                    }
                }
                if (validConnection)
                {
                    rightConnections.Add(tile);
                }
            }

            // Check for up connections
            if (!floorTiles.Contains(tile + Vector3Int.up) && // Must be an edge tile
                !floorTiles.Contains(tile + Vector3Int.up + Vector3Int.left) && // To the left must also be an edge
                floorTiles.Contains(tile + Vector3Int.left)) // Has a neighbor to the left
            {
                bool validConnection = true;
                for (int i = 0; i < width; i++)
                {
                    Vector3Int rightTile = tile + new Vector3Int(i + 1, 0, 0);
                    if (!floorTiles.Contains(rightTile) || floorTiles.Contains(rightTile + Vector3Int.up))
                    {
                        validConnection = false;
                        break;
                    }
                }
                if (validConnection)
                {
                    upConnections.Add(tile);
                }
            }

            // Check for down connections
            if (!floorTiles.Contains(tile + Vector3Int.down) && // Must be an edge tile
                !floorTiles.Contains(tile + Vector3Int.down + Vector3Int.left) && // To the left must also be an edge
                floorTiles.Contains(tile + Vector3Int.left)) // Has a neighbor to the left
            {
                bool validConnection = true;
                for (int i = 0; i < width; i++)
                {
                    Vector3Int rightTile = tile + new Vector3Int(i + 1, 0, 0);
                    if (!floorTiles.Contains(rightTile) || floorTiles.Contains(rightTile + Vector3Int.down))
                    {
                        validConnection = false;
                        break;
                    }
                }
                if (validConnection)
                {
                    downConnections.Add(tile);
                }
            }
        }

        // Return all four lists of connection points
        return new List<Vector3Int>[] { downConnections, leftConnections, upConnections, rightConnections };
    }

    public class Room
    {
        public List<Vector3Int> FloorTileCoordinates { get; private set; }
        private Vector3Int position;

        public Room(List<Vector3Int> floorTileCoordinates)
        {
            FloorTileCoordinates = floorTileCoordinates;
        }

        public Vector3Int GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector3Int newPosition)
        {
            position = newPosition;
        }
    }

    public void InstantiateRoom(Room room, Vector3Int startPosition)
    {
        room.SetPosition(startPosition);

        foreach (Vector3Int tileCoord in room.FloorTileCoordinates)
        {
            Vector3Int tilePosition = startPosition + tileCoord;
            BuildFloor(tilePosition);
        }
    }

    public bool CheckInstantiateRoom(Room room, Vector3Int startPosition)
    {
        Vector3Int[] neighbors = new Vector3Int[]
        {
            new Vector3Int(-1,  2, 0), new Vector3Int(0,  2, 0), new Vector3Int(1,  2, 0),
            new Vector3Int(-1,  1, 0), new Vector3Int(0,  1, 0), new Vector3Int(1,  1, 0),
            new Vector3Int(-1,  0, 0),                          new Vector3Int(1,  0, 0),
            new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
        };

        foreach (Vector3Int tileCoord in room.FloorTileCoordinates)
        {
            Vector3Int tilePosition = startPosition + tileCoord;
            foreach (var neighbor in neighbors)
            {
                Vector3Int neighborPos = tilePosition + neighbor;
                bool overlapsWalls = tilemapWalls.GetTile(neighborPos) != null;
                if (overlapsWalls) return false;
            }
        }

        return true;
    }

    private Room CreateSquareRoom(int width, int height)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }
        }

        Room newRoom = new Room(floorTileCoordinates);
        rooms.Add(newRoom);
        return new Room(floorTileCoordinates);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    /*
    void Start()
    {
        Vector3Int pos01 = new Vector3Int(0, 0, 0);

        CreateSquareRoom(4, 6);
        CreateSquareRoom(4, 4);
        InstantiateRoom(rooms[0], pos01);

        Vector3Int pos02 = new Vector3Int(-3, -8, 0);
        if (CheckInstantiateRoom(rooms[1], pos02))
        {
            InstantiateRoom(rooms[1], pos02);
        }
    }
    */

    private void Start()
    {
        Vector3Int pos01 = new Vector3Int(0, 0, 0);

        // Create and place the initial room
        Room initialRoom = CreateSquareRoom(6, 6);
        InstantiateRoom(initialRoom, pos01);
        rooms.Add(initialRoom);

        for (int n = 1; n < numRooms; n++)
        {
            int randomWidth = 10;
            int randomHeight = 9;
            Room room1 = CreateSquareRoom(randomWidth, randomHeight);
            bool roomPlaced = false;

            // Create a list of all existing room indices and shuffle them
            List<int> roomIndices = Enumerable.Range(0, rooms.Count).ToList();
            ShuffleList(roomIndices);

            foreach (int roomIndex in roomIndices)
            {
                Room room0 = rooms[roomIndex];

                // Down, left, up, right
                List<Vector3Int>[] room0ConnectionPoints = GetConnectionPoints(room0, 2);
                List<Vector3Int>[] room1ConnectionPoints = GetConnectionPoints(room1, 2);

                // Create a list of sides [0, 1, 2, 3] and shuffle them
                List<int> sides = new List<int> { 0, 1, 2, 3 };
                ShuffleList(sides);

                foreach (int side in sides)
                {
                    List<Vector3Int> room0Sides = room0ConnectionPoints[side];
                    List<Vector3Int> room1Sides = room1ConnectionPoints[(side + 2) % 4];

                    // Create a list of lengths [3, 4, ..., 10] and shuffle them
                    List<int> lengths = Enumerable.Range(3, 1).ToList();
                    ShuffleList(lengths);

                    foreach (int length in lengths)
                    {
                        foreach (var room0Point in room0Sides)
                        {
                            foreach (var room1Point in room1Sides)
                            {
                                Vector3Int posHallway = Vector3Int.zero;
                                Vector3Int posHallwayEnd = Vector3Int.zero;
                                Vector3Int offset = room0.GetPosition();

                                if (side == 0) // Down
                                {
                                    posHallway = offset + room0Point + new Vector3Int(0, -length - 1, 0);
                                    posHallwayEnd = posHallway;

                                    if (CheckInstantiateRoom(room1, posHallwayEnd - room1Point) &&
                                        CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), 2, length))
                                    {
                                        InstantiateRoom(room1, posHallwayEnd - room1Point);
                                        roomPlaced = true;
                                        BuildSquare(posHallway + new Vector3Int(0, 1, 0), 2, length);
                                        break;
                                    }
                                }
                                else if (side == 1) // Left
                                {
                                    posHallway = offset + room0Point + new Vector3Int(-length - 1, 0, 0);
                                    posHallwayEnd = posHallway;

                                    if (CheckInstantiateRoom(room1, posHallwayEnd - room1Point) &&
                                        CheckBuildHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, 2))
                                    {
                                        InstantiateRoom(room1, posHallwayEnd - room1Point);
                                        roomPlaced = true;
                                        BuildSquare(posHallway + new Vector3Int(1, 0, 0), length, 2);
                                        break;
                                    }
                                }
                                else if (side == 2) // Up
                                {
                                    posHallway = offset + room0Point + new Vector3Int(0, 1, 0);
                                    posHallwayEnd = posHallway + new Vector3Int(0, length, 0);

                                    if (CheckInstantiateRoom(room1, posHallwayEnd - room1Point) &&
                                        CheckBuildHallwayVertical(posHallway, 2, length))
                                    {
                                        InstantiateRoom(room1, posHallwayEnd - room1Point);
                                        roomPlaced = true;
                                        BuildSquare(posHallway, 2, length);
                                        break;
                                    }
                                }
                                else if (side == 3) // Right
                                {
                                    posHallway = offset + room0Point + new Vector3Int(1, 0, 0);
                                    posHallwayEnd = posHallway + new Vector3Int(length, 0, 0);

                                    Debug.Log(offset);
                                    Debug.Log(room0Point);
                                    Debug.Log(posHallway);
                                    Debug.Log(posHallwayEnd);

                                    if (CheckInstantiateRoom(room1, posHallwayEnd - room1Point) &&
                                        CheckBuildHallwayHorizontal(posHallway, length, 2))
                                    {
                                        InstantiateRoom(room1, posHallwayEnd - room1Point);
                                        roomPlaced = true;
                                        BuildSquare(posHallway, length, 2);
                                        break;
                                    }
                                }
                            }
                            if (roomPlaced) break;
                        }
                        if (roomPlaced) break;
                    }
                    if (roomPlaced) break;
                }
                if (roomPlaced)
                {
                    rooms.Add(room1);  // Add newly placed room to the list
                    break;
                }
            }
        }
    }

    private void ShuffleList<T>(List<T> list, int seed = 0)
    {
        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = rng.Next(0, i + 1); // Controlled randomness
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

}
