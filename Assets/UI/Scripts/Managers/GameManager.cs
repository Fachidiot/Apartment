using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private SelectLevel levelSelect;
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] private StageInfo stageInfo;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Button Methods.
    public void ExitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        if (0 == Time.timeScale)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Test");
    }

    public void EndGame()
    {
        SceneManager.LoadScene("Title");
    }
}

public enum GameState
{
    Title,
    Game
}