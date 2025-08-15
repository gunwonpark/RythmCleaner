using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_GameEnd : MonoBehaviour
{
    // 플레이어 최종 점수
    // 역대 최고 점수
    // 완료한 라운드 수
    // 청소한 먼지 수
    // 전체랭킹 확인 버튼
    // 닉네임 입력 란
    // 종료 버튼

    // ---------------------
    // 전체랭킹 확인 창
    // 종료 버튼

    public TextMeshProUGUI finalScoreText; // 최종 점수
    public TextMeshProUGUI bestScoreText; // 역대 최고 점수
    public TextMeshProUGUI completedRoundsText; // 완료한 라운드 수
    public TextMeshProUGUI cleanedDustCountText; // 청소한 먼지 수
    public Button rankingButton; // 전체랭킹 확인 버튼
    public Button registerButton; // 점수 등록 버튼
    public Button exitButton; // 종료 버튼

    public UI_TotalRanking rankingPanel; // 전체랭킹 확인 창

    public Animator animator;
    public Animator SuccessAnimator;

    public GameObject SuccessObject;
    public GameObject BackgroundPanel;
    
    public Button pauseButton;
    public void SetData()
    {
        // 최종 점수 설정
        finalScoreText.text = "내 점수: " + SaveManager.instance.TotalScore.ToString();
        // 역대 최고 점수 설정
        List<ScoreEntry> topScores = SaveManager.instance.GetTopScores(1);
        if (topScores.Count > 0)
            bestScoreText.text = topScores[0].score.ToString();
        else
            bestScoreText.text = "역대 최고 점수: " + 0;
        // 완료한 라운드 수 설정
        completedRoundsText.text = SaveManager.instance.TotalClearRound.ToString();
        // 청소한 먼지 수 설정
        cleanedDustCountText.text = SaveManager.instance.TotalDustCount.ToString();
        // 랭킹 버튼 클릭 이벤트 등록
        rankingButton.onClick.AddListener(OnRankingButtonClicked);
        // 종료 버튼 클릭 이벤트 등록
        exitButton.onClick.AddListener(OnExitButtonClicked);
        // 점수 등록 가능하게 표시
        registerButton.onClick.AddListener(RegisterScore);

        BackgroundPanel.SetActive(true);
    }

    public void Win()
    {
        SuccessObject.SetActive(true);
    }

    public void SuccessAnimation()
    {
        SuccessAnimator.SetTrigger("Success");
    }

    private void RegisterScore()
    {
        rankingPanel.gameObject.SetActive(true);
        rankingPanel.InputEnable();
    }

    private void OnRankingButtonClicked()
    {
        rankingPanel.gameObject.SetActive(true);
    }
    private void OnExitButtonClicked()
    {
        Debug.Log("게임 종료 버튼 클릭됨");
        SaveManager.instance.ResetData();
        SceneManager.LoadScene("Main"); // 메인 메뉴로 이동
    }

    public void DoAnimation()
    {
        animator.SetTrigger("FallingPlayer");
    }
}
