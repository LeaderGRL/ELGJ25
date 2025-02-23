using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GridWord
{
    public Vector2Int StartPosition;
    public string SolutionWord;
    private Dictionary<Vector2Int, char> m_currentWord = new ();
    public bool IsRow;
    public string Description = "";
    public int Difficulty;

    private HashSet<Vector2Int> _validatedPositions = new();

    public bool IsValidated { get; private set; }

    public event Action<GridWord> OnValidate;

    public void Initialize()
    {
        m_currentWord.Clear();
        IsValidated = false;
        foreach (var solutionLetter in GetAllLetterSolutionPositions())
        {
            m_currentWord[solutionLetter.Key] = '\0';
        }
    }
    
    public Dictionary<Vector2Int, char> GetAllLetterSolutionPositions()
    {
        Dictionary<Vector2Int, char> result = new();
        for (int i = 0; i < SolutionWord.Length; i++)
        {
            result[IsRow ? new Vector2Int(StartPosition.x + i, StartPosition.y) : 
                new Vector2Int(StartPosition.x , StartPosition.y - i)] = SolutionWord[i];
        }

        return result;
    }

    public Dictionary<Vector2Int, char> GetAllLetterCurrentWordPositions()
    {
        return m_currentWord
            .Where(kvp => !IsPositionValidated(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public string GetCurrentWordToString()
    {
        string result = "";

        foreach (var letterLocation in m_currentWord)
        {
            result += letterLocation.Value;
        }
        return result;
    }

    public char GetCurrentLetterAtLocation(Vector2Int location)
    {
        return m_currentWord[location];
    }

    public void  SetLetterAtLocation(Vector2Int location, char letter)
    {
        m_currentWord[location] = letter;
    }

    public void Validate()
    {
        IsValidated = true;
        OnValidate?.Invoke(this);
    }

    public void ValidatePosition(Vector2Int position)
    {
        _validatedPositions.Add(position);

        // Vérifier si tout le mot est validé
        if (GetAllLetterSolutionPositions().Keys.All(_validatedPositions.Contains))
        {
            Validate();
        }
    }

    public bool IsPositionValidated(Vector2Int position)
    {
        return _validatedPositions.Contains(position);
    }


}
