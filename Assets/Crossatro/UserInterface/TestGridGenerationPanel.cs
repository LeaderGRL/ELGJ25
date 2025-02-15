using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestGridGenerationPanel : MonoBehaviour
{
    [SerializeField] 
    private HorizontalLayoutGroup m_horizontalLayoutGroup;
    [SerializeField] 
    private VerticalLayoutGroup m_verticalLayoutGroupPrefab;

    [SerializeField] 
    private Image m_gridCellPrefab;
    
    public void GenerateGrid(Dictionary<Vector2Int, char> characterPlacement)
    {
        var MinMaxpos = GetMinAndMaxPositionCharacterPlacement(characterPlacement);
        Vector2Int size = new Vector2Int(MinMaxpos.Value.x - MinMaxpos.Key.x, MinMaxpos.Value.y - MinMaxpos.Key.y);
        for (int i = 0; i <= size.x + 2; i++)
        {
            var verticalLayoutGroup = Instantiate(m_verticalLayoutGroupPrefab, m_horizontalLayoutGroup.transform);
            for (int j = size.y; j > - 2; j--)
            {
                var gridCell = Instantiate(m_gridCellPrefab, verticalLayoutGroup.transform);
                Vector2Int currentKey = new Vector2Int(i + MinMaxpos.Key.x, j + MinMaxpos.Key.y);
                if (characterPlacement.ContainsKey(currentKey))
                {
                    gridCell.GetComponentInChildren<TextMeshProUGUI>().text = characterPlacement[currentKey].ToString();
                }
            }
        }
    }

    private KeyValuePair<Vector2Int, Vector2Int> GetMinAndMaxPositionCharacterPlacement(Dictionary<Vector2Int, char> characterPlacement)
    {
        Vector2Int minValue = Vector2Int.zero;
        Vector2Int maxValue = Vector2Int.zero;
        foreach (var  key in characterPlacement.Keys)
        {
            if (key.x < minValue.x)
            {
                minValue.x = key.x;
            }
            if (key.y < minValue.y)
            {
                minValue.y = key.y;
            }
            if (key.x > maxValue.x)
            {
                maxValue.x = key.x;
            }
            if (key.y > maxValue.y)
            {
                maxValue.y = key.y;
            }
        }
        return new KeyValuePair<Vector2Int, Vector2Int>(minValue, maxValue);
    }
    
}
