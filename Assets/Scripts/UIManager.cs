using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _pauseScreen;
    public bool IsPaused;

    
    // Start is called before the first frame update
    void Start()
    {
        IsPaused = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0) return;
        _pauseButton.onClick.AddListener(TogglePause);
    }

    public void StartNew()
    {
        SceneManager.LoadScene("Game Scene");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Game Scene");
        Time.timeScale = 1f;

    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void TogglePause()
    {
        if (!IsPaused)
        {
            Time.timeScale = 0f;
            IsPaused = true;
            _pauseScreen.SetActive(true);
            _pauseButton.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f;
            IsPaused = false;
            _pauseScreen.SetActive(false);
            _pauseButton.gameObject.SetActive(true);
        }
        
    }
}
