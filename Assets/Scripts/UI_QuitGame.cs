using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_QuitGame : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private GameObject quitPanel;

    [Header("BackgroundImage")]
    [SerializeField] private GameObject backgroundPanel;

    void Start()
    {
        // 초기 UI 설정
        backgroundPanel.SetActive(false);
        quitPanel.SetActive(false);

        pauseButton.onClick.AddListener(ShowUI);
        noButton.onClick.AddListener(DisableUI);
        yesButton.onClick.AddListener(QuitGame);
    }

    public void Update()
    {
        if(GameManager.instance.IsPaused)
        {
            return; // 게임이 일시정지 상태면 입력 처리 중단
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitPanel.activeSelf)
            {
                DisableUI();
            }
            else
            {
                ShowUI();
            }
        }
    }

    private void ShowUI()
    {
        quitPanel.SetActive(true);
        backgroundPanel.SetActive(true);
        GameManager.instance.PauseGame();
    }

    private void DisableUI()
    {
        quitPanel.SetActive(false);
        backgroundPanel.SetActive(false);
        GameManager.instance.ResumeGame();
    }

    private void QuitGame()
    {
        GameManager.instance.ResumeGame();
        SceneManager.LoadScene("Main");
    }

    
}
