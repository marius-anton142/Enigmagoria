using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDotScript : MonoBehaviour
{
    void Start()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Vector2 offset = new Vector2(Screen.width * 7 / 8f, Screen.height * 1 / 8f);

        rectTransform.anchoredPosition = offset;
    }
}
