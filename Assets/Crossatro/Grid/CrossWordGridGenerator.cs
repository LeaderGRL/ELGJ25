using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CrossWordGridGenerator : MonoBehaviour
{
    [SerializeField] 
    private GridGenerationData m_generationData;

    [SerializeField] 
    private LetterTile m_letterTilePrefab;
    [SerializeField] 
    private ShopTile m_shopTilePrefab;
    
    private Board m_board = null;
    private Grid m_grid;
    public bool IsStarted { get; private set; } = false;

    public event Action<Grid> OnEndGridGeneration = null;

    private void Start()
    {
        m_board = Board.GetInstance();
        m_grid = CharacterPlacementGenerator.GenerateCharPlacements(m_generationData.Database,
            m_generationData.NumWordsToGenerate, "");
        m_grid.OnValidateAllWorlds += OnValidateAllWordsCallback;
        m_grid.OnAddWord += GenerateWord;
        m_board.SetGrid(m_grid);
        IsStarted = true;

    }

    public void GenerateBase()
    {
        foreach (var letterLocation in m_grid.GetWordsToGridValues())
        {
            LetterTile newTile = Instantiate(Random.Range(0, 100) == 50 ? m_shopTilePrefab : m_letterTilePrefab, transform);
            newTile.DisplayText.text = "";
            m_board.PlaceTileRefacto(letterLocation.Key, newTile);
        } 
        OnEndGridGeneration?.Invoke(m_grid);
        
    }

    private void OnValidateAllWordsCallback(GridWord lastWord)
    {
        CharacterPlacementGenerator.GenrateCharPlacementsForExistingGrid(m_generationData.Database, 5, "", m_grid, lastWord);
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
            LetterTile newTile = Instantiate(Random.Range(0, 100) == 50 ? m_shopTilePrefab : m_letterTilePrefab, transform);
            newTile.DisplayText.text = "";
            m_board.PlaceTileRefacto(letterLocation.Key, newTile);
        }
        OnEndGridGeneration?.Invoke(m_grid);

    }
}
