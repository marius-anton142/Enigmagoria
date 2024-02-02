using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float arrowVelocity;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float damage = 3; 

    // Start is called before the first frame update
    void Start()
    {
        arrowVelocity = 3;
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = transform.up * arrowVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
