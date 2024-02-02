using System;
using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    public static event Action<ItemData> OnItemCollected;
    
    [SerializeField] private ItemData collectibleData;

    public ItemData CollectibleData => collectibleData;

    // Functia care sterge obiectul de pe jos si invoca Event-ul la care este abonata functia Add din Inventory
    public virtual void Collect()
    {
        Destroy(gameObject);
        OnItemCollected?.Invoke(collectibleData);
    }
}
