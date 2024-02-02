using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    [CreateAssetMenu(fileName = "AttackSpeedPotionItem", menuName = "Inventory/AttackSpeedPotionItem")]

    
    public class AttackSpeedPotionItem : ItemData
    {
        [SerializeField] private float attackSpeedIncrease;

        public override void UseItem()
        {
            Inventory.StartCoroutine(MyCoroutine());
        }

        IEnumerator MyCoroutine()
        {
            Inventory.Player.AttackCooldown -= attackSpeedIncrease;

            yield return new WaitForSeconds(4f);

            Inventory.Player.AttackCooldown += attackSpeedIncrease;
        }
    }
}
