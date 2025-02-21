using System;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] 
    private CrossWordGridGenerator m_crossGridGenerator;

    [SerializeField] 
    private Transform m_cameraTarget;

    private void Awake()
    {
        m_crossGridGenerator.OnEndGridGeneration += OnGenerateGridCallback;
    }

    private void OnGenerateGridCallback(Grid grid)
    {
        Vector2Int gridMiddle = grid.GetMiddleGrid();
        m_cameraTarget.position = new Vector3(gridMiddle.x, 0, gridMiddle.y);
    }
}
