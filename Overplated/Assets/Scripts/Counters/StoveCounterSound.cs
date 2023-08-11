using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{

    [SerializeField] private StoveCounter stoveCounter;
    private AudioSource audioSource;

    private float warningSoundTimer;
    [SerializeField] private float warningSoundTimerMax;
    [SerializeField] float burningWarningProgressAmount;
    private bool playWarningSound;



    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        SoundManager.Instance.OnSoundEffectVolumeChanged += SoundManager_OnSoundEffectVolumeChanged;

        audioSource.volume = SoundManager.GetVolume();
    }

    private void SoundManager_OnSoundEffectVolumeChanged(object sender, SoundManager.OnSoundEffectVolumeChangedEventArgs e)
    {
        audioSource.volume = e.volume;
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        playWarningSound = (stoveCounter.IsStoveBurning() && e.progressNormalized >= burningWarningProgressAmount);
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
    {
        bool playSound = (e.state == StoveCounter.State.Cooking || e.state == StoveCounter.State.Cooked);
        if (playSound) {
            audioSource.Play();
        } else {
            audioSource.Pause();
        }
    }


    private void Update()
    {
        if (playWarningSound)
        {
            warningSoundTimer -= warningSoundTimer > 0f  ?  Time.deltaTime : 0f;
            if (warningSoundTimer <= 0f)
            {
                warningSoundTimer = warningSoundTimerMax;

                SoundManager.Instance.PlayWarningSound(stoveCounter.transform.position);
            }
        }
    }       
}
