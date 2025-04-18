using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BoardController : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private Board board;
    [SerializeField] private BoardInputHandler inputHandler;
    [SerializeField] private CrossWordGridGenerator gridGenerator;

    [Header("Game Systems")]
    [SerializeField] private HealthController healthController;
    [SerializeField] private CoinController coinController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private TimerController timerController;

    [Header("UI Components")]
    [SerializeField] private TMPro.TMP_InputField inputField;

    [Header("Events")]
    public UnityEvent<BoardController> OnBoardInitialized;
    public UnityEvent<BoardController> OnBoardCompleted;

    public Board Board => board;
    public BoardInputHandler InputHandler => inputHandler;
    public CrossWordGridGenerator GridGenerator => gridGenerator;
    public HealthController HealthController => healthController;
    public CoinController CoinController => coinController;
    public ScoreController ScoreController => scoreController;
    public TimerController TimerController => timerController;

    private bool isInitialized = false;

    private void Awake()
    {
        // Validate components
        ValidateComponents();
    }

    public void Initialize(GridGenerationData generationData, CoinController coinCtrontroller, HealthController healthController, TimerController timerController, ScoreController scoreController, TMPro.TMP_InputField inputField)
    {
        if (isInitialized)
            return;

        SetInputField(inputField);
        this.coinController = coinCtrontroller;
        this.healthController = healthController;
        this.timerController = timerController;
        this.scoreController = scoreController;

        // Link components together
        ConnectComponents();

        // Set up the grid generator
        gridGenerator.SetGenerationData(generationData);

        // Subscribe to board completion event
        gridGenerator.OnEndGridGeneration += HandleGridGeneration;

        // Generate the grid
        gridGenerator.GenerateBase();

        isInitialized = true;
        OnBoardInitialized?.Invoke(this);
    }

    private void ValidateComponents()
    {
        if (board == null)
            board = GetComponentInChildren<Board>();

        if (inputHandler == null)
            inputHandler = GetComponentInChildren<BoardInputHandler>();

        if (gridGenerator == null)
            gridGenerator = GetComponentInChildren<CrossWordGridGenerator>();

        if (healthController == null)
            healthController = GetComponentInChildren<HealthController>();

        if (coinController == null)
            coinController = GetComponentInChildren<CoinController>();

        if (scoreController == null)
            scoreController = GetComponentInChildren<ScoreController>();

        if (timerController == null)
            timerController = GetComponentInChildren<TimerController>();
    }

    private void ConnectComponents()
    {
        // Set board references
        //board.SetInputHandler(inputHandler);
        //board.SetCoinController(coinController);

        // Set grid generator references
        gridGenerator.SetBoard(board);

        Debug.Log(board);
        // Set input handler references
        inputHandler.Initialize(board, healthController, coinController,
                               scoreController, timerController, inputField);

        // Puzzle completion tracking
        if (board.GetWordGrid() != null)
        {
            board.GetWordGrid().OnValidateAllWorlds += HandlePuzzleCompletion;
        }
    }

    private void HandleGridGeneration(CrossWordsGameGrid grid)
    {
        // Subscribe to all words validated event on the new grid
        grid.OnValidateAllWorlds += HandlePuzzleCompletion;
    }

    private void HandlePuzzleCompletion(GridWord lastWord)
    {
        Debug.Log($"Board completed with word: {lastWord.SolutionWord}");
        OnBoardCompleted?.Invoke(this);

        // Additional completion logic here
        //scoreController.CalculateFinalScore(timerController.GetRemainingTime());
    }

    public void SetInputField(TMPro.TMP_InputField newInputField)
    {
        inputField = newInputField;
        if (inputHandler != null)
        {
            inputHandler.SetInputField(inputField);
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (gridGenerator != null)
            gridGenerator.OnEndGridGeneration -= HandleGridGeneration;

        if (board != null && board.GetWordGrid() != null)
            board.GetWordGrid().OnValidateAllWorlds -= HandlePuzzleCompletion;
    }
}