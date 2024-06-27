using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemsDict : MonoBehaviour
{
    // Define the Item structure
    public struct Item
    {
        public string Name;
        public int ID;

        public Item(string name, int id, Tile tile = null)
        {
            Name = name;
            ID = id;
            tile = tile;
        }
    }

    // Create the dictionary
    public Dictionary<string, Item> itemsByTile = new Dictionary<string, Item>();

    public List<Tile> tiles = new List<Tile>();
    void Start()
    {
        // Add items to the dictionary
        itemsByTile.Add("100", new Item("Dirt", 100, tiles[0]));
        itemsByTile.Add("101", new Item("Bedrock", 101, tiles[1]));
        itemsByTile.Add("102", new Item("Red Block", 102, tiles[2]));
    }

    // Method to access an item by tile ID
    public Item GetItemByTileID(string tileID)
    {
        Item item;
        itemsByTile.TryGetValue(tileID, out item);
        return item;
    }

    // Example usage
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Example: Get the item for "tile1"
            Item item = GetItemByTileID("tile1");
            Debug.Log("Item: " + item.Name);
        }
    }
}