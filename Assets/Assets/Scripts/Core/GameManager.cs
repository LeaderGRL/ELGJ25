using Crossatro.Events;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.XR;

/// <summary>
/// GameManager is responsible for
/// - Initialize all services in the correct order
/// - Manage Game State
/// - React to game event
/// 
/// This should be the first script to execute
/// </summary>
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [Header("Configuration")]
    [SerializeField] private GameConfig _config;

    [Header("Services")]
    [SerializeField] private PlayerDataService _playerDataService;

    public enum GameState
    {
        Initializing,
        Playing,
        Shopping,
        Paused,
        GameOver,
        Victory,
    }

    private GameState _currentState = GameState.Initializing;
    private GameState _previousState;
    private int _currentTurn = 0;

    /// <summary>
    /// Current Game State
    /// </summary>
    public GameState CurrentState => _currentState;

    /// <summary>
    /// Current turn number
    /// </summary>
    public int CurrentTurn => _currentTurn;

    private void Awake()
    {
        if (_instance != null & _instance != this)
        {
            Debug.LogWarning($"[GameManager] Duplicate instance destroyed!");
            Destroy(gameObject);
            return;
        }

        _instance = this;

        InitializeCoreServices();
    }

    private void Start()
    {
        SubscribeToEvents();
        RegisterServices();
        StartGame();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Initialize core services that don't depend on scene objectS.
    /// Called in Awake() to ensure they exist before any Start() methods.
    /// </summary>
    private void InitializeCoreServices()
    {
        // Maje sure Service Locator and EventBus are initializes
        _ = ServiceLocator.Instance;
        _ = EventBus.Instance;

        Debug.Log("[GameManager] Core services initialized");
    }

    /// <summary>
    /// Register all scenes services with the ServiceLocator
    /// </summary>
    private void RegisterServices()
    {
        // Register GameConfig to allow all systems to acces to settings
        if (_config != null)
        {
            ServiceLocator.Instance.Register(_config);
            Debug.Log("[GameManager] Registered: GameConfig");
        }
        else
        {
            Debug.LogError("[GameManager] GameConfig is not assigned!");
        }

        if (_playerDataService != null)
        {
            ServiceLocator.Instance.Register(_playerDataService);
            Debug.Log("[GameManager] Registered: PlayerDataService");
        }
        else
        {
            Debug.LogError("[GameManager] PlayerDataService is not assigned!");
        }

        // Register the GameManager itself so others scripts can access to game state
        ServiceLocator.Instance.Register(this);
        Debug.Log("[GameManager] All services registered");
    }

    /// <summary>
    /// Subscribe to all events the GameManager needs to react to.
    /// </summary>
    private void SubscribeToEvents()
    {
        EventBus.Instance.Subscribe<PlayerDamageEvent>(OnPlayerDamaged);
        //EventBus.Instance.Subscribe<BoardCompletedEvent>(OnBoardCompleted)
    }

    /// <summary>
    /// Unsubscribe to all events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (EventBus.Instance == null) return;

        EventBus.Instance.Unsubscribe<PlayerDamageEvent>(OnPlayerDamaged);
    }

    /// <summary>
    /// Start a new game.
    /// Initialize stats and begin the first turn.
    /// </summary>
    private void StartGame()
    {
        Debug.Log("[GameManager] Starting game...");

        // Initialize player with config values
        if (_playerDataService != null &&  _config != null)
        {
            _playerDataService.Initialize(_config.StartingHealth, _config.StartingCoin);
        }

        // Reset turn counter
        _currentTurn = 0;

        // Notify all systems that the game has started
        EventBus.Instance.Publish(new GameStartedEvent
        {
            StartingHealth = _config?.StartingHealth ?? 100,
            StartingCoins = _config?.StartingCoin ?? 5,
        });

        StartNewTurn();
    }

    /// <summary>
    /// Begin a new turn
    /// </summary>
    private void StartNewTurn()
    {
        _currentTurn++;

        ChangeState(GameState.Playing);

        EventBus.Instance.Publish(new TurnStartedEvent
        {
            TurnNumber = _currentTurn,
            TimeRemaining = _config?.TurnDuration ?? 120f,
        });

        Debug.Log($"[GameManager] Turn {_currentTurn} started");
    }

    /// <summary>
    /// End the current turn
    /// </summary>
    private void EndTurn()
    {
        EventBus.Instance.Publish(new TurnEndedEvent
        {
            TurnNumber = _currentTurn,
        });
    }

    // ============================================================
    // Game State Management
    // ============================================================

    /// <summary>
    /// Change the game state
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

        Debug.Log($"[GameManager] State change: {_currentState} -> {newState}");

        OnExitState(_currentState);

        _previousState = _currentState;
        _currentState = newState;

        OnEnterNewState(newState);
    }

}
