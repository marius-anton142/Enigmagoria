using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteScript : MonoBehaviour
{
    [SerializeField] private bool rotate = false;
    public List<Sprite> sprites;
    private bool isIdleIncreasing = true;
    private SpriteRenderer spriteRenderer;

    public Sprite GetRandomSprite(bool rotate = false)
    {
        if (sprites != null && sprites.Count > 0)
        {
            Sprite selectedSprite = sprites[Random.Range(0, sprites.Count)];
            this.rotate = rotate;
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

    public Sprite GetRandomSpriteWithProbabilities(List<float> probabilities, bool rotate = false)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning("No sprites available!");
            return null;
        }

        // If probabilities count is less than sprites count, fill the rest with 0
        if (probabilities == null)
        {
            Debug.LogWarning("Probabilities list is null! Defaulting to 0 for all probabilities.");
            probabilities = new List<float>(new float[sprites.Count]);
        }
        else if (probabilities.Count < sprites.Count)
        {
            Debug.LogWarning("Probabilities list size is less than the number of sprites. Filling missing values with 0.");
            while (probabilities.Count < sprites.Count)
            {
                probabilities.Add(0f);
            }
        }

        // Calculate total weight
        float totalWeight = 0f;
        for (int i = 0; i < probabilities.Count; i++)
        {
            totalWeight += probabilities[i];
        }

        if (totalWeight <= 0f)
        {
            Debug.LogError("Total probability weight must be greater than zero!");
            return null;
        }

        // Generate a random value between 0 and the total weight
        float randomValue = Random.value * totalWeight;

        // Select a sprite based on the weighted random value
        float cumulativeWeight = 0f;
        for (int i = 0; i < sprites.Count; i++)
        {
            cumulativeWeight += probabilities[i];
            if (randomValue <= cumulativeWeight)
            {
                if (rotate)
                {
                    this.rotate = rotate;
                }
                return sprites[i];
            }
        }

        // Fallback in case of rounding errors
        return sprites[sprites.Count - 1];
    }
}
