using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Michsky.MUIP;
using System.Collections.Generic;
using System.Linq;
using System;

public class OptionManager : MonoBehaviour
{
    [Header("Screen Setting")]
    [SerializeField] private Toggle m_VSync;
    [SerializeField] private HorizontalSelector m_Fullscreen;
    [SerializeField] private Slider m_Brightness;
    [SerializeField] private CustomDropdown m_Resolution;
    public CustomDropdown Resolution{get{return m_Resolution;}}

    [Header("Graphic Setting")]
    [SerializeField] private TMP_Dropdown m_QualityDropdown;
    [SerializeField] private TMP_Dropdown m_ShadowDropdown;
    [SerializeField] private TMP_Dropdown m_AODropdown;
    [SerializeField] private TMP_Dropdown m_ReflectionDropdown;

    [Header("Sound Setting")]
    [SerializeField] private Slider m_MasterSoundSlider;
    [SerializeField] private Slider m_BGMSoundSlider;
    [SerializeField] private Slider m_EffectSoundSlider;

    [Header("Gameplay Setting")]
    [SerializeField] private TMP_Dropdown m_LanguageDropdown;
    [SerializeField] private Toggle m_ScreenVibration;

    [Header("Shortcut Setting")]
    [SerializeField] private KeyCode m_KeyMoveLeft;
    [SerializeField] private KeyCode m_KeyMoveRight;
    [SerializeField] private KeyCode m_KeyMoveForward;
    [SerializeField] private KeyCode m_KeyMoveBack;

    [SerializeField] private KeyCode m_KeyEscape;
    [SerializeField] private KeyCode m_KeyInventory;

    private List<Resolution> resolutions = new List<Resolution>();

    public void InitMenuLayouts()
    {
        Resolution[] temp = Screen.resolutions;
        HashSet<CustomDropdown.Item> options = new HashSet<CustomDropdown.Item>();
        // build시 중복되는 해상도가 만들어지는 오류.
        string prevItem = "";
        int index = 0;
        for (int i = 0; i < temp.Length; ++i)
        {
            if (prevItem != temp[i].width + "x" + temp[i].height)
            {
                resolutions.Add(temp[i]);
                CustomDropdown.Item item = new CustomDropdown.Item();
                item.itemName = temp[i].width + "x" + temp[i].height;
                item.itemIndex = index++;

                options.Add(item);
                prevItem = item.itemName;
            }
        }
        m_Resolution.items = new List<CustomDropdown.Item>(options);
        m_Resolution.selectedItemIndex = OptionDataManager.Instance.OptionData.m_ScreenResolution;
        m_Resolution.SetupDropdown();

        m_MasterSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_MasterVolume);
        m_BGMSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_BgmVolume);
        m_EffectSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_EffectVolume);

        // m_QualityDropdown.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_GraphicQuality);

        // switch (OptionDataManager.Instance.OptionData.m_Language)
        // {
        //     case SystemLanguage.Korean:
        //         {
        //             m_LanguageDropdown.SetValueWithoutNotify(0);
        //             break;
        //         }
        //     case SystemLanguage.Japanese:
        //         {
        //             m_LanguageDropdown.SetValueWithoutNotify(2);
        //             break;
        //         }
        //     case SystemLanguage.English:
        //     default:
        //         {
        //             m_LanguageDropdown.SetValueWithoutNotify(1);
        //             break;
        //         }
        // }
    }

    public void SetResolution(int resolutionIndex)
    {
        // 주사율 계산
        RefreshRate refreshRate = new RefreshRate();
        refreshRate.numerator = (uint)Math.Round(OptionDataManager.Instance.OptionData.m_RefreshRate) * 1000;
        refreshRate.denominator = 1001;
        Debug.Log(refreshRate.value);

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, OptionDataManager.Instance.OptionData.m_FullScreenMode, refreshRate);
        OptionDataManager.Instance.OptionData.m_ScreenResolution = resolutionIndex;
        OptionDataManager.Instance.SaveOptionData();
    }

    public void SetFullScreenMode(int fullScreenMode)
    {
        OptionDataManager.Instance.OptionData.m_FullScreenMode = (FullScreenMode)(fullScreenMode + 1);
        OptionDataManager.Instance.SaveOptionData();
        SetResolution(OptionDataManager.Instance.OptionData.m_ScreenResolution);
    }

    public void SetRefreshRate(int refreshRate)
    {
        OptionDataManager.Instance.OptionData.m_RefreshRate = refreshRate;
        OptionDataManager.Instance.SaveOptionData();
        SetResolution(OptionDataManager.Instance.OptionData.m_ScreenResolution);
    }

    public void SelectQualityDropdown()
    {
        QualitySettings.SetQualityLevel(m_QualityDropdown.value, true);
        OptionDataManager.Instance.OptionData.m_GraphicQuality = m_QualityDropdown.value;
        OptionDataManager.Instance.SaveOptionData();
    }

    public void SelectLangDropdown()
    {
        switch (m_LanguageDropdown.value)
        {
            //한국어
            case 0:
                OptionDataManager.Instance.OptionData.m_Language = SystemLanguage.Korean;
                break;
            case 1:
                OptionDataManager.Instance.OptionData.m_Language = SystemLanguage.English;
                break;
            case 2:
                OptionDataManager.Instance.OptionData.m_Language = SystemLanguage.Japanese;
                break;
        }

        OptionDataManager.Instance.SaveOptionData();
    }

    public void MasterValueChanged()
    {
        OptionDataManager.Instance.OptionData.m_MasterVolume = m_MasterSoundSlider.value;
        OptionDataManager.Instance.SaveOptionData();
    }

    public void BGMValueChanged()
    {
        OptionDataManager.Instance.OptionData.m_BgmVolume = m_BGMSoundSlider.value;
        OptionDataManager.Instance.SaveOptionData();
    }

    public void EffectValueChanged()
    {
        OptionDataManager.Instance.OptionData.m_EffectVolume = m_EffectSoundSlider.value;
        OptionDataManager.Instance.SaveOptionData();
    }
}