using Crossatro.Events;
using Crossatro.Grid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Crossatro.Board
{
    /// <summary>
    /// Coordinates board gameplay logic.
    /// Connects input events to game action, manage word selection and validation.
    /// </summary>
    public class BoardController: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Board Components")]
        [SerializeField] private Board _board;
        [SerializeField] private BoardInputHandler _inputHandler;

        [Header("UI")]
        [Tooltip("Text input field for typing words")]
        [SerializeField] private TMPro.TMP_InputField _inputField;

        // ============================================================
        // State
        // ============================================================

        /// <summary>
        /// The crossword grid data with words solution, position, validation...
        /// </summary>
        private CrossWordsGameGrid _grid;

        /// <summary>
        /// Currently selected word.
        /// </summary>
        private GridWord _currentSelectedWord;

        /// <summary>
        /// Timestamp of last processed text input.
        /// </summary>
        private float _lastInputTime;

        /// <summary>
        /// Whether the controller has been initialized.
        /// </summary>
        private bool _isInitialized;

        // ============================================================
        // Properties
        // ============================================================

        /// <summary>
        /// The currently selected word.
        /// </summary>
        public GridWord CurrentSelectedWord => _currentSelectedWord;

        /// <summary>
        /// The crossword grid data.
        /// </summary>
        public CrossWordsGameGrid Grid => _grid;

        /// <summary>
        /// The Board component managed by this controller.
        /// </summary>
        public Board Board => _board;

        // ============================================================
        // Initialization
        // ============================================================

        /// <summary>
        /// Initialize the controller with a crossword grid.
        /// Call this after the grid has been generated and placed on the board.
        /// </summary>
        /// <param name="grid"></param>
        public void Initialize(CrossWordsGameGrid grid)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[BoardController] Already initialized. Call Cleanup() first.");
                return;
            }

            if (grid == null)
            {
                Debug.LogError("[BoardController] Cannot initialize with null grid!");
                return;
            }

            _grid = grid;

            SubscribeToInputEvents();
            SetupInputField();
            SubscribeToGridEvents();

            _isInitialized = true;
            Debug.Log("[BoardController] Initialized");
        }

        /// <summary>
        /// Cleanup subscription and reset state.
        /// Call before re-initializing with a new grid.
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromInputEvents();
            UnsubscribeFromGridEvents();
            CleanupInputField();
            ClearSelection();

            _grid = null;
            _isInitialized = false;
        }

        // ============================================================
        // Event subscriptions
        // ============================================================

        private void SubscribeToInputEvents()
        {
            if (_inputHandler == null ) return;

            _inputHandler.OnTileClicked += HandleTileClicked;
        }

        private void UnsubscribeFromInputEvents()
        {
            if (_inputHandler == null) return;

            _inputHandler.OnTileClicked -= HandleTileClicked;
        }

        private void SubscribeToGridEvents()
        {
            if (_grid == null ) return;

            _grid.OnValidateAllWords += HandleAllWordsValidated;
        }

        private void UnsubscribeFromGridEvents()
        {
            if (_grid == null ) return;

            _grid.OnValidateAllWords -= HandleAllWordsValidated;
        }

        private void SetupInputField()
        {
            if (_inputField == null) return;

            _inputField.onValueChanged.AddListener(HandleTextInput);
            _inputField.onSubmit.AddListener(HandleWordSubmit);
        }

        private void CleanupInputField()
        {
            if (_inputField == null) return;

            _inputField.onValueChanged.RemoveListener(HandleTextInput);
            _inputField.onSubmit.RemoveListener(HandleWordSubmit);
        }

        // ============================================================
        // Lifecycle
        // ============================================================

        private void OnDestroy()
        {
            Cleanup();
        }

        // ============================================================
        // Tile Click
        // ============================================================

        /// <summary>
        /// Called when BoardInputHandler detects a tile click.
        /// Find a word at that position and select it.
        /// </summary>
        /// <param name="gridPosition">Position of the tile clicked</param>
        private void HandleTileClicked(Vector2 gridPosition)
        {
            if (_grid == null) return;

            GridWord word = _grid.GetWordAtLocation(gridPosition);
            if (word == null || word.IsValidated) return;

            SelectWord(word, gridPosition);
        }

        /// <summary>
        /// Select a word for tje player to attempt.
        /// Updates tile visuals, show the clue popup and activate the input field.
        /// </summary>
        /// <param name="word"></param>
        /// <param name="clickedPosition"></param>
        private void SelectWord(GridWord word, Vector2 clickedPosition)
        {
            // If a different word was selected, deselect the previous one
            if (_currentSelectedWord != null && _currentSelectedWord != word)
            {
                DeselectCurrentWord();
            }

            _currentSelectedWord = word;

            HighlightSelectedWord();

            // Show clue popup above the clicked tile
            string clue = word.Description ?? "";
            _board.ShowCluePopup(clickedPosition, clue);

            ConfigureInputField(word);
        }

        /// <summary>
        /// Remove selection visuals from the current word.
        /// </summary>
        private void DeselectCurrentWord()
        {
            if (_currentSelectedWord == null) return;

            var positions = _currentSelectedWord.GetAllLetterSolutionPositions().Keys;
            foreach ( var position in positions )
            {
                // Reset tiles from the previous selected word to default state.
                _board.SetTileState(position, TileState.Default);
            }
        }

        /// <summary>
        /// Set all tiles of the selected word to selected state.
        /// Skip tiles that are already validated.
        /// </summary>
        private void HighlightSelectedWord()
        {
            if ( _currentSelectedWord == null) return;

            var positions = _currentSelectedWord.GetAllLetterCurrentWordPositions().Keys;
            foreach (Vector2 position in positions )
            {
                if (!IsPositionLocked(position))
                {
                    _board.SetTileState(position, TileState.Selected);
                }
            }
        }

        // ============================================================
        // Text Input 
        // ============================================================
        
        /// <summary>
        /// Called when the player typed in the input field.
        /// Update the current word's letters and tile visuals.
        /// </summary>
        /// <param name="text"></param>
        private void HandleTextInput(string text)
        {
            if (_currentSelectedWord == null) return;

            UpdateWordLetters(text.ToUpper());
        }

        /// <summary>
        /// Apply typed text to the current word's position.
        /// </summary>
        /// <param name="inputText"></param>
        private void UpdateWordLetters(string inputText)
        {
            // Get only unlocked positions
            var unlockedPositions = GetUnlockedPosition(_currentSelectedWord);

            for (int i = 0; i < unlockedPositions.Count; i++)
            {
                Vector2 position = unlockedPositions[i];
                char newChar = i < inputText.Length ? inputText[i] : '\0';

                // Only update if the character actually changed
                char currentChar = _currentSelectedWord.GetCurrentLetterAtLocation(position);
                if (newChar == currentChar) continue;
                
                _currentSelectedWord.SetLetterAtLocation(position, newChar);
                _board.UpdateTileLetter(position, newChar);
            }
        }

        // ============================================================
        // Word Submission
        // ============================================================

        /// <summary>
        /// Called when the player press Enter/Submit
        /// Check if the current word matches the solution.
        /// </summary>
        /// <param name="inputText"></param>
        private void HandleWordSubmit(string inputText)
        {
            if (_currentSelectedWord == null) return;

            string currentWord = _currentSelectedWord.GetCurrentWordToString();
            bool isCorrect = currentWord == _currentSelectedWord.SolutionWord;

            if (isCorrect)
                HandleCorrectWord();
            else
                HandleIncorrectWord();

            ResetInputField();
        }

        // ============================================================
        // Correct Word Handler
        // ============================================================

        /// <summary>
        /// - Propagate validated letters to intersecting words
        /// - Set tile state to validate
        /// - Calculate and publish score
        /// - Check for special tile (coins, heal...)
        /// - Validated the GridWord
        /// - Publish events
        /// </summary>
        private void HandleCorrectWord()
        {
            GridWord word = _currentSelectedWord;
            var letterPosition = word.GetAllLetterSolutionPositions();
        
            PropagateLettersToIntersection(letterPosition);

            _board.SetTileStates(letterPosition.Keys, TileState.Validated);

            int letterScore = CalculateLetterScore(letterPosition);
            
            int coinReward = CalculateCoinReward(letterPosition.Keys);

            word.Validate();

            ClearSelection();

            PublishWordCompleted(word, letterScore, coinReward);

            Debug.Log($"[BoardController] Correct: \"{word.SolutionWord}\" " + $"(score: {letterScore}, coins: {coinReward}");
        }

        /// <summary>
        /// Set validated letters on all intersecting words.
        /// </summary>
        /// <param name="letterPositions">Intersection position</param>
        private void PropagateLettersToIntersection(Dictionary<Vector2, char> letterPositions)
        {
            foreach (var kvp in letterPositions)
            {
                Vector2 pos = kvp.Key;  
                char letter = kvp.Value;

                // Find all word at this position
                var wordAtPos = _grid.GetAllWordAtLocation(pos);
                if (wordAtPos == null) continue;

                foreach (GridWord intersectiongWord in wordAtPos)
                {
                    // Set the letter on the crossing word
                    intersectiongWord.SetLetterAtLocation(pos, letter);

                    // Lock this position so it can't be modified from others words
                    intersectiongWord.ValidatePosition(pos);
                    
                }

                // Update the tile's layer to Validate
                _board.SetTileState(pos, TileState.Validated);
            }
        }


        /// <summary>
        /// Calculate the total letter weight score for a word.
        /// </summary>
        /// <param name="letterPositions">Position of each tile of a validated word</param>
        /// <returns></returns>
        private int CalculateLetterScore(Dictionary<Vector2, char> letterPositions)
        {
            int total = 0;

            foreach (char letter in letterPositions.Values)
            {
                total += LetterWeight.GetLetterWeight(letter);
            }

            return total;
        }

        private int CalculateCoinReward(IEnumerable<Vector2> positions)
        {
            int total = 0;

            foreach (Vector2 pos in positions)
            {
                CoinTile cointile = _board.GetTileAt(pos) as CoinTile;
                if (cointile != null)
                {
                    total += cointile.CoinValue;
                }
            }

            return total;
        }

        // ============================================================
        // Incorrect word handler
        // ============================================================

        /// <summary>
        /// Clear selection.
        /// </summary>
        private void HandleIncorrectWord()
        {
            ClearSelection();

            Debug.Log("[BoardController] Incorrect word!");
        }

        // ============================================================
        // Letter Reveal
        // ============================================================

        /// <summary>
        /// Reveal all instance of a specific letter across the grid.
        /// </summary>
        /// <param name="letter">Revealed letters</param>
        public void RevealLetter(char letter)
        {
            if (_grid == null) return;

            List<Vector2> revealedPositions = _grid.RevealLetterInAllWords(letter);

            foreach (Vector2 pos in revealedPositions)
            {
                // Update the tile visual with the revealed letter
                char revealedChar = _grid.GetCurrentLetterAtPosition(pos);
                _board.UpdateTileLetter(pos, revealedChar);
                _board.SetTileState(pos, TileState.Validated);
            }

            Debug.Log($"[BoardController] Revealed '{letter}' at {revealedPositions.Count} positions");
        }

        // ============================================================
        // Selection management
        // ============================================================

        /// <summary>
        /// Clear the current word selection, reset input field and hide popup.
        /// </summary>
        public void ClearSelection()
        {
            DeselectCurrentWord();
            _currentSelectedWord = null;
            _board.HideCluePopup();
            ResetInputField();
        }

        // ============================================================
        // Input field management
        // ============================================================

        /// <summary>
        /// Configure the input field for the selected word.
        /// Sets character limit to the number of unlocked tiles and pre-fills alredy typed letters.
        /// </summary>
        /// <param name="word">Selected word</param>
        private void ConfigureInputField(GridWord word)
        {
            if (_inputField == null) return;

            var unlockedPositions = GetUnlockedPosition(word);

            // Pre-fill with existing letters
            string existingText = "";
            foreach (Vector2 pos in unlockedPositions)
            {
                char c = word.GetCurrentLetterAtLocation(pos);
                existingText += c == '\0' ? "" : c.ToString();
            }

            _inputField.characterLimit = unlockedPositions.Count;
            _inputField.text = existingText;
            _inputField.caretPosition = _inputField.text.Length;
            _inputField.ActivateInputField();
        }

        /// <summary>
        /// Clear and deactivate the input field.
        /// </summary>
        private void ResetInputField()
        {
            if (_inputField == null) return;

            _inputField.text = "";
            _inputField.characterLimit = 0;
            _inputField.DeactivateInputField();
        }

        /// <summary>
        /// Set a new input field reference.
        /// </summary>
        /// <param name="inputField"></param>
        public void SetInputField(TMPro.TMP_InputField inputField)
        {
            CleanupInputField();
            _inputField = inputField;

            if (_isInitialized)
                SetupInputField();
        }

        // ============================================================
        // Position Helper
        // ============================================================

        /// <summary>
        /// Check if a grid position is locked (Validated word).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsPositionLocked(Vector2 position)
        {
            if (_grid == null) return false;

            var words = _grid.GetAllWordAtLocation(position);
            if (words == null) return false;

            foreach (GridWord word in words)
            {
                // Fully validated word => Lock all positions
                if (word.IsValidated) return true;

                // Individually validated letter => Lock letter position
                if (word.IsPositionValidated(position)) return true;
            }

            return false;
            //return words.Any(w => w.IsValidated);
        }

        /// <summary>
        /// Get all unlocked position of a word.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private List<Vector2> GetUnlockedPosition(GridWord word)
        {
            return word.GetAllLetterCurrentWordPositions()
                .Where(kvp => !IsPositionLocked(kvp.Key))
                .Select(kvp => kvp.Key)
                .OrderBy(pos => word.IsRow ? pos.x: -pos.y)
                .ToList();
        }

        // ============================================================
        // Grid Completion
        // ============================================================

        /// <summary>
        /// Called when all words in the grid are validated.
        /// </summary>
        /// <param name="lastWord"></param>
        private void HandleAllWordsValidated(GridWord lastWord)
        {
            Debug.Log("[BoardController] All words validated!");

            _board.HideCluePopup();

            EventBus.Instance.Publish(new BoardCompletedEvent
            {
                BoardIndex = 0,
                BonusReward = 0,
            });
        }

        // ============================================================
        // Event publishing
        // ============================================================

        /// <summary>
        /// Publish events when a word is completed correctly.
        /// </summary>
        /// <param name="word">Validated word</param>
        /// <param name="score">Score win with this word</param>
        /// <param name="coins">Coins win with this word</param>
        private void PublishWordCompleted(GridWord word, int score, int coins)
        {
            EventBus.Instance.Publish(new WorldCompletedEvent
            {
                Word = word.SolutionWord,
                Damage = score,
                Coins = coins,
            });

            EventBus.Instance.Publish(new ScoreChangedEvent
            {
                NewScore = score,
                Delta = score,
            });

            if (coins > 0)
            {
                EventBus.Instance.Publish(new CoinsChangedEvent
                {
                    NewAmount = coins,
                    Delta = coins,
                });
            }
        }
    }
}
