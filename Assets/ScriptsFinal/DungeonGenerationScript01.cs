using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine.SceneManagement;
using System.Linq;
using static DungeonGenerationScript01;

public class DungeonGenerationScript01 : MonoBehaviour
{
    [SerializeField] private int numRooms;
    [SerializeField] private int maxLHallwayLength;
    [SerializeField] private int roomsPlaced;

    [Header("Special Rooms")]
    [SerializeField] private float chanceRoomSpecial;
    [SerializeField] private float chanceRoomSquareHole;
    [SerializeField] private float chanceRoomBelt;
    [SerializeField] private float chanceRoomPlus;
    [SerializeField] private float chanceRoomLCorner;
    [SerializeField] private float chanceRoomChess;

    [Header("Dungeon Level Chances")]
    [SerializeField] private float chanceDungeonFloorNew;
    [SerializeField] private float chanceRegionEmeraldDungeon;

    [Header("Regions")]
    [SerializeField] private float chanceRegionEmerald;
    [SerializeField] private int regionEmeraldSizeMin, regionEmeraldSizeMax;
    [SerializeField] private int regionEmeraldReset;
    [SerializeField] private float chanceRegionTileEmerald;
    private bool regionEmeraldDungeon = false;
    private int regionEmeraldCounter;

    [Header("Objects")]
    [SerializeField] private GameObject PlantPrefab01;
    [SerializeField] private GameObject CobwebPrefab01;
    [SerializeField] private GameObject TreePrefab01;
    [SerializeField] private GameObject BuddhaPrefab01;
    [SerializeField] private GameObject PotPrefab01;
    [SerializeField] private GameObject BookshelfSmallPrefab01;
    [SerializeField] private GameObject BookstackPrefab01;
    [SerializeField] private GameObject Table2x2Prefab01;
    [SerializeField] private GameObject Table1x2Prefab01;
    [SerializeField] private GameObject DoorDownPrefab01, DoorUpPrefab01, DoorDownPrefab02, DoorUpPrefab02, DoorRightPrefab01, DoorLeftPrefab01;
    [SerializeField] private GameObject StonePrefab01;
    [SerializeField] private GameObject TorchPrefab01;

    [Header("Parameters")]
    [SerializeField] private float chanceAdditionalHallways;
    [SerializeField] private int maxAdditionalHallways;
    [SerializeField] private float chanceAnyTileFloorsMin, chanceAnyTileFloorsMax;
    private float chanceAnyTileFloors;
    [SerializeField] private float chanceHallwayColumns;
    [SerializeField] private float chanceHallwayWidth1;
    [SerializeField] private float chanceAnyTileWallBaseBroken;
    [SerializeField] private float chanceRoomFloorCornerBroken;
    [SerializeField] private float chanceCornerFloorCornerBroken;
    [SerializeField] private float chanceRoomFloorShadow;
    [SerializeField] private float chanceRoomFloorLush;
    [SerializeField] private float chanceLushLight;
    [SerializeField] private float chanceLushMixed;
    [SerializeField] private float chanceRoomFloorBig;
    [SerializeField] private float chanceRoomFloorBigFull;
    [SerializeField] private float chanceRoomFloorFour; //chane of a room with four slab tiles
    [SerializeField] private float chanceTileFloorFour; //chance of a particular tile to be a four slab tile
    [SerializeField] private float chanceRoomFloorFourOne; //chance of a particular four slab tile to be a semi four slab tile
    [SerializeField] private float chanceRoomFloorFourFull; //chance of a room full of four slab tiles
    [SerializeField] private float chanceRoomFloorFourFullOne; //chance of a particular four slab tile to be a semi four slab tile in a full room
    [SerializeField] private float chanceRoomFloorFourFullNoBroken; //chance of a room full of four slab tiles to have no one tiles
    [SerializeField] private float chanceRoomFloorFourFullBroken; //chance of a room full of four slab tiles to have only one tiles
    [SerializeField] private float chancePlantIgnore;
    [SerializeField] private float chancePlants;
    [SerializeField] private float chancePlantAny;
    [SerializeField] private float chanceRoomCobweb;
    [SerializeField] private float chanceCornerCobweb;
    [SerializeField] private float chanceRoomTree;
    [SerializeField] private float chanceRoomBuddha;
    [SerializeField] private float chanceDoorwayBuddha;
    [SerializeField] private float chanceRoomPot;
    [SerializeField] private float chanceRoomPotDoorway;
    [SerializeField] private float chanceRoomPotEntrance;
    [SerializeField] private float chanceRoomPotEdges;
    [SerializeField] private float chanceRoomPotCorners;
    [SerializeField] private float chanceRoomPotRandom;
    [SerializeField] private float chanceRoomStones;
    [SerializeField] private float chanceStonesMany;

    [SerializeField] private float chanceRoomBookstack;
    [SerializeField] private float chanceRoomBookstackDoorway;
    [SerializeField] private float chanceRoomBookstackCorners;
    [SerializeField] private float chanceRoomBookstackRandom;

    [SerializeField] private float chanceCarpet;
    [SerializeField] private float chanceCarpetFull;
    [SerializeField] private float chanceTable;
    [SerializeField] private float chanceTableSmall;
    [SerializeField] private float chanceRoomBookshelf;
    [SerializeField] private float chanceBookshelfSmall;
    [SerializeField] private float chanceBookstack;
    [SerializeField] private float chanceEnemy01;
    [SerializeField] private float[] chanceTileFloors = new float[5];
    [SerializeField] private float[] chanceTileWallBaseBroken = new float[3];

    [Header("Enemies")]
    [SerializeField] private GameObject EnemyPrefab01;
    [SerializeField] private GameObject EnemyKnightPrefab;

    [Header("Tile Maps")]
    [SerializeField] private Tilemap tilemapFloor;
    [SerializeField] private Tilemap tilemapWalls, tilemapWallsFix;

    [Header("Floor Tiles")]
    [SerializeField] private Tile tileFloor01;
    [SerializeField] private Tile tileFloorFour01;
    [SerializeField] private List<Tile> tileFloorOptions;
    [SerializeField] private Tile tileFloorBigDownLeft, tileFloorBigDownRight, tileFloorBigUpLeft, tileFloorBigUpRight;
    [SerializeField] private Tile tileFloor02, tileFloor03, tileFloor04, tileFloor05;
    [SerializeField] private Tile tileFloorChessWhite, tileFloorChessBlack;
    [SerializeField] private Tile tileFloorShadow01;
    [SerializeField] private Tile tileFloorLushDark01, tileFloorLushLight01;

    [SerializeField] private Tile tileCarpet01CornerUpLeft, tileCarpet01CornerUpRight, tileCarpet01CornerDownLeft, tileCarpet01CornerDownRight;
    [SerializeField] private Tile tileCarpet01Up, tileCarpet01Down, tileCarpet01Left, tileCarpet01Right;
    [SerializeField] private Tile tileCarpet01Full;

    [SerializeField] private Tile tileCarpet02CornerUpLeft, tileCarpet02CornerUpRight, tileCarpet02CornerDownLeft, tileCarpet02CornerDownRight;
    [SerializeField] private Tile tileCarpet02Up, tileCarpet02Down, tileCarpet02Left, tileCarpet02Right;
    [SerializeField] private Tile tileCarpet02Full;

    [Header("Wall Tiles")]
    [SerializeField] private Tile tileWallHorizontal;
    [SerializeField] private Tile tileWallHorizontalUpLeft, tileWallHorizontalUpRight, tileWallHorizontalDownLeft, tileWallHorizontalDownRight, tileWallLeft, tileWallRight, tileWallCornerUpLeft, tileWallCornerUpRight, tileWallBase, tileWallBaseCornerLeft, tileWallBaseCornerRight, tileWallBaseDownLeft, tileWallBaseDownRight, tileWallBaseUpLeft, tileWallBaseUpRight, tileWallBaseBroken01, tileWallBaseBroken02;
    [SerializeField] private Tile tileFloorBrokenUpLeft, tileFloorBrokenUpRight, tileFloorBrokenDownLeft, tileFloorBrokenDownRight;

    [SerializeField] private Tile tileWallHorizontalFix, tileWallHorizontalUpLeftFix, tileWallHorizontalUpRightFix, tileWallHorizontalDownLeftFix, tileWallHorizontalDownRightFix, tileWallCornerUpLeftFix, tileWallCornerUpRightFix;
    [SerializeField] private Tile tileWallColumnBase, tileWallColumnMid, tileWallColumnTopFix, tileWallColumnBaseFloor;

    private List<Room> rooms = new List<Room>();
    private List<Vector3Int> plantsInRoom = new List<Vector3Int>();
    private List<Vector3Int> cobwebsInRoom = new List<Vector3Int>();
    private List<Vector3Int> treesInRoom = new List<Vector3Int>();
    private List<Vector3Int> buddhasInRoom = new List<Vector3Int>();
    private List<Vector3Int> potsInRoom = new List<Vector3Int>();
    private List<Vector3Int> bookstacksInRoom = new List<Vector3Int>();
    private List<Vector3Int> bookshelvesInRoom = new List<Vector3Int>();
    private List<Vector3Int> tables1x2InRoom = new List<Vector3Int>();
    private List<Vector3Int> tables2x2InRoom = new List<Vector3Int>();
    private List<Vector3Int> doorsHorizontalInRoom = new List<Vector3Int>();
    private List<Vector3Int> doorsHorizontalInRoomWidth1 = new List<Vector3Int>();
    private List<Vector3Int> doorsVerticalInRoom = new List<Vector3Int>();
    private List<Vector3Int> doorsVerticalInRoomWidth1 = new List<Vector3Int>();
    private List<Vector3Int> stonesInRoom = new List<Vector3Int>();

    private TilemapBaker floorBaker;
    private TilemapBaker wallsBaker;
    private TilemapBaker wallsFixBaker;

