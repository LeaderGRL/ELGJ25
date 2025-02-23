using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Crossatro.UserInterface.GameOver.Scripts
{
    public class GameOverPanelController : MonoBehaviour
    {
        [SerializeField] private Timer m_timer;
        [SerializeField] private GameObject m_panel;

        private void Start()
        {
            m_timer.OnTimerFinished += OnTimerFinishedCallback;
        }

        private void OnTimerFinishedCallback()
        {
            m_panel.SetActive(true);
            Time.timeScale = 0;
        }

        public void RestartGame()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            Time.timeScale = 1;

            SceneManager.LoadScene(currentSceneName);
        }
    }
}