using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponDotScript : MonoBehaviour
{
    public RectTransform canvasRect;
    public Vector2 normalizedOffset = new Vector2(6.5f / 8f, 3f / 8f);  // Normalized (0-1) position relative to the screen size
    [SerializeField] private List<GameObject> slots = new List<GameObject>();
    [SerializeField] private GameObject slotSelected;
    [SerializeField] private Sprite spriteBoxSelected, spriteBoxEmpty;

    private int slotCount;

    void Start()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        // Anchor to the center of the canvas instead of the bottom-left
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // Center anchor
        rectTransform.pivot = new Vector2(0.5f, 0.5f);     // Pivot at center

        // Set the position relative to the center of the screen, using normalized values
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        Vector2 offset = new Vector2(canvasWidth * (normalizedOffset.x - 0.5f), canvasHeight * (normalizedOffset.y - 0.5f));
        rectTransform.anchoredPosition = offset;

        slotSelected = slots[0];
        slotCount = 4;
    }

    public Vector3 GetOffset()
    {
        // Return the offset based on normalized screen space
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        return new Vector3(canvasWidth * (normalizedOffset.x - 0.5f), canvasHeight * (normalizedOffset.y - 0.5f), 0f);
    }

    public void SelectSlot(int slotIndex)
    {
        slotSelected = slots[slotIndex];

        for (int i = 0; i < slotCount; i++)
        {
            slots[i].GetComponent<Image>().sprite = spriteBoxEmpty;
        }

        slotSelected.GetComponent<Image>().sprite = spriteBoxSelected;
    }
}
