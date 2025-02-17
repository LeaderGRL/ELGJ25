using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] 
    private List<string> m_possibleWords;

    [SerializeField] 
    private int m_numberWordToAddGrid;

    [SerializeField] 
    private TestGridGenerationPanel m_gridGeneration;
    
    private static Random rng = new Random();
    private void Start()
    {
        // List<string> words = m_possibleWords.OrderBy(_ => rng.Next()).ToList();
        // var placements =
        //     CharacterPlacementGenerator.GenerateCharPlacements(words, m_numberWordToAddGrid, "");
        // m_gridGeneration.GenerateGrid(placements.CharacterPlacements);
    }
}
