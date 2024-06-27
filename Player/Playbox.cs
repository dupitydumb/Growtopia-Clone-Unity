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
    public BlockData faceData;
    public GameObject chest;
    public BlockData chestData;
    public GameObject back;
    public BlockData backData;
    public GameObject hair;
    public BlockData hairData;
    private void Awake()
    {
        if (isLocalPlayer)
        {
            LocalPlayerInstance = this;
        }
        
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

        //set the player name text to playerpref
        playerNameText.text = PlayerPrefs.GetString("playerName");

        //Apply the player parts
        ApplyPlayerParts();
        inventory.OnInventoryChange += ApplyPlayerParts;
    }
    [Command]
    public void CmdSendMessage(string message)
    {
        LogUI.instance.RpcDisplayMessage(message);
    }
    
    public void ApplyPlayerParts()
    {
        //Stop the coroutine if it's already running
        StopAllCoroutines();

        faceData = inventory.GetWearableData(ItemWearType.Face);
        chestData = inventory.GetWearableData(ItemWearType.Chest);
        backData = inventory.GetWearableData(ItemWearType.Back);
        hairData = inventory.GetWearableData(ItemWearType.Hair);

        LogUI.instance.AddLog("Face Data: " + faceData);
        LogUI.instance.AddLog("Chest Data: " + chestData);
        LogUI.instance.AddLog("Back Data: " + backData);
        LogUI.instance.AddLog("Hair Data: " + hairData);
        // Face
        if (faceData != null && faceData.itemSprites != null && faceData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(face, faceData.itemSprites, 0.5f));
            face.SetActive(true);
        }
        else
        {
            face.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Chest
        if (chestData != null && chestData.itemSprites != null && chestData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(chest, chestData.itemSprites, 0.5f));
            chest.SetActive(true);
        }
        else
        {
            chest.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Back
        if (backData != null && backData.itemSprites != null && backData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(back, backData.itemSprites, 0.5f));
            back.SetActive(true);
        }
        else
        {
            back.SetActive(false); // Disable the GameObject if the data is empty or null
        }

        // Hair

        if (hairData != null && hairData.itemSprites != null && hairData.itemSprites.Length > 0)
        {
            StartCoroutine(AnimateItem(hair, hairData.itemSprites, 0.5f));
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

