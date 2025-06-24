using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private float loadCompleteTime = 5.0f;
    [SerializeField] private Animation image;
    [SerializeField] private LoadingText loadingText;
    [Header("Loading Complete")]
    [SerializeField] private GameObject complete;
    [SerializeField] private GameObject completeTMP;
    [Header("PrevScene")]
    [SerializeField] private GameObject prevScene;

    private bool loadingDone = false;
    public void Load()
    {
        loadingText.LoadingStart();
        StartCoroutine(LoadingStart());
    }
    private void Update()
    {
        if (Input.anyKey && loadingDone)
            NextScene();
    }

    public void NextScene()
    {
        GameManager.Instance.StartGame();
        prevScene.SetActive(false);
        AudioManager.Instance.PlayBackgroundMusic();
        gameObject.SetActive(false);

        Init();
    }

    private void Init()
    {
        loadingText.gameObject.SetActive(true);
        image.Play();
        image.gameObject.SetActive(true);

        complete.SetActive(false);
        completeTMP.SetActive(false);
        loadingDone = false;
    }

    IEnumerator LoadingStart()
    {
        yield return new WaitForSeconds(loadCompleteTime);

        Debug.Log("Complete");
        loadingText.LoadingDone();
        image.Stop();
        image.gameObject.SetActive(false);

        complete.SetActive(true);
        completeTMP.SetActive(true);
        loadingDone = true;
    }
}
