using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WordData
{
    public string word;
    public string description1;
    public string description2;
    public string category;
    public int difficulty;
}

[System.Serializable]
public class WordDatabaseJSON
{
    public List<WordData> words;
}
