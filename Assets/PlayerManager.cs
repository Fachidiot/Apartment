using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private LifeController m_LifeController;
    private AttackController m_AttackController;
    private WeaponController m_WeaponController;
    private PlayerInputs m_Input;

    void Start()
    {
        m_LifeController = GetComponent<LifeController>();
        m_AttackController = GetComponent<AttackController>();
        m_WeaponController = GetComponent<WeaponController>();

        m_Input = GameManager.Instance.GetComponent<PlayerInputs>();
    }

    void Update()
    {
        Input();
    }

    private void Input()
    {
        if (m_Input.GetAttack())
        {
            m_AttackController.DoAttack(m_WeaponController.GetCurrentWeapon());
        }
    }
}
