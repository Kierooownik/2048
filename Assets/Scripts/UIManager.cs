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

    //Start new game, go to game scene from menu
    public void StartNew()
    {
        SceneManager.LoadScene("Game Scene");
    }

    //Restart game in pause menu
    public void RestartGame()
    {
        SceneManager.LoadScene("Game Scene");
        Time.timeScale = 1f;

    }

    //Exit game or quit unity editor
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    //Go to main menu from pause menu
    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        IsPaused = true;
        _pauseScreen.SetActive(true);
        _pauseButton.gameObject.SetActive(false);
    }
    public void UnpauseGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        _pauseScreen.SetActive(false);
        _pauseButton.gameObject.SetActive(true);
    }
}
