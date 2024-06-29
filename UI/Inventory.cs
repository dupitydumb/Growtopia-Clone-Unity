using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{

    public int inventorySize = 10;
    public List<BlockData> blocks = new List<BlockData>();

    [SerializeField]
    private GameObject slotPrefab;

    [SerializeField]
    private GameObject parent;
    public BlockData selectedBlock;

    public EquipItem itemEquiped;

    public Action OnInventoryChange;
        
    // Start is called before the first frame update
    void Start()
    {
        InitializeInventory();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitializeInventory()
    {
        foreach (BlockData block in blocks)
        {
            GameObject slot = Instantiate(slotPrefab, parent.transform);
            slot.GetComponent<InventorySlot>().isOccupied = true;
            slot.GetComponent<InventorySlot>().blockData = block;
            slot.GetComponent<InventorySlot>().amount = block.amount;
        }

        //Fill the rest of the inventory with empty slots
        for (int i = blocks.Count; i < inventorySize; i++)
        {
            GameObject slot = Instantiate(slotPrefab, parent.transform);
            slot.GetComponent<InventorySlot>().isOccupied = false;
        }

        RearangeInventory();
    }

    //Rearange the inventory beetwen the siblings
    public void RearangeInventory()
    {
        //Make the occupied slots go first
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).GetComponent<InventorySlot>().isOccupied)
            {
                parent.transform.GetChild(i).SetSiblingIndex(i);
            }
        }
        //Sort base on rarity
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).GetComponent<InventorySlot>().isOccupied)
            {
                for (int j = i + 1; j < parent.transform.childCount; j++)
                {
                    if (parent.transform.GetChild(j).GetComponent<InventorySlot>().isOccupied)
                    {
                        if (parent.transform.GetChild(i).GetComponent<InventorySlot>().blockData.rarity < parent.transform.GetChild(j).GetComponent<InventorySlot>().blockData.rarity)
                        {
                            parent.transform.GetChild(j).SetSiblingIndex(i);
                        }
                    }
                }
            }
        }
    }
}
