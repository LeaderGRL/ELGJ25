using System;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] 
    private Board m_board;

    [SerializeField] 
    private Transform m_cameraTarget;

    private void Awake()
    {
        m_board.OnGenerateGrid += OnGenerateGridCallback;
    }

    private void OnGenerateGridCallback(Grid grid)
    {
        Vector2Int gridMiddle = grid.GetMiddleGrid();
        m_cameraTarget.position = new Vector3(gridMiddle.x, 0, gridMiddle.y);
    }
}
