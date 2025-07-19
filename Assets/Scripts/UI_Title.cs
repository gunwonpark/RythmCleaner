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
        howToPlayRectTransform.sizeDelta = new Vector2(400, 452);
        howToPlayImage.sprite = howToPlayClickSprite;
        howToPlayObject.SetActive(true);
    }

    private void OnExitButtonClicked()
    {
        exitRectTransform.sizeDelta = new Vector2(400, 452);
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
        creditRectTransform.sizeDelta = new Vector2(400, 452);
        creditImage.sprite = creditClickSprite;
        creditObject.SetActive(true);
    }

    private void OnStartButtonClicked()
    {
        startRectTransform.sizeDelta = new Vector2(400, 452);
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
        creditRectTransform.sizeDelta = new Vector2(400, 200);
        creditImage.sprite = creditOriginSprite;
        creditObject.SetActive(false);
    }
    private void OnDisableHowtoPlayButtonClicked()
    {
        howToPlayRectTransform.sizeDelta = new Vector2(400, 200);
        howToPlayImage.sprite = howToPlayOriginSprite;
        howToPlayObject.SetActive(false);
    }
}
