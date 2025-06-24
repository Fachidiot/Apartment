
using Michsky.MUIP;
using UnityEngine;

public class MenuUIManager : MonoBehaviour
{
    [Header("Menu UI Panels")]
    [SerializeField] private GameObject titleUI;
    [SerializeField] private GameObject worldMakeUI;
    [SerializeField] private GameObject characterMakeUI;
    [SerializeField] private GameObject mapSelectUI;
    [SerializeField] private ModalWindowManager exitGameModal;
    [SerializeField] private GameObject optionUI;
    [SerializeField] private GameObject[] optionUIs;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            int isOptionEnable = IsOptionEnable();
            if (-1 != isOptionEnable)
                optionUIs[isOptionEnable].SetActive(false);
            else if (optionUI.activeSelf)
                optionUI.SetActive(false);
            else if (mapSelectUI.activeSelf)
                mapSelectUI.SetActive(false);
            else if (characterMakeUI.activeSelf)
                characterMakeUI.SetActive(false);
            else if (worldMakeUI.activeSelf)
                worldMakeUI.SetActive(false);
            else
                exitGameModal.Open();
        }
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
