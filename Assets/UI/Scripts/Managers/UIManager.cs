using System.Collections;
using System.Collections.Generic;
using Michsky.MUIP;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [Header("Game UI Panels")]
    [SerializeField] private GameObject HUD_UI;
    [SerializeField] private GameObject mobile_UI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameInfo pause_GameInfo;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject[] optionUIs;

    private void Update()
    {
        Inputs();
    }

    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !pauseUI.activeSelf)
            InventoryToggle();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            int isOptionEnable = IsOptionEnable();
            if (-1 != isOptionEnable)
                optionUIs[isOptionEnable].SetActive(false);
            else if (optionUI.activeSelf)
                optionUI.SetActive(false);
            else if (pauseUI.activeSelf) {
                pauseUI.SetActive(false);
                GameManager.Instance.PauseGame();
            }
            else if (inventoryUI.activeSelf)
                inventoryUI.SetActive(false);
            else
            {
                pauseUI.SetActive(true);
                GameManager.Instance.PauseGame();
            }
        }
    }

    public void InventoryToggle()
    {
        if (inventoryUI.activeSelf)
            inventoryUI.SetActive(false);
        else
            inventoryUI.SetActive(true);
    }

    public void GameManager_PauseGame()
    {
        GameManager.Instance.PauseGame();
    }

    private int IsOptionEnable()
    {
        for (int i = 0; i < optionUIs.Length; ++i)
        {
            if (optionUIs[i].activeSelf)
                return i;
        }
        return -1;
    }
}