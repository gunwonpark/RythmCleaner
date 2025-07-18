using UnityEngine;

public class MainUIManager : MonoBehaviour
{
    public GameObject titleCanvas;
    public GameObject levelSelectCanvas;
    public GameObject explanationCanvas;

    // 레벨 추가 후, 변경
    public void LoadLevel(string levelName)
    {
        LoadingSceneManager.Instance.LoadScene("Merge");
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
        titleCanvas.gameObject.SetActive(true);
        levelSelectCanvas.gameObject.SetActive(false);
        explanationCanvas.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}