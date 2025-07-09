using System;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [Header("Base Attack")]
    [SerializeField] private Attack _baseAttack;
    [SerializeField] private Weapon _currentWeapon;

    public void DoAttack(Weapon _weapon = null)
    {
        if (null != _weapon)
        {   // 무기가 있을때
            Debug.Log(_weapon);
        }
        else
        {   // 무기가 없을때
            Debug.Log(_baseAttack);
        }
    }
}

[Serializable]
public class Attack
{
    public float m_Amount = 1;
    public AttackState m_State;
    public float m_StateTime;
    public float m_CoolTime;
}

public enum AttackState {
    None,
    Bleeding,
    Injured,
    Infected,

}