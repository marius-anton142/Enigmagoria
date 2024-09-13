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
    [SerializeField] private int maxLHallwayLength;
    [SerializeField] private int roomsPlaced;

    [Header("Special Rooms")]
    [SerializeField] private float chanceRoomSpecial;
    [SerializeField] private float chanceRoomPlus;
    [SerializeField] private float chanceRoomChess;

    [Header("Parameters")]
    [SerializeField] private float chanceAdditionalHallways;
    [SerializeField] private int maxAdditionalHallways;
    [SerializeField] private float chanceAnyTileFloors;
    [SerializeField] private float chanceAnyTileWallBaseBroken;
    [SerializeField] private float chanceTileFloorCornerBroken;
    [SerializeField] private float chanceRoomFloorFour; //chane of a room with four slab tiles
    [SerializeField] private float chanceTileFloorFour; //chance of a particular tile to be a four slab tile
    [SerializeField] private float chanceRoomFloorFourOne; //chance of a particular four slab tile to be a semi four slab tile
    [SerializeField] private float chanceRoomFloorFourFull; //chance of a room full of four slab tiles
    [SerializeField] private float chanceRoomFloorFourFullOne; //chance of a particular four slab tile to be a semi four slab tile in a full room
    [SerializeField] private float chanceRoomFloorFourFullNoBroken; //chance of a room full of four slab tiles to have no one tiles
    [SerializeField] private float chanceRoomFloorFourFullBroken; //chance of a room full of four slab tiles to have only one tiles
    [SerializeField] private float chancePlantAny;
    [SerializeField] private float chanceTable;
    [SerializeField] private float chanceTableSmall;
    [SerializeField] private float chanceEnemy01;
    [SerializeField] private float[] chanceTileFloors = new float[5];
    [SerializeField] private float[] chanceTileWallBaseBroken = new float[3];

    [Header("Objects")]
    [SerializeField] private GameObject PlantPrefab01;
    [SerializeField] private GameObject Table2x2Prefab01;
    [SerializeField] private GameObject Table1x2Prefab01;

    [Header("Enemies")]
    [SerializeField] private GameObject EnemyPrefab01;
    [SerializeField] private GameObject EnemyPrefab02;

    [Header("Tile Maps")]
    [SerializeField] private Tilemap tilemapFloor;
    [SerializeField] private Tilemap tilemapWalls, tilemapWallsFix, tilemapWallColliders;

    [Header("Floor Tiles")]
    [SerializeField] private Tile tileFloor01;
    [SerializeField] private Tile tileFloorFour01;
    [SerializeField] private List<Tile> tileFloorOptions;
    [SerializeField] private Tile tileFloor02, tileFloor03, tileFloor04, tileFloor05;
    [SerializeField] private Tile tileFloorChessWhite, tileFloorChessBlack;

    [Header("Wall Tiles")]
    [SerializeField] private Tile tileWallHorizontal;
    [SerializeField] private Tile tileWallHorizontalUpLeft, tileWallHorizontalUpRight, tileWallHorizontalDownLeft, tileWallHorizontalDownRight, tileWallLeft, tileWallRight, tileWallCornerUpLeft, tileWallCornerUpRight, tileWallBase, tileWallBaseCornerLeft, tileWallBaseCornerRight, tileWallBaseDownLeft, tileWallBaseDownRight, tileWallBaseUpLeft, tileWallBaseUpRight, tileWallBaseBroken01, tileWallBaseBroken02;
    [SerializeField] private Tile tileFloorBrokenUpLeft, tileFloorBrokenUpRight, tileFloorBrokenDownLeft, tileFloorBrokenDownRight;

    [SerializeField] private Tile tileWallHorizontalFix, tileWallHorizontalUpLeftFix, tileWallHorizontalUpRightFix, tileWallHorizontalDownLeftFix, tileWallHorizontalDownRightFix, tileWallCornerUpLeftFix, tileWallCornerUpRightFix;

    private List<Room> rooms = new List<Room>();

    TileBase getRandomTileFloor()
    {
        TileBase tileToPaint;

        if (Random.value > chanceAnyTileFloors)
        {
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
            else
            {
                tileToPaint = tileFloor02;
            }
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
        tilemapWallsFix.SetTile(position, null);
        foreach (var neighbor in neighbors)
        {
            tilemapWalls.SetTile(position + neighbor, null);
            tilemapWallsFix.SetTile(position + neighbor, null);
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
            tilemapWallsFix.SetTile(pos, tileWallCornerUpLeftFix);
            tilemapWalls.SetTile(pos, tileWallLeft);
        }
        else if (!downright && !center && !right && !left && !up && !down && !upleft && !upright && downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallCornerUpRight);
            tilemapWallsFix.SetTile(pos, tileWallCornerUpRightFix);
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
            tilemapWallsFix.SetTile(pos, tileWallHorizontalDownRightFix);
            tilemapWalls.SetTile(pos, tileWallBaseDownRight);
        }
        else if (!center && !right && left && !up && down && !upright && downleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalDownLeft);
            tilemapWallsFix.SetTile(pos, tileWallHorizontalDownLeftFix);
            tilemapWalls.SetTile(pos, tileWallBaseDownLeft);
        }
        else if (!center && right && !left && up && !down && !downleft && upright)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalUpRight);
            tilemapWallsFix.SetTile(pos, tileWallHorizontalUpRightFix);
            tilemapWalls.SetTile(pos, tileWallBaseUpRight);
        }
        else if (!center && !right && left && up && !down && !downright && upleft)
        {
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontalUpLeft);
            tilemapWallsFix.SetTile(pos, tileWallHorizontalUpLeftFix);
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
            SetTileWalls(pos, "base");
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontal);
            tilemapWallsFix.SetTile(pos, tileWallHorizontalFix);
        }
        else if (!downright && !center && !right && !left && up && !down && !downleft)
        {
            SetTileWalls(pos, "base");
            tilemapWalls.SetTile(pos + new Vector3Int(0, 1, 0), tileWallHorizontal);
            tilemapWallsFix.SetTile(pos, tileWallHorizontalFix);
        }
    }

    private void SetTileWalls(Vector3Int pos, string type)
    {
        if (type == "base")
        {
            TileBase tileToPaint;

            if (Random.value > chanceAnyTileWallBaseBroken)
            {
                if (Random.value > chanceTileWallBaseBroken[2])
                {
                    tileToPaint = tileWallBaseBroken02;
                }
                else
                {
                    tileToPaint = tileWallBaseBroken01;
                }
            }
            else
            {
                tileToPaint = tileWallBase;
            }

            tilemapWalls.SetTile(pos, tileToPaint);
        }
    }

    private void BuildFloor(Vector3Int position)
    {
        TileBase tileToPaint = getRandomTileFloor();
        tilemapFloor.SetTile(position, tileToPaint);
        ClearWalls(position);
        BuildWall(position);
    }

    private void ClearFloor(Vector3Int position)
    {
        tilemapFloor.SetTile(position, null);
        ClearWalls(position);
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
        private string type;

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

        public string GetType()
        {
            return type;
        }

        public void SetType(string newType)
        {
            type = newType;
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

    public void ClearRoom(Room room)
    {
        rooms.Remove(room);
        roomsPlaced--;
        foreach (Vector3Int tileCoord in room.FloorTileCoordinates)
        {
            Vector3Int tilePosition = room.GetPosition() + tileCoord;
            ClearFloor(tilePosition);
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

    private Room CreateRoomSquare(int width, int height)
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

    private Room CreateRoomPlus(int centralWidth, int centralHeight, int armLength, int armWidth)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();

        // Add the central part
        for (int x = 0; x < centralWidth; x++)
        {
            for (int y = 0; y < centralHeight; y++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }
        }

        // Calculate start positions for the arms
        int centralStartX = (centralWidth - armWidth) / 2;
        int centralStartY = (centralHeight - armWidth) / 2;

        // Add the vertical arms (top and bottom)
        for (int x = centralStartX; x < centralStartX + armWidth; x++)
        {
            // Top arm
            for (int y = centralHeight; y < centralHeight + armLength; y++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }

            // Bottom arm
            for (int y = -armLength; y < 0; y++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }
        }

        // Add the horizontal arms (left and right)
        for (int y = centralStartY; y < centralStartY + armWidth; y++)
        {
            // Right arm
            for (int x = centralWidth; x < centralWidth + armLength; x++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }

            // Left arm
            for (int x = -armLength; x < 0; x++)
            {
                floorTileCoordinates.Add(new Vector3Int(x, y, 0));
            }
        }

        // Create the room with the calculated floor tiles
        return new Room(floorTileCoordinates);
    }

    private Room CreateRoomIrregular(int maxSteps, int iterations, int stepSize)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        //int k = 0;
        for (int i = 0; i < iterations; i++)
        {
            // Start at the center of the designated room area
            Vector3Int currentPos = new Vector3Int(0, 0, 0);
            if (!visited.Contains(currentPos))
            {
                AddTiles(floorTileCoordinates, currentPos, stepSize);
                visited.Add(currentPos);
            }

            // Perform a random walk
            for (int j = 0; j < maxSteps; j++)
            {
                Vector3Int nextPos = GetNextPosition(currentPos, stepSize);
                if (!visited.Contains(nextPos))
                {
                    // Check if nextPos is valid
                    if (CheckAddTiles(floorTileCoordinates, nextPos, stepSize))
                    {
                        currentPos = nextPos;
                        AddTiles(floorTileCoordinates, nextPos, stepSize);
                        visited.Add(nextPos);
                        //k++;
                    }
                    else
                    {
                        // Try other directions if initial nextPos is invalid
                        bool validMove = false;
                        List<int> directions = new List<int> { 0, 1, 2, 3 };
                        ShuffleList(directions); // Shuffle to randomize direction checks

                        foreach (int direction in directions)
                        {
                            nextPos = GetNextPosition(currentPos, stepSize, direction);

                            if (!visited.Contains(nextPos) && CheckAddTiles(floorTileCoordinates, nextPos, stepSize))
                            {
                                AddTiles(floorTileCoordinates, nextPos, stepSize);
                                visited.Add(nextPos);
                                currentPos = nextPos;
                                validMove = true;
                                //k++;
                                break; // Break loop if a valid move is found
                            }
                        }
                    }
                } 
                else
                {
                    currentPos = nextPos;
                    //k++;
                }
            }
        }
        /*
        Debug.Log("A");
        Debug.Log(k);
        Debug.Log(maxSteps * iterations);
        Debug.Log("B");
        */
        Room newRoom = new Room(floorTileCoordinates);
        return newRoom;
    }

    private Vector3Int GetNextPosition(Vector3Int currentPos, int stepSize, int direction = -1)
    {
        // Generate a random direction if not specified
        if (direction == -1)
        {
            direction = Random.Range(0, 4);
        }

        Vector3Int nextPos = currentPos;

        switch (direction)
        {
            case 0: // Up
                nextPos += new Vector3Int(0, stepSize, 0);
                break;
            case 1: // Down
                nextPos += new Vector3Int(0, -stepSize, 0);
                break;
            case 2: // Left
                nextPos += new Vector3Int(-stepSize, 0, 0);
                break;
            case 3: // Right
                nextPos += new Vector3Int(stepSize, 0, 0);
                break;
        }

        return nextPos;
    }

    private void AddTiles(List<Vector3Int> tiles, Vector3Int centerPos, int stepSize)
    {
        // Add a block of tiles with size stepSize x stepSize centered around centerPos
        for (int x = 0; x < stepSize; x++)
        {
            for (int y = 0; y < stepSize; y++)
            {
                Vector3Int tilePos = centerPos + new Vector3Int(x, y, 0);
                if (!tiles.Contains(tilePos))  // Prevent duplicate tiles
                {
                    tiles.Add(tilePos);
                }
            }
        }
    }

    private bool CheckAddTiles(List<Vector3Int> tiles, Vector3Int centerPos, int stepSize)
    {
        Vector3Int[] neighborsVertical = new Vector3Int[]
        {
        new Vector3Int(0,  3, 0),
        new Vector3Int(0,  2, 0),
        new Vector3Int(0,  1, 0),
        };

        Vector3Int[] neighborsHorizontal = new Vector3Int[]
        {
        new Vector3Int(2,  0, 0),
        new Vector3Int(1,  0, 0),
        };

        for (int x = -2; x < stepSize + 2; x++)
        {
            Vector3Int tilePos = centerPos + new Vector3Int(x, stepSize - 1, 0);
            if (!tiles.Contains(tilePos + neighborsVertical[2]) && (tiles.Contains(tilePos + neighborsVertical[1]) || tiles.Contains(tilePos + neighborsVertical[0])))
            {
                return false;
            }

            tilePos = centerPos + new Vector3Int(x, 0, 0);
            if (!tiles.Contains(tilePos - neighborsVertical[2]) && (tiles.Contains(tilePos - neighborsVertical[1]) || tiles.Contains(tilePos - neighborsVertical[0])))
            {
                return false;
            }
        }

        for (int y = 0; y < stepSize; y++)
        {
            Vector3Int tilePos = centerPos + new Vector3Int(stepSize - 1, y, 0);
            if (!tiles.Contains(tilePos + neighborsHorizontal[1]) && tiles.Contains(tilePos + neighborsHorizontal[0]))
            {
                return false;
            }

            tilePos = centerPos + new Vector3Int(0, y, 0);
            if (!tiles.Contains(tilePos - neighborsHorizontal[1]) && tiles.Contains(tilePos - neighborsHorizontal[0]))
            {
                return false;
            }
        }

        /*
        Vector3Int tilePosCornerDownLeft = centerPos + new Vector3Int(0, 0, 0);
        Vector3Int tilePosCornerUpLeft = centerPos + new Vector3Int(0, stepSize - 1, 0);
        Vector3Int tilePosCornerDownRight = centerPos + new Vector3Int(stepSize - 1, 0, 0);
        Vector3Int tilePosCornerUpRight = centerPos + new Vector3Int(stepSize - 1, stepSize - 1, 0);

        if (!tiles.Contains(tilePosCornerDownLeft + new Vector3Int(0, -1, 0)) && !tiles.Contains(tilePosCornerDownLeft + new Vector3Int(0, -2, 0)) && tiles.Contains(tilePosCornerDownLeft + new Vector3Int(-1, -3, 0)))
        {
            return false;
        }

        if (!tiles.Contains(tilePosCornerUpLeft + new Vector3Int(0, 1, 0)) && !tiles.Contains(tilePosCornerUpLeft + new Vector3Int(0, 2, 0)) && tiles.Contains(tilePosCornerUpLeft + new Vector3Int(-1, 3, 0)))
        {
            return false;
        }

        if (!tiles.Contains(tilePosCornerDownRight + new Vector3Int(0, -1, 0)) && !tiles.Contains(tilePosCornerDownRight + new Vector3Int(0, -2, 0)) && tiles.Contains(tilePosCornerDownRight + new Vector3Int(1, -3, 0)))
        {
            return false;
        }

        if (!tiles.Contains(tilePosCornerUpRight + new Vector3Int(0, 1, 0)) && !tiles.Contains(tilePosCornerUpRight + new Vector3Int(0, 2, 0)) && tiles.Contains(tilePosCornerUpRight + new Vector3Int(1, 3, 0)))
        {
            return false;
        }

        */

        return true;
    }

    private void TryBuildAdditionalHallways(Room currentRoom, Room neighborRoom)
    {
        int hallwayAttempts = 1;

        if (Random.value < chanceAdditionalHallways)
        {
            hallwayAttempts = Random.Range(2, maxAdditionalHallways);
        }

        if (hallwayAttempts == 1)
        {
            return;
        }

        List<Room> closestRooms = FindClosestRooms(currentRoom, 6, neighborRoom);

        int hallwaysBuilt = 0;

        foreach (Room targetRoom in closestRooms)
        {
            if (BuildHallwayBetweenRooms(currentRoom, targetRoom))
            {
                hallwaysBuilt++;

                if ((hallwayAttempts == 2 && hallwaysBuilt >= 1) ||
                    (hallwayAttempts == 3 && hallwaysBuilt >= 2) ||
                    (hallwayAttempts == 4 && hallwaysBuilt >= 3))
                {
                    break; // Stop once the required number of hallways is built
                }
            }
        }
    }

    private Vector3 CalculateCenterOfMass(Room room)
    {
        List<Vector3Int> floorTiles = room.FloorTileCoordinates;
        if (floorTiles == null || floorTiles.Count == 0)
        {
            return Vector3.zero; // Return zero vector if no tiles are present
        }

        float sumX = 0f;
        float sumY = 0f;
        float sumZ = 0f;

        foreach (Vector3Int tile in floorTiles)
        {
            sumX += tile.x;
            sumY += tile.y;
            sumZ += tile.z;
        }

        int tileCount = floorTiles.Count;
        return new Vector3(sumX / tileCount, sumY / tileCount, sumZ / tileCount);
    }

    private Vector3 CalculateCenterOfMassPosition(Room room)
    {
        return CalculateCenterOfMass(room) + room.GetPosition();
    }

    // Helper function to calculate the Euclidean distance between two rooms
    private float CalculateDistance(Room room1, Room room2)
    {
        Vector3 center1 = CalculateCenterOfMassPosition(room1);
        Vector3 center2 = CalculateCenterOfMassPosition(room2);
        return Vector3.Distance(center1, center2);
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

        List<int> hallwayTypes = new List<int> { 0, 1 };
        ShuffleList(hallwayTypes);

        // Try to build a hallway
        foreach (int hallwayType in hallwayTypes) {
            if (hallwayType == 0)
            {
                List<int> sides = new List<int> { 0, 1, 2, 3 };
                ShuffleList(sides);

                List<int> lengths = new List<int> { 3, 4, 5, 6, 7, 8, 9 };
                ShuffleList(lengths);

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
                                if (room0.GetPosition().x + room0Point.x != room1.GetPosition().x + room1Point.x && room0.GetPosition().y + room0Point.y != room1.GetPosition().y + room1Point.y)
                                {
                                    continue;
                                }

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
            }
            else if (hallwayType == 1)
            {
                List<int> sides0 = new List<int> { 0, 2 };
                ShuffleList(sides0);
                List<int> sides1 = new List<int> { 1, 3 };
                ShuffleList(sides1);

                int minLength = int.MaxValue;
                List<object> bestParams = new List<object>();

                foreach (int side0 in sides0)
                {
                    foreach (int side1 in sides1)
                    {
                        List<Vector3Int> room0Sides = room0ConnectionPoints[side0];
                        List<Vector3Int> room1Sides = room1ConnectionPoints[side1];

                        ShuffleList(room0Sides);
                        ShuffleList(room1Sides);

                        foreach (var room0Point in room0Sides)
                        {
                            foreach (var room1Point in room1Sides)
                            {
                                if (side0 == 0)
                                {
                                    Vector3Int posHallway = Vector3Int.zero;
                                    Vector3Int posHallwayEnd = Vector3Int.zero;
                                    Vector3Int offset = room0.GetPosition();

                                    if (side1 == 3)
                                    {
                                        if (room0Point.x + room0.GetPosition().x <= room1Point.x + room1.GetPosition().x || room0Point.y + room0.GetPosition().y <= room1Point.y + room1.GetPosition().y)
                                        {
                                            continue;
                                        }

                                        posHallway = offset + room0Point + new Vector3Int(0, -1 * (room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y) - 1, 0);
                                        Vector3Int posHallwayEnd0 = posHallway;
                                        Vector3Int posHallwayEnd1 = room1.GetPosition() + room1Point + new Vector3Int(1, 0, 0);

                                        if (CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), 2, room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y) &&
                                            CheckBuildHallwayHorizontal(posHallwayEnd1, room0Point.x + room0.GetPosition().x - room1Point.x - room1.GetPosition().x + 1, 2))
                                        {
                                            int length = Mathf.Abs(room0.GetPosition().y + room0Point.y - room1.GetPosition().y - room1Point.y) + Mathf.Abs(room0.GetPosition().x + room0Point.x - room1.GetPosition().x - room1Point.x);
                                            if (length < minLength)
                                            {
                                                minLength = length;
                                                bestParams = new List<object> { posHallway + new Vector3Int(0, 1, 0), 2, room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y, posHallwayEnd1, room0Point.x + room0.GetPosition().x - room1Point.x - room1.GetPosition().x + 1, 2 };
                                            }
                                        }
                                    }
                                    else if (side1 == 1)
                                    {
                                        if (room0Point.x + room0.GetPosition().x >= room1Point.x + room1.GetPosition().x || room0Point.y + room0.GetPosition().y <= room1Point.y + room1.GetPosition().y)
                                        {
                                            continue;
                                        }

                                        posHallway = offset + room0Point + new Vector3Int(0, -1 * (room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y) - 1, 0);
                                        Vector3Int posHallwayEnd0 = room1.GetPosition() + room1Point + new Vector3Int(-(room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x), 0, 0);
                                        Vector3Int posHallwayEnd1 = room1.GetPosition() + room1Point + new Vector3Int(-1, 0, 0);

                                        if (CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), 2, room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y) &&
                                            CheckBuildHallwayHorizontal(posHallwayEnd0, room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x, 2))
                                        {
                                            int length = -1 + Mathf.Abs(room0.GetPosition().y + room0Point.y - room1.GetPosition().y - room1Point.y) + Mathf.Abs(room0.GetPosition().x + room0Point.x - room1.GetPosition().x - room1Point.x);
                                            if (length < minLength)
                                            {
                                                minLength = length;
                                                bestParams = new List<object> { posHallway + new Vector3Int(0, 1, 0), 2, room0Point.y + room0.GetPosition().y - room1Point.y - room1.GetPosition().y, posHallwayEnd0, room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x, 2 };
                                            }
                                        }
                                    }
                                }
                                else if (side0 == 2)
                                {
                                    Vector3Int posHallway = Vector3Int.zero;
                                    Vector3Int posHallwayEnd = Vector3Int.zero;
                                    Vector3Int offset = room0.GetPosition();

                                    if (side1 == 3)
                                    {
                                        if (room0Point.x + room0.GetPosition().x <= room1Point.x + room1.GetPosition().x || room0Point.y + room0.GetPosition().y >= room1Point.y + room1.GetPosition().y)
                                        {
                                            continue;
                                        }

                                        posHallway = offset + room0Point + new Vector3Int(0, 1, 0);
                                        Vector3Int posHallwayEnd0 = posHallway;
                                        Vector3Int posHallwayEnd1 = room1.GetPosition() + room1Point + new Vector3Int(1, 0, 0);

                                        if (CheckBuildHallwayVertical(posHallway, 2, room1Point.y + room1.GetPosition().y - room0Point.y - room0.GetPosition().y + 1) &&
                                            CheckBuildHallwayHorizontal(posHallwayEnd1, room0Point.x + room0.GetPosition().x - room1Point.x - room1.GetPosition().x + 1, 2))
                                        {
                                            int length = 1 + Mathf.Abs(room0.GetPosition().y + room0Point.y - room1.GetPosition().y - room1.GetPosition().y) + Mathf.Abs(room0.GetPosition().x + room0Point.x - room1.GetPosition().x - room1Point.x);
                                            if (length < minLength)
                                            {
                                                minLength = length;
                                                bestParams = new List<object> { posHallway, 2, room1Point.y + room1.GetPosition().y - room0Point.y - room0.GetPosition().y + 1, posHallwayEnd1, room0Point.x + room0.GetPosition().x - room1Point.x - room1.GetPosition().x + 1, 2 };
                                            }
                                        }
                                    }
                                    else if (side1 == 1)
                                    {
                                        if (room0Point.x + room0.GetPosition().x >= room1Point.x + room1.GetPosition().x || room0Point.y + room0.GetPosition().y >= room1Point.y + room1.GetPosition().y)
                                        {
                                            continue;
                                        }

                                        posHallway = offset + room0Point + new Vector3Int(0, 1, 0);
                                        Vector3Int posHallwayEnd0 = room1.GetPosition() + room1Point + new Vector3Int(-(room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x), 0, 0);
                                        Vector3Int posHallwayEnd1 = room1.GetPosition() + room1Point + new Vector3Int(-(room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x), 0, 0);

                                        if (CheckBuildHallwayVertical(posHallway, 2, room1Point.y + room1.GetPosition().y - room0Point.y - room0.GetPosition().y + 1) &&
                                            CheckBuildHallwayHorizontal(posHallwayEnd0, room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x, 2))
                                        {
                                            int length = Mathf.Abs(room0.GetPosition().y + room0Point.y - room1.GetPosition().y - room1.GetPosition().y) + Mathf.Abs(room0.GetPosition().x + room0Point.x - room1.GetPosition().x - room1Point.x);
                                            if (length < minLength)
                                            {
                                                minLength = length;
                                                bestParams = new List<object> { posHallway, 2, room1Point.y + room1.GetPosition().y - room0Point.y - room0.GetPosition().y + 1, posHallwayEnd0, room1Point.x + room1.GetPosition().x - room0Point.x - room0.GetPosition().x, 2 };
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (minLength < maxLHallwayLength)
                {
                    //Debug.Log(minLength);
                    BuildSquare((Vector3Int)bestParams[0], (int)bestParams[1], (int)bestParams[2]);
                    BuildSquare((Vector3Int)bestParams[3], (int)bestParams[4], (int)bestParams[5]);
                    return true;
                }
            }
        }

        return false;
    }

    public List<Vector3Int> GetFloorCorner2x2(Room room, string missingCorner)
    {
        List<Vector3Int> matchingRegions = new List<Vector3Int>();

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            if (IsCorner2x2(room, tile, missingCorner))
            {
                Vector3Int tilePosition = new Vector3Int(0, 0, 0);
                switch (missingCorner.ToLower())
                    {
                    case "downleft":
                        tilePosition = new Vector3Int(-1, -1, 0);
                        break;
                    case "downright":
                        tilePosition = new Vector3Int(1, -1, 0);
                        break;
                    case "upleft":
                        tilePosition = new Vector3Int(-1, 1, 0);
                        break;
                    case "upright":
                        tilePosition = new Vector3Int(1, 1, 0);
                        break;
                }
                matchingRegions.Add(tile + tilePosition);
            }
        }
        return matchingRegions;
    }

    private bool IsCorner2x2(Room room, Vector3Int startPos, string missingCorner)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        Vector3Int tilePosition = new Vector3Int(0, 0, 0);

        switch (missingCorner.ToLower())
        {
            case "downleft":
                tilePosition = new Vector3Int(-1, -1, 0);
                neighbors = new List<Vector3Int>
                {
                    new Vector3Int(1, 0, 0),
                    new Vector3Int(0, 1, 0),
                    new Vector3Int(1, 1, 0)
                };
                break;
            case "downright":
                tilePosition = new Vector3Int(1, -1, 0);
                neighbors = new List<Vector3Int>
                {
                    new Vector3Int(-1, 0, 0),
                    new Vector3Int(0, 1, 0),
                    new Vector3Int(-1, 1, 0)
                };
                break;
            case "upleft":
                tilePosition = new Vector3Int(-1, 1, 0);
                neighbors = new List<Vector3Int>
                {
                    new Vector3Int(1, 0, 0),
                    new Vector3Int(0, -1, 0),
                    new Vector3Int(1, -1, 0)
                };
                break;
            case "upright":
                tilePosition = new Vector3Int(1, 1, 0);
                neighbors = new List<Vector3Int>
                {
                    new Vector3Int(-1, 0, 0),
                    new Vector3Int(0, -1, 0),
                    new Vector3Int(-1, -1, 0)
                };
                break;
        }

        foreach (Vector3Int neighbor in neighbors)
        {
            if (!room.FloorTileCoordinates.Contains(startPos + tilePosition + neighbor))
            {
                return false;
            }
        }
        if (room.FloorTileCoordinates.Contains(startPos + tilePosition))
        {
            return false;
        }
        return true;
    }

    public void AddFloorCornerBroken(Room room)
    {
        string[] cornerTypes = { "downleft", "downright", "upleft", "upright" };
        Dictionary<string, Tile> cornerTiles = new Dictionary<string, Tile>
        {
            { "downleft", tileFloorBrokenDownLeft },
            { "downright", tileFloorBrokenDownRight },
            { "upleft", tileFloorBrokenUpLeft },
            { "upright", tileFloorBrokenUpRight }
        };

        List<Vector3Int> allCorners = new List<Vector3Int>();

        // Get all types of corners
        foreach (string cornerType in cornerTypes)
        {
            List<Vector3Int> corners = GetFloorCorner2x2(room, cornerType);
            Vector3Int tilePosition = new Vector3Int(0, 0, 0);

            foreach (Vector3Int corner in corners)
            {
                switch (cornerType.ToLower())
                {
                    case "downleft":
                        tilePosition = corner + new Vector3Int(0, 0, 0);
                        break;
                    case "downright":
                        tilePosition = corner + new Vector3Int(-1, 0, 0);
                        break;
                    case "upleft":
                        tilePosition = corner + new Vector3Int(0, -1, 0);
                        break;
                    case "upright":
                        tilePosition = corner + new Vector3Int(-1, -1, 0);
                        break;
                }
                tilePosition += room.GetPosition();
                allCorners.Add(tilePosition);
            }
        }

        foreach (Vector3Int tilePos in allCorners)
        {
            if (Random.value < chanceTileFloorCornerBroken)
            {
                if (tilemapFloor.GetTile(tilePos) != null)
                    tilemapFloor.SetTile(tilePos, tileFloorBrokenDownLeft);
                if (tilemapFloor.GetTile(tilePos + new Vector3Int(1, 0, 0)) != null)
                    tilemapFloor.SetTile(tilePos + new Vector3Int(1, 0, 0), tileFloorBrokenDownRight);
                if (tilemapFloor.GetTile(tilePos + new Vector3Int(0, 1, 0)) != null)
                    tilemapFloor.SetTile(tilePos + new Vector3Int(0, 1, 0), tileFloorBrokenUpLeft);
                if (tilemapFloor.GetTile(tilePos + new Vector3Int(1, 1, 0)) != null)
                    tilemapFloor.SetTile(tilePos + new Vector3Int(1, 1, 0), tileFloorBrokenUpRight);
            }
        }
    }

    private List<Vector3Int> RandomWalk(Room room, int maxSteps, int iterations, int stepSize = 1)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        // Possible directions: Up, Down, Left, Right
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(0, stepSize, 0),   // Up
        new Vector3Int(0, -stepSize, 0),  // Down
        new Vector3Int(stepSize, 0, 0),   // Right
        new Vector3Int(-stepSize, 0, 0)   // Left
        };

        Vector3Int currentPos = room.FloorTileCoordinates[Random.Range(0, room.FloorTileCoordinates.Count)];

        for (int iter = 0; iter < iterations; iter++)
        {
            Vector3Int startPosition = currentPos;

            for (int i = 0; i < maxSteps; i++)
            {
                // Filter valid positions: must be within the room and not visited
                List<Vector3Int> possiblePositions = directions
                    .Select(dir => currentPos + dir)
                    .Where(pos => room.FloorTileCoordinates.Contains(pos) && !visited.Contains(pos))
                    .ToList();

                if (possiblePositions.Count == 0)
                    break; // If no valid moves, exit the loop

                // Randomly select the next position from the valid positions
                Vector3Int nextPos = possiblePositions[Random.Range(0, possiblePositions.Count)];

                floorTileCoordinates.Add(nextPos);
                visited.Add(nextPos);
                currentPos = nextPos;
            }

            currentPos = startPosition; // Reset to start position for next iteration
        }

        return floorTileCoordinates;
    }

    private void FillRoomWithPlants(Room room, GameObject plantPrefab, float chancePlantAny)
    {
        List<Vector3Int> walkedTilesPlants = RandomWalk(room, 10, 3);
        Sprite selectedPlantSprite = plantPrefab.GetComponent<PlantScript>().GetRandomPlantSprite();

        if (Random.value < chancePlantAny)
        {
            // Choose a random sprite for each plant
            foreach (Vector3Int pos in walkedTilesPlants)
            {
                Sprite randomSprite = plantPrefab.GetComponent<PlantScript>().GetRandomPlantSprite();
                FillRegionWithObject(new List<Vector3Int> { pos }, plantPrefab, room.GetPosition(), randomSprite);
            }
        }
        else
        {
            // Use the selectedPlantSprite for all plants in this room
            FillRegionWithObject(walkedTilesPlants, plantPrefab, room.GetPosition(), selectedPlantSprite);
        }
    }

    private void FillRegionWithObject(List<Vector3Int> region, GameObject objectToPlace, Vector3Int offset, Sprite selectedSprite)
    {
        foreach (Vector3Int pos in region)
        {
            GameObject instance = Instantiate(objectToPlace, pos + offset + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            instance.GetComponent<PlantScript>().SetPlantSprite(selectedSprite);
        }
    }

    private void FillRoomWithFloor(Room room, Tile tileToPlace)
    {
        if (Random.value < chanceRoomFloorFour)
        {
            if (tileToPlace == tileFloorFour01)
            {
                List<Vector3Int> walkedTilesFloorFour = RandomWalk(room, 8, 6);

                Vector3Int offset = room.GetPosition();
                foreach (Vector3Int pos in walkedTilesFloorFour)
                {
                    if (Random.value < chanceTileFloorFour)
                    {
                        if (Random.value < chanceRoomFloorFourOne)
                        {
                            tileToPlace = tileFloorOptions[Random.Range(0, tileFloorOptions.Count)];
                        } else
                        {
                            tileToPlace = tileFloorFour01;
                        }

                        tilemapFloor.SetTile(pos + offset, tileToPlace);
                    }
                }
            }
        }
    }

    private void FillRoomWithFloorFull(Room room, Tile tileToPlace)
    {
        if (Random.value < chanceRoomFloorFourFull)
        {
            bool noBroken = false, fullBroken = false;

            if (Random.value < chanceRoomFloorFourFullBroken)
            {
                fullBroken = true;
            }
            else if (Random.value < chanceRoomFloorFourFullNoBroken)
            {
                noBroken = true;
            }

            if (tileToPlace == tileFloorFour01)
            {
                List<Vector3Int> fullTilesFloorFour = room.FloorTileCoordinates;

                Vector3Int offset = room.GetPosition();
                foreach (Vector3Int pos in fullTilesFloorFour)
                {
                    if (noBroken)
                    {
                        tileToPlace = tileFloorFour01;
                    }
                    else if (fullBroken || Random.value < chanceRoomFloorFourFullOne)
                    {
                        tileToPlace = tileFloorOptions[Random.Range(0, tileFloorOptions.Count)];
                    }
                    else
                    {
                        tileToPlace = tileFloorFour01;
                    }

                    tilemapFloor.SetTile(pos + offset, tileToPlace);
                }
            }
        }
    }

    private void FillRoomWithFloorChess(Room room)
    {
        if (room.GetType() == "chess")
        {
            List<Vector3Int> fullTilesFloorFour = room.FloorTileCoordinates;
            Tile tileToPlace;
            Vector3Int offset = room.GetPosition();
            int step = 0;
            bool flip = false;

            foreach (Vector3Int pos in fullTilesFloorFour)
            {
                if (step % 8 == 0) flip = !flip;

                if (step % 2 == 0 && flip || step % 2 == 1 && !flip)
                {
                    tileToPlace = tileFloorChessBlack;
                } else
                {
                    tileToPlace = tileFloorChessWhite;
                }
                ++step;

                tilemapFloor.SetTile(pos + offset, tileToPlace);
            }
        }
    }

    private void FillRoomWithTables(Room room)
    {
        if (Random.value < chanceTable)
        {
            if (Random.value < chanceTableSmall)
            {
                Vector3Int? freePosition = GetRectanglesInRoomFree(room, 1, 2);
                if (freePosition != null)
                {
                    float spriteHeightInUnits = 32f / 16f;
                    float pivotYPercentage = 0.9f;
                    float yOffset = spriteHeightInUnits * pivotYPercentage;
                    Vector3 pivotOffset = new Vector3(0f, yOffset, 0f);

                    PlaceObject(freePosition.Value, Table1x2Prefab01, room.GetPosition() + pivotOffset);
                }
            }
            else
            {
                Vector3Int? freePosition = GetRectanglesInRoomFree(room, 2, 2);
                if (freePosition != null)
                {
                    float spriteHeightInUnits = 37f / 16f;
                    float pivotYPercentage = 0.9f;
                    float yOffset = spriteHeightInUnits * pivotYPercentage;
                    Vector3 pivotOffset = new Vector3(0f, yOffset, 0f);

                    PlaceObject(freePosition.Value, Table2x2Prefab01, room.GetPosition() + pivotOffset);
                }
            }
        }
    }

    private void FillRoomWithEnemies(Room room)
    {
        if (Random.value < chanceEnemy01)
        {
            int numberOfPlacements = Random.Range(1, 4); // Random number between 1 and 4

            for (int i = 0; i < numberOfPlacements; i++)
            {
                Vector3Int? freePosition = GetRectanglesInRoomFree(room, 1, 2);
                if (freePosition != null)
                {
                    float spriteHeightInUnits = 24f / 16f;
                    float pivotYPercentage = 1f;
                    float yOffset = spriteHeightInUnits * pivotYPercentage;
                    Vector3 pivotOffset = new Vector3(0.5f, yOffset, 0f);

                    PlaceObject(freePosition.Value, EnemyPrefab01, room.GetPosition() + pivotOffset);
                }
            }
        }
    }

    private bool CheckTileNextDoor(Room room, Vector3Int position)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0)
        };

        foreach (var direction in directions)
        {
            Vector3Int neighborPos = position + direction;
            Vector3Int neighborPosWithOffset = neighborPos + room.GetPosition();

            if (!room.FloorTileCoordinates.Contains(neighborPos) && tilemapFloor.GetTile(neighborPosWithOffset) != null)
            {
                return false;
            }
        }
        return true;
    }

    private List<Vector3Int> GetRectanglesInRoom(Room room, int width, int height)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            bool isValid = true;

            for (int x = 0; x < width && isValid; x++)
            {
                for (int y = 0; y < height && isValid; y++)
                {
                    Vector3Int checkPos = new Vector3Int(tile.x + x, tile.y + y, tile.z);
                    if (!room.FloorTileCoordinates.Contains(checkPos))
                    {
                        isValid = false;
                    }
                }
            }

            if (isValid)
            {
                validPositions.Add(tile);
            }
        }

        return validPositions;
    }

    private Vector3Int? GetRectanglesInRoomFree(Room room, int width, int height)
    {
        // Get all valid positions for the rectangle
        List<Vector3Int> validPositions = GetRectanglesInRoom(room, width, height);

        // Shuffle the valid positions list
        ShuffleList(validPositions);

        // Check each position to see if it satisfies the CheckTileNextDoor condition
        foreach (Vector3Int position in validPositions)
        {
            bool allTilesValid = true;

            for (int x = 0; x < width && allTilesValid; x++)
            {
                for (int y = 0; y < height && allTilesValid; y++)
                {
                    Vector3Int checkPos = new Vector3Int(position.x + x, position.y + y, position.z);
                    if (!CheckTileNextDoor(room, checkPos))
                    {
                        allTilesValid = false;
                    }
                }
            }

            if (allTilesValid)
            {
                return position; // Return the first valid position
            }
        }

        return null; // Return null if no valid position is found
    }

    private void PlaceObject(Vector3Int position, GameObject objectToPlace, Vector3 offset, Sprite selectedSprite = null)
    {
        GameObject instance = Instantiate(objectToPlace, position + offset, Quaternion.identity);

        if (selectedSprite != null)
        {
            instance.GetComponent<SpriteRenderer>().sprite = selectedSprite;
        }
    }

    bool roomTemplate(Room room)
    {
        if (room.GetType() == "chess")
        {
            return true;
        } else
        {
            return false;
        }
    }

    private void Start()
    {
        Vector3Int pos01 = new Vector3Int(0, 0, 0);

        Room initialRoom = CreateRoomSquare(6, 6);
        InstantiateRoom(initialRoom, pos01);

        BuildRooms();

        foreach (Room room in rooms)
        {
            if (!roomTemplate(room))
            {
                FillRoomWithTables(room);

                FillRoomWithPlants(room, PlantPrefab01, chancePlantAny);

                FillRoomWithFloor(room, tileFloorFour01);
                AddFloorCornerBroken(room);
                FillRoomWithFloorFull(room, tileFloorFour01);
            } else
            {
                FillRoomWithFloorChess(room);
            }

            FillRoomWithEnemies(room);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    private void BuildRooms()
    {
        for (int n = 0; n < numRooms; n++)
        {
            Room room1 = null;
            string type = "square";

            if (Random.value < chanceRoomSpecial)
            {
                if (Random.value < chanceRoomChess)
                {
                    type = "chess";

                    int randomWidth = 8;
                    int randomHeight = 8;
                    room1 = CreateRoomSquare(randomWidth, randomHeight);
                }
                else if (Random.value < chanceRoomPlus)
                {
                    type = "plus";

                    int[] widthOptions = { 6, 8, 10 };
                    int[] heightOptions = { 6, 8, 10 };
                    int[] roomSquareOptions = { 3, 4, 5, 6, 7 };
                    int[] otherOptions = { 2, 4, 6 };

                    int randomWidth = widthOptions[Random.Range(0, widthOptions.Length)];
                    int randomHeight = heightOptions[Random.Range(0, heightOptions.Length)];
                    int randomSquareSize = roomSquareOptions[Random.Range(0, roomSquareOptions.Length)];
                    int randomOtherSize = otherOptions[Random.Range(0, otherOptions.Length)];

                    room1 = CreateRoomPlus(randomWidth, randomHeight, randomSquareSize, randomOtherSize);
                }
                else
                {
                    type = "irregular";
                    room1 = CreateRoomIrregular(6, 8, 2);
                }
            } else
            {
                int randomWidth = Random.Range(5, 9);
                int randomHeight = Random.Range(5, 9);
                room1 = CreateRoomSquare(randomWidth, randomHeight);
            }
            room1.SetType(type);

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
                                if (room0Point.x != room1Point.x && room0Point.y != room1Point.y)
                                {
                                    continue;
                                }

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

                                        if (CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), 2, length))
                                        {
                                            roomPlaced = true;
                                            BuildSquare(posHallway + new Vector3Int(0, 1, 0), 2, length);
                                            break;
                                        }
                                        else
                                        {
                                            ClearRoom(room1);
                                        }
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

                                        if (CheckBuildHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, 2))
                                        {
                                            roomPlaced = true;
                                            BuildSquare(posHallway + new Vector3Int(1, 0, 0), length, 2);
                                            break;
                                        }
                                        else
                                        {
                                            ClearRoom(room1);
                                        }
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

                                        if (CheckBuildHallwayVertical(posHallway, 2, length))
                                        {
                                            roomPlaced = true;
                                            BuildSquare(posHallway, 2, length);
                                            break;
                                        }
                                        else
                                        {
                                            ClearRoom(room1);
                                        }
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

                                        if (CheckBuildHallwayHorizontal(posHallway, length, 2))
                                        {
                                            roomPlaced = true;
                                            BuildSquare(posHallway, length, 2);
                                            break;
                                        }
                                        else
                                        {
                                            ClearRoom(room1);
                                        }
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
