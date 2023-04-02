using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    //Width & height of game board
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;

    //References to tile, board and block prefabs
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private Block blockPrefab;

    //List of block types
    [SerializeField] private List<BlockType> types;

    //Time needed for a block to move to another tile
    [SerializeField] private float _travelTime = 0.2f;

    //Winning condition
    [SerializeField] private int winCondition = 2048;

    //Game over screens
    [SerializeField] private GameObject winScreen, loseScreen;

    //Lists of tiles and blocks currently in game
    private List<Tile> tiles;
    private List<Block> blocks;
    
    //Current state of the game & round counter
    private GameState state;
    private int round;

    //Returning block type with gives value
    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.value == value);

    // Start is called before the first frame update
    void Start()
    {
        // Start the game by generating the grid
        ChangeState(GameState.GenerateLevel);
    }

    private void Update()
    {
        // If a game is not waiting for input, return
        if (state != GameState.WaitingInput) return;

        //Input handling based on keyboard arrows 
        if (Input.GetKeyDown(KeyCode.LeftArrow)) { Shift(Vector2.left); };
        if (Input.GetKeyDown(KeyCode.RightArrow)) { Shift(Vector2.right); };
        if (Input.GetKeyDown(KeyCode.UpArrow)) { Shift(Vector2.up); };
        if (Input.GetKeyDown(KeyCode.DownArrow)) { Shift(Vector2.down); };
    }

    //Changing current state of the game and performs actions based on the new state
    private void ChangeState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            //Generate game board
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            // Spawn blocks
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1);
                break;
            // Wait for user input
            case GameState.WaitingInput:      
                break;
            //Move blocks
            case GameState.Moving:
                break;
            // Display the win screen
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            // Display the lose screen
            case GameState.Lose:
                loseScreen.SetActive(true);
                break;

        }
    }

    //Generates game grid and initializes the game board & tiles
    void GenerateGrid()
    {
        round = 0;            
        
        //Create new lists of tiles & blocks
        tiles = new List<Tile>();
        blocks = new List<Block>();

        //Instantiate tiles for game grid
        for (int x=0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tile = Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity);
                tiles.Add(tile);
            }
        }

        //Create and position the game board in the center of grid
        var center = new Vector2(width / 2 - 0.5f, height / 2 - 0.5f);
        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        //Move camera to the center of grid
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        //Change state to spawning blocks
        ChangeState(GameState.SpawningBlocks);
    }

    //Spawn given amount of blocks on game board
    void SpawnBlocks(int amount)
    {
        //Get a list of all tiles that don't have a block on them & order them randomly
        var freeTiles = tiles.Where(n => n.occupiedBlock == null).OrderBy(b => Random.value).ToList() ;
        
        //Spawn blocks on the selected tiles with 80% chance for value to be "2"
        foreach(var tile in freeTiles.Take(amount))
        {
            SpawnBlock(tile, Random.value > 0.8f ? 4 : 2);
        }

        //If there is only 1 tile left, the game is over
        if (freeTiles.Count() == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }

        //If there is a block with value equals winCondition, the game is won, otherwise wait for user input
        ChangeState(blocks.Any(b=>b.value == winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    //Spawn a new block with specific value at specific tile
    void SpawnBlock(Tile tile, int value)
    {
        //Create new block and initialize it with specific value and color
        var block = Instantiate(blockPrefab, tile.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));

        //Set the block to occupy specified tile
        block.SetBlock(tile);

        //Add the block to list of blocks on the game board
        blocks.Add(block);
    }

    //Shifting the blocks in game board in given direction
    void Shift( Vector2 direction)
    {
        //Change the game state to 'Moving' to indicate that blocks are currently in move
        ChangeState(GameState.Moving);

        //Order the blocks firstly by their x position, and then by y
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();

        //If the shift direction is to the right or up, reverse the order of list
        if (direction == Vector2.right || direction == Vector2.up) orderedBlocks.Reverse();

        // For each block in ordered block list, move it to the next available tile
        foreach (var block in orderedBlocks)
        {
            var next = block.Tile;
            //Keep moving the block to next available tile until it has reached 
            do
            {
                block.SetBlock(next);

                //Check if there is a tile in the given direction
                var possibleTile = GetTileAtPosition(next.Pos + direction);
                
                //If there is a tile and it is occupied by another block, check if it blocks can merge
                if(possibleTile != null)
                {
                    if (possibleTile.occupiedBlock != null && possibleTile.occupiedBlock.CanMerge(block.value))
                    {
                        block.MergeBlock(possibleTile.occupiedBlock);
                    }
                    //If the tile is not occupied, set the next tile to be the tile for current block
                    else if (possibleTile.occupiedBlock == null) next = possibleTile;
                }

            } while (next != block.Tile);

            

        }

        //Use DOTween to animate blocks movement
        var sequence = DOTween.Sequence();

        //For each block in ordered blocks list, insert a move animation
        foreach (var block in orderedBlocks)
        {
            //If the block is merging with another block, move it to the merging block's position
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Tile.Pos : block.Tile.Pos;

            sequence.Insert(0, block.transform.DOMove(block.Tile.Pos, _travelTime));
        }

        //Once the move animation sequence is complete, merge any blocks
        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b=>b.MergingBlock != null))
            {
                MergeBlocks(block.MergingBlock, block);
            }
            //Change the game state to indicate that the next round of blocks can be spawned
            ChangeState(GameState.SpawningBlocks);
        });
    }

    //Merge two blocks by removing two merging blocks and generating new one with doubled value
    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Tile, baseBlock.value * 2);

        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    //Remove block from game board
    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    //Get a tile at given position
    Tile GetTileAtPosition(Vector2 pos)
    {
        return tiles.FirstOrDefault(n => n.Pos == pos);
    }
}

//Struct representing block type with value & color
[Serializable]
public struct BlockType
{
    public int value;
    public Color color;
}

//Enum with all possible game states
public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}