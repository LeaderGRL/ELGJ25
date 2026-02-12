    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;


    public class GridWord_old
    {
        public Vector2 StartPosition;
        public string SolutionWord;
        private Dictionary<Vector2, char> m_currentWord = new ();
        public bool IsRow;
        public string Description = "";
        public int Difficulty;
        public float offset = 0.2f;

        private HashSet<Vector2> _validatedPositions = new();

        public bool IsValidated { get; private set; }

        public event Action<GridWord_old> OnValidate;

        public void Initialize()
        {
            m_currentWord.Clear();
            IsValidated = false;
            foreach (var solutionLetter in GetAllLetterSolutionPositions())
            {
                m_currentWord[solutionLetter.Key] = '\0';
            }
        }
    
        public Dictionary<Vector2, char> GetAllLetterSolutionPositions()
        {
            Dictionary<Vector2, char> result = new();
            for (int i = 0; i < SolutionWord.Length; i++)
            {
                result[IsRow ? new Vector2(StartPosition.x + i, StartPosition.y) : 
                    new Vector2(StartPosition.x , StartPosition.y - i)] = SolutionWord[i];
            }

            return result;
        }

        public Dictionary<Vector2, char> GetAllLetterCurrentWordPositions()
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

        public char GetCurrentLetterAtLocation(Vector2 location)
        {
            return m_currentWord[location];
        }

        public void  SetLetterAtLocation(Vector2 location, char letter)
        {
            m_currentWord[location] = letter;
        }

        public void Validate()
        {
            IsValidated = true;
            OnValidate?.Invoke(this);
        }

        public void ValidatePosition(Vector2 position)
        {
            _validatedPositions.Add(position);

            // Vérifier si tout le mot est validé
            if (GetAllLetterSolutionPositions().Keys.All(_validatedPositions.Contains))
            {
                Validate();
            }
        }

        public bool IsPositionValidated(Vector2 position)
        {
            return _validatedPositions.Contains(position);
        }


    }
