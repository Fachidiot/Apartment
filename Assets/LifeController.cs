using System;
using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField] private float m_Level = 0;
    [SerializeField] private float m_Health = 100;
    [SerializeField] private float m_Stamina = 100;
    [SerializeField] private int m_Karma = 0;
    [SerializeField] private Attribute m_Attribute;

    public void AddLevel(float _value)
    {
        m_Level += _value;
    }
    public void SetHealth(float _value)
    {
        m_Health += _value;
        if (m_Health <= 0)
            GameManager.Instance.PlayerDeath();
    }
    public void SetStamina(float _value)
    {
        m_Stamina += _value;
    }
    public void SetKarma(int _value)
    {
        m_Karma += _value;
    }

    public void SetAttribute(Attribution attribution, int _value)
    {
        switch (attribution)
        {
            case Attribution.Strength:
                m_Attribute.m_Strength += _value;
            break;
            case Attribution.Intelligence:
                m_Attribute.m_Intelligence += _value;
            break;
            case Attribution.Agility:
                m_Attribute.m_Agility += _value;
            break;
            case Attribution.Luck:
                m_Attribute.m_Luck += _value;
            break;
            case Attribution.Friendly:
                m_Attribute.m_Friendly += _value;
            break;
            case Attribution.Dexterity:
                m_Attribute.m_Dexterity += _value;
            break;
            case Attribution.Charisma:
                m_Attribute.m_Charisma += _value;
            break;
        }
    }
}

[Serializable]
public class Attribute
{
    public int m_Strength;
    public int m_Intelligence;
    public int m_Agility;
    public int m_Luck;
    public int m_Friendly;
    public int m_Dexterity;
    public int m_Charisma;
}

public enum Attribution {
    None,
    Strength,
    Intelligence,
    Agility,
    Luck,
    Friendly,
    Dexterity,
    Charisma
}
