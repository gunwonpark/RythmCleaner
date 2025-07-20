using System;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public GameObject titleCanvas;
    public GameObject levelSelectCanvas;
    public GameObject explanationCanvas;

    private void Start()
    {
        // 배경화면 사운드 재생
        AudioManager.instance.PlayBgm(0,true);
    }

    public void LevelSelectCanvasOn()
    {
        titleCanvas.gameObject.SetActive(false);
        levelSelectCanvas.gameObject.SetActive(true);
    }

    public void ExplanationCanvasOn()
    {
        titleCanvas.gameObject.SetActive(false);
        explanationCanvas.gameObject.SetActive(true);
    }

    public void ReturnToMainMenu()
    {   
        AudioManager.instance.PlaySfx(0, 0.5f);
        
        titleCanvas.gameObject.SetActive(true);
        levelSelectCanvas.gameObject.SetActive(false);
        explanationCanvas.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}