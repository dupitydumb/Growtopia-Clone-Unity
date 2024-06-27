using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks : MonoBehaviour
{
    public BlockData blockData;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitializeBlock();
    }

    void InitializeBlock()
    {
        spriteRenderer.sprite = blockData.blockSprites.Original;

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void CheckCondition()
    {

    }
}
