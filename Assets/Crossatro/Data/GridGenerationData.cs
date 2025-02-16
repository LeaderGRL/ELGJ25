using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridGenerationData", menuName = "Scriptable Objects/GridGenerationData")]
public class GridGenerationData : ScriptableObject
{
    [field: SerializeField] 
    public List<string> PossibleWords { get; private set; }

    [field: SerializeField] 
    public int NumWorToGenerate { get; private set; }
}
