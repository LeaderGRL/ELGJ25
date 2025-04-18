using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BoardInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player m_player;
    [SerializeField] private Board m_board;
    [SerializeField] private HealthController m_healthController;
    [SerializeField] private CoinController m_coinController;
    [SerializeField] private ScoreController m_scoreController;
    [SerializeField] private TimerController m_timerController;

    [SerializeField] private InputActionReference m_selectAction;
    [SerializeField] private TMPro.TMP_InputField m_inputField;

    [SerializeField] private float m_inputCooldown = 0.1f;
    private float m_lastInputTime;

    private Vector2 m_currentHoverPosition;
    private GridWord m_currentSelectedWord;

    public GridWord CurrentSelectedWord => m_currentSelectedWord;

    private void Awake()
    {
        m_selectAction.action.Enable();
    }

    //private void Start()
    //{
    //    m_inputField.onValueChanged.AddListener(HandleTextInput);
    //    m_inputField.onSubmit.AddListener(ValidateWord);
    //}

    private void OnEnable()
    {
        m_selectAction.action.performed += HandleSelection;
        LetterTile.OnTileSelected += HandleTileSelected;
    }

    private void OnDisable()
    {
        m_selectAction.action.performed -= HandleSelection;
        LetterTile.OnTileSelected -= HandleTileSelected;
    }

    private void Update()
    {
        UpdateHoverPosition();
    }

    public void Initialize(Board board, HealthController healthController,
                          CoinController coinController, ScoreController scoreController,
                          TimerController timerController, TMPro.TMP_InputField inputField)
    {
        m_board = board;
        m_healthController = healthController;
        m_coinController = coinController;
        m_scoreController = scoreController;
        m_timerController = timerController;
        m_inputField = inputField;

        SetupInputField();
    }

    private void SetupInputField()
    {
        m_inputField.onValueChanged.AddListener(HandleTextInput);
        m_inputField.onSubmit.AddListener(ValidateWord);
    }

    public void SetInputField(TMPro.TMP_InputField inputField)
    {
        // Remove listeners from old field if exists
        if (m_inputField != null)
        {
            m_inputField.onValueChanged.RemoveListener(HandleTextInput);
            m_inputField.onSubmit.RemoveListener(ValidateWord);
        }

        m_inputField = inputField;

        // Add listeners to new field
        if (m_inputField != null)
        {
            m_inputField.onValueChanged.AddListener(HandleTextInput);
            m_inputField.onSubmit.AddListener(ValidateWord);
        }
    }

    private void HandleTileSelected(LetterTile tile, Board board, Vector2 position)
    {
        if (board != m_board) return;

        HandleWordSelection(board.GetWordGrid().GetWordAtLocation(position));
    }

    private void UpdateHoverPosition()
    {
        if (!Camera.main) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 100, LayerMask.GetMask("Letter", "Hover", "Select")))
        {
            m_currentHoverPosition = m_board.GetTilePosition(hit.transform.gameObject);
        }
    }

    public void HandleSelection(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var tileObject = m_board.GetTile(m_currentHoverPosition);
        if (tileObject == null) return;
        var tile = tileObject.GetComponent<LetterTile>();
        if (tile == null) return;
        tile.ManagePopup();
        HandleWordSelection(m_board.GetWordGrid().GetWordAtLocation(m_currentHoverPosition));
    }

    public void HandleWordSelection(GridWord word)
    {
        Debug.Log("Solution word: " + word.SolutionWord);
        if (m_currentSelectedWord != null && word != m_currentSelectedWord && word != null && !m_currentSelectedWord.IsValidated)
        {
            ClearPreviousSelection();
        }

        m_inputField.ActivateInputField();

        if (word != null && !word.IsValidated)
        {
            SetNewWordSelection(word);
        }
    }

    private void HandleTextInput(string text)
    {
        if (Time.time - m_lastInputTime < m_inputCooldown) return;
        m_lastInputTime = Time.time;

        UpdateWordLetters(text);
    }

    private void HandleCorrectWord()
    {
        HandleCrossLetterForNextWord();

        foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
        {
            m_board.UpdateTileState(position, TileState.Validated);
            m_scoreController.AddScore(LetterWeight.GetLetterWeight(
                m_currentSelectedWord.GetAllLetterSolutionPositions()[position]
            ));
            m_board.CheckForCoinTile(m_coinController, position);
        }

        m_scoreController.AddScore(m_currentSelectedWord.Difficulty * 10);
        m_timerController.AddTime(m_timerController.timer.additionalTimeOnCorrectWord);
        m_coinController.AddCoins(1);
        m_currentSelectedWord.Validate();
        ResetSelection();
    }

    private void HandleCrossLetterForNextWord()
    {
        var letters = m_currentSelectedWord.GetAllLetterSolutionPositions();
        foreach (var letter in letters)
        {
            m_board.GetTile(letter.Key).layer = LayerMask.NameToLayer("Validate");
            var wordsAtLocaiton = m_board.GetWordGrid().GetAllWordAtLocation(letter.Key);
            foreach (var word in wordsAtLocaiton)
            {
                word.SetLetterAtLocation(letter.Key, letter.Value);
            }
        }
    }

    private void HandleIncorrectWord()
    {
        m_healthController.RemoveHealth(10);
        ResetSelection();
    }

    private void UpdateWordLetters(string inputText)
    {
        if (m_currentSelectedWord == null) return;

        inputText = inputText.ToUpper();
        var currentWordLetters = m_currentSelectedWord.GetAllLetterCurrentWordPositions();
        var unlockedLetters = currentWordLetters
            .Where(kvp => !IsTileLocked(kvp.Key))
            .ToList();

        var lettersBeforeUpdate = m_currentSelectedWord.GetAllLetterCurrentWordPositions();
        // Update word letters
        for (int i = 0; i < unlockedLetters.Count; i++)
        {
            var pos = unlockedLetters[i].Key;
            var newChar = i < inputText.Length ? inputText[i] : '\0';
            if (newChar == lettersBeforeUpdate[pos])
            {
             continue;   
            }
            m_currentSelectedWord.SetLetterAtLocation(pos, newChar);
            UpdateTileVisual(pos, newChar);
        }
    }

    public void UpdateTileVisual(Vector2 position, char character)
    {
        var tile = m_board.GetTile(position);
        if (tile != null && tile.TryGetComponent<LetterTile>(out var letterTile))
        {
            letterTile.DisplayText.text = character.ToString();
            letterTile.PlayJumpAnimation();
        }
    }

    private void ClearPreviousSelection()
    {
        foreach (var position in m_currentSelectedWord.GetAllLetterSolutionPositions().Keys)
        {
            m_board.UpdateTileState(position, TileState.Default);
        }
    }

    private void SetNewWordSelection(GridWord word)
    {
        m_currentSelectedWord = word;
        SetupInputFieldForWord(word);
        UpdateTileStatesForSelection(word);
    }

    private void SetupInputFieldForWord(GridWord word)
    {
        var unlockedTiles = word.GetAllLetterCurrentWordPositions()
            .Where(kvp => !m_board.IsTileLocked(kvp.Key))
            .ToList();

        //string inputText = "";
        //foreach (var pos in word.GetAllLetterSolutionPositions().Keys.OrderBy(p => p.x + p.y))
        //{
        //    inputText += IsTileLocked(pos) ? " " : word.GetCurrentLetterAtLocation(pos);
        //}

        m_inputField.text = string.Join("", unlockedTiles.Select(kvp => kvp.Value));
        m_inputField.characterLimit = unlockedTiles.Count;
        m_inputField.caretPosition = m_inputField.text.Length;
    }

    private void UpdateTileStatesForSelection(GridWord word)
    {
        foreach (var position in word.GetAllLetterSolutionPositions().Keys)
        {
            if (!IsTileLocked(position))
            {
                m_board.UpdateTileState(position, TileState.Selected);
            }
        }
    }

    public void ResetSelection()
    {
        m_currentSelectedWord = null;
        m_inputField.text = "";
    }

    public bool IsTileLocked(Vector2 position)
    {
        return m_board.GetWordGrid().GetAllWordAtLocation(position)?
            .Any(word => word.IsValidated) ?? false;
    }

    private void ValidateWord(string inputText)
    {
        if (m_currentSelectedWord == null) return;

        bool isComplete = m_currentSelectedWord.GetCurrentWordToString() ==
                         m_currentSelectedWord.SolutionWord;

        if (isComplete)
        {
            HandleCorrectWord();
        }
        else
        {
            HandleIncorrectWord();
        }

        ResetInputField();
    }

    private void ResetInputField()
    {
        m_inputField.text = "";
        m_inputField.DeactivateInputField();
    }



}