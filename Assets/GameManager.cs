using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
