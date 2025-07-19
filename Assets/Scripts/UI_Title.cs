using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Title : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        SaveManager.instance.SaveSelectLevel(1);
    }
}
