using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    [CreateAssetMenu(fileName = "ArmorItem", menuName = "Inventory/ArmorItem")]
    public class ArmorItem : ItemData
    {
        private bool onPlayer = false;
        private float armorValue = 100;
        public override void UseItem()
        {
            if (!onPlayer)
            {
                onPlayer = true;
                Inventory.Player.Armor = armorValue;
            }
            else
            {
                onPlayer = false;
                Inventory.Player.Armor = 0;
            }
        }
    }
}
