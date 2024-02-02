using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryItems
{
    [CreateAssetMenu(fileName = "DamagePotionItem", menuName = "Inventory/DamagePotionItem")]

    public class DamagePotion : ItemData
    {
        [SerializeField] private float multiplier;

        public override void UseItem()
        {
            Inventory.StartCoroutine(MyCoroutine());
        }

        IEnumerator MyCoroutine()
        {
            Inventory.Player.MeleeDamage *= multiplier;

            yield return new WaitForSeconds(4f);

            Inventory.Player.MeleeDamage /= multiplier;
        }
    }
}
