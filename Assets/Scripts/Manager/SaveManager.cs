using UnityEditor;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    public void SaveSelectLevel(int selectLevel)
    {
        PlayerPrefs.SetInt("Level", selectLevel);
        Debug.Log("저장한 값은 =>" + PlayerPrefs.GetInt("Level"));
        LoadingSceneManager.Instance.LoadScene("Merge");
    }
    
#if UNITY_EDITOR
    [MenuItem("Window/Level 1 Change")]
    private static void ChangeLevel_1()
    {
        PlayerPrefs.SetInt("Level", 1);
        Debug.Log("레벨 체인지 1");
    }
    
    [MenuItem("Window/Level 2 Change")]
    private static void ChangeLevel_2()
    {
        PlayerPrefs.SetInt("Level", 2);
        Debug.Log("레벨 체인지 2");
    }
    
    [MenuItem("Window/Level 3 Change")]
    private static void ChangeLevel_3()
    {
        PlayerPrefs.SetInt("Level", 3);
        Debug.Log("레벨 체인지 3");
    }
#endif
    
}
