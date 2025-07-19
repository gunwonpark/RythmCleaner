using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public string nickname;
    public int score;
}

// 랭킹 목록 전체를 담을 컨테이너 클래스
[Serializable]
public class HighScoreData
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public HighScoreData highScoreData;
    public string savePath;
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            savePath = Path.Combine(Application.persistentDataPath, "highscores.json");
            LoadScores(); 
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public int TotalClearRound = 0;
    public int Round1RemainTime = 0;
    public int Round2RemainTime = 0;
    public int Round3RemainTime = 0;
    public int TotalScore = 0;
    public int TotalDustCount = 0;

    public void ResetData()
    {
        TotalClearRound = 0;
        Round1RemainTime = 0;
        Round2RemainTime = 0;
        Round3RemainTime = 0;
        TotalScore = 0;
        TotalDustCount = 0;
    }
    public void SaveSelectLevel(int selectLevel)
    {
        PlayerPrefs.SetInt("Level", selectLevel);
        Debug.Log("저장한 값은 =>" + PlayerPrefs.GetInt("Level"));
        LoadingSceneManager.Instance.LoadScene("Merge");
    }

    public void SaveScores()
    {
        // 데이터를 JSON 문자열로 변환
        string json = JsonUtility.ToJson(highScoreData, true);
        // 파일에 쓰기
        File.WriteAllText(savePath, json);
    }
    public List<ScoreEntry> GetTopScores(int count)
    {
        // 점수를 내림차순으로 정렬
        highScoreData.scores.Sort((a, b) => b.score.CompareTo(a.score));
        // 상위 N개의 점수 반환
        return highScoreData.scores.GetRange(0, Mathf.Min(count, highScoreData.scores.Count));
    }
    public void LoadScores()
    {
        if (File.Exists(savePath))
        {
            // 파일이 존재하면 읽어와서 데이터로 변환
            string json = File.ReadAllText(savePath);
            highScoreData = JsonUtility.FromJson<HighScoreData>(json);
        }
        else
        {
            // 파일이 없으면 새로 생성
            highScoreData = new HighScoreData();
        }
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
