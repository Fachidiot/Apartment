using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioClip[] backgroundMusicClips;
    
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this);
        Instance = this;
        DontDestroyOnLoad(this);

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        int random = Random.RandomRange(0, backgroundMusicClips.Length);
        audioSource.clip = backgroundMusicClips[random];
    }
}
