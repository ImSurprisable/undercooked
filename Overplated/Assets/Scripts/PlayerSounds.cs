using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{

    [SerializeField] private float footstepTimerMax;
    [SerializeField] private float firstStepTimerMultiplier;
    private float footstepTimer;
    
    [SerializeField] private float volume;


    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        footstepTimer -= footstepTimer > 0  ?  Time.deltaTime : 0f;
        if (!player.IsWalking())
        {
            footstepTimer = footstepTimerMax * firstStepTimerMultiplier;
        }
        else if (footstepTimer <= 0)
        {
            if (player.IsWalking()) {
                SoundManager.Instance.PlayFootstepSound(transform.position, volume);
            }

            footstepTimer = footstepTimerMax;
        }

    }

}
