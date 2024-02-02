using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    
    [CreateAssetMenu(fileName = "PermanentMSPotionItem", menuName = "Inventory/PermanentMSPotionItem")]
    public class PermanentMSPotionItem : ItemData
    {

        [SerializeField] private float permanentMS_Added;

        public override void UseItem()
        {
            Inventory.Player.MovementSpeed += permanentMS_Added;
        }
    }
}
