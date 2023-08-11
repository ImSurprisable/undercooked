using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

    public static SoundManager Instance { get; private set; }

    public event EventHandler<OnSoundEffectVolumeChangedEventArgs> OnSoundEffectVolumeChanged;
    public class OnSoundEffectVolumeChangedEventArgs : EventArgs {
        public float volume;
    }

    [SerializeField] private AudioClipRefsSO clipLibrary;

    private static float volume = 1f;



    private void Awake()
    {
        Instance = this;

        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        PlayerController.OnAnyObjectPickup += PlayerController_OnObjectPickup;
        BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(clipLibrary.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        PlaySound(clipLibrary.objectDrop, baseCounter.transform.position);
    }

    private void PlayerController_OnObjectPickup(object sender, EventArgs e)
    {
        PlayerController player = sender as PlayerController;
        PlaySound(clipLibrary.objectPickup, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(clipLibrary.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(clipLibrary.deliveryFail, deliveryCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        PlaySound(clipLibrary.deliverySuccess, DeliveryCounter.Instance.transform.position);
    }




    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(audioClipArray[UnityEngine.Random.Range(0, audioClipArray.Length)], position, volumeMultiplier * volume);
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
    }

    public void PlayFootstepSound(Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(clipLibrary.footsteps, position, volumeMultiplier * volume);
    }

    public void PlayCountdownSound(float volumeMultiplier = 1f)
    {
        PlaySound(clipLibrary.warning[1], Vector3.zero, volumeMultiplier * volume);
    }

    public void PlayWarningSound(Vector3 position, float volumeMultiplier = 1f)
    {
        PlaySound(clipLibrary.warning[0], position, volumeMultiplier * volume);
    }

    public static void ChangeVolume(float value)
    {
        volume = value;

        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);

        if (Instance != null) {
            Instance.OnSoundEffectVolumeChanged?.Invoke(Instance, new OnSoundEffectVolumeChangedEventArgs {
                volume = volume
            });
        }
    }

    public static float GetVolume()
    {
        return volume;
    }


}
