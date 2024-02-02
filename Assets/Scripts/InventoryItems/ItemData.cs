using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public int maxStackSize;
    public string itemType;
    public Inventory Inventory { get; set; }
    
    // Functia este suprascrisa pentru fiecare tip de item in parte, in functie de ce face itemul
    public abstract void UseItem();
}
