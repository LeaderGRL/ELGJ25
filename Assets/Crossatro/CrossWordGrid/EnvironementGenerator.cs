using System;
using System.Collections.Generic;
using Crossatro.Data;
using UnityEngine;

public class EnvironementGenerator : MonoBehaviour
{
    [SerializeField] 
    private Vector2Int m_minPosBase;
    [SerializeField] 
    private Vector2Int m_maxPosBase;
    [SerializeField] 
    private Tile m_baseTilePrefab;

    [SerializeField] 
    private PlayerCameraController m_pCameraController;

    [SerializeField] 
    private NoiseMapGenerationData m_noiseMapData;


    private int m_noiseMapSeed = 0;
    
    private Board m_board;

    public bool IsStarted { get; private set; } = false;

    private void Start()
    {
        m_board = Board.GetInstance();
        //m_pCameraController.OnTargetCameraMove += OnTargetCameraMoveCallback;
        m_noiseMapSeed = UnityEngine.Random.Range(0, 100);
        IsStarted = true;
    }

    public void GenerateBase()
    {
        for (int x = m_minPosBase.x; x <= m_maxPosBase.x; x++)
        {
            for (int y = m_minPosBase.y; y <= m_maxPosBase.y; y++)
            {
                Tile newTile = Instantiate(m_baseTilePrefab, m_board.transform);
                var tilePos = new Vector2Int(x, y);
                float noiseValue = GetNoiseMapValueAtCoord(tilePos);
                newTile.GetComponent<MeshRenderer>().material = GetMaterialByNoiseValue(noiseValue);
                m_board.PlaceTileRefacto(tilePos, newTile);
            }
        }
        
    }

    private void OnTargetCameraMoveCallback(Vector3 targetCameraPosition)
    {
        Vector2Int gridPositionTargetCamera = 
            new Vector2Int(Mathf.RoundToInt(targetCameraPosition.x), Mathf.RoundToInt(targetCameraPosition.z));

        List<Vector2Int> nonExistentTilesAtPositions = GetAllNonExistentTilesAtPositions(gridPositionTargetCamera);
        m_board.ResetDoTweenDelay();
        foreach (var tilePos in nonExistentTilesAtPositions)
        {
            Tile newTile = Instantiate(m_baseTilePrefab, m_board.transform);
            float noiseValue = GetNoiseMapValueAtCoord(tilePos);
            newTile.GetComponent<MeshRenderer>().material = GetMaterialByNoiseValue(noiseValue);
            m_board.PlaceTileRefacto(tilePos, newTile);
        }
    }

    private List<Vector2Int> GetAllNonExistentTilesAtPositions(Vector2Int position)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = position.x + m_minPosBase.x ; x <= position.x + m_maxPosBase.x; x++)
        {
            for (int y =position.y  + m_minPosBase.y; y <= position.y +  m_maxPosBase.y; y++)
            {
                var tilePosition = new Vector2Int(x, y);
                if (m_board.GetTile(tilePosition) == null)
                {
                    result.Add(tilePosition);
                }
            }
        }

        return result;
    }

    private Material GetMaterialByNoiseValue(float noiseValue)
    {
        int i = 0;
        Material material = null;
        while (i < m_noiseMapData.NoiseValueDatas.Count)
        {
            if (m_noiseMapData.NoiseValueDatas[i].NoiseValue < noiseValue)
            {
                material = m_noiseMapData.NoiseValueDatas[i].AssociatedMaterial;
            }

            i++;
        }

        if (material == null)
        {
            material = m_noiseMapData.NoiseValueDatas[m_noiseMapData.NoiseValueDatas.Count - 1].AssociatedMaterial;
        }

        return material;
    }


    private float GetNoiseMapValueAtCoord(Vector2Int coord)
    {
        return Mathf.PerlinNoise(m_noiseMapSeed + (coord.x / m_noiseMapData.ZoomValue), m_noiseMapSeed + (coord.y/m_noiseMapData.ZoomValue));
    }
}
