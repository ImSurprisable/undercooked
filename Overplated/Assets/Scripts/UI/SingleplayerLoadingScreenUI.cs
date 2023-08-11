using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleplayerLoadingScreenUI : MonoBehaviour
{
    void Start()
    {
        if (GameMultiplayer.playSingleplayer == false) {
            Hide();
        }
    }



    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
