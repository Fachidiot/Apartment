using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager m_Instance;
    public static AudioManager Instance{get{return m_Instance;}}
    [SerializeField] private AudioClip[] lobbyMusicClips;
    [SerializeField] private AudioClip[] loadingMusicClips;
    [SerializeField] private AudioClip[] backgroundMusicClips;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        int random = Random.Range(0, lobbyMusicClips.Length);
        audioSource.clip = lobbyMusicClips[random];
        audioSource.Play();
    }

    public void PlayLobbyMusic()
    {
        if (lobbyMusicClips.Length <= 0)
            return;
        int random = Random.Range(0, lobbyMusicClips.Length);
        audioSource.clip = lobbyMusicClips[random];
        audioSource.Play();
    }

    public void PlayLoadingMusic()
    {
        if (loadingMusicClips.Length <= 0)
            return;
        int random = Random.Range(0, loadingMusicClips.Length);
        audioSource.clip = loadingMusicClips[random];
        audioSource.Play();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicClips.Length <= 0)
            return;
        int random = Random.Range(0, backgroundMusicClips.Length);
        audioSource.clip = backgroundMusicClips[random];
        audioSource.Play();
    }
}
