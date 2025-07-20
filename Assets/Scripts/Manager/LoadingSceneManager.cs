using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager  : MonoBehaviour
{
    private static LoadingSceneManager instance;  // 싱글톤(미리 생성)
    public static LoadingSceneManager Instance    // 마찬가지로 static 실행.
    {
        get
        {
            if (instance == null)
            {
                var obj = FindAnyObjectByType<LoadingSceneManager>();
                if (obj != null)
                    instance = obj;      // LoadingSceneController가 있으면 만들지 않음.
                else
                    instance = Create(); // LoadingSceneController없으면 만듬.
            }
            return instance;
        }
    }
    
    [SerializeField] 
    private CanvasGroup canvasGroup;

    [SerializeField] 
    private Image progressBar;

    private string loadSceneName;

    private static LoadingSceneManager Create() // 로딩 씬 프리팹 미리 생성
    {
        return Instantiate(Resources.Load<LoadingSceneManager>("UI/Loading Canvas"));
    }
    
    // 활성화된  스크립트 인스턴스가 로드될 때 Awake가 호출됩니다. -> Static이기 때문에, 정적으로 생성 될 때, Awake가 호출된다.
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);  // 컨트롤러 계속 유지하기
    }

    public void LoadScene(string sceneName)
    {
        gameObject.SetActive(true);
        SceneManager.sceneLoaded += OnSceneLoaded;  // 씬 로딩이 완료 되었을 때, 콜백 내장 함수 실행(LoadSceneProcess 코루틴이 break되는 순간에 호출된다!)
        loadSceneName = sceneName;
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        progressBar.fillAmount = 0f;
        yield return StartCoroutine(Fade(true));

        AsyncOperation op       = SceneManager.LoadSceneAsync(loadSceneName);
        op.allowSceneActivation = false;    // 씬의 동기화(=불러오기)가 완료되면, 바로 넘어가게 할 것 인지?

        float timer = 0f;
        while (!op.isDone)  // 불러오기가 완료되지 않았으면, 계속 진행
        {
            yield return null;
            if (op.progress < 0.8f) // 0.9이하면 op.progress만큼 표시
            {
                progressBar.fillAmount = op.progress;
            }
            else                    // 동기화가 0.9이상 완료 되었으면, 마지막은 1초동안 천천히 차오르도록 하고, 완료되면 넘어가도록 함.
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if (progressBar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)  // 콜백 내장 함수
    {
        if (arg0.name == loadSceneName)
        {
            StartCoroutine(Fade(false));
            SceneManager.sceneLoaded -= OnSceneLoaded; // 내장 콜백 함수 제거(제거하지 않으면, 중첩되어 문제가 생길 수 있음.)
        }
    }
    
    private IEnumerator Fade(bool isFadeIn)
    {
        float timer = 0f;
        while (timer <= 1f)
        {
            yield return null;
            timer += Time.unscaledDeltaTime * 3f;
            canvasGroup.alpha = isFadeIn ? Mathf.Lerp(0f, 1f, timer) : Mathf.Lerp(1f, 0f, timer);   // 전체 페이트 관리
        }

        if (!isFadeIn)
        {
            gameObject.SetActive(false);
        }
    }
    
}
