using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScript : MonoBehaviour
{
    [SerializeField] private bool rotate = false;
    [SerializeField] private List<Sprite> sprites;
    private bool isIdleIncreasing = true;
    private SpriteRenderer spriteRenderer;

    public Sprite GetRandomSprite(bool rotate = false)
    {
        if (sprites != null && sprites.Count > 0)
        {
            Sprite selectedSprite = sprites[Random.Range(0, sprites.Count)];
            return selectedSprite;
        }
        return null;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rotate)
        {
            int randomAngle = Random.Range(0, 4) * 90;
            transform.rotation = Quaternion.Euler(0, 0, randomAngle);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.sprite = sprite;
    }
}
