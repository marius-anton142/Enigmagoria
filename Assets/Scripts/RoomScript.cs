using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 nextPos;

    public float moveSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        nextPos = rb.position;
    }

    void FixedUpdate()
    {

        if (Vector2.Distance(rb.position, nextPos) < 0.1f)
        {
            nextPos = new Vector2(Mathf.Round(rb.position.x), Mathf.Round(rb.position.y));
            rb.velocity = Vector2.zero; // Set velocity to zero when reaching a grid point
            rb.position = nextPos; // Round the position to the nearest integer
        }
    }
}
