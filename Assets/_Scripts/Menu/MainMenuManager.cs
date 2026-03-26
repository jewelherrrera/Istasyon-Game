using UnityEngine;
using UnityEngine.SceneManagement;

namespace Istasyon.Manager
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource musicSource;        // ← NEW
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip backgroundMusic;      // ← NEW

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowMainMenu();

            if (musicSource != null && backgroundMusic != null) // ← NEW
            {                                                   // ← NEW
                musicSource.clip = backgroundMusic;             // ← NEW
                musicSource.Play();                             // ← NEW
            }                                                   // ← NEW
        }

        public void OnPlayButton()
        {
            PlayClick();
            SceneManager.LoadScene("Tutorial");
        }

        public void OnSettingsButton()
        {
            PlayClick();
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }

        public void OnCreditsButton()
        {
            PlayClick();
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        public void OnExitButton()
        {
            PlayClick();
            Application.Quit();
        }

        public void OnBackButton()
        {
            PlayClick();
            ShowMainMenu();
        }

        private void PlayClick()
        {
            if (audioSource != null && buttonClickSound != null)
                audioSource.PlayOneShot(buttonClickSound);
        }

        private void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
        }
    }
}