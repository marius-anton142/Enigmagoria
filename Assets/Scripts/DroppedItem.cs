using UnityEngine;

public class DroppedItem : Collectible
{
    // Fiecare item de pe jos e de tipul DroppedItem, o clasa derivata din clasa Collectible
    // In scriptul de player, cand se intampla coliziunea dintre player si item, se apeleaza aceasta functie 
    // care o apeleaza pe cea din clasa de baza 
    public override void Collect()
    {
        Debug.Log("Item Collected");
        base.Collect();
    }
}
