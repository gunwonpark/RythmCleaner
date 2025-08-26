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
    public  float EnableTime = 60f; // 라운드당 가능한 시간 (음악 길이로 자동 설정됨)
    private float remainTIme;       // 현재 남아있는 시간
    private double gameAudioStartTime; // 게임 시작 시의 오디오 시간
    private float musicTotalLength;    // 음악 총 길이

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

    // 게임 진행 관리
    private bool isPaused = false; // 게임 일시정지 상태
    public bool IsPaused => isPaused;

    private int currentBeadCount = 0;
    private bool IsLastBeatEnd = false;

    private void Awake()
    {
        instance = this;
        
        Application.targetFrameRate = 60;
    
        // 저장된 PlayerPrefs값으로 현재 씬 세팅(리스트는 0번부터 시작하기 때문에, 1 빼주기)
        currentLevelData = levelDataList[PlayerPrefs.GetInt("Level") - 1];
        audioSource.clip = currentLevelData.audioClip;               // 음악 변경
        beatCounter      = 0;  // 0부터 시작해서 createAndMoveCountBeat까지 카운트

        // 음악 길이에 따라 게임 시간 설정
        if (currentLevelData.audioClip != null)
        {
            musicTotalLength = currentLevelData.audioClip.length;
            EnableTime = musicTotalLength;
            Debug.Log($"🎵 음악 길이: {musicTotalLength:F2}초, 게임 시간으로 설정됨");
        }

        mapSprite.sprite = currentLevelData.mapSprite; // 맵 스프라이트 설정
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
    
        RemainTime     = EnableTime; // 음악 길이로 설정된 시간으로 초기화
        CurrentRound   = currentLevelData.level;    // 현재 라운드 설정
        RoundText.text = $"Round : {CurrentRound}"; // UI에 현재 라운드 표시
        midText.text   = $"Round {CurrentRound}";   // 중앙 텍스트 표시
        SetAttackCursor();                          // 커서 변환 적용

        yield return WaitAndGo(); // 게임 시작 대기 애니메이션
        
        isGameStart = true;
        EndUI.pauseButton.interactable = true;  // WaitAndGo끝나기 전에 퍼즈버튼 누를 수 없도록 수정
        
        // 게임 시작과 동시에 첫 번째 패턴 생성
        if (PatternGenerator.instance != null)
        {
            PatternGenerator.instance.GenerateNextPattern();
        }
        
        // 게임 시작 시의 오디오 시간 기록 (현재 시점)
        gameAudioStartTime = AudioSettings.dspTime;
        
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
        if(isPaused || isGameOver)
        {
            return;
        }

        // 사운드가 시작될 때, 시간도 같이 체크
        if (AudioSyncManager.instance.musicStarted)
        {
            // 음악 진행 시간 계산 (음악 시작부터의 실제 진행 시간)
            double musicProgressTime = AudioSettings.dspTime - AudioSyncManager.instance.PauseDelayTime - AudioSyncManager.instance.SongStartTime;
            
            // 남은 시간 = 음악 총 길이 - 음악 진행 시간
            float targetTime = musicTotalLength - (float)musicProgressTime;
            
            // 부드러운 전환을 위해 lerp 사용 (급격한 변화 방지)
            RemainTime = Mathf.Lerp(RemainTime, targetTime, Time.deltaTime * 2f);
        }
        
        // 게임 종료 체크 (게임이 시작된 후에만 체크)
        if (isGameStart)
        {
            // 음악이 실제로 진행된 시간 계산
            double musicProgressTime = AudioSyncManager.instance.musicStarted ? 
                AudioSettings.dspTime - AudioSyncManager.instance.PauseDelayTime - AudioSyncManager.instance.SongStartTime : 0;
            
            // 디버그: 음악 진행 상황 표시 (5초마다)
            // if (musicProgressTime > 0 && (int)musicProgressTime % 5 == 0 && (int)musicProgressTime != 0)
            // {
            //     Debug.Log($"🎵 음악 진행: {musicProgressTime:F1}초 / {musicTotalLength:F1}초");
            // }
            
            // 게임 종료 조건
            bool timeUp = RemainTime <= 0;
            bool musicEnded = AudioSyncManager.instance.musicStarted && 
                             AudioSyncManager.instance.SongStartTime > 0 && 
                             musicProgressTime >= musicTotalLength;
            
            if (timeUp || musicEnded)
            {
                Debug.Log($"🎮 게임 종료! 시간끝:{timeUp}, 음악끝:{musicEnded}, 진행시간:{musicProgressTime:F2}초");
                GameClear();
            }
        }

        if(currentLevelData.stringData.Count <= currentBeadCount)
        {
            IsLastBeatEnd = true;
        }

        if(IsLastBeatEnd)
        {
            // 필드에 남아있는 몬스터가 없으면 게임 클리어
            if(monsters.TrueForAll(m => ReferenceEquals(m, null)))
            {
                GameClear();
            }
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
                // 디버그 로그: 패턴 생성 및 이동
                Debug.Log("생성 및 이동!");
                
                // 쓰레기 이동 진행
                PatternGenerator.instance.GenerateNextPattern();
                currentBeadCount++;

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

            // 디버그 로그: 현재 패턴과 비트 카운터
            int currentPatternNumber = PatternGenerator.instance != null ? PatternGenerator.instance.CurrentStringDataIndex : 1;
            Debug.Log($"패턴 {currentPatternNumber} / 비트 {beatCounter}");

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

    public void PauseGame()
    {
        AudioSyncManager.instance.PauseGame();
        isPaused = true;
    }

    public void ResumeGame()
    {
        AudioSyncManager.instance.ResumeGame();
        isPaused = false;
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