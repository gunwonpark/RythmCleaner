using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public List<LevelData> levelDataList;  
    public LevelData       currentLevelData;   

    public bool isGameStart = false;
    public bool isGameOver  = false;
    
    [Header("비트관리")]
    public int  beatCounter      = 0;       // 노드 생성 때, 카운트 증가
    public bool leftNodeDestory  = false;   // 좌우 노드 다 삭제되야, 비트 증가
    public bool rightNodeDestory = false;   // 좌우 노드 다 삭제되야, 비트 증가

    [Header("커서 관리")]
    public Texture2D AttackCursurTexture;

    [Header("UI References")]
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI remainTimeText;
    public TextMeshProUGUI midText;
    public Slider remainTimeSlider;
    public Slider tailSlider;
    
    [Header("현재 게임 정보")]
    public float EnableTime = 60f; // 라운드당 가능한 시간
    private float remainTIme; // 현재 남아있는 시간

    [Header("사운드 시작 관리")]
    public bool isSountStart = false;
    public AudioSource audioSource;
    public float RemainTime
    {
        get { return remainTIme; }
        set
        {
            remainTIme = value;
            UpdateRemainTime(); // UI 업데이트
        }
    }
    public int CurrentRound;

    public int KillDustCount = 0;

    public UI_GameEnd EndUI; // 게임 종료 UI

    private void Awake()
    {
        instance = this;
        
        Application.targetFrameRate = 60;
    
        // 저장된 PlayerPrefs값으로 현재 씬 세팅(리스트는 0번부터 시작하기 때문에, 1 빼주기)
        currentLevelData = levelDataList[PlayerPrefs.GetInt("Level") - 1];
        audioSource.clip = currentLevelData.audioClip;          // 음악 변경
        beatCounter = currentLevelData.createAndMoveCountBeat;  // 비터카운트값 변경
    }

    private IEnumerator Start()
    {

        isGameStart = true;
        RemainTime = 60f;
        CurrentRound = currentLevelData.level; // 현재 라운드 설정
        RoundText.text = $"Round : {CurrentRound}"; // UI에 현재 라운드 표시
        midText.text = $"Round {CurrentRound}"; // 중앙 텍스트 표시

        yield return WaitAndGo(); // 게임 시작 대기
        // 커서 변환 적용
        SetAttackCursor();
        StartCoroutine(NodeSpawnManager.Instance.SpawnNotesOnBeat()); // 노드 생성 시작

        yield return null;
        
        StartCoroutine(BeatManagement()); // 비트 관리
    }

    private IEnumerator WaitAndGo()
    {
        midText.transform.DOScale(1, 1f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(2f);
        midText.text = "START!";
        midText.DOFade(0, 1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.3f);
    }

    private void Update()
    {
        // 사운드가 시작될 때, 시간도 같이 체크
        if (!isSountStart)
            return;

        if(isGameOver)
        {
            return;
        }

        // 남은 시간이 0보다 크면 계속 시간을 감소시킴
        if (RemainTime > 0)
        {
            RemainTime -= Time.deltaTime; // Time.deltaTime은 한 프레임당 걸린 시간
        }
        else
        {
            // 남은 시간이 0 이하가 되면 게임 클리어 처리
            GameClear();
        }
    }

    // 🚀 최적화된 비트 관리 - 더 효율적인 대기 시간
    IEnumerator BeatManagement()
    {
        // 60fps 기준으로 적절한 대기 시간 설정 (매 프레임 체크는 과도함)
        WaitForSeconds waitTime = new WaitForSeconds(0.016f); // 대략 60fps
        
        while (isGameStart && !isGameOver)
        {
            if (beatCounter >= currentLevelData.createAndMoveCountBeat)
            {
                // 쓰레기 이동 진행
                PatternGenerator.instance.GenerateNextPattern();
                
                // 비트 초기화
                beatCounter = 0;
            }
            yield return waitTime; // 🚀 고정된 대기 시간으로 최적화
        }
    }

    private void PlayerBeatMove()
    {
        TestManager.Instance.player.Move(TestManager.Instance.player.moveDirection, TestManager.Instance.player.MoveDelay);
    }

    private void EnemyBeatMove()
    {
        // 🚀 최적화: null 체크와 역순 순회로 안전하게 처리
        var monsters = TestManager.Instance.Monsters;
        for (int i = monsters.Count - 1; i >= 0; i--)
        {
            if (monsters[i] != null)
                monsters[i].Move(0.15f);
            else
                monsters.RemoveAt(i); // null 참조 제거
        }
    }
    
    // 좌우 노드 체크(=> 비트 관리)
    public void CurrnetNodeDestoryCheck(NoteType inputType)
    {
        if (isGameOver)
        {
            return;
        }
        // 좌우 노드 삭제 체크 
        if (inputType == NoteType.LeftNote)
            leftNodeDestory  = true;
        else if (inputType == NoteType.RightNote)
            rightNodeDestory = true;
        
        // 초기화
        if (rightNodeDestory && leftNodeDestory)
        {
            //🚀 사운드 시작 최적화 (중복 호출 방지)
            if (!isSountStart && audioSource != null && !audioSource.isPlaying)
            {
                isSountStart = true;
                Debug.Log("🎵 사운드 시작!");
                audioSource.Play();
            }
            
            leftNodeDestory  = false;
            rightNodeDestory = false;
            beatCounter++;

            PlayerBeatMove();    // 플레이어 비트 이동
            EnemyBeatMove();     // 적 비트 이동
        }
    }

    public void UpdateTailUI(int currentCount, int maxCount)
    {
        tailSlider.value = (float)currentCount / maxCount;
    }

    public void GameOver()
    {
        if(isGameOver)
        {
            return;
        }
        isGameOver = true;
        
        // 🚀 사운드 정지 최적화
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log("🔴 게임 오버!");
        
        if(CurrentRound == 1)
        {
            SaveManager.instance.Round1RemainTime = (int)RemainTime;
            SaveManager.instance.TotalScore = (int)(60f - RemainTime) * 100;
        }
        else if(CurrentRound == 2)
        {
            SaveManager.instance.Round2RemainTime = (int)RemainTime;
            SaveManager.instance.TotalScore = (int)(60 + SaveManager.instance.Round1RemainTime +  60f - RemainTime) * 100;
        }
        else if(CurrentRound == 3)
        {
            SaveManager.instance.Round3RemainTime = (int)RemainTime;
            SaveManager.instance.TotalScore = (int)(180f + SaveManager.instance.Round1RemainTime + SaveManager.instance.Round2RemainTime +  60f - RemainTime) * 100;
        }

        SaveManager.instance.TotalClearRound = CurrentRound;
        SaveManager.instance.TotalDustCount += KillDustCount;
        // 커서 초기화
        ResetCursor();

        // 실패 UI 띄어 주기
        EndUI.SetData();
        EndUI.transform.DOMove(new Vector2(960, 559), 1f).SetEase(Ease.OutBounce);
        EndUI.DoAnimation();
    }

    [ContextMenu("GameClear")]
    public void GameClear()
    {
        // 이미 게임이 종료된 상태라면 중복 실행 방지
        if (isGameOver) return;

        isGameOver = true;

        // 🚀 사운드 정지 최적화
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (CurrentRound == 1)
        {
            SaveManager.instance.Round1RemainTime = (int)RemainTime;
            StartCoroutine(MoveNextLevel(2));
            return;
        }
        else if (CurrentRound == 2)
        {
            SaveManager.instance.Round2RemainTime = (int)RemainTime;
            StartCoroutine(MoveNextLevel(3));
            return;
        }
        else if (CurrentRound == 3)
        {
            SaveManager.instance.Round3RemainTime = (int)RemainTime;
            SaveManager.instance.TotalScore = (int)(360f + SaveManager.instance.Round1RemainTime + SaveManager.instance.Round2RemainTime + 
                SaveManager.instance.Round3RemainTime) * 100;
        }

        SaveManager.instance.TotalClearRound = CurrentRound; // 현재 라운드 저장
        SaveManager.instance.TotalDustCount += KillDustCount;
        // UI 띄우기
        EndUI.SetData();
        EndUI.Win();
        EndUI.transform.DOMove(new Vector2(960, 580), 1f).SetEase(Ease.OutBounce)
            .OnComplete(() => 
            {
                EndUI.SuccessAnimator.SetTrigger("Success");
            });

        // 커서 초기화
        ResetCursor();

        // TODO: 게임 클리어 UI 팝업, 다음 라운드로 넘어가는 로직 등 추가
    }

    private IEnumerator MoveNextLevel(int v)
    {
        midText.color = new Color(midText.color.r, midText.color.g, midText.color.b, 1f); // 투명도 초기화
        midText.text = "Clear!";
        midText.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // 크기 초기화
        midText.transform.DOScale(1, 1f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(3f);
        SaveManager.instance.SaveSelectLevel(v); // 다음 레벨로 이동
    }

    void UpdateRemainTime()
    {
        if (remainTimeText != null)
        {
            remainTimeText.text = $"{(int)Mathf.Max(0, remainTIme)}"; 
        }
        remainTimeSlider.value = Mathf.Clamp01(remainTIme / EnableTime); // 슬라이더 값 업데이트
    }

    #region 커서 변경 함수
    public void SetAttackCursor()
    {
        Vector2 centerHotspot = new Vector2(AttackCursurTexture.width / 2f, AttackCursurTexture.height / 2f);
        Cursor.SetCursor(AttackCursurTexture, centerHotspot, CursorMode.ForceSoftware);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion
}