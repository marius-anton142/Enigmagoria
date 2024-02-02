using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{

    [CreateAssetMenu(fileName = "PermanentDmgPotionItem", menuName = "Inventory/PermanentDmgPotionItem")]
    public class PermanentDmgPotionItem : ItemData
    {
        [SerializeField] private float permanentDmg_Added;

        public override void UseItem()
        {
            Inventory.Player.MeleeDamage += permanentDmg_Added;
        }
    }
}
