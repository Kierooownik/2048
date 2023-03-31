using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float _travelTime = 0.2f;
    [SerializeField] private int winCondition = 2048;
    [SerializeField] private GameObject winScreen, loseScreen;

    private List<Tile> tiles;
    private List<Block> blocks;
    private GameState state;
    private int round;

    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.value == value);

    // Start is called before the first frame update
    void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void Update()
    {
        if (state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) { Shift(Vector2.left); };
        if (Input.GetKeyDown(KeyCode.RightArrow)) { Shift(Vector2.right); };
        if (Input.GetKeyDown(KeyCode.UpArrow)) { Shift(Vector2.up); };
        if (Input.GetKeyDown(KeyCode.DownArrow)) { Shift(Vector2.down); };
    }

    private void ChangeState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            case GameState.Lose:
                winScreen.SetActive(true);
                break;

        }
    }

    void GenerateGrid()
    {
        round = 0;
        tiles = new List<Tile>();
        blocks = new List<Block>();
        for (int x=0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                tiles.Add(tile);
            }
        }

        var center = new Vector2(width / 2 - 0.5f, height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    void SpawnBlocks(int amount)
    {
        var freeTiles = tiles.Where(n => n.occupiedBlock == null).OrderBy(b => Random.value).ToList() ;
        
        foreach(var tile in freeTiles.Take(amount))
        {
            SpawnBlock(tile, Random.value > 0.8f ? 4 : 2);
        }


        if (freeTiles.Count() == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }
        ChangeState(blocks.Any(b=>b.value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Tile tile, int value)
    {
        var block = Instantiate(blockPrefab, tile.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(tile);
        blocks.Add(block);
    }

    void Shift( Vector2 direction)
    {
        ChangeState(GameState.Moving);
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (direction == Vector2.right || direction == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Tile;
            do
            {
                block.SetBlock(next);
                var possibleTile = GetTileAtPosition(next.Pos + direction);
                if(possibleTile != null)
                {
                    if (possibleTile.occupiedBlock != null && possibleTile.occupiedBlock.CanMerge(block.value))
                    {
                        block.MergeBlock(possibleTile.occupiedBlock);
                    }
                    else if (possibleTile.occupiedBlock == null) next = possibleTile;
                }

            } while (next != block.Tile);

            

        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Tile.Pos : block.Tile.Pos;

            sequence.Insert(0, block.transform.DOMove(block.Tile.Pos, _travelTime));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b=>b.MergingBlock != null))
            {
                MergeBlocks(block.MergingBlock, block);
            }
            ChangeState(GameState.SpawningBlocks);
        });
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Tile, baseBlock.value * 2);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Tile GetTileAtPosition(Vector2 pos)
    {
        return tiles.FirstOrDefault(n => n.Pos == pos);
    }
}

[Serializable]
public struct BlockType
{
    public int value;
    public Color color;
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}