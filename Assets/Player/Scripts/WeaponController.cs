using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;

    private int m_weaponIndex;

    public Weapon GetCurrentWeapon()
    {
        return weapons[m_weaponIndex];
    }
}

[Serializable]
public class Weapon
{
    public string m_Name;
    public WeaponType m_Type;
    public bool m_CanThrowable;
}

public enum WeaponType
{
    None,
    Melee,
    Bow,
    Gun,
    Grenade
}
