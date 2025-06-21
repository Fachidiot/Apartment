using System;
using System.IO;
using System.Threading;
using System.Globalization;
using UnityEngine;

public class OptionDataManager : MonoBehaviour
{
    // Singleton
    private static OptionDataManager m_Instance;
    public static OptionDataManager Instance { get{ return m_Instance; } }

    private string OptionDataFileName = "\\Option.json";
    private SystemLanguage m_Language;

    public OptionData OptionData;

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(this);
            return;
        }
        m_Instance = this;

        LoadOptionData();
        SaveOptionData();

        // 언어 확인 후 UI언어들 초기화
        InitLanguage();

        // 옵션 확인 후 옵션 UI 초기화
        FindObjectOfType<OptionManager>().InitMenuLayouts();

        //품질 설정
        QualitySettings.SetQualityLevel(OptionData.m_CurrentSelectQualityID, true);
    }

    private void LoadOptionData()
    {
        string filePath = Application.persistentDataPath + OptionDataFileName;

        if (File.Exists(filePath))
        {
            string FromJsonData = File.ReadAllText(filePath);
            OptionData = JsonUtility.FromJson<OptionData>(FromJsonData);
        }

        // 저장된 게임이 없다면
        else
        {
            ResetOptionData();
        }
    }

    // 옵션 데이터 저장하기
    public void SaveOptionData()
    {
        string ToJsonData = JsonUtility.ToJson(OptionData);
        string filePath = Application.persistentDataPath + OptionDataFileName;

        // 이미 저장된 파일이 있다면 덮어쓰기
        File.WriteAllText(filePath, ToJsonData);
    }

    // 데이터를 초기화(새로 생성 포함)하는경우
    public void ResetOptionData()
    {
        print("새로운 옵션 파일 생성");
        OptionData = null;
        OptionData = new OptionData();

        //새로 생성하는 데이터들은 이곳에 선언하기
        OptionData.language = Application.systemLanguage;

        //옵션 데이터 저장
        SaveOptionData();
    }

    private void InitLanguage()
    {
        if (PlayerPrefs.GetInt("Language") != 0)
        {
            m_Language = (SystemLanguage)PlayerPrefs.GetInt("Language");
            return;
        }
        else
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

            switch (cultureInfo.TwoLetterISOLanguageName)
            {
                case "en":
                    m_Language = SystemLanguage.English;
                    break;
                case "ko":
                    m_Language = SystemLanguage.Korean;
                    break;
                case "ja":
                    m_Language = SystemLanguage.Japanese;
                    break;
                default:
                    m_Language = SystemLanguage.English;
                    break;
            }
            PlayerPrefs.SetInt("Language", (int)m_Language);
        }
    }
}

[Serializable]
public class OptionData
{
    public SystemLanguage language;
    public int m_CurrentSelectQualityID;
    public float m_MasterVolume;
    public float m_BgmVolume;
    public float m_EffectVolume;
}