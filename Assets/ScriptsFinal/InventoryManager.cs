using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState
{
    Free,
    Equipped,
    Inventory,
    Dropped
}

public class InventoryManager : MonoBehaviour
{
    public GameObject[] weaponSlots = new GameObject[4];  // 4 inventory slots
    public Transform player;  // The player's transform
    private int currentWeaponIndex = -1;

    // Method to add a weapon to the inventory
    public bool AddWeapon(GameObject newWeapon)
    {
        // Find the first empty slot
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = newWeapon;

                SwordController swordController = newWeapon.GetComponent<SwordController>();
                if (swordController != null)
                {
                    swordController.SetState(WeaponState.Inventory);

                    // Check if this is the only weapon in the inventory
                    bool isFirstWeapon = true;
                    for (int j = 0; j < weaponSlots.Length; j++)
                    {
                        if (weaponSlots[j] != null && weaponSlots[j] != newWeapon)
                        {
                            isFirstWeapon = false; // Other weapons are present
                            break;
                        }
                    }

                    // Equip the weapon if it's the only one in the inventory
                    if (isFirstWeapon)
                    {
                        EquipWeapon(i); // Equip the newly added weapon
                    }
                }
                return true;
            }
        }

        // Inventory full, handle appropriately (e.g., deny pickup or swap)
        Debug.Log("Inventory is full!");
        return false;
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

    // Drop the currently equipped weapon
    public void DropWeapon()
    {
        if (currentWeaponIndex != -1 && weaponSlots[currentWeaponIndex] != null)
        {
            GameObject weaponToDrop = weaponSlots[currentWeaponIndex];
            weaponSlots[currentWeaponIndex] = null;  // Remove the weapon from the inventory

            SwordController swordController = weaponToDrop.GetComponent<SwordController>();
            if (swordController != null)
            {
                swordController.SetState(WeaponState.Dropped); // Set the weapon to dropped state
            }

            currentWeaponIndex = -1; // Reset the equipped weapon index
        }
    }
}
