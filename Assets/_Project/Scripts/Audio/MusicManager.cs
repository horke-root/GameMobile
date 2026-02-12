using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;
    private List<AudioClip> songs = new List<AudioClip>();
    private int currentSongIndex = -1;
    private bool isStopped = false;
    [Header("Settings")]
    [Tooltip("Path to music folder in Resources folder")]
    public string resourcesPath = "Music"; 
    public bool playOnStart = true;
    public bool shuffle = true;
    
    [Range(0f, 1f)]
    public float volume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.volume = volume;
        audioSource.loop = false; 

        LoadSongs();

        if (playOnStart && songs.Count > 0)
        {
            PlayNextSong();
        }
    }

    private void LoadSongs()
    {
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>(resourcesPath);
        if (loadedClips != null && loadedClips.Length > 0)
        {
            songs.AddRange(loadedClips);
            Debug.Log($"MusicManager: Loaded {songs.Count} songs");
        }
        else
        {
            Debug.LogWarning($"MusicManager: No failes found in Resources/{resourcesPath}.");
        }
    }

    private void Update()
    {
        if (audioSource != null && !audioSource.isPlaying && songs.Count > 0 && !isStopped)
        {
            if (audioSource.clip != null && audioSource.time == 0) 
            {
                 PlayNextSong();
            }
            else if (audioSource.clip == null)
            {
                PlayNextSong();
            }
        }
    }

    public void PlayNextSong()
    {
        if (songs.Count == 0) return;

        if (shuffle)
        {
            int newIndex = Random.Range(0, songs.Count);
            if (songs.Count > 1 && newIndex == currentSongIndex)
            {
                newIndex = (newIndex + 1) % songs.Count;
            }
            currentSongIndex = newIndex;
        }
        else
        {
            currentSongIndex = (currentSongIndex + 1) % songs.Count;
        }

        PlaySong(currentSongIndex);
    }

    public void PlaySong(int index)
    {
        if (index < 0 || index >= songs.Count) return;

        audioSource.clip = songs[index];
        audioSource.Play();
    }
    
    public void SetVolume(float vol)
    {
        volume = Mathf.Clamp01(vol);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            isStopped = true;
            audioSource.Stop();
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            isStopped = false;
            if (!audioSource.isPlaying && audioSource.clip != null)
            {
                audioSource.Play();
            }
            else if (audioSource.clip == null && songs.Count > 0)
            {
                PlayNextSong();
            }
        }
    }
}