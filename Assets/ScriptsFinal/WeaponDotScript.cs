using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WeaponDotScript : MonoBehaviour
{
    public RectTransform canvasRect;
    public Vector2 normalizedOffset = new Vector2(6.5f / 8f, 3f / 8f);  // Normalized (0-1) position relative to the screen size
    [SerializeField] private List<GameObject> slots = new List<GameObject>();
    [SerializeField] private List<Image> slotImages = new List<Image>();
    [SerializeField] private GameObject slotSelected;
    [SerializeField] private Sprite spriteBoxSelected, spriteBoxEmpty;

    private int slotCount;
    private Image slotSelectedImage;

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

        SelectSlot(0);
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
        slotSelectedImage = slotImages[slotIndex];
    }

    public void AddItemToSelectedSlot(Sprite itemSprite, Vector2 maxSize = default)
    {
        if (slotSelectedImage == null) return;

        // Set default maxSize if not specified
        if (maxSize == default)
        {
            maxSize = new Vector2(16f, 24f); // Adjust these values based on the slot size
        }

        // Set the item sprite on the selected Image component
        slotSelectedImage.sprite = itemSprite;

        // Ensure that the RectTransform anchors are centered
        slotSelectedImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        slotSelectedImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        // Calculate the aspect ratio of the item
        float aspectRatio = itemSprite.rect.width / itemSprite.rect.height;

        // Set the height to maxSize.y (24) and calculate width to maintain aspect ratio
        float adjustedHeight = maxSize.y;
        float adjustedWidth = adjustedHeight * aspectRatio;

        // Apply the adjusted size to the RectTransform’s sizeDelta
        slotSelectedImage.rectTransform.sizeDelta = new Vector2(adjustedWidth, adjustedHeight);

        // Force the layout to update if part of a layout group
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotSelectedImage.rectTransform);

        slotSelectedImage.enabled = true;
    }
}
