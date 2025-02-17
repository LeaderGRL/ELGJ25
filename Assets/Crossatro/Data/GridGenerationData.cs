using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "GridGenerationData", menuName = "Scriptable Objects/GridGenerationData")]
public class GridGenerationData : ScriptableObject
{

    [field: SerializeField] 
    public int NumWorToGenerate { get; private set; }
    
    public string FileName = "Words.json";
    public WordDatabaseJSON Database;
    [Button("Load Database")]
    public void LoadDataBase()
    {
        string filePath = Path.Combine(Application.dataPath + "/Database/", FileName);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            Database = JsonUtility.FromJson<WordDatabaseJSON>(dataAsJson);
            Debug.Log("Chargement de la base de donn�es de mots r�ussi. " + Database.words.Count + " mots charg�s.");
            foreach (var word in Database.words)
            {
                word.word = RemoveDiacritics(word.word);
                word.word = word.word.ToUpper();
                
            }
        }
        else
        {
            Debug.LogError("Fichier JSON non trouv� �: " + filePath);
        }
        
    }
    
    static string RemoveDiacritics(string text) 
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}
