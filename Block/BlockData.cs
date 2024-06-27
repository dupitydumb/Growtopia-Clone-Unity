using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BlockData", menuName = "Block Data", order = 51)]
public class BlockData : ScriptableObject
{
    public ItemType itemType;
    public string blockName;
    public int amount;
    public BlockCondition blockCondition;
    public BlockSprites blockSprites;
    public int rarity;
    public bool isSolid;
    public bool isBackground;

    public ItemWearType itemWearType;
    public Sprite[] itemSprites;

    public Tile tile()
    {
        Tile tile = new Tile();
        tile.sprite = blockSprites.Original;
        return tile;
    }
}
[System.Serializable]
public struct BlockSprites
{
    public Sprite Original;
    public Sprite TopLeft;
    public Sprite Top;
    public Sprite TopRight;
    public Sprite Left;
    public Sprite Center;
    public Sprite Right;
    public Sprite BottomLeft;
    public Sprite Bottom;
    public Sprite BottomRight;
}
[System.Serializable]
public enum BlockCondition
{
    Original,
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}


public enum ItemType
{
    Block,
    Lock,
    Wearable,
}

public enum ItemWearType
{
    none,
    Hair,
    Face,
    Chest,
    Back,
}

