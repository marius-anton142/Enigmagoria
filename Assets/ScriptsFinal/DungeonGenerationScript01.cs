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
    [SerializeField] private int roomsPlaced;
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
        rooms.Add(room);
        room.SetPosition(startPosition);
        ++roomsPlaced;
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
        return new Room(floorTileCoordinates);
    }

    private void TryBuildAdditionalHallways(Room currentRoom, Room neighborRoom)
    {
        // Get a random variable between 1 and 3 to determine how many hallways to attempt
        int hallwayAttempts = Random.Range(1, 4);

        if (hallwayAttempts == 1)
        {
            return; // Don't attempt any additional hallways
        }

        // Find the closest rooms to the current room
        List<Room> closestRooms = FindClosestRooms(currentRoom, 6, neighborRoom);

        // Iterate over the closest rooms and attempt to build hallways
        int hallwaysBuilt = 0;

        foreach (Room targetRoom in closestRooms)
        {
            if (BuildHallwayBetweenRooms(currentRoom, targetRoom))
            {
                hallwaysBuilt++;

                if ((hallwayAttempts == 2 && hallwaysBuilt >= 1) ||
                    (hallwayAttempts == 3 && hallwaysBuilt >= 2))
                {
                    break; // Stop once the required number of hallways is built
                }
            }
        }
    }

    // Helper function to calculate the Euclidean distance between two rooms
    private float CalculateDistance(Room room1, Room room2)
    {
        Vector3Int pos1 = room1.GetPosition();
        Vector3Int pos2 = room2.GetPosition();
        return Vector3.Distance(new Vector3(pos1.x, pos1.y, pos1.z), new Vector3(pos2.x, pos2.y, pos2.z));
    }

    // Function to find the closest rooms to the given room
    private List<Room> FindClosestRooms(Room currentRoom, int count, Room neighborRoom)
    {
        return rooms
            .Where(room => room != currentRoom && room != neighborRoom) // Exclude the current room itself
            .OrderBy(room => CalculateDistance(currentRoom, room))
            .Take(count)
            .ToList();
    }

    // Function to attempt building a hallway between two rooms
    private bool BuildHallwayBetweenRooms(Room room0, Room room1)
    {
        // Get connection points for both rooms
        List<Vector3Int>[] room0ConnectionPoints = GetConnectionPoints(room0, 2);
        List<Vector3Int>[] room1ConnectionPoints = GetConnectionPoints(room1, 2);

        // Shuffle sides and lengths to introduce randomness
        List<int> sides = new List<int> { 0, 1, 2, 3 };
        ShuffleList(sides);

        List<int> lengths = new List<int> { 3, 4, 5, 6 };
        ShuffleList(lengths);

        // Try to build a hallway
        foreach (int side in sides)
        {
            List<Vector3Int> room0Sides = room0ConnectionPoints[side];
            List<Vector3Int> room1Sides = room1ConnectionPoints[(side + 2) % 4];

            ShuffleList(room0Sides);
            ShuffleList(room1Sides);

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

                            if (room1.GetPosition() == posHallwayEnd - room1Point && CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), 2, length))
                            {
                                BuildSquare(posHallway + new Vector3Int(0, 1, 0), 2, length);
                                return true;
                            }
                        }
                        else if (side == 1) // Left
                        {
                            posHallway = offset + room0Point + new Vector3Int(-length - 1, 0, 0);
                            posHallwayEnd = posHallway;

                            if (room1.GetPosition() == posHallwayEnd - room1Point && CheckBuildHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, 2))
                            {
                                BuildSquare(posHallway + new Vector3Int(1, 0, 0), length, 2);
                                return true;
                            }
                        }
                        else if (side == 2) // Up
                        {
                            posHallway = offset + room0Point + new Vector3Int(0, 1, 0);
                            posHallwayEnd = posHallway + new Vector3Int(0, length, 0);

                            if (room1.GetPosition() == posHallwayEnd - room1Point && CheckBuildHallwayVertical(posHallway, 2, length))
                            {
                                BuildSquare(posHallway, 2, length);
                                return true;
                            }
                        }
                        else if (side == 3) // Right
                        {
                            posHallway = offset + room0Point + new Vector3Int(1, 0, 0);
                            posHallwayEnd = posHallway + new Vector3Int(length, 0, 0);

                            if (room1.GetPosition() == posHallwayEnd - room1Point && CheckBuildHallwayHorizontal(posHallway, length, 2))
                            {
                                BuildSquare(posHallway, length, 2);
                                return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    private void Start()
    {
        Vector3Int pos01 = new Vector3Int(0, 0, 0);

        // Create and place the initial room
        Room initialRoom = CreateSquareRoom(6, 6);
        InstantiateRoom(initialRoom, pos01);

        for (int n = 0; n < numRooms; n++)
        {
            int randomWidth = Random.Range(4, 11);
            int randomHeight = Random.Range(4, 11);
            Room room1 = CreateSquareRoom(randomWidth, randomHeight);
            Room room0 = null;
            bool roomPlaced = false;

            List<int> roomIndices = Enumerable.Range(0, rooms.Count).ToList();
            ShuffleList(roomIndices);

            foreach (int roomIndex in roomIndices)
            {
                room0 = rooms[roomIndex];

                // Down, left, up, right
                List<Vector3Int>[] room0ConnectionPoints = GetConnectionPoints(room0, 2);
                List<Vector3Int>[] room1ConnectionPoints = GetConnectionPoints(room1, 2);

                List<int> sides = new List<int> { 0, 1, 2, 3 };
                ShuffleList(sides);

                foreach (int side in sides)
                {
                    List<Vector3Int> room0Sides = room0ConnectionPoints[side];
                    List<Vector3Int> room1Sides = room1ConnectionPoints[(side + 2) % 4];

                    List<int> lengths = new List<int> { 3, 4, 5, 6 };
                    ShuffleList(lengths);
                    ShuffleList(room0Sides);
                    ShuffleList(room1Sides);

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
                if (roomPlaced) break;
            }
            Debug.Log(roomsPlaced);
            
            if (roomPlaced)
            {
                TryBuildAdditionalHallways(room1, room0);
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
