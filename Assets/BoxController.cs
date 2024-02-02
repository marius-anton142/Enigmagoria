using UnityEngine;

public class BoxController : MonoBehaviour
{
    public GameObject DungeonManager;
    void Start()
    {
        DungeonManager = GameObject.FindGameObjectWithTag("DungeonManager");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        DungeonManager.GetComponent<DungeonGenerationScript>().objectPositions.Remove(gameObject.transform.position);
        Destroy(gameObject);
        Debug.Log("a");
    }
}