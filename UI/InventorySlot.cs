using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Mirror;
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Inventory inventory;
    public BlockData blockData;
    public bool isOccupied = false;
    public int amount;
    [SerializeField]
    private Image displayImage;
    [SerializeField]
    private TMP_Text amountText;

    [SerializeField]
    private Sprite[] slotDisplayImage;

    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // Time in seconds to register a double click
    // Start is called before the first frame update
    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        if (isOccupied)
        {
            // Slot is occupied, show the block sprite and amount
            gameObject.GetComponent<Image>().sprite = slotDisplayImage[0];
            displayImage.gameObject.SetActive(true); // Ensure the displayImage is visible
            amountText.gameObject.SetActive(true); // Ensure the amountText is visible
            displayImage.sprite = blockData.blockSprites.Original;
            amountText.text = amount.ToString();
        }
        else
        {
            // Slot is not occupied, show the empty slot sprite and hide displayImage and amountText
            gameObject.GetComponent<Image>().sprite = slotDisplayImage[1];
            displayImage.gameObject.SetActive(false); // Hide the displayImage
            amountText.gameObject.SetActive(false); // Hide the amountText
        }

        if (isOccupied && blockData.itemType == ItemType.Wearable)
        {
            displayImage.sprite = blockData.itemSprites[0];
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            DoubleClick();
        }
    }

    public void SelectThisSlot()
    {
        switch (blockData.itemType)
        {
            case ItemType.Block:
                inventory.selectedBlock = blockData;
                break;
            case ItemType.Lock:
                inventory.selectedBlock = blockData;
                break;
            case ItemType.Wearable:
                Debug.Log("Wearable Item Selected");
                WearItem();
                break;
        }

    }

    private void DoubleClick()
    {
        // Check if the time since the last click is within the double-click threshold
        if (Time.time - lastClickTime <= doubleClickThreshold)
        {
            Debug.Log("Double Click Detected");
            // Double-click detected
            if (isOccupied && blockData.itemType == ItemType.Wearable)
            {
                Debug.Log("Wearing Item");
                WearItem();
            }
        }
        lastClickTime = Time.time;
    }
    private void WearItem()
    {

        switch (blockData.itemWearType)
        {
            case ItemWearType.none:
                inventory.selectedBlock = blockData;
                inventory.OnInventoryChange?.Invoke();
                break;
            case ItemWearType.Face:
                inventory.faceData = blockData;
                inventory.OnInventoryChange?.Invoke();
                break;
            case ItemWearType.Chest:
                inventory.chestData = blockData;
                inventory.OnInventoryChange?.Invoke();
                break;
            case ItemWearType.Back:
                inventory.backData = blockData;
                inventory.OnInventoryChange?.Invoke();
                break;
            case ItemWearType.Hair:
                inventory.hairData = blockData;
                inventory.OnInventoryChange?.Invoke();
                break;
        }
    }
        
}
