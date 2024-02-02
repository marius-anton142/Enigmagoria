using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemName; 
        [SerializeField] private Image itemIcon; 
        [SerializeField] private TextMeshProUGUI itemStackSize;
        [SerializeField] private Button useButton;
        private bool isActive = false;
        [SerializeField] private Sprite activeItemImage;
        [SerializeField] private Sprite inactiveItemImage;
        private string itemType;

        public TextMeshProUGUI ItemName => itemName;
        public Image ItemIcon => itemIcon;
        public TextMeshProUGUI ItemStackSize
        {
            get => itemStackSize;
            set => itemStackSize = value;
        }

        public string ItemType { get; set; }
        
        public event Action<ItemSlot> OnRemove;
        public event Action<ItemSlot> OnUse;

        public void RemoveItem() => OnRemove?.Invoke(this);

        // Functia este legata de butonul "Use" si se apeleaza cand acesta este apasat.
        public void UseItem()
        {
            if (!isActive && ItemType == "Passive")
            {
                isActive = true;
                useButton.GetComponent<Image>().sprite = activeItemImage;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
            else
            {
                isActive = false;
                useButton.GetComponent<Image>().sprite = inactiveItemImage;
                useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Use";
            }
            OnUse?.Invoke(this);
        }

    }
}