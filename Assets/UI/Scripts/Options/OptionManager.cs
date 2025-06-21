using UnityEngine.UI;
using UnityEngine;

public class OptionManager : MonoBehaviour
{
    // 설정창 오브젝트
    [SerializeField]
    private GameObject m_MenuParent;

    [Header("Graphic Setting")]
    [SerializeField]
    private TMPro.TMP_Dropdown m_QualityDropdown;

    [Header("Audio Setting")]
    [SerializeField]
    private Slider m_MasterSoundSlider;
    [SerializeField]
    private Slider m_BGMSoundSlider;
    [SerializeField]
    private Slider m_EffectSoundSlider;

    [Header("System Setting")]
    [SerializeField]
    private TMPro.TMP_Dropdown m_LanguageDropdown;

    [Header("Customize Setting")]
    [SerializeField]
    private GameObject mFPSButton;

    // 설정창 활성화 여부
    private static bool m_IsOpenMenu = false;

    public void TryOpenMenu()
    {
        if (m_IsOpenMenu)   // 설정창 토글
            CloseMenu();    // 설정 닫기
        else
            OpenMenu();     // 설정 열기
        m_IsOpenMenu = !m_IsOpenMenu;
    }

    public void InitMenuLayouts()
    {
        m_MasterSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_MasterVolume);
        m_BGMSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_BgmVolume);
        m_EffectSoundSlider.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_EffectVolume);

        m_QualityDropdown.SetValueWithoutNotify(OptionDataManager.Instance.OptionData.m_CurrentSelectQualityID);
        
        switch (OptionDataManager.Instance.OptionData.language)
        {
            case SystemLanguage.Korean:
                {
                    m_LanguageDropdown.SetValueWithoutNotify(0);
                    break;
                }
            case SystemLanguage.Japanese:
                {
                    m_LanguageDropdown.SetValueWithoutNotify(2);
                    break;
                }
            case SystemLanguage.English:
            default:
                {
                    m_LanguageDropdown.SetValueWithoutNotify(1);
                    break;
                }
        }

        m_MenuParent.SetActive(false);
    }

    public void OpenMenu()
    {
        m_MenuParent.SetActive(true);
    }

    public void CloseMenu()
    {
        m_MenuParent.SetActive(false);
    }

    public void SelectQualityDropdown()
    {
        QualitySettings.SetQualityLevel(m_QualityDropdown.value, true);
        OptionDataManager.Instance.OptionData.m_CurrentSelectQualityID = m_QualityDropdown.value;
        OptionDataManager.Instance.SaveOptionData();
    }

    public void SelectLangDropdown()
    {
        switch (m_LanguageDropdown.value)
        {
            //한국어
            case 0:
                {
                    OptionDataManager.Instance.OptionData.language = SystemLanguage.Korean;
                    break;
                }
            case 1:
                {
                    OptionDataManager.Instance.OptionData.language = SystemLanguage.English;
                    break;
                }
            case 2:
                {
                    OptionDataManager.Instance.OptionData.language = SystemLanguage.Japanese;
                    break;
                }
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

    public static bool GetIsMenuOpen()
    {
        return m_IsOpenMenu;
    }
}