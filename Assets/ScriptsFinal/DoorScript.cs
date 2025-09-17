using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] private int hp = 3;
    [SerializeField] private Sprite doorOpenSprite;
    [SerializeField] private Sprite doorOpenFloorSprite;

    private SpriteRenderer sr;
    private SpriteRenderer childSr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (transform.childCount > 0)
        {
            childSr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }
    }

    public int GetHp() { return hp; }
    public void DecreaseHp()
    {
        hp--;

        if (hp == -1 && doorOpenSprite != null)
        {
            sr.sprite = doorOpenSprite;

            // Change child sprite if assigned
            if (childSr != null && doorOpenFloorSprite != null)
            {
                childSr.sprite = doorOpenFloorSprite;
            }

            Destroy(GetComponent<Rigidbody2D>());
            Destroy(GetComponent<PolygonCollider2D>());
        }
    }
}
