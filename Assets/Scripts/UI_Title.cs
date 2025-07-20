using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Title : MonoBehaviour
{
    public Button startButton;
    public Button creditButton;
    public Button exitButton;
    public Button howtoPlayButton;

    public Button DisableCredit;
    public Button DisableHowtoPlay;

    public GameObject creditObject;
    public GameObject howToPlayObject;

    public Image startImage;
    public Image creditImage;
    public Image exitImage;
    public Image howToPlayImage;

    public Sprite startClickSprite;
    public Sprite creditOriginSprite;
    public Sprite creditClickSprite;
    public Sprite exitClickSprite;
    public Sprite howToPlayOriginSprite;
    public Sprite howToPlayClickSprite;

    public RectTransform startRectTransform;
    public RectTransform creditRectTransform;
    public RectTransform exitRectTransform;
    public RectTransform howToPlayRectTransform;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        creditButton.onClick.AddListener(OnCreditButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        howtoPlayButton.onClick.AddListener(OnHowToPlayButtonClicked);
        DisableCredit.onClick.AddListener(OnDisableCreditButtonClicked);
        DisableHowtoPlay.onClick.AddListener(OnDisableHowtoPlayButtonClicked);
    }

    private void OnHowToPlayButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Splash,0.25f);
        howToPlayRectTransform.sizeDelta = new Vector2(360, 320);
        howToPlayImage.sprite = howToPlayClickSprite;
        howToPlayObject.SetActive(true);
    }

    private void OnExitButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Splash, 0.25f);
        exitRectTransform.sizeDelta = new Vector2(360, 320);
        StartCoroutine(ExitButton());
    }

    private  IEnumerator ExitButton()
    {  
        exitImage.sprite = exitClickSprite;
        yield return new WaitForSeconds(0.2f);
        Application.Quit();
    }

    private void OnCreditButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Splash, 0.25f);
        creditRectTransform.sizeDelta = new Vector2(360, 320);
        creditImage.sprite = creditClickSprite;
        creditObject.SetActive(true);
    }

    private void OnStartButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Splash, 0.25f);
        startRectTransform.sizeDelta = new Vector2(360, 320);
        StartCoroutine(StartButton());
    }
    
    private IEnumerator StartButton()
    {
        startImage.sprite = startClickSprite;
        yield return new WaitForSeconds(0.2f);
        SaveManager.instance.SaveSelectLevel(1);
    }
    private void OnDisableCreditButtonClicked()
    {
        AudioManager.instance.PlaySfx(0,0.5f);
        creditRectTransform.sizeDelta = new Vector2(360, 146);
        creditImage.sprite = creditOriginSprite;
        creditObject.SetActive(false);
    }
    private void OnDisableHowtoPlayButtonClicked()
    {
        AudioManager.instance.PlaySfx(0, 0.5f);
        howToPlayRectTransform.sizeDelta = new Vector2(360, 146);
        howToPlayImage.sprite = howToPlayOriginSprite;
        howToPlayObject.SetActive(false);
    }
}
