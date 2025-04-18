using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CrossWordGridGenerator : MonoBehaviour
{
    [SerializeField] private GridGenerationData m_generationData;

    [SerializeField] private LetterTile m_letterTilePrefab;
    [SerializeField] private ShopTile m_shopTilePrefab;
    [SerializeField] private CoinTile m_coinTilePrefab;

    [SerializeField] private int m_shopTilePercentageApparition = 100/25;

    private CrossWordsGameGrid m_crossWordsGameGrid;

    public Board m_board;
    public bool IsStarted { get; private set; } = false;

    public event Action<CrossWordsGameGrid> OnEndGridGeneration = null;

    private void Awake()
    {
        //m_crossWordsGameGrid = CharacterPlacementGenerator.GenerateCharPlacements(m_generationData.Database,
        //m_generationData.NumWordsToGenerate, "");
    }

    //private void Start()
    //{
    //    foreach (var word in m_crossWordsGameGrid.Words)
    //    {
    //        GenerateWord(word);
    //    }
    //    m_crossWordsGameGrid.OnValidateAllWorlds += OnValidateAllWordsCallback;
    //    m_crossWordsGameGrid.OnAddWord += GenerateWord;
    //    m_board.SetGrid(m_crossWordsGameGrid);
    //    IsStarted = true;
    //}

    public void Start()
    {
        //GenerateBase();
    }

    public void SetGenerationData(GridGenerationData data)
    {
        m_generationData = data;
    }

    public void SetBoard(Board board)
    {
        m_board = board;
    }

    public void GenerateBase()
    {
        m_crossWordsGameGrid = CharacterPlacementGenerator.GenerateCharPlacements(
            m_generationData.Database,
            m_generationData.NumWordsToGenerate,
            "");

        foreach (var word in m_crossWordsGameGrid.Words)
        {
            GenerateWord(word);
        }

        m_crossWordsGameGrid.OnValidateAllWorlds += OnValidateAllWordsCallback;
        m_crossWordsGameGrid.OnAddWord += GenerateWord;
        m_board.SetGrid(m_crossWordsGameGrid);

        OnEndGridGeneration?.Invoke(m_crossWordsGameGrid);
    }

    private void OnValidateAllWordsCallback(GridWord lastWord)
    {
        //CharacterPlacementGenerator.GenrateCharPlacementsForExistingGrid(
        //    m_generationData.Database, 5, "", m_crossWordsGameGrid, lastWord, (() => OnEndGridGeneration?.Invoke(m_crossWordsGameGrid)));

    }

    private void GenerateWord(GridWord newWord)
    {
        //m_board.ResetDoTweenDelay();
        foreach (var letterLocation in newWord.GetAllLetterSolutionPositions())
        {
            if (m_board == null)
            {
                Debug.LogError("Board is null");
            }

            if (letterLocation.Key == null)
            {
                Debug.LogError("Tile is null");
            }

            GameObject tileObject = m_board.GetTile(letterLocation.Key);
            if (tileObject!= null && tileObject.GetComponent<LetterTile>() != null)
            {
                continue;
            }
            LetterTile newTile = Instantiate(Random.Range(1, 101) <= m_shopTilePercentageApparition ? m_coinTilePrefab : m_letterTilePrefab, transform);
            newTile.board = m_board;
            newTile.SetPopupPosAndRotByIsRow(newWord.IsRow);
            newTile.DisplayText.text = "";
            m_board.PlaceTile(letterLocation.Key, newTile);
        }

    }

    public CrossWordsGameGrid GetCrossWordsGameGrid()
    {
        return m_crossWordsGameGrid;
    }
}
