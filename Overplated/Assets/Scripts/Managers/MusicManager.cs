using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;
    [SerializeField] private float volumeMultiplier;
    private static float volume = 1f;




    private void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, 1f);
        
        audioSource.volume = volumeMultiplier * volume;
    }

    public void ChangeVolume(float value)
    {
        volume = value;

        audioSource.volume = volumeMultiplier * volume;

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, volume);
    }

    public static float GetVolume()
    {
        return volume;
    }

}
