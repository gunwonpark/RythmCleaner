using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI remainTimeText;
    
    [Header("현재 게임 정보")]
    public  float EnableTime = 60f; // 라운드당 가능한 시간
    private float remainTIme;       // 현재 남아있는 시간

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
    public int   CurrentRound;
    public float TotalCunsumTime = 0f; // 마지막에 총 소모된 시간 보여주는 변수

    private float score = 0f;
    public float Score
    {
        get { return score; }
        set
        {
            score = value;
            UpdateScoreUI(); // UI 업데이트
        }
    }

    private void Awake()
    {
        instance = this;
        
        Application.targetFrameRate = 60;
    
        // 저장된 PlayerPrefs값으로 현재 씬 세팅(리스트는 0번부터 시작하기 때문에, 1 빼주기)
        currentLevelData = levelDataList[PlayerPrefs.GetInt("Level") - 1];
        audioSource.clip = currentLevelData.audioClip;               // 음악 변경
        beatCounter      = currentLevelData.createAndMoveCountBeat;  // 비터카운트값 변경
    }

    private IEnumerator Start()
    {
        isGameStart = true;
        remainTIme  = 60f;
        
        // 커서 변환 적용
        SetAttackCursor();

        yield return null;
        
        isSountStart = true;
        
        StartCoroutine(BeatManagement()); // 비트 관리
    }

    private void Update()
    {
        // 사운드가 시작될 때, 시간도 같이 체크
        if (!isSountStart)
            return;

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
    
    // 패턴 비트 관리(지속 체크)
    IEnumerator BeatManagement()
    {
        while (isGameStart && !isGameOver)
        {
            // beatCounter가 행동 카운트 createAndMoveCountBeat를 넘어가면, 다음 패턴 진행 
            if (beatCounter >= currentLevelData.createAndMoveCountBeat)
            {
                // 패턴 적 생성 진행
                PatternGenerator.instance.GenerateNextPattern();
                
                // 비트 초기화
                beatCounter = 0;
            }
            yield return 0.016f; // 🚀 고정된 대기 시간으로 최적화(60)
        }
    }
    
    // 플레이어 이동 관리
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
    public void CurrnetNodeDestoryCheck(NodeType inputType)
    {
        // 좌우 노드 삭제 체크 
        if (inputType == NodeType.LeftNode)
            leftNodeDestory  = true;
        else if (inputType == NodeType.RightNode)
            rightNodeDestory = true;
        
        // 초기화
        if (rightNodeDestory && leftNodeDestory)
        {
            leftNodeDestory  = false;
            rightNodeDestory = false;
            beatCounter++;

            // 1비트 즉, 노드 타이밍 마다 체크할 작업들!!!
            PlayerBeatMove(); // 플레이어 비트 이동
            EnemyBeatMove();  // 적 방향 이동 진행
        }
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
        
        // 커서 초기화
        ResetCursor();

        // TODO : 다른 필요한 로직들 ex) 노드 생성 중지, UI 팝업 띄어주기 등
        // 로직을 보았을때 노드 생성을 중지 하면 몬스터 움직임도 멈춤
    }

    public void GameClear()
    {
        // 이미 게임이 종료된 상태라면 중복 실행 방지
        if (isGameOver) return;

        isGameOver = true;

        float consumedTime = EnableTime - Mathf.Max(0, RemainTime);
        TotalCunsumTime += consumedTime;
        
        // 🚀 사운드 정지 최적화
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log($"🎉 게임 클리어! 소모 시간: {consumedTime:F2}초, 총 시간: {TotalCunsumTime:F2}초");

        // 커서 초기화
        ResetCursor();

        // TODO: 게임 클리어 UI 팝업, 다음 라운드로 넘어가는 로직 등 추가
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score : {score:F0}"; // 🚀 string interpolation으로 최적화
        }
    }

    void UpdateRemainTime()
    {
        if (remainTimeText != null)
        {
            remainTimeText.text = $"Remain Time : {Mathf.Max(0, remainTIme):F2}"; // 🚀 string interpolation으로 최적화
        }
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