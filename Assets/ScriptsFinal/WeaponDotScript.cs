using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public enum WeaponState
{
    Free,
    Equipped,
    Inventory,
    Dropped
}

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

    public GameObject[] weaponSlots = new GameObject[4];  // 4 inventory slots
    public Transform player;  // The player's transform
    public int currentWeaponIndex = -1;

    void Start()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();

        if (Application.platform != RuntimePlatform.Android)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            if (TryGetComponent(out Image img))
            {
                img.enabled = false;
            }
        }

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
        if (slotSelected == slots[slotIndex])
        {
            DropWeapon(slotIndex);
        }
        else 
        {
            slotSelected = slots[slotIndex];

            for (int i = 0; i < slotCount; i++)
            {
                slots[i].GetComponent<Image>().sprite = spriteBoxEmpty;
            }

            slotSelected.GetComponent<Image>().sprite = spriteBoxSelected;
            slotSelectedImage = slotImages[slotIndex];
        }
    }

    public bool AddWeapon(GameObject newWeapon, Sprite itemSprite, Vector2 maxSize = default)
    {
        // Set default maxSize if not specified
        if (maxSize == default)
        {
            maxSize = new Vector2(16f, 24f); // Adjust these values based on the slot size
        }

        int selectedSlotIndex = slots.IndexOf(slotSelected);

        // Check if the selected slot is available
        if (selectedSlotIndex != -1 && weaponSlots[selectedSlotIndex] == null)
        {
            AssignWeaponToSlot(newWeapon, itemSprite, maxSize, selectedSlotIndex, selectedSlotIndex);
            return true;
        }

        // If selected slot is occupied, find the first available slot
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                AssignWeaponToSlot(newWeapon, itemSprite, maxSize, i);
                return true;
            }
        }

        // Inventory full, handle appropriately
        Debug.Log("Inventory is full!");
        return false;
    }

    private void AssignWeaponToSlot(GameObject weapon, Sprite itemSprite, Vector2 maxSize, int slotIndex, int selectedSlotIndex = -1)
    {
        // Assign the weapon to the slot
        weaponSlots[slotIndex] = weapon;

        SwordController swordController = weapon.GetComponent<SwordController>();
        if (swordController != null)
        {
            swordController.SetState(WeaponState.Inventory);
        }

        // Set the visual representation in the UI slot
        slotImages[slotIndex].sprite = itemSprite;

        // Adjust the size of the image to maintain aspect ratio
        float aspectRatio = itemSprite.rect.width / itemSprite.rect.height;
        float adjustedHeight = maxSize.y;
        float adjustedWidth = adjustedHeight * aspectRatio;
        slotImages[slotIndex].rectTransform.sizeDelta = new Vector2(adjustedWidth, adjustedHeight);
        slotImages[slotIndex].enabled = true;

        if (slotIndex == selectedSlotIndex)
        {
            EquipWeapon(slotIndex); // Equip the newly added weapon
        }
    }

    // Method to switch to a specific weapon by index
    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length || weaponSlots[index] == null)
        {
            Debug.Log("Invalid weapon index");
        }

        // Unequip the current weapon
        if (currentWeaponIndex != -1)
        {
            GameObject currentWeapon = weaponSlots[currentWeaponIndex];
            if (currentWeapon != null)
            {
                SwordController currentSwordController = currentWeapon.GetComponent<SwordController>();
                if (currentSwordController != null)
                {
                    currentSwordController.SetState(WeaponState.Inventory); // Move current weapon to inventory
                }
            }
        }

        // Equip the selected weapon
        EquipWeapon(index);
    }

    // Method to equip a weapon
    public void EquipWeapon(int index)
    {
        GameObject newWeapon = weaponSlots[index];
        if (newWeapon != null)
        {
            SwordController swordController = newWeapon.GetComponent<SwordController>();
            if (swordController != null)
            {
                swordController.SetState(WeaponState.Equipped); // Set the new weapon as equipped
            }

            currentWeaponIndex = index; // Track the currently equipped weapon
        }
    }

    // Method to swap two weapon slots in inventory
    public void SwapWeaponSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= weaponSlots.Length || indexB < 0 || indexB >= weaponSlots.Length)
        {
            Debug.Log("Invalid swap indices");
            return;
        }

        // Swap the weapons in the slots
        GameObject temp = weaponSlots[indexA];
        weaponSlots[indexA] = weaponSlots[indexB];
        weaponSlots[indexB] = temp;
    }
    
    public void DropWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots.Length && weaponSlots[slotIndex] != null)
        {
            GameObject weaponToDrop = weaponSlots[slotIndex];
            weaponSlots[slotIndex] = null; // Remove the weapon from the specified slot in the inventory

            // Clear the UI image for the dropped weapon’s slot
            slotImages[slotIndex].sprite = null;
            slotImages[slotIndex].enabled = false;

            // Activate the weapon if it’s not already active
            if (!weaponToDrop.activeSelf)
            {
                weaponToDrop.SetActive(true);
            }

            // Apply an impulse to the weapon's Rigidbody2D to "drop" it to the right
            Rigidbody2D weaponRb = weaponToDrop.GetComponent<Rigidbody2D>();
            if (weaponRb != null)
            {
                weaponRb.isKinematic = false; // Enable physics for the Rigidbody2D

                Vector2 dropDirection = player.GetComponent<SpriteRenderer>().flipX ? Vector2.left : Vector2.right;
                weaponRb.AddForce(dropDirection * 17f, ForceMode2D.Impulse);
            }

            // Set the weapon's state to dropped
            SwordController swordController = weaponToDrop.GetComponent<SwordController>();
            if (swordController != null)
            {
                swordController.SetState(WeaponState.Dropped);
            }

            // Update currentWeaponIndex if this was the equipped weapon
            if (currentWeaponIndex == slotIndex)
            {
                currentWeaponIndex = -1; // Reset the equipped weapon index
            }
        }
    }
}
