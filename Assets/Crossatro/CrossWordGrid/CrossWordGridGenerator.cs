using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CrossWordGridGenerator : MonoBehaviour
{
    [SerializeField] 
    private GridGenerationData m_generationData;

    [SerializeField] private LetterTile m_letterTilePrefab;
    [SerializeField] private ShopTile m_shopTilePrefab;
    [SerializeField] private CoinTile m_coinTilePrefab;

    [SerializeField] private int m_shopTilePercentageApparition = 100/25;

    
    private Board m_board = null;
    private CrossWordsGameGrid m_crossWordsGameGrid;
    public bool IsStarted { get; private set; } = false;

    public event Action<CrossWordsGameGrid> OnEndGridGeneration = null;

    private void Awake()
    {
        m_crossWordsGameGrid = CharacterPlacementGenerator.GenerateCharPlacements(m_generationData.Database,
        m_generationData.NumWordsToGenerate, "");

        Debug.Log("CrossWordsGameGrid: " + m_crossWordsGameGrid);
    }
    private void Start()
    {
        m_board = Board.GetInstance();

        m_crossWordsGameGrid.OnValidateAllWorlds += OnValidateAllWordsCallback;
        m_crossWordsGameGrid.OnAddWord += GenerateWord;
        m_board.SetGrid(m_crossWordsGameGrid);
        IsStarted = true;

    }

    public void GenerateBase()
    {
        foreach (var word in m_crossWordsGameGrid.Words)
        {
            GenerateWord(word);
        }
        OnEndGridGeneration?.Invoke(m_crossWordsGameGrid);
        
    }

    private void OnValidateAllWordsCallback(GridWord lastWord)
    {
        CharacterPlacementGenerator.GenrateCharPlacementsForExistingGrid(
            m_generationData.Database, 5, "", m_crossWordsGameGrid, lastWord, (() => OnEndGridGeneration?.Invoke(m_crossWordsGameGrid)));

    }

    private void GenerateWord(GridWord newWord)
    {
        m_board.ResetDoTweenDelay();
        foreach (var letterLocation in newWord.GetAllLetterSolutionPositions())
        {
            GameObject tileObject = m_board.GetTile(letterLocation.Key);
            if (tileObject!= null && tileObject.GetComponent<LetterTile>() != null)
            {
                continue;
            }
            LetterTile newTile = Instantiate(Random.Range(1, 101) <= m_shopTilePercentageApparition ? m_coinTilePrefab : m_letterTilePrefab, transform);
            newTile.SetPopupPosAndRotByIsRow(newWord.IsRow);
            newTile.DisplayText.text = "";
            m_board.PlaceTileRefacto(letterLocation.Key, newTile);
        }

    }

    public CrossWordsGameGrid GetCrossWordsGameGrid()
    {
        if (m_crossWordsGameGrid == null)
        {
            Debug.Log("CrossWordsGameGrid is nullllllllllllllllllllllllllll");
        }
        return m_crossWordsGameGrid;
    }
}
