using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum NodeType
{
    LeftNote,  // 왼쪽에서 생성   => 마우스로 타격
    RightNote  // 오른쪽에서 생성 => 방향키로 타격
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public List<LevelData> levelDataList;  
    public LevelData       currentLevelData;   

    public bool isGameStart = false;
    public bool isGameOver  = false;
    
    [Header("비트관리")]
    public int  beatCounter      = 0;       // 노드 생성 때, 카운트 증가
    public bool leftNodeDestroy  = false;   // 좌우 노드 다 삭제되야, 비트 증가
    public bool rightNodeDestroy = false;   // 좌우 노드 다 삭제되야, 비트 증가

    [Header("커서 관리")]
    public Texture2D attackCursorTexture;
    
    [Header("몬스터 관리")]
    public MonsterDatas  monsterData;
    public List<Monster> monsters = new List<Monster>();

    [Header("UI References")]
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI remainTimeText;
    public TextMeshProUGUI midText;
    public Slider          remainTimeSlider;
    public Slider          tailSlider;
    
    [Header("현재 게임 정보")]
    public  float EnableTime = 60f; // 라운드당 가능한 시간
    private float remainTIme;       // 현재 남아있는 시간

    [Header("사운드 시작 관리")]
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

    public SpriteRenderer mapSprite;

    private void Awake()
    {
        instance = this;
        
        Application.targetFrameRate = 60;
    
        // 저장된 PlayerPrefs값으로 현재 씬 세팅(리스트는 0번부터 시작하기 때문에, 1 빼주기)
        currentLevelData = levelDataList[PlayerPrefs.GetInt("Level") - 1];
        audioSource.clip = currentLevelData.audioClip;               // 음악 변경
        beatCounter      = currentLevelData.createAndMoveCountBeat;  // 비터카운트값 변경

        mapSprite.sprite = currentLevelData.mapSprite; // 맵 스프라이트 설정
    }

    private IEnumerator Start()
    {
        RemainTime     = 60;
        CurrentRound   = currentLevelData.level;    // 현재 라운드 설정
        RoundText.text = $"Round : {CurrentRound}"; // UI에 현재 라운드 표시
        midText.text   = $"Round {CurrentRound}";   // 중앙 텍스트 표시
        SetAttackCursor();                          // 커서 변환 적용

        yield return WaitAndGo(); // 게임 시작 대기 애니메이션
        
        isGameStart = true;
        
        AudioSyncManager.instance.PrepareGame(); // 오디오 시간 기반 게임 시작
    }

    private IEnumerator WaitAndGo()
    {
        midText.transform.DOScale(1, 1f).SetEase(Ease.OutBounce);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.RoundNumber, 0.5f);
        yield return new WaitForSeconds(2f);
        midText.text = "START!";
        midText.DOFade(0, 1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(1f);    // DOFade 시간보다 길게 설정해야 함. 일단을 동일하게 1F로 설정
    }

    private void Update()
    {
        // 사운드가 시작될 때, 시간도 같이 체크
        if (AudioSyncManager.instance.musicStarted)
        {
            // 남은 시간이 0보다 크면 계속 시간을 감소시킴
            if (RemainTime >= 0)
            {
                RemainTime -= Time.deltaTime; // Time.deltaTime은 한 프레임당 걸린 시간
            }
        }
        
        // 게임 종료 체크
        if(RemainTime < 0)
        {
            GameClear();
        }
    }
    
    private void PlayerBeatMove()
    {
        PlayerController.instance.Move(PlayerController.instance.moveDirection, PlayerController.instance.MoveDelay);
    }
    
    private void EnemyBeatMove()
    {
        var monsters = instance.monsters;
        for (int i = monsters.Count - 1; i >= 0; i--)
        {
            if (monsters[i] != null)
                monsters[i].Move(0.15f);
            else
                monsters.RemoveAt(i);
        }
    }
    
    private void BeatManagement()
    {
        if (isGameStart && !isGameOver)
        {
            if (beatCounter >= currentLevelData.createAndMoveCountBeat)
            {
                // 쓰레기 이동 진행
                PatternGenerator.instance.GenerateNextPattern();
                
                // 비트 초기화
                beatCounter = 0;
            }
        }
    }    
    
    // 좌우 노드 체크(=> 비트 관리)
    public void CurrentNodeDestroyCheck(NodeType inputType)
    {
        if (isGameOver || !isGameStart)
        {
            return;
        }
        
        // 좌우 노드 삭제 체크 
        if      (inputType == NodeType.LeftNote)  leftNodeDestroy  = true;
        else if (inputType == NodeType.RightNote) rightNodeDestroy = true;
        
        // 초기화
        if (rightNodeDestroy && leftNodeDestroy)
        {
            leftNodeDestroy  = false;
            rightNodeDestroy = false;
            beatCounter++;

            PlayerBeatMove();    // 플레이어 비트 이동
            EnemyBeatMove();     // 적 비트 이동
            BeatManagement();    // 패턴 생성
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
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Game_Over, 0.5f);
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
            
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Game_Clear, 0.5f);
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
        Vector2 centerHotspot = new Vector2(attackCursorTexture.width / 2f, attackCursorTexture.height / 2f);
        Cursor.SetCursor(attackCursorTexture, centerHotspot, CursorMode.ForceSoftware);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion
}