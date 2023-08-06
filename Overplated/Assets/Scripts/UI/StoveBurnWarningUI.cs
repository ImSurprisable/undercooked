using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveBurnWarningUI : MonoBehaviour
{

    private const string FLASH_SPEED = "FlashSpeed";
    
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private Vector2 burnShowProgressAmountMinMax;

    [SerializeField] private float maxAnimationSpeed;
    private float animationSpeed;

    private Animator animator;



    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;

        UpdateAnimatorFlashSpeed(1f);

        Hide();
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        bool show = (stoveCounter.IsStoveBurning() && e.progressNormalized >= burnShowProgressAmountMinMax.x);

        if (show) {
            Show();
            animationSpeed = Mathf.Lerp(1, maxAnimationSpeed, (e.progressNormalized - burnShowProgressAmountMinMax.x) / (burnShowProgressAmountMinMax.y - burnShowProgressAmountMinMax.x)); // Converts the stove's normalized range of 0-1 to a 0-1 range between the minimum threshold and maximum thresholds of the animation.
            UpdateAnimatorFlashSpeed(animationSpeed);
        } else {
            Hide();
            UpdateAnimatorFlashSpeed(1f);
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateAnimatorFlashSpeed(float value)
    {
        animator.SetFloat(FLASH_SPEED, value);
        animationSpeed = value;
    }
}
