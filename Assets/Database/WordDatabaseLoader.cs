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
            Debug.Log("Chargement de la base de donn�es de mots r�ussi. " + database.words.Count + " mots charg�s.");
        }
        else
        {
            Debug.LogError("Fichier JSON non trouv� �: " + filePath);
        }
    }
}
