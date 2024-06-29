using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Mirror;
using TMPro;

public class Playbox : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string playerName;
    public TMP_Text playerNameText;
    public static Playbox LocalPlayerInstance;
    public GameObject playerArm;
    public float speed = 5f; // Speed of the player movement
    public float jumpForce = 10f; // Force of the jump
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private bool isGrounded = true; // Check if the player is grounded
    public GameObject blockPrefab; // Reference to the block prefab
    public BlockData selectedBlock()
    {
        return inventory.selectedBlock;
    }
    private Tilemap tilemap; // Reference to the Tilemap component
    public GameObject dustPrefab;

    private Inventory inventory;

    [Header("Player Parts")]
    public GameObject face;
    public GameObject chest;
    public GameObject back;
    public GameObject hair;
    [SyncVar(hook = nameof(OnPlayerPartsChanged))]
    public EquipItem playerParts;
    private void Awake()
    {
        if (isLocalPlayer)
        {
            LocalPlayerInstance = this;
        }
        
    }

    void OnPlayerNameChanged(string oldName, string newName)
    {
        playerNameText.text = newName;
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        tilemap = FindObjectOfType<Tilemap>(); // Find the Tilemap component in the scene
        inventory = FindObjectOfType<Inventory>();

        if (!isLocalPlayer)
        {
            //Change the color of the player to differentiate between local and remote players
            GetComponent<SpriteRenderer>().color = Color.red;

        }
        else
        {
            //Change the color of the player to differentiate between local and remote players
            gameObject.tag = "LocalPlayer";
        }

        //set the player name text to username
        CmdSetPlayerName(PlayerPrefs.GetString("username"));
        

        //Apply the player parts
        ApplyPlayerParts();
        inventory.OnInventoryChange += InventoryChanged;
    }
    [Command]
    public void CmdSendMessage(string message)
    {
        LogUI.instance.RpcDisplayMessage(message);
    }


    void InventoryChanged()
    {
        var item = inventory.itemEquiped;
        if (item.faceData != null)
        {
            EquipPart(item.faceData.blockName);
        }
        if (item.chestData != null)
        {
            EquipPart(item.chestData.blockName);
        }
        if (item.backData != null)
        {
            EquipPart(item.backData.blockName);
        }
        if (item.hairData != null)
        {
            EquipPart(item.hairData.blockName);
        }
    }   
    [Command]
    public void EquipPart(string blockName)
    {
        BlockData blockData = Resources.Load<BlockData>("Cosmetic/" + blockName);
        if (blockData == null)
        {
            Debug.LogError("Block data not found for block name: " + blockName);
            return;
        }
        // Update the part on the server
        UpdatePlayerPart(blockData);

        // Notify all clients to update this part
        RpcUpdatePlayerPart(blockData.blockName, blockData.itemWearType);
    }

    [ClientRpc]
    public void RpcUpdatePlayerPart(string blockName, ItemWearType wearType)
    {
        // This method will run on all clients
        // Ensure you apply the part change here, similar to how it's done in EquipPart
        BlockData blockData = Resources.Load<BlockData>("Cosmetic/" + blockName);
        switch (wearType)
        {
            case ItemWearType.Face:
                playerParts.faceData = blockData;
                break;
            case ItemWearType.Chest:
                playerParts.chestData = blockData;
                break;
            case ItemWearType.Back:
                playerParts.backData = blockData;
                break;
            case ItemWearType.Hair:
                playerParts.hairData = blockData;
                break;
        }
        ApplyPlayerParts(); // Assuming this method applies the changes visually
    }
    private void UpdatePlayerPart(BlockData blockData)
    {
        // This method updates the part on the server
        switch (blockData.itemWearType)
        {
            case ItemWearType.Face:
                playerParts.faceData = blockData;
                break;
            case ItemWearType.Chest:
                playerParts.chestData = blockData;
                break;
            case ItemWearType.Back:
                playerParts.backData = blockData;
                break;
            case ItemWearType.Hair:
                playerParts.hairData = blockData;
                break;
        }
    }

    void OnPlayerPartsChanged(EquipItem oldParts, EquipItem newParts)
    {
        ApplyPlayerParts();
    }

    [ClientRpc]
    void ApplyPlayerParts()
    {

        StopAllCoroutines(); // Stop all running coroutines before starting new ones
        Debug.Log("Applying player parts");
        if (playerParts.faceData != null && playerParts.faceData.itemSprites != null && playerParts.faceData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(face, playerParts.faceData.itemSprites, 0.5f));
            face.SetActive(true);
        }
        else
        {
            face.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Chest
        if (playerParts.chestData != null && playerParts.chestData.itemSprites != null && playerParts.chestData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(chest, playerParts.chestData.itemSprites, 0.5f));
            chest.SetActive(true);
        }
        else
        {
            chest.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Back
        if (playerParts.backData != null && playerParts.backData.itemSprites != null && playerParts.backData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(back, playerParts.backData.itemSprites, 0.5f));
            back.SetActive(true);
        }
        else
        {
            back.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Hair

        if (playerParts.hairData != null && playerParts.hairData.itemSprites != null && playerParts.hairData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(hair, playerParts.hairData.itemSprites, 0.5f));
            hair.SetActive(true);
        }
        else
        {
            hair.SetActive(false); // Disable the GameObject if the data is empty or null
        }
        
    }

    // Coroutine to animate item sprites
    IEnumerator AnimateItem(GameObject itemObject, Sprite[] sprites, float animationSpeed)
    {
        int currentSpriteIndex = 0;
        while (true) // Loop to keep the animation running
        {
            itemObject.GetComponent<SpriteRenderer>().sprite = sprites[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length; // Loop back to the first sprite after the last one
            yield return new WaitForSeconds(animationSpeed); // Wait for the specified time before switching to the next sprite
        }
    }
    void Update()
    {

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            CmdMovePlayer(moveHorizontal, moveVertical);
        }

        if (Input.GetMouseButtonDown(0))
        {
            BuildBlock(); // Existing build block logic
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            StretchArmTowards(mouseWorldPosition);
        }
    }
    void CmdMovePlayer(float moveHorizontal, float moveVertical)
    {
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb.velocity = movement * speed; // Assuming 'speed' is a defined float representing the player's speed

        // Flip the player sprite based on the movement direction
        if (moveHorizontal < 0)
        {
            SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
            playerRenderer.flipX = true;
            //for each sprite renderer in children of the player object flip the sprite
            foreach (Transform child in transform)
            {
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipX = true;
                }
            }
        }
        else if (moveHorizontal > 0)
        {
            SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
            playerRenderer.flipX = false;
            //for each sprite renderer in children of the player object flip the sprite
            foreach (Transform child in transform)
            {
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipX = false;
                }
            }
        }
    }

    Vector3Int WorldToGridPosition(Vector2 worldPosition)
    {
        // Use the Tilemap's grid to convert world position to cell position
        return tilemap.WorldToCell(worldPosition);
    }


    void BuildBlock()
    {
        if (selectedBlock() == null) return;
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = WorldToGridPosition(mouseWorldPosition);
        StartCoroutine(BuildBlockWithDelay(gridPosition));
    }

    
    IEnumerator BuildBlockWithDelay(Vector3Int gridPosition)
    {
        Vector3Int playerGridPosition = WorldToGridPosition(transform.position);

        // Calculate grid distance
        int gridDistance = Mathf.Max(Mathf.Abs(gridPosition.x - playerGridPosition.x), Mathf.Abs(gridPosition.y - playerGridPosition.y));
        int maxGridDistance = 5; // Maximum distance in grid cells

        // Check if within max grid distance
        if (gridDistance <= maxGridDistance)
        {
            Vector3 worldPosition = tilemap.GetCellCenterWorld(gridPosition);

            // Check if there is already a block at the target position
            if (tilemap.GetTile(gridPosition) != null)
            {
                Debug.Log("Cannot place block here. Tile :" + tilemap.GetTile(gridPosition).name + " already exists.");
                yield break;
            }   
            // Instantiate dust effect at the position where the block will be placed
            var dust = Instantiate(dustPrefab, worldPosition, Quaternion.identity);
            // Wait for a specified delay before placing the block
            yield return new WaitForSeconds(0.3f); // 1 second delay
            //Destroy the dust effect
            Destroy(dust);

            // Place the block at the target position

            TryPlaceBlock(gridPosition, selectedBlock().name);


            
        }
    }

    [Command]
    public void CmdBuildBlock(Vector3Int gridPosition, string blockName)
    {
        RpcBuildBlock(gridPosition, blockName);
    }

    [ClientRpc]
    public void RpcBuildBlock(Vector3Int gridPosition, string blockName)
    {
        // Find the block data by name
        BlockData blockData = Resources.Load<BlockData>("Assets/BlockData/" + blockName);
        if (blockData == null)
        {
            Debug.LogError("Block data not found for block name: " + blockName);
            
            return;
        }

        var dust = Instantiate(dustPrefab, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
        Destroy(dust, 0.3f);
        // Place the block at the target position
        tilemap.SetTile(gridPosition, blockData.tile());
    }

    void TryPlaceBlock(Vector3Int gridPosition, string blockName)
    {
        if (isLocalPlayer)
        {
            CmdBuildBlock(gridPosition, blockName);
        }
    }
    
    





    void StretchArmTowards(Vector3 targetPosition)
    {
        if (playerArm == null) return;

        Vector3 armPosition = playerArm.transform.position;
        // Assuming the arm stretches from its current position towards the target along Y-axis
        float verticalDistance = Mathf.Abs(targetPosition.y - armPosition.y);

        // Calculate the stretch factor. This example assumes a 1-to-1 ratio for simplicity.
        // You might need to adjust this calculation based on your game's scale and desired behavior.
        float defaultReach = 5f; // Default vertical reach when the arm is not stretched
        float stretchFactor = verticalDistance / defaultReach;

        // Ensure the stretch factor has a minimum value to prevent the arm from looking too thin or disappearing
        stretchFactor = Mathf.Max(stretchFactor, 1f);

        // Apply the stretch factor to the Y-axis scale of the arm
        playerArm.transform.localScale = new Vector3(playerArm.transform.localScale.x, stretchFactor, playerArm.transform.localScale.z);
    }
}

[System.Serializable]
public class EquipItem
{
    public BlockData faceData;
    
    public BlockData chestData;
    
    public BlockData backData;
    
    public BlockData hairData;

}


