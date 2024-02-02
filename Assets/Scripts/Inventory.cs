using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 12;
    [SerializeField] private Player player;
    private List<InventoryItem> items = new List<InventoryItem>();

    public Transform itemContent;
    public ItemSlot itemSlotPrefab;

    public Toggle enableRemove;
    public Player Player => player;
    
    // La pornirea scriptului se adauga functia Add pe Event-ul OnItemCollected care se apeleaza la 
    // colectarea unui item de pe jos
    // De asemenea acesta trebuie sa fie dezabonat de la Event la inchiderea scriptului
    
    private void OnEnable() => Collectible.OnItemCollected += Add;
    private void OnDisable() => Collectible.OnItemCollected -= Add;
    
    private InventoryItem foundItem;
    
    // Adaugam item in inventar, daca nu a mai fost adaugat inainte, se creaza un nou slot pentru el
    public void Add(ItemData itemData)
    {
        Debug.Log("Adaugam un item in inventar");
        foundItem = items.FirstOrDefault(item => item.itemData == itemData && item.itemData.maxStackSize > item.stackSize);
        itemData.Inventory = this;
        if (foundItem != null)
        {
            foundItem.AddToStack();
            var itemStackSize = foundItem.itemSlot.ItemStackSize;
            itemStackSize.text = foundItem.stackSize.ToString();
            Debug.Log($"{foundItem.itemData.displayName} total stack is now {foundItem.stackSize}");
        }
        else
        {
            if(items.Count >= maxSlots) return;
            var newItem = new InventoryItem(itemData);
            items.Add(newItem);
            var itemSlot = Instantiate(itemSlotPrefab, itemContent);
            newItem.itemSlot = itemSlot;
            itemSlot.OnRemove += RemoveStack;
            itemSlot.OnUse += newItem.UseItem;
            itemSlot.ItemName.text = newItem.itemData.displayName;
            itemSlot.ItemIcon.sprite = newItem.itemData.icon;
            itemSlot.ItemStackSize.text = newItem.stackSize.ToString();
            itemSlot.ItemType = newItem.itemData.itemType;
            EnableItemsRemove();
            Debug.Log($"Added {itemData.displayName} to the inventory for the first time");
        }
    }
    //photopea e un editor moca de tiless
    public void Remove(ItemData itemData)
    {
        foundItem = items.FirstOrDefault(item => item.itemData == itemData);
        if (foundItem != null)
        {
            foundItem.RemoveFromStack();
            if (foundItem.stackSize == 0)
            {
                Destroy(foundItem.itemSlot.gameObject);
                items.RemoveAt(items.IndexOf(foundItem));
            }
        }
    }
    
    // Cand apasam pe butonul de remove, se apeleaza aceasta functie care permite stergerea itemelor din inventar

    public void EnableItemsRemove()
    {
        if (enableRemove.isOn)
        {
            foreach (Transform itemSlot in itemContent)
            {
                itemSlot.Find("RemoveButton").gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform itemSlot in itemContent)
            {
                itemSlot.Find("RemoveButton").gameObject.SetActive(false);
            }
        }
    }
    
    // Stergem itemul din inventar
    
    public void RemoveStack(ItemSlot itemSlot)
    {
        foundItem = items.FirstOrDefault(item => item.itemData.displayName == itemSlot.ItemName.text);
        if(foundItem == null) return;
        if (!enableRemove.isOn && foundItem.stackSize > 0) return;
        Debug.Log("RemoveStack is called");
        itemSlot.OnRemove -= RemoveStack;
        itemSlot.OnUse -= foundItem.UseItem;
        items.Remove(foundItem);
        Destroy(itemSlot.gameObject);
        Debug.Log("itemSlot destroyed");
    }
}
