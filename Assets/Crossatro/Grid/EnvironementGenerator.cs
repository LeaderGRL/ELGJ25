using System;
using UnityEngine;

public class EnvironementGenerator : MonoBehaviour
{
    [SerializeField] 
    private Vector2Int m_minPosBase;
    [SerializeField] 
    private Vector2Int m_maxPosBase;
    [SerializeField] 
    private Tile m_baseTilePrefab;
    

    private Board m_board;

    public bool IsStarted { get; private set; } = false;

    private void Start()
    {
        m_board = Board.GetInstance();
        IsStarted = true;
    }

    public void GenerateBase()
    {
        for (int x = m_minPosBase.x; x <= m_maxPosBase.x; x++)
        {
            for (int y = m_minPosBase.y; y <= m_maxPosBase.y; y++)
            {
                Tile newTile = Instantiate(m_baseTilePrefab, m_board.transform);
                m_board.PlaceTileRefacto(new Vector2Int(x, y), newTile);
            }
        }
        
    }
}
