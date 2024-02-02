using UnityEngine;

namespace InventoryItems
{
    [CreateAssetMenu(fileName = "HPPotionItem", menuName = "Inventory/HPPotionItem")]
    public class HPPotionItem : ItemData
    {
        [SerializeField] private float hp;
        public override void UseItem()
        {
            Inventory.Player.CurrentHP = hp;
        }
    }
}