using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDotScript : MonoBehaviour
{
    public RectTransform canvasRect;

    void Start()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        Vector2 offset = new Vector2(canvasWidth * 6.5f / 8f, canvasHeight * 3 / 8f);

        rectTransform.anchoredPosition = offset;
    }
}
