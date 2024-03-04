using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour
{
    public GameObject gameOverUI;
    public Button retryBtn, quitBtn;
    public PlayerHealth playerHealth;

    private void Awake()
    {
        retryBtn.onClick.AddListener(() =>
        {
            gameOverUI.SetActive(false);
            Time.timeScale = 1f;
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }



}
