using UnityEngine;

public class CobwebScript : MonoBehaviour
{
    public int minBumpsToEscape = 2;
    public int maxBumpsToEscape = 5;
    private int bumpsRequired;
    private int bumpCount = 0;

    private void Start()
    {
        bumpsRequired = Random.Range(minBumpsToEscape, maxBumpsToEscape + 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }

            other.GetComponent<PlayerScript>().SetStuck(bumpsRequired);
            other.transform.position = transform.position;
        }

        if (other.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = other.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.velocity = Vector2.zero;
                enemyRb.angularVelocity = 0f;
            }

            other.GetComponent<EnemyAI>().SetStuck(bumpsRequired);
            other.transform.position = transform.position;
        }
    }
}
