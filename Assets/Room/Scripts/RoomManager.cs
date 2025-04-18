using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private List<string> roomStack = new List<string>();
    private string currentRoomName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadNextRoom(string roomName)
    {
        if (!string.IsNullOrEmpty(currentRoomName))
        {
            roomStack.Add(currentRoomName);
        }
        StartCoroutine(LoadRoomCoroutine(roomName));
    }

    private IEnumerator LoadRoomCoroutine(string roomName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene loadedScene = SceneManager.GetSceneByName(roomName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
            currentRoomName = roomName;
        }
    }

    public void UnloadRoom(string roomName)
    {
        if (SceneManager.GetSceneByName(roomName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(roomName);
        }
    }
}
