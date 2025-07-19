using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TotalRanking : MonoBehaviour
{
    public TMP_InputField nicknameInputField;
    public UI_ConfirmPopup confirmPopup; // 게임 종료 UI 참조
    public List<RankingNode> rankingNodes; // 랭킹 노드 프리팹
    public Button quitButton; // 종료 버튼
    public RankingNode myNode;

    private void Start()
    {
        nicknameInputField.onSubmit.AddListener(OnNicknameSubmitted);
        quitButton.onClick.AddListener(() =>
        {
            this.gameObject.SetActive(false);
        });
    }

    private void OnEnable()
    {
        List<ScoreEntry> node = SaveManager.instance.GetTopScores(6);

        for(int i = 0; i < rankingNodes.Count; i++)
        {
            if (i < node.Count)
            {
                rankingNodes[i].SetData(i + 1, node[i].nickname, node[i].score);
                rankingNodes[i].gameObject.SetActive(true);
            }
            else
            {
                rankingNodes[i].gameObject.SetActive(false);
            }
        }

        List<ScoreEntry> myNodeData = SaveManager.instance.highScoreData.scores;

        int ranking = -1;
        for (int i = 0; i < myNodeData.Count; i++)
        {
            if(SaveManager.instance.TotalScore > myNodeData[i].score)
            {
                ranking = i + 1; 
            }
        }
        if (ranking == -1)
        {
            ranking = myNodeData.Count + 1; // 현재 점수가 가장 낮은 경우
        }

        myNode.SetData(ranking, "익명의 누군가", SaveManager.instance.TotalScore);
    }

    private void OnDisable()
    {
        nicknameInputField.gameObject.SetActive(false);
    }

    private void OnNicknameSubmitted(string arg0)
    {
        string nickname = arg0;

        // 닉네임이 비어있는지 확인
        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("닉네임을 입력해주세요.");
            return;
        }

        int currentScore = SaveManager.instance.TotalScore;

        HighScoreData highScoreData = SaveManager.instance.highScoreData;

        for (int i = 0; i < highScoreData.scores.Count; i++)
        {
            // 이미 같은 닉네임이 있는 경우
            if (highScoreData.scores[i].nickname == nickname)
            {
                // 점수 업데이트
                highScoreData.scores[i].score = Mathf.Max(highScoreData.scores[i].score, currentScore);
                SaveManager.instance.SaveScores();
                return;
            }
        }

        highScoreData.scores.Add(new ScoreEntry { nickname = nickname, score = currentScore });
        SaveManager.instance.SaveScores();

        confirmPopup.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void InputEnable()
    {
        List<ScoreEntry> myNodeData = SaveManager.instance.highScoreData.scores;

        myNodeData.Sort((a, b) => b.score.CompareTo(a.score));

        int ranking = -1;
        for (int i = 0; i < myNodeData.Count; i++)
        {
            if (SaveManager.instance.TotalScore > myNodeData[i].score)
            {
                ranking = i + 1;
            }
        }
        if (ranking == -1)
        {
            ranking = myNodeData.Count + 1; 
        }

        myNode.SetData(ranking, "", SaveManager.instance.TotalScore);
        nicknameInputField.gameObject.SetActive(true);
    }
}
