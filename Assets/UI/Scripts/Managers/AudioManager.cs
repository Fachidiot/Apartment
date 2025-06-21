using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioClip[] lobbyMusicClips;
    [SerializeField] private AudioClip[] loadingMusicClips;
    [SerializeField] private AudioClip[] backgroundMusicClips;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
        DontDestroyOnLoad(this);

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
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
