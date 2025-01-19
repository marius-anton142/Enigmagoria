using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBaker : MonoBehaviour
{
    public Tilemap tilemap; // Assign the procedurally generated tilemap here

    public void BakeTilemap()
    {
        // Get the mesh from the tilemap
        Mesh mesh = new Mesh();
        tilemap.GetComponent<TilemapRenderer>().GetComponent<MeshFilter>().mesh = mesh;

        // Optional: Mark this tilemap as static for optimization
        gameObject.isStatic = true;

        // Disable TilemapRenderer after baking to reduce rendering overhead
        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
}
