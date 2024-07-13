using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Custom Rule Tile", menuName = "Tiles/Custom Rule Tile")]
public class CustomRuleTile : RuleTile<CustomRuleTile.Neighbor>
{
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int NotThis = 3; // Custom neighbor type for ignoring specific tiles
    }

    public TileBase[] leftWallTiles; // Array of left wall tiles
    public TileBase[] rightWallTiles; // Array of right wall tiles

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        switch (neighbor)
        {
            case Neighbor.NotThis:
                return !IsLeftWall(other) && !IsRightWall(other); // Ignore specific neighbors
        }
        return base.RuleMatch(neighbor, other);
    }

    private bool IsLeftWall(TileBase tile)
    {
        foreach (var leftWallTile in leftWallTiles)
        {
            if (tile == leftWallTile)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsRightWall(TileBase tile)
    {
        foreach (var rightWallTile in rightWallTiles)
        {
            if (tile == rightWallTile)
            {
                return true;
            }
        }
        return false;
    }
}
