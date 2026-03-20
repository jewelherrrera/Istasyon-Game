using UnityEngine;
using UnityEngine.SceneManagement;

namespace Istasyon.Manager
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance;

        [Header("UI")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel;

        private bool _isPaused = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;

            if (_isPaused)
                Pause();
            else
                Resume();
        }

        private void Pause()
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;                // ← freeze game
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Resume()
        {
            pauseMenuPanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            Time.timeScale = 1f;                // ← unfreeze game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnResumeButton()
        {
            Resume();
        }

        public void OnSettingsButton()
        {
            pauseMenuPanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        public void OnMainMenuButton()
        {
            Time.timeScale = 1f;               // ← reset before scene change
            SceneManager.LoadScene("MainMenu");
        }

        public void OnBackButton()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
        }
    }
}