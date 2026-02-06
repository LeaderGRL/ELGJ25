using UnityEngine;
using UnityEngine.Android;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Initializing,
        Playing,
        Shopping,
        Paused,
        GameOver,
    }

    private void Awake()
    {
        InitializeServices();
    }

    private void Start()
    {
        SubscribeToEvents();
        StartGame();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeServices()
    {

    }

    private void SubscribeToEvents()
    {

    }

    private void UnsubscribeFromEvents()
    {

    }

    private void StartGame()
    {

    }

    private void StartTurn()
    {

    }

    private void EndTurn()
    {

    }

    private void OnBoardCompleted()
    {

    }
}
