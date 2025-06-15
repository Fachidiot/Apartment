using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectLevel : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private Difficulty difficulty = Difficulty.Easy;

    public void Prev()
    {
        if (Difficulty.Easy == difficulty)
            return;
        --difficulty;
        ChangeText();
    }

    public void Next()
    {
        if (Difficulty.God == difficulty)
            return;
        ++difficulty;
        ChangeText();
    }

    private void ChangeText()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                tmpText.text = "���� �ʺ���";
                break;
            case Difficulty.Normal:
                tmpText.text = "���� �Թ���";
                break;
            case Difficulty.Difficult:
                tmpText.text = "���� �߱���";
                break;
            case Difficulty.Hard:
                tmpText.text = "���� ������";
                break;
            case Difficulty.God:
                tmpText.text = "���� ������";
                break;
        }
    }
}

[Serializable]
public enum Difficulty
{
    None,
    Easy,
    Normal,
    Difficult,
    Hard,
    God
}
