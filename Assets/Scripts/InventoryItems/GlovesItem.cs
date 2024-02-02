using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    
    [CreateAssetMenu(fileName = "GlovesItem", menuName = "Inventory/GlovesItem")]
    public class GlovesItem : ItemData
    {
        private bool onPlayer = false;
        private float attackSpeedIncrease = 2f;

        public override void UseItem()
        {
            if (!onPlayer)
            {
                onPlayer = true;
                Inventory.Player.AttackCooldown -= attackSpeedIncrease;
            }
            else
            {
                onPlayer = false;
                Inventory.Player.AttackCooldown += attackSpeedIncrease;
            }
        }
    }
}
