using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardController boardPrefab;
    [SerializeField] private Transform boardsContainer;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private GridGenerationData generationData;
    [SerializeField] private TMPro.TMP_InputField inputField;
    [SerializeField] private CoinController coinController;
    [SerializeField] private HealthController healthController;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private TimerController timerController;

    [Header("Board Settings")]
    [SerializeField] private float boardSpacing = 20f;

    private List<BoardController> activeBoards = new List<BoardController>();
    private BoardController currentActiveBoard;

    private void Awake()
    {
        if (boardsContainer == null)
            boardsContainer = transform;
    }

    private void Start()
    {
        // Initialize the first board
        CreateNewBoard(generationData);
    }

    public BoardController CreateNewBoard(GridGenerationData generationData)
    {
        Vector3 position = CalculateNextBoardPosition();
        var newBoard = Instantiate(boardPrefab, position, Quaternion.identity, boardsContainer);

        // Setup the board
        newBoard.Initialize(generationData, coinController, healthController, timerController, scoreController, inputField);

        activeBoards.Add(newBoard);
        SetActiveBoard(newBoard);

        return newBoard;
    }

    public void RemoveBoard(BoardController board)
    {
        if (activeBoards.Contains(board))
        {
            activeBoards.Remove(board);

            // If removing active board, switch to another one
            if (currentActiveBoard == board && activeBoards.Count > 0)
                SetActiveBoard(activeBoards[0]);

            Destroy(board.gameObject);

            // Rearrange remaining boards
            RearrangeBoards();
        }
    }

    private void SetActiveBoard(BoardController board)
    {
        if (!activeBoards.Contains(board))
            return;

        currentActiveBoard = board;

        // Subscribe to the OnBoardCompleted event of the current active board  
        currentActiveBoard.OnBoardCompleted.AddListener(OnBoardCompleted);

        // Focus camera on active board  
        FocusCameraOnBoard(board);
    }

    private void OnBoardCompleted(BoardController completedBoard)
    {
        CreateNewBoard(generationData);
    }

    public List<BoardController> GetActiveBoards()
    {
        return new List<BoardController>(activeBoards);
    }

    public BoardController GetActiveBoard()
    {
        return currentActiveBoard;
    }

    private Vector3 CalculateNextBoardPosition()
    {
        if (activeBoards.Count == 0)
            return boardsContainer.position;

        // Simple positioning: place boards in a row with spacing
        return new Vector3(
            boardsContainer.position.x + (activeBoards.Count * boardSpacing),
            boardsContainer.position.y,
            boardsContainer.position.z
        );
    }

    private void RearrangeBoards()
    {
        for (int i = 0; i < activeBoards.Count; i++)
        {
            Vector3 newPosition = new Vector3(
                boardsContainer.position.x + (i * boardSpacing),
                boardsContainer.position.y,
                boardsContainer.position.z
            );

            activeBoards[i].transform.position = newPosition;
        }
    }

    private void FocusCameraOnBoard(BoardController board)
    {
        if (cameraPivot == null || board == null)
            return;

        // Calculate desired camera position
        Vector3 targetPosition = new Vector3(
            board.transform.position.x,
            cameraPivot.transform.position.y,
            board.transform.position.z - 10 // Adjust for camera distance
        );

        // Could animate the camera transition here
        cameraPivot.transform.position = targetPosition;
    }
}