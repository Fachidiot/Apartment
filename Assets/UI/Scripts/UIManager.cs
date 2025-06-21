using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    private bool inventoryToggle;

    private void Start()
    {

    }

    private void Update()
    {
        Inputs();
    }

    private void Inputs()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
            InventoryToggle();
    }

    public void InventoryToggle()
    {
        inventoryToggle = !inventoryToggle;

        inventoryUI.SetActive(inventoryToggle);
    }
}
