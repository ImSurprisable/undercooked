using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{

    [SerializeField] private Image timerImage;
    [SerializeField] private Gradient imageGradient;



    private void Start()
    {
        if (GameManager.Instance.GetGamemode() != GameManager.Gamemode.Timer)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        timerImage.fillAmount = GameManager.Instance.GetGamePlayingTimerNormalized();
        timerImage.color = imageGradient.Evaluate(1 - (GameManager.Instance.GetGamePlayingTimerNormalized()));
    }

}
