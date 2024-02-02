using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryItems
{
    [CreateAssetMenu(fileName = "SwordItem", menuName = "Inventory/SwordItem")]
    public class SwordItem : ItemData
    {
        private bool onPlayer = false;
        private double damageIncrease = 0.5;

        public override void UseItem()
        {
            if (!onPlayer)
            {
                onPlayer = true;
                Inventory.Player.MeleeDamage += damageIncrease;
            }
            else
            {
                onPlayer = false;
                Inventory.Player.MeleeDamage -= damageIncrease;
            }
        }

    }
}