    TileBase getRandomTileFloor()
    {
        TileBase tileToPaint;

        if (Random.value < chanceRegionEmerald && regionEmeraldCounter == 0)
        {
            regionEmeraldCounter = Random.Range(regionEmeraldSizeMin, regionEmeraldSizeMax);
        } 
        else if (regionEmeraldCounter < 0)
        {
            ++regionEmeraldCounter;
        } 
        else if (regionEmeraldCounter == 0)
        {
            regionEmeraldCounter = -regionEmeraldReset;
        }

        if (regionEmeraldCounter > 0)
        {
            --regionEmeraldCounter;
        }

        if (Random.value < chanceAnyTileFloors)
        {
            if (Random.value < chanceTileFloors[4] || (regionEmeraldDungeon && regionEmeraldCounter > 0 && Random.value < chanceRegionTileEmerald))
            {
                tileToPaint = tileFloor05;
            }
            else if (Random.value < chanceTileFloors[3])
            {
                tileToPaint = tileFloor04;
            }
            else if (Random.value < chanceTileFloors[2])
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

    private void AddColumnsToHallwayHorizontal(Vector3Int startPos, int width, int height)
    {
        if (width % 2 == 1)
        {
            for (int i = 1; i < width - 1; i += 2)
            {
                Vector3Int pos = startPos + new Vector3Int(i, -1, 0);
                tilemapWalls.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBase);
                tilemapWalls.SetTile(pos, tileWallColumnMid);
                tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);

                pos = startPos + new Vector3Int(i, height, 0);
                tilemapFloor.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBaseFloor);
                tilemapWalls.SetTile(pos, tileWallColumnMid);
                tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);
            }
        }
        else
        {
            Vector3Int pos = startPos + new Vector3Int(1, -1, 0);
            tilemapWalls.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBase);
            tilemapWalls.SetTile(pos, tileWallColumnMid);
            tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);

            pos = startPos + new Vector3Int(1, height, 0);
            tilemapFloor.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBaseFloor);
            tilemapWalls.SetTile(pos, tileWallColumnMid);
            tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);

            pos = startPos + new Vector3Int(width - 2, -1, 0);
            tilemapWalls.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBase);
            tilemapWalls.SetTile(pos, tileWallColumnMid);
            tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);

            pos = startPos + new Vector3Int(width - 2, height, 0);
            tilemapFloor.SetTile(pos + new Vector3Int(0, -1, 0), tileWallColumnBaseFloor);
            tilemapWalls.SetTile(pos, tileWallColumnMid);
            tilemapWallsFix.SetTile(pos, tileWallColumnTopFix);
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

        foreach (Vector3Int tile in room.FloorTileCoordinates.Except(room.ExpandedTiles))
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

        ShuffleList(downConnections);
        ShuffleList(leftConnections);
        ShuffleList(upConnections);
        ShuffleList(rightConnections);

        // Return all four lists of connection points
        return new List<Vector3Int>[] { downConnections, leftConnections, upConnections, rightConnections };
    }

    public class Room
    {
        public List<Vector3Int> FloorTileCoordinates { get; private set; }
        private Vector3Int position;
        private string type, subType, floorType = "default";
        private int width;
        private int height;

        public List<Vector3Int> ExpandedTiles { get; private set; } = new List<Vector3Int>();
        public bool HasEdgeExpansion => hasEdgeExpansion;
        private bool hasEdgeExpansion = false;

        public Room(List<Vector3Int> floorTileCoordinates)
        {
            FloorTileCoordinates = floorTileCoordinates;
        }

        public Room(List<Vector3Int> floorTileCoordinates, int width, int height)
        {
            FloorTileCoordinates = floorTileCoordinates;
            this.width = width;
            this.height = height;
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

        public string GetSubType()
        {
            return subType;
        }

        public void SetType(string newType)
        {
            type = newType;
        }

        public void SetSubType(string newSubType)
        {
            subType = newSubType;
        }

        public string GetFloorType()
        {
            return floorType;
        }

        public void SetFloorType(string newType)
        {
            floorType = newType;
        }

        public int GetWidth()
        {
            return width;
        }

        public void SetWidth(int value)
        {
            width = value;
        }

        public int GetHeight()
        {
            return height;
        }

        public void SetHeight(int value)
        {
            height = value;
        }

        public List<Vector3Int> FloorTileCoordinatesExcludeEdges()
        {
            HashSet<Vector3Int> floorTileSet = new HashSet<Vector3Int>(FloorTileCoordinates);
            List<Vector3Int> innerTiles = new List<Vector3Int>();

            foreach (var tile in FloorTileCoordinates)
            {
                // Check all four neighbors
                bool hasAllNeighbors = floorTileSet.Contains(tile + Vector3Int.left) &&
                                       floorTileSet.Contains(tile + Vector3Int.right) &&
                                       floorTileSet.Contains(tile + Vector3Int.up) &&
                                       floorTileSet.Contains(tile + Vector3Int.down);

                if (hasAllNeighbors)
                {
                    innerTiles.Add(tile); // Add tile if it has all four neighbors
                }
            }

            return innerTiles;
        }

        public void AddEdgeExpansion(int width, int height)
        {
            var tileSet = new HashSet<Vector3Int>(FloorTileCoordinates);
            List<(Vector3Int anchor, Vector3Int dir)> validEdges = new List<(Vector3Int, Vector3Int)>();

            int maxX = FloorTileCoordinates.Max(t => t.x);
            int maxY = FloorTileCoordinates.Max(t => t.y);
            int minX = FloorTileCoordinates.Min(t => t.x);
            int minY = FloorTileCoordinates.Min(t => t.y);

            // Helper to check if a buffer around the expansion is clear
            bool IsBufferAreaClear(Vector3Int start, int width, int height, Vector3Int direction)
            {
                for (int dx = -2; dx < width + 2; dx++)
                {
                    for (int dy = -2; dy < height + 2; dy++)
                    {
                        // Skip inside the actual expansion area
                        if (dx >= 0 && dx < width && dy >= 0 && dy < height)
                            continue;

                        // Skip side touching the main room (based on expansion direction)
                        if (direction == Vector3Int.down && dy >= height) continue;
                        if (direction == Vector3Int.up && dy < 0) continue;
                        if (direction == Vector3Int.left && dx >= width) continue;
                        if (direction == Vector3Int.right && dx < 0) continue;

                        Vector3Int checkPos = start + new Vector3Int(dx, dy, 0);
                        if (tileSet.Contains(checkPos))
                            return false;
                    }
                }
                return true;
            }

            foreach (var tile in FloorTileCoordinates)
            {
                // Right
                if (tile.x <= maxX - 2 &&
                    tileSet.Contains(tile) &&
                    tileSet.Contains(tile + Vector3Int.right) &&
                    tileSet.Contains(tile + Vector3Int.right * 2) &&
                    !tileSet.Contains(tile + Vector3Int.right * 3))
                {
                    Vector3Int startPos = tile + Vector3Int.right * 3;
                    if (IsBufferAreaClear(startPos, width, height, Vector3Int.right))
                        validEdges.Add((startPos, Vector3Int.right));
                }

                // Left
                if (tile.x >= minX + 2 &&
                    tileSet.Contains(tile) &&
                    tileSet.Contains(tile + Vector3Int.left) &&
                    tileSet.Contains(tile + Vector3Int.left * 2) &&
                    !tileSet.Contains(tile + Vector3Int.left * 3))
                {
                    Vector3Int startPos = tile + Vector3Int.left * 3 - new Vector3Int(width - 1, 0, 0);
                    if (IsBufferAreaClear(startPos, width, height, Vector3Int.left))
                        validEdges.Add((startPos, Vector3Int.left));
                }

                // Up
                if (tile.y <= maxY - 2 &&
                    tileSet.Contains(tile) &&
                    tileSet.Contains(tile + Vector3Int.up) &&
                    tileSet.Contains(tile + Vector3Int.up * 2) &&
                    !tileSet.Contains(tile + Vector3Int.up * 3))
                {
                    Vector3Int startPos = tile + Vector3Int.up * 3;
                    if (IsBufferAreaClear(startPos, width, height, Vector3Int.up))
                        validEdges.Add((startPos, Vector3Int.up));
                }

                // Down
                if (tile.y >= minY + 2 &&
                    tileSet.Contains(tile) &&
                    tileSet.Contains(tile + Vector3Int.down) &&
                    tileSet.Contains(tile + Vector3Int.down * 2) &&
                    !tileSet.Contains(tile + Vector3Int.down * 3))
                {
                    Vector3Int startPos = tile + Vector3Int.down * 3 - new Vector3Int(0, height - 1, 0);
                    if (IsBufferAreaClear(startPos, width, height, Vector3Int.down))
                        validEdges.Add((startPos, Vector3Int.down));
                }
            }

            if (validEdges.Count == 0) return;

            var (startPosFinal, directionFinal) = validEdges[Random.Range(0, validEdges.Count)];

            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    Vector3Int offset = new Vector3Int(dx, dy, 0);
                    Vector3Int tilePos = startPosFinal + offset;

                    if (!tileSet.Contains(tilePos))
                    {
                        FloorTileCoordinates.Add(tilePos);
                        ExpandedTiles.Add(tilePos);
                    }
                }
            }

            hasEdgeExpansion = true;
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

        Room newRoom = new Room(floorTileCoordinates, width, height);
        return newRoom;
    }

    private Room CreateRoomSquareHole(int width, int height)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();

        // Define the minimum and maximum dimensions for the hole
        int minHoleSize = 3;
        int maxHoleWidth = width - 3;
        int maxHoleHeight = height - 3;

        // Determine random dimensions for the hole within allowed range
        int holeWidth = Random.Range(minHoleSize, maxHoleWidth);
        int holeHeight = Random.Range(minHoleSize, maxHoleHeight);

        // Determine a random position for the top-left corner of the hole, ensuring at least a 1-tile border around the hole
        int holeStartX = Random.Range(1, width - holeWidth - 1);
        int holeStartY = Random.Range(1, height - holeHeight - 1);

        // Generate floor tiles, excluding the hole area
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Check if the current tile is within the hole boundaries
                bool isWithinHole = (x >= holeStartX && x < holeStartX + holeWidth) &&
                                    (y >= holeStartY && y < holeStartY + holeHeight);

                // Only add the tile if it is not part of the hole
                if (!isWithinHole)
                {
                    floorTileCoordinates.Add(new Vector3Int(x, y, 0));
                }
            }
        }

        Room newRoom = new Room(floorTileCoordinates);
        return newRoom;
    }

    private Room CreateRoomBelt(int width, int height)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();

        int holeWidth = width - 2;
        int holeHeight = height - 2;
        int holeStartX = 1;
        int holeStartY = 1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWithinHole = (x >= holeStartX && x < holeStartX + holeWidth) &&
                                    (y >= holeStartY && y < holeStartY + holeHeight);

                if (!isWithinHole)
                {
                    floorTileCoordinates.Add(new Vector3Int(x, y, 0));
                }
            }
        }

        Room newRoom = new Room(floorTileCoordinates);
        return newRoom;
    }

    private Room CreateRoomLCorner(int width, int height)
    {
        List<Vector3Int> floorTileCoordinates = new List<Vector3Int>();

        // Define minimum width and height for the main room part after corner removal
        int minRemainingWidth = 2;
        int minRemainingHeight = 2;

        // Ensure room width and height are sufficient for a corner cut
        if (width < minRemainingWidth + 1 || height < minRemainingHeight + 1)
        {
            Debug.LogWarning("Room dimensions too small for an L-corner room. Adjusting to minimum size.");
            width = minRemainingWidth + 1;
            height = minRemainingHeight + 1;
        }

        // Randomly determine the size of the corner to be removed
        int cornerWidth = Random.Range(1, width - minRemainingWidth);
        int cornerHeight = Random.Range(1, height - minRemainingHeight);

        // Randomly choose which corner to remove (0: top-left, 1: top-right, 2: bottom-left, 3: bottom-right)
        int cornerToCut = Random.Range(0, 4);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isCornerCut = false;

                // Determine if the current tile is within the cut-out corner area based on the chosen corner
                switch (cornerToCut)
                {
                    case 0: // Top-left corner
                        isCornerCut = (x < cornerWidth && y >= height - cornerHeight);
                        break;
                    case 1: // Top-right corner
                        isCornerCut = (x >= width - cornerWidth && y >= height - cornerHeight);
                        break;
                    case 2: // Bottom-left corner
                        isCornerCut = (x < cornerWidth && y < cornerHeight);
                        break;
                    case 3: // Bottom-right corner
                        isCornerCut = (x >= width - cornerWidth && y < cornerHeight);
                        break;
                }

                // Only add the tile if it's outside the cut-out corner
                if (!isCornerCut)
                {
                    floorTileCoordinates.Add(new Vector3Int(x, y, 0));
                }
            }
        }

        Room newRoom = new Room(floorTileCoordinates);
        return newRoom;
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
        if (Random.value < chanceRoomFloorCornerBroken)
        {
            List<Vector3Int> allCorners = GetCornersIn(room);

            foreach (Vector3Int tilePos in allCorners)
            {
                if (Random.value < chanceCornerFloorCornerBroken)
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
    }

    public void FillRoomWithBuddha(Room room)
    {
        if (Random.value < chanceRoomBuddha)
        {
            var doorways = GetDoorwaysSides(room);

            // Choose a single random sprite for the entire room
            List<float> probabilities = new List<float> { 0.51f, 0.37f, 0.08f, 0.04f };
            Sprite selectedSprite;

            if (room.GetFloorType() == "LushMixed" || room.GetFloorType() == "LushDark01" || room.GetFloorType() == "LushLight01")
            {
                selectedSprite = BuddhaPrefab01.GetComponent<SpriteScript>().sprites[4];
            }
            else if (room.GetFloorType() == "Shadow01")
            {
                float randomValue = Random.value;
                if (randomValue < 0.78f)
                {
                    selectedSprite = BuddhaPrefab01.GetComponent<SpriteScript>().sprites[5];
                }
                else
                {
                    selectedSprite = BuddhaPrefab01.GetComponent<SpriteScript>().sprites[6];
                }
            }
            else
            {
                selectedSprite = BuddhaPrefab01.GetComponent<SpriteScript>().GetRandomSpriteWithProbabilities(probabilities);
            }

            for (int dir = 0; dir < doorways.Length; dir++)
            {
                foreach (var doorwayTile in doorways[dir])
                {
                    // Decide whether to place Buddhas at this doorway
                    if (Random.value < chanceDoorwayBuddha)
                    {
                        // Get the world position of the doorway tile
                        Vector3Int tilePos = doorwayTile + room.GetPosition();

                        // Check if the position is valid for Buddha placement
                        if (!IsEntityAtPosition(tilePos))
                        {
                            GameObject obj = PlaceObject(tilePos, BuddhaPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                            buddhasInRoom.Add(tilePos);
                        }
                    }
                }
            }
        }
    }
    //[SerializeField] private float chanceRoomPotDoorway;
    //[SerializeField] private float chanceRoomPotEntrance;
    //[SerializeField] private float chanceRoomPotEdges;
    //[SerializeField] private float chanceRoomPotCorners;
    //[SerializeField] private float chanceRoomPotRandom;

    public void FillEdgeExpansionWithPots(Room room)
    {
        if (!room.HasEdgeExpansion || room.ExpandedTiles.Count == 0)
            return;

        Sprite selectedSprite = PotPrefab01.GetComponent<SpriteScript>().GetRandomSpriteWithProbabilities(new List<float> { 1f });

        foreach (var tile in room.ExpandedTiles)
        {
            Vector3Int worldPos = tile + room.GetPosition();
            if (!IsEntityAtPosition(worldPos))
            {
                GameObject obj = PlaceObject(worldPos, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                potsInRoom.Add(worldPos);
            }
        }
    }

    public void FillRoomWithPots(Room room)
    {
        if (Random.value < chanceRoomPot && room.GetSubType() != "library")
        {
            // Choose a single random sprite for the entire room
            List<float> probabilities = new List<float> { 1f };
            Sprite selectedSprite;
            selectedSprite = PotPrefab01.GetComponent<SpriteScript>().GetRandomSpriteWithProbabilities(probabilities);

            var doorways = GetDoorwaysSides(room);
            if (Random.value < chanceRoomPotDoorway)
            {
                doorways = GetDoorwaysSides(room);
                float[] choices = { 0.25f, 0.5f, 1f };
                float chanceDoorwayPot = choices[Random.Range(0, choices.Length)];

                for (int dir = 0; dir < doorways.Length; dir++)
                {
                    foreach (var doorwayTile in doorways[dir])
                    {
                        if (Random.value < chanceDoorwayPot)
                        {
                            Vector3Int tilePos = doorwayTile + room.GetPosition();

                            if (!IsEntityAtPosition(tilePos))
                            {
                                GameObject obj = PlaceObject(tilePos, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                                potsInRoom.Add(tilePos);
                            }
                        }
                    }
                }
            }
            
            if (Random.value < chanceRoomPotEntrance)
            {
                doorways = GetEntrances(room);
                float[] choices = { 1f };
                float chanceEntrancePot = choices[Random.Range(0, choices.Length)];

                for (int dir = 0; dir < doorways.Length; dir++)
                {
                    foreach (var doorwayTile in doorways[dir])
                    {
                        if (Random.value < chanceEntrancePot)
                        {
                            Vector3Int tilePos = doorwayTile + room.GetPosition();

                            if (!IsEntityAtPosition(tilePos))
                            {
                                GameObject obj = PlaceObject(tilePos, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                                potsInRoom.Add(tilePos);
                            }
                        }
                    }
                }
            } 
            
            if (Random.value < chanceRoomPotEdges)
            {
                List<Vector3Int> edges = GetEdgesAndInnerCornersExcludingEntrances(room);
                float[] choices = { 0.25f, 0.5f, 1f };
                float chanceEdgePot = choices[Random.Range(0, choices.Length)];

                foreach (var edgeTile in edges)
                {
                    if (Random.value < chanceEdgePot) // Adjust chance as needed
                    {
                        Vector3Int tilePos = edgeTile + room.GetPosition();
                        if (!IsEntityAtPosition(tilePos))
                        {
                            GameObject obj = PlaceObject(tilePos, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                            potsInRoom.Add(tilePos);
                        }
                    }
                }
            }

            // Place pots in corners
            if (Random.value < chanceRoomPotCorners)
            {
                List<Vector3Int> corners = GetRoomCorners(room);
                foreach (var cornerTile in corners)
                {
                    if (!IsEntityAtPosition(cornerTile))
                    {
                        GameObject obj = PlaceObject(cornerTile, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                        potsInRoom.Add(cornerTile);
                    }
                }
            }

            // Scatter pots randomly in the room
            if (Random.value < chanceRoomPotRandom)
            {
                int tileCount = room.FloorTileCoordinates.Count;
                int minPots = Mathf.Max(1, tileCount / 8); // Ensure at least 1 pot
                int maxPots = Mathf.Max(1, tileCount / 4); // Ensure at least 1 pot
                List<Vector3Int> randomPositions = GetRandomRoomPositions(room, minPots, maxPots);

                foreach (var position in randomPositions)
                {
                    if (!IsEntityAtPosition(position))
                    {
                        GameObject obj = PlaceObject(position, PotPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                        potsInRoom.Add(position);
                    }
                }
            }
        }
    }

    public void FillRoomWithBookstacks(Room room)
    {
        if (Random.value < chanceRoomBookstack)
        {
            int maxBookstacks = Random.Range(2, 8);
            int placedCount = 0;

            // Choose a single random sprite for the entire room
            List<float> probabilities = new List<float> { 1f };
            Sprite selectedSprite = BookstackPrefab01.GetComponent<SpriteScript>().GetRandomSpriteWithProbabilities(probabilities);

            List<Vector3Int> candidatePositions = new List<Vector3Int>();

            // Doorway tiles
            if (Random.value < chanceRoomBookstackDoorway)
            {
                var doorways = GetDoorwaysSides(room);
                foreach (var side in doorways)
                {
                    foreach (var tile in side)
                    {
                        candidatePositions.Add(tile + room.GetPosition());
                    }
                }
            }

            // Corner tiles
            if (Random.value < chanceRoomBookstackCorners)
            {
                List<Vector3Int> corners = GetRoomCorners(room);
                candidatePositions.AddRange(corners);
            }

            // Shuffle doorway + corner positions
            candidatePositions = candidatePositions.OrderBy(_ => Random.value).ToList();

            // Place at doorway/corner positions
            foreach (var pos in candidatePositions)
            {
                if (placedCount >= maxBookstacks) break;

                if (!IsEntityAtPosition(pos))
                {
                    GameObject obj = PlaceObject(pos, BookstackPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                    bookstacksInRoom.Add(pos);
                    placedCount++;
                }
            }

            // Now try random placement if under the max and random chance fulfilled
            if (placedCount < maxBookstacks && Random.value < chanceRoomBookstackRandom)
            {
                int remaining = maxBookstacks - placedCount;
                List<Vector3Int> randomPositions = GetRandomRoomPositions(room, remaining, remaining);

                foreach (var pos in randomPositions)
                {
                    if (placedCount >= maxBookstacks) break;

                    if (!IsEntityAtPosition(pos))
                    {
                        GameObject obj = PlaceObject(pos, BookstackPrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                        bookstacksInRoom.Add(pos);
                        placedCount++;
                    }
                }
            }
        }
    }

    public void FillRoomWithTrees(Room room)
    {
        if (Random.value < chanceRoomTree)
        {
            Vector3Int? freePosition = GetRectanglesInRoomFree(room, 1, 1);
            if (freePosition != null)
            {
                if (!IsEntityAtPosition(freePosition.Value + room.GetPosition()) && !IsEntityAtPosition(freePosition.Value + room.GetPosition() + Vector3Int.up))
                {
                    Sprite selectedCobwebSprite = CobwebPrefab01.GetComponent<SpriteScript>().GetRandomSprite();
                    GameObject tree = PlaceObject(freePosition.Value + room.GetPosition(), TreePrefab01, new Vector3(0.5f, 0.5f, 0));
                    treesInRoom.Add(freePosition.Value + room.GetPosition());
                    //Debug.Log(freePosition.Value + room.GetPosition());
                }
            }
        }
    }

    public void FillRoomWithStones(Room room)
    {
        if (Random.value < chanceRoomStones)
        {
            int totalToPlace = Random.Range(1, 4); // 1 to 3 stones

            if (Random.value < chanceStonesMany)
            {
                totalToPlace += Random.Range(7, 13); // 7 to 12 stones
            }

            if (totalToPlace == 0)
                return;

            List<Vector3Int> randomPositions = GetRandomRoomPositions(room, totalToPlace, totalToPlace);

            foreach (var position in randomPositions)
            {
                if (!IsEntityAtPosition(position))
                {
                    Sprite selectedSprite = StonePrefab01.GetComponent<SpriteScript>().GetRandomSprite(rotate: true);
                    GameObject obj = PlaceObject(position, StonePrefab01, new Vector3(0.5f, 0.5f, 0), selectedSprite);
                    stonesInRoom.Add(position);
                }
            }
        }
    }

    public void FillRoomWithTorches(DungeonGenerationScript01.Room room)
    {
        var edgesUD = GetUpDownEdgeTiles(room);
        var edgesNoDoors = ExcludeDoorwaysFromUpDownEdges(room, edgesUD);

        var downRuns = SplitIntoConsecutiveRunsByRow(edgesNoDoors[0]);
        var upRuns = SplitIntoConsecutiveRunsByRow(edgesNoDoors[1]);

        Vector3 baseOffset = room.GetPosition() + new Vector3(0.5f, 0.5f, 0f);

        // Up pivots (y = 5 px)
        Vector3 torchPivotUpOdd = new Vector3(-0.5f, -2f / 16f, 0f);
        Vector3 torchPivotUpEven = new Vector3(-1.0f, -2f / 16f, 0f);

        // Down pivots (y = 6 px)
        Vector3 torchPivotDownOdd = new Vector3(-0.5f, 0f / 16f, 0f);
        Vector3 torchPivotDownEven = new Vector3(-1.0f, 0f / 16f, 0f);

        // ----- UP wall
        foreach (var run in upRuns)
        {
            int n = run.Count;
            if (n == 0) continue;

            if (n >= 8)
            {
                var first = run[0] + Vector3Int.up;
                var last = run[n - 1] + Vector3Int.up;

                PlaceObject(first, TorchPrefab01, baseOffset + torchPivotUpOdd);
                PlaceObject(last, TorchPrefab01, baseOffset + torchPivotUpOdd);
            }
            else if (n % 2 == 1)
            {
                var mid = run[n / 2] + Vector3Int.up;
                PlaceObject(mid, TorchPrefab01, baseOffset + torchPivotUpOdd);
            }
            else
            {
                var midRight = run[n / 2] + Vector3Int.up;
                PlaceObject(midRight, TorchPrefab01, baseOffset + torchPivotUpEven);
            }
        }

        // ----- DOWN wall
        foreach (var run in downRuns)
        {
            int n = run.Count;
            if (n == 0) continue;

            if (n >= 8)
            {
                var first = run[0] + Vector3Int.down;
                var last = run[n - 1] + Vector3Int.down;

                PlaceObject(first, TorchPrefab01, baseOffset + torchPivotDownOdd);
                PlaceObject(last, TorchPrefab01, baseOffset + torchPivotDownOdd);
            }
            else if (n % 2 == 1)
            {
                var mid = run[n / 2] + Vector3Int.down;
                PlaceObject(mid, TorchPrefab01, baseOffset + torchPivotDownOdd);
            }
            else
            {
                var midRight = run[n / 2] + Vector3Int.down;
                PlaceObject(midRight, TorchPrefab01, baseOffset + torchPivotDownEven);
            }
        }
    }

    // Helper method to get the perpendicular direction for edge placement
    private Vector3Int GetPerpendicularDirection(int dir)
    {
        // Perpendicular directions: Down/Up => Left/Right, Left/Right => Up/Down
        return dir % 2 == 0 ? Vector3Int.right : Vector3Int.up;
    }

    public void FillRoomWithCobweb(Room room)
    {
        if (Random.value < chanceRoomCobweb)
        {
            List<Vector3Int> allCorners = GetCornersIn(room);

            foreach (Vector3Int tilePos in allCorners)
            {
                if (Random.value < chanceCornerCobweb)
                {
                    if (tilemapFloor.GetTile(tilePos) != null)
                    {
                        if (!IsEntityAtPosition(tilePos))
                        {
                            Sprite selectedCobwebSprite = CobwebPrefab01.GetComponent<SpriteScript>().GetRandomSprite(rotate: true);
                            GameObject cobweb = PlaceObject(tilePos, CobwebPrefab01, new Vector3(0.5f, 0.5f, 0), selectedCobwebSprite);
                            cobwebsInRoom.Add(tilePos);
                        }
                    }
                    if (tilemapFloor.GetTile(tilePos + new Vector3Int(1, 0, 0)) != null)
                    {
                        if (!IsEntityAtPosition(tilePos + new Vector3Int(1, 0, 0)))
                        {
                            Sprite selectedCobwebSprite = CobwebPrefab01.GetComponent<SpriteScript>().GetRandomSprite(rotate: true);
                            GameObject cobweb = PlaceObject(tilePos + new Vector3Int(1, 0, 0), CobwebPrefab01, new Vector3(0.5f, 0.5f, 0), selectedCobwebSprite);
                            cobwebsInRoom.Add(tilePos + new Vector3Int(1, 0, 0));
                        }
                    }
                    if (tilemapFloor.GetTile(tilePos + new Vector3Int(0, 1, 0)) != null)
                    {
                        if (!IsEntityAtPosition(tilePos + new Vector3Int(0, 1, 0)))
                        {
                            Sprite selectedCobwebSprite = CobwebPrefab01.GetComponent<SpriteScript>().GetRandomSprite(rotate: true);
                            GameObject cobweb = PlaceObject(tilePos + new Vector3Int(0, 1, 0), CobwebPrefab01, new Vector3(0.5f, 0.5f, 0), selectedCobwebSprite);
                            cobwebsInRoom.Add(tilePos + new Vector3Int(0, 1, 0));
                        }
                    }
                    if (tilemapFloor.GetTile(tilePos + new Vector3Int(1, 1, 0)) != null)
                    {
                        if (!IsEntityAtPosition(tilePos + new Vector3Int(1, 1, 0)))
                        {
                            Sprite selectedCobwebSprite = CobwebPrefab01.GetComponent<SpriteScript>().GetRandomSprite(rotate: true);
                            GameObject cobweb = PlaceObject(tilePos + new Vector3Int(1, 1, 0), CobwebPrefab01, new Vector3(0.5f, 0.5f, 0), selectedCobwebSprite);
                            cobwebsInRoom.Add(tilePos + new Vector3Int(1, 1, 0));
                        }
                    }
                }
            }
        }
    }

    private List<Vector3Int> GetRandomRoomPositions(Room room, int minCount, int maxCount)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>(room.FloorTileCoordinates);
        ShuffleList(validPositions);

        // Ensure we don't exceed the number of available positions
        int potCount = Mathf.Min(validPositions.Count, Random.Range(minCount, maxCount + 1));

        return validPositions.Take(potCount).Select(tile => tile + room.GetPosition()).ToList();
    }

    private List<Vector3Int> GetCornersIn(Room room)
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

        return allCorners;
    }

    private List<Vector3Int> GetRoomEdges(Room room)
    {
        List<Vector3Int> edges = new List<Vector3Int>();
        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);

        foreach (Vector3Int tile in floorTiles)
        {
            int neighborCount = 0;

            // Check if this tile is an edge by counting neighbors
            Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
            foreach (Vector3Int dir in directions)
            {
                if (floorTiles.Contains(tile + dir))
                {
                    neighborCount++;
                }
            }

            // An edge tile should have fewer than 4 neighbors
            if (neighborCount < 4)
            {
                edges.Add(tile);
            }
        }

        return edges;
    }

    private List<Vector3Int> GetRoomUpperEdges(Room room)
    {
        List<Vector3Int> upperEdges = new List<Vector3Int>();
        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);

        foreach (Vector3Int tile in floorTiles)
        {
            int neighborCount = 0;
            Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

            foreach (Vector3Int dir in directions)
            {
                if (floorTiles.Contains(tile + dir))
                {
                    neighborCount++;
                }
            }

            // It's an edge and has no tile directly above
            if (neighborCount < 4 && !floorTiles.Contains(tile + Vector3Int.up))
            {
                upperEdges.Add(tile);
            }
        }

        return upperEdges;
    }

    private List<Vector3Int> GetRoomUpperEdgesExcludingEntrances(Room room)
    {
        var upperEdges = GetRoomUpperEdges(room);
        var entranceEdgeTiles = GetEntrancesAtEdges(room).ToHashSet();

        // Remove entrance tiles from upper edge tiles
        var filteredEdges = upperEdges.Where(tile => !entranceEdgeTiles.Contains(tile)).ToList();
        return filteredEdges;
    }

    private List<Vector3Int> GetRoomCorners(Room room)
    {
        List<Vector3Int> corners = new List<Vector3Int>();
        Vector3Int roomPosition = room.GetPosition();
        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);

        foreach (Vector3Int tile in floorTiles)
        {
            int neighborCount = 0;

            // Check if the tile has two adjacent neighbors
            Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
            foreach (Vector3Int dir in directions)
            {
                if (floorTiles.Contains(tile + dir))
                {
                    neighborCount++;
                }
            }

            // A corner should have exactly 2 neighbors (diagonal)
            if (neighborCount == 2)
            {
                corners.Add(tile + roomPosition);
            }
        }

        return corners;
    }

    private List<Vector3Int>[] GetDoorways(Room room)
    {
        // Initialize lists for each direction
        var doorways = new List<Vector3Int>[4]
        {
        new List<Vector3Int>(), // Down
        new List<Vector3Int>(), // Left
        new List<Vector3Int>(), // Up
        new List<Vector3Int>()  // Right
        };

        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.down,  // Down
        Vector3Int.left,  // Left
        Vector3Int.up,    // Up
        Vector3Int.right  // Right
        };

        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);
        Vector3Int roomPosition = room.GetPosition();

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            // Check each direction to see if the tile is a doorway
            for (int dir = 0; dir < directions.Length; dir++)
            {
                Vector3Int neighbor = tile + directions[dir];
                Vector3Int neighborWorldPos = neighbor + roomPosition;

                // Check if the tile is a doorway (edge of the room and has a neighbor outside the room)
                if (!floorTiles.Contains(neighbor) && tilemapFloor.GetTile(neighborWorldPos) != null)
                {
                    doorways[dir].Add(tile); // Add the doorway tile
                    break; // No need to check further directions for this tile
                }
            }
        }

        return doorways;
    }

    private List<Vector3Int>[] GetDoorwaysUnique(Room room)
    {
        var doorways = GetDoorways(room);
        var uniqueDoorways = new List<Vector3Int>[doorways.Length];

        for (int dir = 0; dir < doorways.Length; dir++)
        {
            // Sort to ensure consecutive order (important if original order isn't guaranteed)
            var sorted = doorways[dir]
                .OrderBy(tile => tile.x)
                .ThenBy(tile => tile.y)
                .ToList();

            uniqueDoorways[dir] = new List<Vector3Int>();

            // For Up/Down, consecutive means consecutive in X (same Y)
            // For Left/Right, consecutive means consecutive in Y (same X)
            bool vertical = (dir == 0 || dir == 2); // Down/Up

            Vector3Int? prev = null;
            foreach (var tile in sorted)
            {
                if (prev == null)
                {
                    uniqueDoorways[dir].Add(tile);
                }
                else
                {
                    bool sameLine = vertical ? (tile.y == prev.Value.y) : (tile.x == prev.Value.x);
                    int diff = vertical ? tile.x - prev.Value.x : tile.y - prev.Value.y;

                    if (!sameLine || diff > 1) // new run starts
                    {
                        uniqueDoorways[dir].Add(tile);
                    }
                }
                prev = tile;
            }
        }

        return uniqueDoorways;
    }

    private List<Vector3Int>[] GetDoorwaysUniqueWidth1(Room room)
    {
        var doorways = GetDoorways(room);
        var result = new List<Vector3Int>[doorways.Length];

        for (int dir = 0; dir < doorways.Length; dir++)
        {
            bool vertical = (dir == 0 || dir == 2); // Down/Up → group along X on same Y
            var sorted = doorways[dir]
                .OrderBy(t => vertical ? t.y : t.x)
                .ThenBy(t => vertical ? t.x : t.y)
                .ToList();

            result[dir] = new List<Vector3Int>();
            Vector3Int? runStart = null;
            int runLen = 0;
            int fixedVal = 0; // y for vertical, x for horizontal

            for (int i = 0; i < sorted.Count; i++)
            {
                var t = sorted[i];
                if (runLen == 0)
                {
                    runStart = t;
                    runLen = 1;
                    fixedVal = vertical ? t.y : t.x;
                }
                else
                {
                    bool sameLine = vertical ? (t.y == fixedVal) : (t.x == fixedVal);
                    int delta = vertical ? (t.x - runStart.Value.x + (runLen - 1)) : (t.y - runStart.Value.y + (runLen - 1));
                    bool consecutive = sameLine && (vertical ? (t.x == runStart.Value.x + runLen) : (t.y == runStart.Value.y + runLen));

                    if (consecutive)
                    {
                        runLen++;
                    }
                    else
                    {
                        if (runLen == 1) result[dir].Add(runStart.Value);
                        runStart = t;
                        runLen = 1;
                        fixedVal = vertical ? t.y : t.x;
                    }
                }
            }
            if (runLen == 1 && runStart.HasValue) result[dir].Add(runStart.Value);
        }

        return result;
    }

    private List<Vector3Int>[] GetDoorwaysUniqueWidthNot1(Room room)
    {
        var startsAll = GetDoorwaysUnique(room);       // starts of every run
        var startsWidth1 = GetDoorwaysUniqueWidth1(room); // starts of runs with length == 1

        var result = new List<Vector3Int>[startsAll.Length];
        for (int dir = 0; dir < startsAll.Length; dir++)
        {
            var setWidth1 = new HashSet<Vector3Int>(startsWidth1[dir]);
            result[dir] = new List<Vector3Int>();
            foreach (var t in startsAll[dir])
                if (!setWidth1.Contains(t)) result[dir].Add(t); // keep only width > 1
        }
        return result;
    }

    //Get further entrance doorway tile
    private List<Vector3Int>[] GetEntrances(Room room)
    {
        // Use GetDoorways to get the doorway tiles
        var doorways = GetDoorways(room);

        // Initialize lists for each direction
        var edgeSides = new List<Vector3Int>[4]
        {
        new List<Vector3Int>(), // Down
        new List<Vector3Int>(), // Left
        new List<Vector3Int>(), // Up
        new List<Vector3Int>()  // Right
        };

        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.down,  // Down
        Vector3Int.left,  // Left
        Vector3Int.up,    // Up
        Vector3Int.right  // Right
        };

        foreach (int dir in Enumerable.Range(0, 4))
        {
            foreach (var doorway in doorways[dir])
            {
                Vector3Int doorwayTile = doorway;

                // Check if it is an edge doorway tile (only one neighbor in its direction or none)
                int neighboringDoorwayCount = 0;
                Vector3Int neighborInDirection = doorwayTile + directions[dir];

                // Count neighbors in the same direction
                if (doorways[dir].Contains(neighborInDirection))
                {
                    neighboringDoorwayCount++;
                }

                // If it's an edge tile (no neighbors or only one neighbor in its direction)
                if (neighboringDoorwayCount <= 1)
                {
                    // Add one perpendicular tile in the opposite direction
                    Vector3Int perpendicularTile = doorwayTile + directions[(dir + 2) % 4];
                    if (tilemapFloor.GetTile(perpendicularTile + room.GetPosition()) != null)
                        edgeSides[dir].Add(perpendicularTile);
                }
            }
        }

        return edgeSides;
    }

    private List<Vector3Int> GetEntrancesAtEdges(Room room)
    {
        // Use GetDoorways to get the doorway tiles
        var doorways = GetDoorways(room);

        // List to hold all edge entrance tiles
        var edgeEntrances = new List<Vector3Int>();

        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.down,  // Down
        Vector3Int.left,  // Left
        Vector3Int.up,    // Up
        Vector3Int.right  // Right
        };

        foreach (int dir in Enumerable.Range(0, 4))
        {
            foreach (var doorway in doorways[dir])
            {
                Vector3Int doorwayTile = doorway;

                // Add the doorway tile itself as the entrance at the edge
                Vector3Int edgeTile = doorwayTile; // Move in the doorway's direction
                if (tilemapFloor.GetTile(edgeTile + room.GetPosition()) != null)
                {
                    edgeEntrances.Add(edgeTile);
                }
            }
        }

        return edgeEntrances;
    }

    private List<Vector3Int> GetInnerCorners(Room room)
    {
        List<Vector3Int> innerCorners = new List<Vector3Int>();
        HashSet<Vector3Int> floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);

        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right
        };

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            // Check for inner corners: must have floor tiles in two perpendicular directions
            foreach (var dir1 in directions)
            {
                foreach (var dir2 in directions)
                {
                    if (dir1 != dir2 && dir1 != -dir2) // Ensure perpendicular directions
                    {
                        Vector3Int neighbor1 = tile + dir1;
                        Vector3Int neighbor2 = tile + dir2;

                        if (floorTiles.Contains(neighbor1) && floorTiles.Contains(neighbor2) &&
                            !floorTiles.Contains(tile + dir1 + dir2)) // Exclude tiles in full adjacency
                        {
                            innerCorners.Add(tile); // Add the world position of the tile
                        }
                    }
                }
            }
        }

        return innerCorners.Distinct().ToList(); // Remove duplicates
    }

    private List<Vector3Int> GetEdgesAndInnerCorners(Room room)
    {
        var edgeTiles = GetRoomEdges(room);
        var innerCornerTiles = GetInnerCorners(room);

        // Combine edge and inner corner tiles
        var combinedTiles = edgeTiles.Union(innerCornerTiles).ToList();
        return combinedTiles;
    }

    private List<Vector3Int> GetEdgesAndInnerCornersExcludingEntrances(Room room)
    {
        var edgeAndCornerTiles = GetEdgesAndInnerCorners(room);
        var entranceEdgeTiles = GetEntrancesAtEdges(room).ToHashSet();

        // Remove entrance edge tiles from the edge and corner tiles
        var result = edgeAndCornerTiles.Where(tile => !entranceEdgeTiles.Contains(tile)).ToList();
        return result;
    }

    private List<Vector3Int>[] GetDoorwaysSides(Room room)
    {
        // Get all doorway tiles
        var doorways = GetDoorways(room);

        // Initialize lists for the edges of doorways
        var edgeSides = new List<Vector3Int>[4]
        {
        new List<Vector3Int>(), // Down
        new List<Vector3Int>(), // Left
        new List<Vector3Int>(), // Up
        new List<Vector3Int>()  // Right
        };

        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.down,  // Down
        Vector3Int.left,  // Left
        Vector3Int.up,    // Up
        Vector3Int.right  // Right
        };

        foreach (int dir in Enumerable.Range(0, 4))
        {
            foreach (var doorwayTile in doorways[dir])
            {
                Vector3Int neighborInDirection = doorwayTile + directions[dir];
                Vector3Int neighborPerpendicular1 = doorwayTile + directions[(dir + 1) % 4]; // First perpendicular direction
                Vector3Int neighborPerpendicular2 = doorwayTile - directions[(dir + 1) % 4]; // Second perpendicular direction

                // Check if this doorway tile has no neighbors in its direction
                bool hasNeighborInDirection = doorways[dir].Contains(neighborInDirection);

                if (!hasNeighborInDirection)
                {
                    // This is an edge tile
                    if (doorways[dir].Contains(neighborPerpendicular1) || doorways[dir].Contains(neighborPerpendicular2))
                    {
                        // If it's part of a larger hallway (at the edge), pick one perpendicular tile
                        if (doorways[dir].Contains(neighborPerpendicular1))
                        {
                            if (tilemapFloor.GetTile(neighborPerpendicular1 + directions[(dir + 1) % 4] + room.GetPosition()) != null)
                                edgeSides[dir].Add(neighborPerpendicular1 + directions[(dir + 1) % 4]); // Further along the edge
                        }
                        else if (doorways[dir].Contains(neighborPerpendicular2))
                        {
                            if (tilemapFloor.GetTile(neighborPerpendicular2 - directions[(dir + 1) % 4] + room.GetPosition()) != null)
                                edgeSides[dir].Add(neighborPerpendicular2 - directions[(dir + 1) % 4]); // Further along the edge
                        }
                    }
                    else
                    {
                        // If it's a single-tile hallway, add both perpendicular tiles
                        if (tilemapFloor.GetTile(neighborPerpendicular1 + room.GetPosition()) != null)
                            edgeSides[dir].Add(neighborPerpendicular1);
                        if (tilemapFloor.GetTile(neighborPerpendicular2 + room.GetPosition()) != null)
                            edgeSides[dir].Add(neighborPerpendicular2);
                    }
                }
            }
        }

        return edgeSides;
    }

    //Get edges code

    // 1) Get all UP/DOWN edge floor tiles (local to the room)
    private List<Vector3Int>[] GetUpDownEdgeTiles(DungeonGenerationScript01.Room room)
    {
        var up = new List<Vector3Int>();
        var down = new List<Vector3Int>();

        var tiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);
        foreach (var t in room.FloorTileCoordinates)
        {
            if (!tiles.Contains(t + Vector3Int.up)) up.Add(t);   // edge to the UP side
            if (!tiles.Contains(t + Vector3Int.down)) down.Add(t); // edge to the DOWN side
        }
        return new List<Vector3Int>[] { down, up }; // [0]=Down, [1]=Up
    }

    // 2) Exclude positions that are doorways (check the WALL cell: tile ± 1 on Y)
    //    Uses your existing IsDoorHorizontalAtPositionAny(...) from this class. :contentReference[oaicite:0]{index=0}
    private List<Vector3Int>[] ExcludeDoorwaysFromUpDownEdges(DungeonGenerationScript01.Room room, List<Vector3Int>[] edges)
    {
        var down = new List<Vector3Int>();
        var up = new List<Vector3Int>();

        // For UP edges, torches go at (tile + (0,+1)); for DOWN edges, at (tile + (0,-1))
        foreach (var t in edges[1]) // Up
        {
            var wallLocal = t + Vector3Int.up;
            var wallWorld = wallLocal + room.GetPosition();
            if (!IsDoorHorizontalAtPositionAny(wallWorld)) up.Add(t);
        }
        foreach (var t in edges[0]) // Down
        {
            var wallLocal = t + Vector3Int.down;
            var wallWorld = wallLocal + room.GetPosition();
            if (!IsDoorHorizontalAtPositionAny(wallWorld)) down.Add(t);
        }
        return new List<Vector3Int>[] { down, up }; // [0]=Down(no doors), [1]=Up(no doors)
    }

    // 3) Split any set of (same-edge) tiles into consecutive runs by X on the same Y
    private List<List<Vector3Int>> SplitIntoConsecutiveRunsByRow(List<Vector3Int> tiles)
    {
        var runs = new List<List<Vector3Int>>();
        var grouped = tiles.GroupBy(t => t.y);
        foreach (var g in grouped)
        {
            var row = g.OrderBy(t => t.x).ToList();
            int i = 0;
            while (i < row.Count)
            {
                int j = i + 1;
                while (j < row.Count && row[j].x == row[j - 1].x + 1) j++;
                runs.Add(row.GetRange(i, j - i));
                i = j;
            }
        }
        return runs;
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
        if (Random.value < chancePlants)
        {
            List<Vector3Int> walkedTilesPlants = RandomWalk(room, 10, 3);
            Sprite selectedPlantSprite = plantPrefab.GetComponent<SpriteScript>().GetRandomSprite();

            if (Random.value < chancePlantAny)
            {
                // Choose a random sprite for each plant
                foreach (Vector3Int pos in walkedTilesPlants)
                {
                    if (Random.value < chancePlantIgnore)
                    {
                        if (!IsEntityAtPosition(pos + room.GetPosition()))
                        {
                            Sprite randomSprite = plantPrefab.GetComponent<SpriteScript>().GetRandomSprite();
                            GameObject plant = PlaceObject(pos, plantPrefab, room.GetPosition() + new Vector3(0.5f, 0.5f, 0), randomSprite);
                            plantsInRoom.Add(pos + room.GetPosition()); // Add each plant to the list
                        }
                    }
                }
            }
            else
            {
                // Use the selectedPlantSprite for all plants in this room
                foreach (Vector3Int pos in walkedTilesPlants)
                {
                    if (Random.value < chancePlantIgnore)
                    {
                        if (!IsEntityAtPosition(pos + room.GetPosition()))
                        {
                            GameObject plant = PlaceObject(pos, plantPrefab, room.GetPosition() + new Vector3(0.5f, 0.5f, 0), selectedPlantSprite);
                            plantsInRoom.Add(pos + room.GetPosition()); // Add each plant to the list
                        }
                    }
                }
            }
        }
    }

    private GameObject FillRegionWithObject(List<Vector3Int> region, GameObject objectToPlace, Vector3Int offset, Sprite selectedSprite)
    {
        foreach (Vector3Int pos in region)
        {
            GameObject instance = Instantiate(objectToPlace, pos + offset + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            instance.GetComponent<SpriteRenderer>().sprite = selectedSprite;
            return instance; // Return the instantiated object
        }
        return null;
    }

    private void FillRoomWithFloorBig(Room room, Tile tileToPlace, float chance)
    {
        if (Random.value < chance)
        {
            if (tileToPlace == tileFloorBigUpLeft && room.GetFloorType() != "fullBroken" && room.GetFloorType() != "fullNoBroken" && room.GetFloorType() != "fullFour01")
            {
                if (Random.value < chanceRoomFloorBigFull && room.GetType() == "square" && room.GetWidth() % 2 == 0 && room.GetHeight() % 2 == 0)
                {
                    Vector3Int offset = room.GetPosition();

                    for (int x = 0; x < room.GetWidth(); x += 2) // Step 2 for width
                    {
                        for (int y = 0; y < room.GetHeight(); y += 2) // Step 2 for height
                        {
                            Vector3Int pos = offset + new Vector3Int(x, y, 0);

                            tilemapFloor.SetTile(pos, tileFloorBigDownLeft);
                            tilemapFloor.SetTile(pos + Vector3Int.right, tileFloorBigDownRight);
                            tilemapFloor.SetTile(pos + Vector3Int.up, tileFloorBigUpLeft);
                            tilemapFloor.SetTile(pos + Vector3Int.right + Vector3Int.up, tileFloorBigUpRight);
                        }
                    }
                } else
                {
                    var validPositions = GetRectanglesInRoomFloorValid(room, 2, 2);
                    if (validPositions.Count == 0) return;
                    Vector3Int offset = room.GetPosition();

                    int floorTileCount = room.FloorTileCoordinates.Count;
                    int numBigTiles = floorTileCount > 100
                        ? Random.Range(3, 10)  // Between 3 and 8
                        : (floorTileCount > 64
                            ? Random.Range(2, 5)  // Between 2 and 4
                            : Random.Range(1, 3)); // Between 1 and 2

                    for (int i = 0; i < numBigTiles && validPositions.Count > 0; i++)
                    {
                        Vector3Int pos = validPositions[Random.Range(0, validPositions.Count)] + offset;

                        tilemapFloor.SetTile(pos, tileFloorBigDownLeft);
                        tilemapFloor.SetTile(pos + Vector3Int.right, tileFloorBigDownRight);
                        tilemapFloor.SetTile(pos + Vector3Int.up, tileFloorBigUpLeft);
                        tilemapFloor.SetTile(pos + Vector3Int.right + Vector3Int.up, tileFloorBigUpRight);

                        validPositions.Remove(pos); // Remove used position to avoid overlap
                        validPositions.Remove(pos + Vector3Int.right - offset);
                        validPositions.Remove(pos + Vector3Int.up - offset);
                        validPositions.Remove(pos + Vector3Int.right + Vector3Int.up - offset);

                        validPositions.Remove(pos + Vector3Int.left - offset);
                        validPositions.Remove(pos + Vector3Int.down - offset);
                        validPositions.Remove(pos + Vector3Int.left + Vector3Int.down - offset);
                        validPositions.Remove(pos + Vector3Int.left + Vector3Int.up - offset);
                        validPositions.Remove(pos + Vector3Int.down + Vector3Int.right - offset);
                    }
                }
            }
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

    private void FillRoomWithFloorFull(Room room, Tile tileToPlace, float chance)
    {
        if (Random.value < chance)
        {
            if (tileToPlace == tileFloorFour01)
            {
                bool noBroken = false, fullBroken = false;

                if (Random.value < chanceRoomFloorFourFullBroken)
                {
                    fullBroken = true;
                    room.SetFloorType("fullBroken");
                }
                else if (Random.value < chanceRoomFloorFourFullNoBroken)
                {
                    noBroken = true;
                    room.SetFloorType("fullNoBroken");
                }
                else
                {
                    room.SetFloorType("fullFour01");
                }

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
            else if (tileToPlace == tileFloorShadow01 && room.GetSubType() != "library")
            {
                room.SetFloorType("Shadow01");
                List<Vector3Int> fullTilesFloorFour = room.FloorTileCoordinates;
                Vector3Int offset = room.GetPosition();

                foreach (Vector3Int pos in fullTilesFloorFour)
                {
                    tilemapFloor.SetTile(pos + offset, tileFloorShadow01);
                }
            }
            else if (tileToPlace == tileFloorLushDark01 && room.GetSubType() != "library")
            {
                Tile tileFloorLush01 = tileFloorLushDark01;
                room.SetFloorType("LushDark01");
                if (Random.value < chanceLushLight)
                {
                    tileFloorLush01 = tileFloorLushLight01;
                    room.SetFloorType("LushLight01");
                }

                bool lushMixed = false;
                float lushMixedDistribution = Random.Range(0.33f, 0.67f);

                if (Random.value < chanceLushMixed)
                {
                    lushMixed = true;
                    room.SetFloorType("LushMixed");
                }

                List<Vector3Int> fullTilesFloorFour = room.FloorTileCoordinates;
                Vector3Int offset = room.GetPosition(); 

                foreach (Vector3Int pos in fullTilesFloorFour)
                {
                    if (lushMixed)
                    {
                        if (Random.value < lushMixedDistribution)
                        {
                            tileFloorLush01 = tileFloorLushDark01;
                        } else
                        {
                            tileFloorLush01 = tileFloorLushLight01;
                        }
                    }

                    tilemapFloor.SetTile(pos + offset, tileFloorLush01);
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
                    if (!IsEntityAtPosition(freePosition.Value + room.GetPosition()) && !IsEntityAtPosition(freePosition.Value + room.GetPosition() + Vector3Int.up))
                    {
                        float spriteHeightInUnits = 32f / 16f;
                        float pivotYPercentage = 0.9f;
                        float yOffset = spriteHeightInUnits * pivotYPercentage;
                        Vector3 pivotOffset = new Vector3(0f, yOffset, 0f);

                        Sprite selectedTable1x2Sprite = Table1x2Prefab01.GetComponent<SpriteScript>().GetRandomSprite();
                        GameObject table1x2 = PlaceObject(freePosition.Value, Table1x2Prefab01, room.GetPosition() + pivotOffset, selectedTable1x2Sprite);
                        tables1x2InRoom.Add(freePosition.Value + room.GetPosition());

                        //RemovePlantAtPosition(freePosition.Value + room.GetPosition());
                        //RemovePlantAtPosition(new Vector3Int(0, 1, 0) + freePosition.Value + room.GetPosition());
                    }
                }
            }
            else
            {
                Vector3Int? freePosition = GetRectanglesInRoomFree(room, 2, 2);
                if (freePosition != null)
                {
                    if (!IsEntityAtPosition(freePosition.Value + room.GetPosition()) && !IsEntityAtPosition(freePosition.Value + room.GetPosition() + Vector3Int.up) && !IsEntityAtPosition(freePosition.Value + room.GetPosition() + Vector3Int.right) && !IsEntityAtPosition(freePosition.Value + room.GetPosition() + Vector3Int.up + Vector3Int.right))
                    {
                        float spriteHeightInUnits = 37f / 16f;
                        float pivotYPercentage = 0.9f;
                        float yOffset = spriteHeightInUnits * pivotYPercentage;
                        Vector3 pivotOffset = new Vector3(0f, yOffset, 0f);

                        GameObject table2x2 = PlaceObject(freePosition.Value, Table2x2Prefab01, room.GetPosition() + pivotOffset);

                        tables2x2InRoom.Add(freePosition.Value + room.GetPosition());

                        //RemovePlantAtPosition(freePosition.Value + room.GetPosition());
                        //RemovePlantAtPosition(new Vector3Int(0, 1, 0) + freePosition.Value + room.GetPosition());
                        //RemovePlantAtPosition(new Vector3Int(1, 0, 0) + freePosition.Value + room.GetPosition());
                        //RemovePlantAtPosition(new Vector3Int(1, 1, 0) + freePosition.Value + room.GetPosition());
                    }
                }
            }
        }
    }

    private void FillRoomWithDoors(Room room)
    {
        var doorStartsWgt = GetDoorwaysUniqueWidthNot1(room); // width > 1 → Prefab01
        var doorStartsW1 = GetDoorwaysUniqueWidth1(room);     // width == 1 → Prefab02

        int[] verticalDirs = { 0, 2 }; // Down = 0, Up = 2
        foreach (int dir in verticalDirs)
        {
            Vector3 pivotOffset = (dir == 2) ? new Vector3(-1f, 1.5f, 0f)
                                             : new Vector3(-1f, -0.5f, 0f);
            // Shift pivot down by 4 px at PPU=16
            pivotOffset.y -= 4 / 16f;

            // Place multi-width doors (Prefab01)
            foreach (var tile in doorStartsWgt[dir])
            {
                var doorObj = PlaceObject(tile, dir == 2 ? DoorDownPrefab01 : DoorUpPrefab01, room.GetPosition() + pivotOffset);
                Vector3Int adjusted = tile; adjusted.y += (dir == 2) ? 1 : -1;
                doorsHorizontalInRoom.Add(adjusted + room.GetPosition());
            }

            // Place 1-width doors (Prefab02)
            foreach (var tile in doorStartsW1[dir])
            {
                var doorObj = PlaceObject(tile, dir == 2 ? DoorUpPrefab02 : DoorDownPrefab02, room.GetPosition() + pivotOffset);
                Vector3Int adjusted = tile; adjusted.y += (dir == 2) ? 1 : -1;
                doorsHorizontalInRoomWidth1.Add(adjusted + room.GetPosition());
            }
        }

        // ----- Left/Right (new) -----
        int[] horizontalDirs = { 1, 3 }; // Left = 1, Right = 3
        foreach (int dir in horizontalDirs)
        {
            Vector3 pivotOffset = (dir == 3) ? new Vector3(1f, 2f, 0f)   // Right
                                             : new Vector3(0f, 2f, 0f);  // Left

            foreach (var tile in doorStartsWgt[dir])
            {
                var prefab = (dir == 3) ? DoorRightPrefab01 : DoorLeftPrefab01;
                var doorObj = PlaceObject(tile, prefab, room.GetPosition() + pivotOffset);

                Vector3Int adjusted = tile;
                adjusted.x += (dir == 3) ? 1 : -1;
                doorsVerticalInRoom.Add(adjusted + room.GetPosition());

                // GameObject cobweb = PlaceObject(adjusted + room.GetPosition(), CobwebPrefab01, new Vector3(0.5f, 0.5f, 0));
            }
        }

        /*
        // 1-width vertical doors (reuse Right prefab; flip for Left)
        foreach (var tile in doorStartsW1[dir])
        {
            var doorObj = PlaceObject(tile, DoorRightPrefab01, room.GetPosition() + pivotOffset);
            var sr = doorObj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = (dir == 1);

            Vector3Int adjusted = tile; adjusted.x += (dir == 3) ? 1 : -1;
            doorsVerticalInRoomWidth1.Add(adjusted + room.GetPosition());
        }
        */
    }

    private void FillRoomWithBookshelves(Room room)
    {
        if (room.GetSubType() == "library")
        {
            List<Vector3Int> edges = GetRoomUpperEdgesExcludingEntrances(room); // Only upper edges

            foreach (var edgeTile in edges)
            {
                Vector3Int worldPos = edgeTile + room.GetPosition();
                Vector3Int worldPosAbove = worldPos + Vector3Int.up;

                // Check if both positions are free (since bookshelf is 1x2)
                if (!IsEntityAtPosition(worldPos) && !IsEntityAtPosition(worldPosAbove))
                {
                    float spriteHeightInUnits = 29f / 16f;
                    float pivotYPercentage = 0.5862069f; //DACA e CU MINUS SE FACE 1 + VALOARE
                    float yOffset = spriteHeightInUnits * pivotYPercentage;
                    Vector3 pivotOffset = new Vector3(0f, yOffset, 0f);

                    Sprite spriteSelected = BookshelfSmallPrefab01.GetComponent<SpriteScript>().GetRandomSprite();
                    GameObject bookshelf = PlaceObject(edgeTile, BookshelfSmallPrefab01, room.GetPosition() + pivotOffset, spriteSelected);
                    bookshelvesInRoom.Add(worldPos);
                }
            }

            FillRoomWithBookstacks(room);
        }
    }

    private void FillRoomWithEnemies(Room room)
    {
        if (room.GetType() != "start")
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
    }

    private void FillRoomWithEnemy(Room room, GameObject EnemyPrefab, int numberOfPlacements)
    {
        if (room.GetType() != "start")
        {
            for (int i = 0; i < numberOfPlacements; i++)
            {
                Vector3Int? freePosition = GetRectanglesInRoomFree(room, 1, 2);
                if (freePosition != null)
                {
                    float spriteHeightInUnits = 24f / 16f;
                    float pivotYPercentage = 1f;
                    float yOffset = spriteHeightInUnits * pivotYPercentage;
                    Vector3 pivotOffset = new Vector3(0.5f, yOffset, 0f);

                    PlaceObject(freePosition.Value, EnemyPrefab, room.GetPosition() + pivotOffset);
                }
            }
        }
    }

    private (Vector3Int start, int width, int height) FindBiggestRectangle(Room room, bool full = true)
    {
        HashSet<Vector3Int> floorTiles;
        if (full)
        {
            floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinates);
        }
        else
        {
            floorTiles = new HashSet<Vector3Int>(room.FloorTileCoordinatesExcludeEdges());
        }

        Dictionary<Vector3Int, int> heights = new Dictionary<Vector3Int, int>();

        int bestWidth = 0;
        int bestHeight = 0;
        Vector3Int bestStart = Vector3Int.zero;

        // Initialize heights for all potential columns in the room's bounding box
        foreach (Vector3Int tile in floorTiles)
        {
            heights[tile] = 0;
        }

        int roomMinY = room.FloorTileCoordinates.Min(t => t.y);
        int roomMaxY = room.FloorTileCoordinates.Max(t => t.y);
        int roomMinX = room.FloorTileCoordinates.Min(t => t.x);
        int roomMaxX = room.FloorTileCoordinates.Max(t => t.x);

        for (int y = roomMinY; y <= roomMaxY; y++)
        {
            // Update the histogram heights for each column in this row
            for (int x = roomMinX; x <= roomMaxX; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (floorTiles.Contains(pos))
                {
                    heights[pos] = heights.ContainsKey(new Vector3Int(x, y - 1, 0)) ? heights[new Vector3Int(x, y - 1, 0)] + 1 : 1;
                }
                else
                {
                    heights[pos] = 0; // Reset height for non-floor tiles
                }
            }

            // Use a stack to find the largest rectangle for the current row's histogram
            Stack<Vector3Int> stack = new Stack<Vector3Int>();
            foreach (int x in Enumerable.Range(roomMinX, roomMaxX - roomMinX + 2)) // Adding a sentinel at the end
            {
                int h = (x <= roomMaxX && heights.ContainsKey(new Vector3Int(x, y, 0))) ? heights[new Vector3Int(x, y, 0)] : 0;
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Calculate area for rectangles in the histogram
                while (stack.Count > 0 && heights[stack.Peek()] >= h)
                {
                    Vector3Int top = stack.Pop();
                    int height = heights[top];
                    int width = stack.Count == 0 ? x - roomMinX : x - stack.Peek().x - 1;

                    Vector3Int candidateStart = new Vector3Int(top.x - width + 1, top.y - height + 1, 0);

                    // Validation check: Ensure all tiles in the candidate rectangle are floor tiles
                    bool isValid = true;
                    for (int dx = 0; dx < width && isValid; dx++)
                    {
                        for (int dy = 0; dy < height && isValid; dy++)
                        {
                            Vector3Int checkPos = candidateStart + new Vector3Int(dx, dy, 0);
                            if (!floorTiles.Contains(checkPos))
                            {
                                isValid = false;
                            }
                        }
                    }

                    if (isValid)
                    {
                        int area = width * height;
                        if (area > bestWidth * bestHeight)
                        {
                            bestWidth = width;
                            bestHeight = height;
                            bestStart = candidateStart;
                        }
                    }
                }
                stack.Push(pos);
            }
        }

        return (bestStart, bestWidth, bestHeight);
    }

    // Method to fill the largest rectangle area within a room with carpet tiles
    private void FillRoomWithCarpet(Room room)
    {
        if (Random.value < chanceCarpet)
        {
            Vector3Int start;
            int width = 0;
            int height = 0;

            if (room.GetType() == "square")
            {
                if (Random.value < chanceCarpetFull)
                {
                    (start, width, height) = FindBiggestRectangle(room);
                } else
                {
                    (start, width, height) = FindBiggestRectangle(room, false);
                }
            }
            else
            {
                (start, width, height) = FindBiggestRectangle(room, false);
            }

            // Only fill if both dimensions are greater than 1
            if (width > 1 && height > 1)
            {
                // Choose between carpet types 01 and 02 based on 50% chance
                bool useType01 = Random.value < 0.5f;

                Tile cornerUpLeft = useType01 ? tileCarpet01CornerUpLeft : tileCarpet02CornerUpLeft;
                Tile cornerUpRight = useType01 ? tileCarpet01CornerUpRight : tileCarpet02CornerUpRight;
                Tile cornerDownLeft = useType01 ? tileCarpet01CornerDownLeft : tileCarpet02CornerDownLeft;
                Tile cornerDownRight = useType01 ? tileCarpet01CornerDownRight : tileCarpet02CornerDownRight;

                Tile upTile = useType01 ? tileCarpet01Up : tileCarpet02Up;
                Tile downTile = useType01 ? tileCarpet01Down : tileCarpet02Down;
                Tile leftTile = useType01 ? tileCarpet01Left : tileCarpet02Left;
                Tile rightTile = useType01 ? tileCarpet01Right : tileCarpet02Right;

                Tile fullTile = useType01 ? tileCarpet01Full : tileCarpet02Full;

                Vector3Int offset = room.GetPosition();

                // Place the tiles for the carpet
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Vector3Int pos = start + new Vector3Int(x, y, 0) + offset;

                        // Remove any plant that exists at this position
                        RemovePlantAtPosition(pos);

                        // Determine tile type based on position
                        if (x == 0 && y == 0) tilemapFloor.SetTile(pos, cornerDownLeft);
                        else if (x == 0 && y == height - 1) tilemapFloor.SetTile(pos, cornerUpLeft);
                        else if (x == width - 1 && y == 0) tilemapFloor.SetTile(pos, cornerDownRight);
                        else if (x == width - 1 && y == height - 1) tilemapFloor.SetTile(pos, cornerUpRight);
                        else if (x == 0) tilemapFloor.SetTile(pos, leftTile);
                        else if (x == width - 1) tilemapFloor.SetTile(pos, rightTile);
                        else if (y == 0) tilemapFloor.SetTile(pos, downTile);
                        else if (y == height - 1) tilemapFloor.SetTile(pos, upTile);
                        else tilemapFloor.SetTile(pos, fullTile);
                    }
                }
            }
        }
    }

    private void RemovePlantAtPosition(Vector3Int position)
    {
        // Convert the position to world space by adding offset for center alignment
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        // Find the GameObject at the given position using a tag or layer filter, if applicable
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
        if (hitCollider != null && hitCollider.CompareTag("Plant")) // Assuming the plant has a "Plant" tag
        {
            Destroy(hitCollider.gameObject); // Remove the GameObject from the scene
        }

        // Finally, remove the position from the plantsInRoom list
        plantsInRoom.Remove(position);
    }

    public void RemoveCobwebAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int cobwebLayer = LayerMask.GetMask("Cobweb"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, cobwebLayer);

        if (hitCollider != null && hitCollider.CompareTag("Cobweb"))
        {
            Destroy(hitCollider.gameObject);
        }

        cobwebsInRoom.Remove(position);
    }

    public void RemoveDoorHorizontalAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int doorLayer = LayerMask.GetMask("Door"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, doorLayer);

        doorsHorizontalInRoom.Remove(position);
        doorsHorizontalInRoom.Remove(position + new Vector3Int(-1, 0, 0));
        doorsHorizontalInRoomWidth1.Remove(position);
    }

    public void RemoveDoorVerticalAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int doorLayer = LayerMask.GetMask("Door"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, doorLayer);

        doorsVerticalInRoom.Remove(position);
        doorsVerticalInRoom.Remove(position + new Vector3Int(0, -1, 0));
        doorsVerticalInRoomWidth1.Remove(position);
    }

    public GameObject IdentifyDoorAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int doorLayer = LayerMask.GetMask("Door"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, doorLayer);

        if (hitCollider != null && hitCollider.CompareTag("Door"))
        {
            return hitCollider.gameObject;
        }
        else
        {
            return null;
        }
    }

    public void HitDoorAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int doorLayer = LayerMask.GetMask("Door"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, doorLayer);

        if (hitCollider != null && hitCollider.CompareTag("Door"))
        {
            hitCollider.gameObject.GetComponent<DoorScript>().DecreaseHp();

            if (hitCollider.gameObject.GetComponent<DoorScript>().GetHp() == -1)
            {
                RemoveDoorHorizontalAtPosition(position);
                RemoveDoorVerticalAtPosition(position);
            }
        }
    }

    public int GetDoorAtPositionHp(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int doorLayer = LayerMask.GetMask("Door"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, doorLayer);

        if (hitCollider != null && hitCollider.CompareTag("Door"))
        {
            return hitCollider.gameObject.GetComponent<DoorScript>().GetHp();
        }
        return -1;
    }

    public void RemoveTreeAtPosition(Vector3Int position)
    {
        Vector3 worldPosition = position + new Vector3(0.5f, 0.5f, 0);

        float detectionRadius = 0.3f; // Small radius for detection
        int treeLayer = LayerMask.GetMask("Tree"); // Make sure the cobweb is assigned to this layer
        Collider2D hitCollider = Physics2D.OverlapCircle(worldPosition, detectionRadius, treeLayer);

        if (hitCollider != null && hitCollider.CompareTag("Tree"))
        {
            Destroy(hitCollider.gameObject);
        }

        treesInRoom.Remove(position);
    }

    private bool IsPlantAtPosition(Vector3Int position)
    {
        return plantsInRoom.Contains(position);
    }

    public bool IsCobwebAtPosition(Vector3Int position)
    {
        return cobwebsInRoom.Contains(position);
    }

    private bool IsTreeAtPosition(Vector3Int position)
    {
        return treesInRoom.Contains(position);
    }

    private bool IsBuddhaAtPosition(Vector3Int position)
    {
        return buddhasInRoom.Contains(position);
    }

    private bool IsPotAtPosition(Vector3Int position)
    {
        return potsInRoom.Contains(position);
    }

    private bool IsStoneAtPosition(Vector3Int position)
    {
        return stonesInRoom.Contains(position);
    }

    private bool IsTable1x2AtPosition(Vector3Int position)
    {
        foreach (var tablePos in tables1x2InRoom)
        {
            if (tablePos == position)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsTable1x2AtPositionAny(Vector3Int position)
    {
        Vector3Int down = position + Vector3Int.down;

        foreach (var tablePos in tables1x2InRoom)
        {
            if (tablePos == position || tablePos == down)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBookshelfSmallAtPosition(Vector3Int position)
    {
        foreach (var bookshelfPos in bookshelvesInRoom)
        {
            if (bookshelfPos == position)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsBookshelfSmallAtPositionAny(Vector3Int position)
    {
        Vector3Int down = position + Vector3Int.down;

        foreach (var bookshelfPos in bookshelvesInRoom)
        {
            if (bookshelfPos == position || bookshelfPos == down)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsBookstackAtPosition(Vector3Int position)
    {
        foreach (var Pos in bookstacksInRoom)
        {
            if (Pos == position)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsTable2x2AtPosition(Vector3Int position)
    {
        foreach (var tablePos in tables2x2InRoom)
        {
            if (tablePos == position)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsTable2x2AtPositionAny(Vector3Int position)
    {
        Vector3Int down = position + Vector3Int.down;
        Vector3Int left = position + Vector3Int.left;
        Vector3Int downLeft = position + Vector3Int.down + Vector3Int.left;

        foreach (var tablePos in tables2x2InRoom)
        {
            if (tablePos == position || tablePos == down || tablePos == left || tablePos == downLeft)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsDoorHorizontalAtPosition(Vector3Int position)
    {
        foreach (var doorPos in doorsHorizontalInRoom)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        foreach (var doorPos in doorsHorizontalInRoomWidth1)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDoorHorizontalAtPositionAny(Vector3Int position)
    {
        Vector3Int left = position + Vector3Int.left;

        foreach (var doorPos in doorsHorizontalInRoom)
        {
            if (doorPos == position || doorPos == left)
            {
                return true;
            }
        }

        foreach (var doorPos in doorsHorizontalInRoomWidth1)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDoorVerticalAtPosition(Vector3Int position)
    {
        foreach (var doorPos in doorsVerticalInRoom)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        foreach (var doorPos in doorsVerticalInRoomWidth1)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDoorVerticalAtPositionAny(Vector3Int position)
    {
        Vector3Int down = position + Vector3Int.down;

        foreach (var doorPos in doorsVerticalInRoom)
        {
            if (doorPos == position || doorPos == down)
            {
                return true;
            }
        }

        foreach (var doorPos in doorsVerticalInRoomWidth1)
        {
            if (doorPos == position)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsDoorAtPositionAny(Vector3Int position)
    {
        return (IsDoorHorizontalAtPositionAny(position) || IsDoorVerticalAtPositionAny(position));
    }

    public bool IsSolidAtPosition(Vector3Int position, bool breakDoors = false)
    {
        if (breakDoors)
        {
            return IsTreeAtPosition(position) || (IsDoorAtPositionAny(position) && GetDoorAtPositionHp(position) > 0);
        }

        return IsTreeAtPosition(position) || IsDoorHorizontalAtPositionAny(position) || IsDoorVerticalAtPositionAny(position);
    }

    private bool IsEntityAtPosition(Vector3Int position)
    {
        Vector3Int down = position + Vector3Int.down;
        Vector3Int left = position + Vector3Int.left;
        Vector3Int downLeft = position + Vector3Int.down + Vector3Int.left;

        if (IsPlantAtPosition(position))
        {
            return true;
        }

        if (IsCobwebAtPosition(position))
        {
            return true;
        }

        if (IsTreeAtPosition(position))
        {
            return true;
        }

        if (IsBuddhaAtPosition(position))
        {
            return true;
        }

        if (IsPotAtPosition(position))
        {
            return true;
        }

        if (IsBookstackAtPosition(position))
        {
            return true;
        }

        if (IsTable1x2AtPositionAny(position))
        {
            return true;
        }

        if (IsBookshelfSmallAtPositionAny(position))
        {
            return true;
        }

        if (IsTable2x2AtPositionAny(position))
        {
            return true;
        }

        if (IsStoneAtPosition(position))
        {
            return true;
        }

        return false;
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

    private List<Vector3Int> GetRectanglesInRoomFloorValid(Room room, int width, int height)
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        Vector3Int offset = room.GetPosition();

        foreach (Vector3Int tile in room.FloorTileCoordinates)
        {
            bool isValid = true;

            for (int x = 0; x < width && isValid; x++)
            {
                for (int y = 0; y < height && isValid; y++)
                {
                    Vector3Int checkPos = new Vector3Int(tile.x + x, tile.y + y, tile.z);

                    bool isInvalidTile = new Tile[] { tileFloorBigDownLeft, tileFloorBigDownRight, tileFloorBigUpLeft, tileFloorBigUpRight }
                        .Contains(tilemapFloor.GetTile(checkPos + offset)) ||
                        new Tile[] { tileFloorBigDownLeft, tileFloorBigDownRight, tileFloorBigUpLeft, tileFloorBigUpRight }
                        .Contains(tilemapFloor.GetTile(checkPos + offset + Vector3Int.right)) ||
                        new Tile[] { tileFloorBigDownLeft, tileFloorBigDownRight, tileFloorBigUpLeft, tileFloorBigUpRight }
                        .Contains(tilemapFloor.GetTile(checkPos + offset + Vector3Int.up)) ||
                        new Tile[] { tileFloorBigDownLeft, tileFloorBigDownRight, tileFloorBigUpLeft, tileFloorBigUpRight }
                        .Contains(tilemapFloor.GetTile(checkPos + offset + Vector3Int.right + Vector3Int.up));

                    if (!room.FloorTileCoordinates.Contains(checkPos) || isInvalidTile)
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

    private GameObject PlaceObject(Vector3Int position, GameObject objectToPlace, Vector3 offset, Sprite selectedSprite = null)
    {
        GameObject instance = Instantiate(objectToPlace, position + offset, Quaternion.identity);

        if (selectedSprite != null)
        {
            instance.GetComponent<SpriteRenderer>().sprite = selectedSprite;
        }

        return instance;
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
        Time.fixedDeltaTime = 0.003f;

        //Dungeon Level Chances
        if (Random.value < chanceDungeonFloorNew)
        {
            chanceAnyTileFloors = 0;
        }
        else
        {
            chanceAnyTileFloors = Random.Range(chanceAnyTileFloorsMin, chanceAnyTileFloorsMax);
        }

        if (Random.value < chanceRegionEmeraldDungeon)
        {
            regionEmeraldDungeon = true;
        }

        Vector3Int pos01 = new Vector3Int(0, 0, 0);

        Room initialRoom = CreateRoomSquare(6, 6);
        InstantiateRoom(initialRoom, pos01);
        initialRoom.SetType("start");

        BuildRooms();
        foreach (Room room in rooms)
        {
            FillRoomWithDoors(room);
            FillRoomWithTorches(room);

            if (!roomTemplate(room))
            {
                FillRoomWithFloor(room, tileFloorFour01);
                AddFloorCornerBroken(room);
                FillRoomWithFloorFull(room, tileFloorFour01, chanceRoomFloorFourFull);
                FillRoomWithFloorBig(room, tileFloorBigUpLeft, chanceRoomFloorBig);

                FillRoomWithFloorFull(room, tileFloorShadow01, chanceRoomFloorShadow);
                FillRoomWithFloorFull(room, tileFloorLushDark01, chanceRoomFloorLush);

                FillRoomWithBookshelves(room);
                FillRoomWithBuddha(room);
                FillRoomWithPots(room);
                FillRoomWithPlants(room, PlantPrefab01, chancePlantAny);
                FillRoomWithTables(room);
                FillRoomWithCobweb(room);
                FillRoomWithTrees(room);
                FillRoomWithStones(room);

                FillRoomWithCarpet(room);

                FillRoomWithEnemies(room);
            } else
            {
                FillRoomWithFloorChess(room);
                FillRoomWithEnemy(room, EnemyKnightPrefab, 2);
            }
        }

        BakeTilemaps();
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
                else if (Random.value < chanceRoomBelt)
                {
                    type = "belt";

                    int randomWidth = Random.Range(6, 13);
                    int randomHeight = Random.Range(6, 13);

                    room1 = CreateRoomBelt(randomWidth, randomHeight);
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
                else if (Random.value < chanceRoomSquareHole)
                {
                    type = "square_hole";

                    int randomWidth = Random.Range(6, 14);
                    int randomHeight = Random.Range(6, 14);

                    room1 = CreateRoomSquareHole(randomWidth, randomHeight);
                }
                else
                {
                    type = "irregular";
                    room1 = CreateRoomIrregular(6, 8, 2);
                }
            }
            else if (Random.value < chanceRoomLCorner)
            {
                type = "Lcorner";

                int randomWidth = Random.Range(6, 14);
                int randomHeight = Random.Range(6, 14);

                room1 = CreateRoomLCorner(randomWidth, randomHeight);
            }
            else
            {
                int randomWidth = Random.Range(5, 9);
                int randomHeight = Random.Range(5, 9);
                room1 = CreateRoomSquare(randomWidth, randomHeight);
            }

            string subType = "normal";

            if (!roomTemplate(room1))
            {
                if (Random.value < chanceRoomBookshelf)
                {
                    subType = "library";
                }

                int w = Random.Range(3, 7);
                int h = Random.Range(3, 7);
                //room1.AddEdgeExpansion(3, 3);
            }

            room1.SetType(type);
            room1.SetSubType(subType);

            Room room0 = null;
            bool roomPlaced = false;

            List<int> roomIndices = Enumerable.Range(0, rooms.Count).ToList();
            ShuffleList(roomIndices);

            List<int> sides = new List<int> { 0, 1, 2, 3 };
            ShuffleList(sides);
            List<int> lengths = new List<int> { 3, 4, 5, 6, 7 };
            ShuffleList(lengths);

            foreach (int roomIndex in roomIndices)
            {
                room0 = rooms[roomIndex];

                foreach (int side in sides)
                {
                    List<int> widths = GetHallwayWidths(chanceHallwayWidth1);

                    foreach (int width in widths)
                    {
                        // Down, left, up, right
                        List<Vector3Int>[] room0ConnectionPoints;
                        List<Vector3Int>[] room1ConnectionPoints;

                        List<Vector3Int> room0Sides;
                        List<Vector3Int> room1Sides;

                        if (width == 1)
                        {
                            // Down, left, up, right
                            room0ConnectionPoints = GetConnectionPoints(room0, 1);
                            room1ConnectionPoints = GetConnectionPoints(room1, 1);

                            room0Sides = room0ConnectionPoints[side];
                            room1Sides = room1ConnectionPoints[(side + 2) % 4];

                            ShuffleList(room0Sides);
                            ShuffleList(room1Sides);
                        } else
                        {
                            // Down, left, up, right
                            room0ConnectionPoints = GetConnectionPoints(room0, 2);
                            room1ConnectionPoints = GetConnectionPoints(room1, 2);

                            room0Sides = room0ConnectionPoints[side];
                            room1Sides = room1ConnectionPoints[(side + 2) % 4];

                            ShuffleList(room0Sides);
                            ShuffleList(room1Sides);
                        }

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
                                            CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), width, length))
                                        {
                                            InstantiateRoom(room1, posHallwayEnd - room1Point);

                                            if (CheckBuildHallwayVertical(posHallway + new Vector3Int(0, 1, 0), width, length))
                                            {
                                                roomPlaced = true;
                                                BuildSquare(posHallway + new Vector3Int(0, 1, 0), width, length);
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
                                            CheckBuildHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, width))
                                        {
                                            InstantiateRoom(room1, posHallwayEnd - room1Point);

                                            if (CheckBuildHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, width))
                                            {
                                                roomPlaced = true;
                                                BuildSquare(posHallway + new Vector3Int(1, 0, 0), length, width);
                                                if (Random.value < chanceHallwayColumns)
                                                {
                                                    AddColumnsToHallwayHorizontal(posHallway + new Vector3Int(1, 0, 0), length, width);
                                                }
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
                                            CheckBuildHallwayVertical(posHallway, width, length))
                                        {
                                            InstantiateRoom(room1, posHallwayEnd - room1Point);

                                            if (CheckBuildHallwayVertical(posHallway, width, length))
                                            {
                                                roomPlaced = true;
                                                BuildSquare(posHallway, width, length);
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
                                            CheckBuildHallwayHorizontal(posHallway, length, width))
                                        {
                                            InstantiateRoom(room1, posHallwayEnd - room1Point);

                                            if (CheckBuildHallwayHorizontal(posHallway, length, width))
                                            {
                                                roomPlaced = true;
                                                BuildSquare(posHallway, length, width);
                                                if (Random.value < chanceHallwayColumns)
                                                {
                                                    AddColumnsToHallwayHorizontal(posHallway, length, width);
                                                }
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
                if (roomPlaced) break;
            }

            if (roomPlaced)
            {
                TryBuildAdditionalHallways(room1, room0);

                //FillEdgeExpansionWithPots(room1);
            }
        }
    }

    private List<int> GetHallwayWidths(float chanceHallwayWidth1)
    {
        List<int> widths = new List<int>();

        if (Random.value < chanceHallwayWidth1)
        {
            widths.Add(1);
            widths.Add(2);
        }
        else
        {
            widths.Add(2);
        }

        return widths;
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

    private void BakeTilemaps()
    {
        if (floorBaker != null)
        {
            floorBaker.BakeTilemap();
        }

        if (wallsBaker != null)
        {
            wallsBaker.BakeTilemap();
        }

        if (wallsFixBaker != null)
        {
            wallsFixBaker.BakeTilemap();
        }

        Debug.Log("Tilemaps baked!");
    }
}
