using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{

    [CreateAssetMenu(fileName = "HpRegenPotionItem", menuName = "Inventory/HpRegenPotionItem")]
    public class HpRegenPotionItem : ItemData
    {
        [SerializeField] private float hpRegenQuantity;

        public override void UseItem()
        {
            Inventory.StartCoroutine(MyCoroutine());
        }

        IEnumerator MyCoroutine()
        {
            Inventory.Player.HpRegenQuantity += hpRegenQuantity;

            yield return new WaitForSeconds(12f);

            Inventory.Player.HpRegenQuantity -= hpRegenQuantity;
        }
    }
}
