using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] private OptionKeyData keyData;

// Movement
    public bool GetMoveLeft()
    {
        return Input.GetKey(keyData.m_KeyMoveLeft);
    }
    public bool GetMoveRight()
    {
        return Input.GetKey(keyData.m_KeyMoveRight);
    }
    public bool GetMoveUp()
    {
        return Input.GetKey(keyData.m_KeyMoveUp);
    }
    public bool GetMoveDown()
    {
        return Input.GetKey(keyData.m_KeyMoveDown);
    }
    public bool GetJump()
    {
        return Input.GetKeyDown(keyData.m_KeyJump);
    }
    public bool GetSprint()
    {
        return Input.GetKey(keyData.m_KeySprint);
    }
    public bool GetCrouch()
    {
        return Input.GetKey(keyData.m_KeyCrouch);
    }

    // Attack
    public bool GetAttack()
    {
        return Input.GetKey(keyData.m_Attack);
    }
    
    public bool GetAimed()
    {
        return Input.GetKey(keyData.m_Aimed);
    }

// Interact
    public bool GetInteract()
    {
        return Input.GetKeyDown(keyData.m_KeyInteract);
    }
    public bool GetInventory()
    {
        return Input.GetKeyDown(keyData.m_KeyInventory);
    }
    public bool GetEscape()
    {
        return Input.GetKeyDown(keyData.m_KeyEscape);
    }

    void Start()
    {
        keyData = OptionDataManager.Instance.OptionData.m_keyData;
    }

    void Update()
    {
        if (keyData != OptionDataManager.Instance.OptionData.m_keyData)
            keyData = OptionDataManager.Instance.OptionData.m_keyData;
    }
}
