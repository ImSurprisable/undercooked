using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{

    [SerializeField] private Image timerImage;
    [SerializeField] private Gradient imageGradient;


    private void Update()
    {
        timerImage.fillAmount = GameManager.Instance.GetGamePlayingTimerNormalized();
        timerImage.color = imageGradient.Evaluate(1 - (GameManager.Instance.GetGamePlayingTimerNormalized()));
    }

}
