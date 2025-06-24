using System;
using System.IO;
using System.Threading;
using System.Globalization;
using UnityEngine;

public class OptionDataManager : MonoBehaviour
{
    // Singleton
    private static OptionDataManager m_Instance;
    public static OptionDataManager Instance { get { return m_Instance; } }

    private string OptionDataFileName = "\\Option.json";
    public OptionData OptionData;
    [SerializeField] private OptionKeyData initialKey;

    private SystemLanguage m_Language;
    private Resolution[] resolutions;
    private OptionManager m_OptionManager;

    private void Awake()
    {
        if (Instance == null)
        {
            m_Instance = this;
            // DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
            
        m_OptionManager = GetComponent<OptionManager>();

        LoadOptionData();
        SaveOptionData();

        // 언어 확인 후 UI언어들 초기화
        InitLanguage();

        // 옵션 확인 후 옵션 UI 초기화
        m_OptionManager.InitMenuLayouts();

        //품질 설정
        QualitySettings.SetQualityLevel(OptionData.m_GraphicQuality, true);
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
        print("새로운 옵션 파일 생성");
        OptionData = null;
        OptionData = new OptionData();

        //새로 생성하는 데이터들은 이곳에 선언하기
        OptionData.m_Language = Application.systemLanguage;
        OptionData.m_keyData = initialKey;
        
        resolutions = Screen.resolutions;

        int currentResolutionIndex = 0;
        double maxRefreshRate = 0;

        for (int i = 0; i < resolutions.Length; ++i)
        {
            // 최적의 해상도 저장
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
            if (resolutions[i].refreshRateRatio.value > maxRefreshRate)
                maxRefreshRate = resolutions[i].refreshRateRatio.value;
        }
        OptionData.m_ScreenResolution = currentResolutionIndex;
        OptionData.m_RefreshRate = maxRefreshRate;
        OptionData.m_FullScreenMode = FullScreenMode.FullScreenWindow;
        
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
            }
            PlayerPrefs.SetInt("Language", (int)m_Language);
        }
    }
}

[Serializable]
public class OptionData
{
    [Header("Screen")]
    public bool m_VSync;
    public FullScreenMode m_FullScreenMode;
    public float m_ScreenBrightness;
    public int m_ScreenResolution;
    public double m_RefreshRate;
    
    [Header("Graphic")]
    public int m_GraphicQuality;
    public int m_ShadowQuality;
    public int m_AmbientOcclusion;
    public int m_ReflectionQuality;

    [Header("Sound")]
    public float m_MasterVolume;
    public float m_BgmVolume;
    public float m_EffectVolume;

    [Header("Gameplay")]
    public bool m_ScreenVibration;
    public SystemLanguage m_Language;

    [Header("Shortcuts")]
    public OptionKeyData m_keyData;
}

[Serializable]
public class OptionKeyData
{
    [Header("Movement")]
    public KeyCode m_KeyMoveLeft;
    public KeyCode m_KeyMoveRight;
    public KeyCode m_KeyMoveUp;
    public KeyCode m_KeyMoveDown;
    public KeyCode m_KeyJump;
    public KeyCode m_KeySprint;
    public KeyCode m_KeyCrouch;

    [Header("Attack")]
    public KeyCode m_Attack;
    public KeyCode m_Aimed;

    [Header("Interaction")]
    public KeyCode m_KeyInteract;
    public KeyCode m_KeyInventory;
    public KeyCode m_KeyEscape;
}