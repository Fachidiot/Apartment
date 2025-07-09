using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon[] _weapons;

    private int _weaponIndex;

    public Weapon GetCurrentWeapon()
    {
        if (_weapons.Length <= _weaponIndex || -1 > _weaponIndex)
            return null;
        return _weapons[_weaponIndex];
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
