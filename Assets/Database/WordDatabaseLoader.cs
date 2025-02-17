using UnityEngine;
using System.IO;

public class WordDataBaseLoader : MonoBehaviour
{
    public string fileName = "Words.json";
    public WordDatabaseJSON database;

    private void Awake()
    {
        string filePath = Path.Combine(Application.dataPath + "/Database/", fileName);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            database = JsonUtility.FromJson<WordDatabaseJSON>(dataAsJson);
            Debug.Log("Chargement de la base de données de mots réussi. " + database.words.Count + " mots chargés.");
        }
        else
        {
            Debug.LogError("Fichier JSON non trouvé à: " + filePath);
        }
    }
}
