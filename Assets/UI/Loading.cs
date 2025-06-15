using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{
    [Header("Loading")]
    [SerializeField] private float loadCompleteTime = 5.0f;
    [SerializeField] private Animation image;
    [SerializeField] private LoadingText loadingText;
    [Header("Loading Complete")]
    [SerializeField] private GameObject complete;
    [SerializeField] private GameObject completeTMP;
    [Header("NextScene")]
    [SerializeField] private GameObject nextScene;

    private bool loadingDone = false;
    private void Start()
    {
        StartCoroutine(LoadingStart());
    }
    private void Update()
    {
        if (Input.anyKey && loadingDone)
            NextScene();
    }

    public void NextScene()
    {
        nextScene.SetActive(true);
        gameObject.SetActive(false);
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
