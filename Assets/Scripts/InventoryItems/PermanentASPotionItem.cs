using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    
    [CreateAssetMenu(fileName = "PermanentASPotionItem", menuName = "Inventory/PermanentASPotionItem")]

    public class PermanentASPotionItem : ItemData
    {
        [SerializeField] private float permanentAS_Added;

        public override void UseItem()
        {
            Inventory.Player.AttackCooldown -= permanentAS_Added;
        }
    }

}
