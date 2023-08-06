using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string TRIGGER_INTERACT = "TriggerInteract";

    [SerializeField] private PlayerController playerController;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_WALKING, playerController.IsWalking());
    }

    public void PlayInteractAnimation()
    {
        animator.SetTrigger(TRIGGER_INTERACT);
    }
}
