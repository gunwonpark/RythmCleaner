using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    public float EnableTime = 60f; // 라운드당 가능한 시간
    private float remainTIme; // 현재 남아있는 시간

    [Header("사운드 시작 관리")]
    public bool isSountStart = false;
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
    }

    private void Start()
    {
        // yield return new waitforseconds 3 2 1 GO 애니메이션 진행
        // 
        //
        
        isGameStart = true;
        remainTIme  = 60f;
        
        StartCoroutine(BeatManagement()); // 비트 관리
    }

    private void Update()
    {
        // 게임이 시작되지 않았거나, 게임 오버 상태라면 아무것도 하지 않음
        if (!isGameStart || isGameOver)
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

    // 지속 비트 체크 및 비트 작업 진행
    IEnumerator BeatManagement()
    {
        // 커서 변환 적용
        SetAttackCursor();

        while (isGameStart && !isGameOver)
        {
            if (beatCounter >= PatternGenerator.instance.levelData.countBeat)
            {
                Debug.Log($"🎯 Beat 목표 달성! beatCounter:{beatCounter} >= countBeat:{PatternGenerator.instance.levelData.countBeat}");
                
                // 쓰레기 이동 진행
                PatternGenerator.instance.GenerateNextPattern();
                
                // 비트 초기화
                beatCounter = 0;
                Debug.Log($"🔄 beatCounter 리셋: {beatCounter}");
            }
            yield return null;
        }
    }

    private void PlayerBeatMove()
    {
        TestManager.Instance.player.Move(TestManager.Instance.player.moveDirection, TestManager.Instance.player.MoveDelay);
    }

    private void EnemyBeatMove()
    {
        // 기존 몬스터 모두 각자 방향으로 이동(monster.Move에서 beatCounter 체크)
        if (TestManager.Instance.Monsters.Count != 0)
        {
            foreach (Monster monster in TestManager.Instance.Monsters)
            {
                if(monster != null)
                    monster.Move(0.15f);
            }
        }
    }
    
    // 좌우 노드 체크(=> 비트 관리)
    public void CurrnetNodeDestoryCheck(NoteType inputType)
    {
        Debug.Log($"🎵 NodeDestroy 체크: {inputType} | Left:{leftNodeDestory} | Right:{rightNodeDestory}");
        
        // 좌우 노드 삭제 체크 
        if (inputType == NoteType.LeftNote)
            leftNodeDestory  = true;
        else if (inputType == NoteType.RightNote)
            rightNodeDestory = true;
            
        Debug.Log($"📋 업데이트 후: Left:{leftNodeDestory} | Right:{rightNodeDestory}");
        
        // 초기화
        if (rightNodeDestory && leftNodeDestory)
        {
            leftNodeDestory  = false;
            rightNodeDestory = false;
            beatCounter++;

            PlayerBeatMove();    // 플레이어 비트 이동
            EnemyBeatMove();     // 적 비트 이동
            
            Debug.Log($"✅ beatCounter 증가! 현재: {beatCounter} | 목표: {PatternGenerator.instance.levelData.countBeat}");

            //사운드 시작 추가
            if (!isSountStart)
            {
                isSountStart = true;
                Debug.Log("사운드 시작!");
                SoundManager.Instance.Play("100bpm_Round3", Sound.Bgm);
            }
        }
    }

    public void GameOver()
    {
        if(isGameOver)
        {
            return;
        }
        isGameOver = true;
        Debug.Log("게임 오버!");
        
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
        Debug.Log($"이번 라운드 소모 시간: {consumedTime}, 총 소모 시간: {TotalCunsumTime}");

        // 커서 초기화
        ResetCursor();

        // TODO: 게임 클리어 UI 팝업, 다음 라운드로 넘어가는 로직 등 추가
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score.ToString();
        }
    }

    void UpdateRemainTime()
    {
        if (remainTimeText != null)
        {
            remainTimeText.text = "Remain Time : " + Mathf.Max(0, remainTIme).ToString("F2");
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