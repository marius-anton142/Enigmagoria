using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{

    [CreateAssetMenu(fileName = "BootsItem", menuName = "Inventory/BootsItem")]

    public class BootsItem : ItemData
    {
        private bool onPlayer = false;
        private float speedIncrease = 4f;

        public override void UseItem()
        {
            if (!onPlayer)
            {
                onPlayer = true;
                Inventory.Player.MovementSpeed += speedIncrease;
            }
            else
            {
                onPlayer = false;
                Inventory.Player.MovementSpeed -= speedIncrease;
            }
        }

    }
}

