using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Block : MonoBehaviour
{
    public int value;
    public Tile Tile;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro text;

    public void Init(BlockType type)
    {
        value = type.value;
        _renderer.color = type.color;
        text.text = type.value.ToString();
    }

    public void SetBlock(Tile tile)
    {
        if (Tile != null) Tile.occupiedBlock = null;
        Tile = tile;
        Tile.occupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        MergingBlock = blockToMergeWith;

        Tile.occupiedBlock = null;
        MergingBlock.Merging = true;
    }

    public bool CanMerge(int _value) => _value == value && !Merging && MergingBlock == null;
   
}
