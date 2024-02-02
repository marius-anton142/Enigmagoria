using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryItems;
public class ChestController : MonoBehaviour
{
    [SerializeField] private bool isOpen;
    [SerializeField] private float stateOfChest;
    [SerializeField] private Animator animator;
    [SerializeField] private ArmorItem armor;
    [SerializeField] private SwordItem sword ;
    [SerializeField] private AttackSpeedPotionItem attackSpeedPotion ;
    [SerializeField] private BootsItem boots ;
    [SerializeField] private CoinItem coin ;
    [SerializeField] private GlovesItem gloves ;
    [SerializeField] private DamagePotion damagePotion ;
    [SerializeField] private HPPotionItem hpPotion ;
    [SerializeField] private HpRegenPotionItem hpRegenPotion ;
    [SerializeField] private PermanentASPotionItem permanentAsPotion ;
    [SerializeField] private PermanentDmgPotionItem permanentDmgPotion ;
    [SerializeField] private PermanentMSPotionItem permanentMsPotion ;
    [SerializeField] private SpeedPotionItem speedPotion ;
    public bool IsOpen
    {
        get { return isOpen; }
        set { isOpen = value; }
    }
    [SerializeField] private Inventory inventory;
    [SerializeField] List<ItemData> items = new List<ItemData>();
    [SerializeField] List<ItemData> itemsInChest = new List<ItemData>();
    public List<ItemData> Items
    {
        get { return items; }
        set { items = value; }
    }
    public List<ItemData> ItemsInChest
    {
        get { return itemsInChest; }
        set { itemsInChest = value; }
    }
    public int numberOfItems
    {
        get { return itemsInChest.Count; }
    }

    public void AddItem(ItemData item)
    {
        itemsInChest.Add(item);
    }
    
    // Adaugam lista cu itemele existente in joc
    public void populateList()
    {
        items.Add(sword);
        items.Add(attackSpeedPotion);
        items.Add(boots);
        items.Add(coin);
        items.Add(gloves);
        items.Add(damagePotion);
        items.Add(hpPotion);
        items.Add(hpRegenPotion);
        items.Add(permanentAsPotion);
        items.Add(permanentDmgPotion);
        items.Add(permanentMsPotion);
        items.Add(speedPotion);
    }

    //stateOfChest Legend:
    //0 Loot closed
    //1 Loot open
    //2 Empty closed
    //3 Empty open

    /*
    O sa avem 5 chesturi
    Fiecare chest are/nu are cateva iteme

    */
    public void InteractWithChest()
    {
        if(stateOfChest == 0) //closed loot
            {
                animator.SetTrigger("OpenLoot");
                //set state of chest 1 from animator
                animator.SetFloat("stateOfChest", 1);
                stateOfChest = 1;
                collectItems();
                Debug.Log("Am deschis chestul");
            }
        else if(stateOfChest == 1) //opened loot
            {
                
                animator.SetTrigger("CloseLoot");
                animator.SetFloat("stateOfChest", 0);
                stateOfChest = 2;
            }
        if(stateOfChest == 2) //closed empty
            {
                animator.SetTrigger("OpenEmpty");
                animator.SetFloat("stateOfChest", 3);
                stateOfChest = 3;
            }
        else if(stateOfChest == 3) //opened empty
            {
                animator.SetTrigger("CloseEmpty");
                animator.SetFloat("stateOfChest", 2);
                stateOfChest = 2;
            }
    }
    public void interactWithItems()
    {
        if(stateOfChest == 1) //opened loot
            {
                //add items to inventory
                
                //delete items from chest
                items.Clear();
                //set state of chest 2 from animator
                animator.SetFloat("stateOfChest", 2);
                stateOfChest = 2;
            }
        else if(stateOfChest == 3) //opened empty
            {
                //set state of chest 0 from animator
                animator.SetFloat("stateOfChest", 0);
                stateOfChest = 0;
            }
    }
    // Start is called before the first frame update
    void Start()
    {
        populateList();
        inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        stateOfChest = 0;
        double probability = Random.Range(0.0f, 1.0f);
        for(int i = 0; i < 3;i++)
        {
            if(probability > 0.5)
            {
                int randomItem = Random.Range(0, items.Count);
                AddItem(items[randomItem]);
            }
        }
        AddItem(items[items.Count - 1]);
        if(numberOfItems > 0)
            {
                stateOfChest = 0;
            }
        else
            {
                stateOfChest = 2;
            }
    }
    
    // Functia de adaugare a itemelor in inventar cand se deschide chestu
    void collectItems()
    {
        List<ItemData> itemsInChestCopy = new List<ItemData>();
        foreach(ItemData item in itemsInChest)
        {
            itemsInChestCopy.Add(item);
        }
        foreach(ItemData item in itemsInChestCopy)
        {
            Debug.Log("Am adaugat itemul " + item.name);
            inventory.Add(item);
            itemsInChest.Remove(item);
        }
        itemsInChest.Clear();
    }
    // Update is called once per frame
    void Update()
    { 
    }
}
