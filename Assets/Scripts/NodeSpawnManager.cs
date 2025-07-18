using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class NodeSpawnManager : MonoBehaviour
{
    public static NodeSpawnManager Instance;

    [Header("Game Settings")]
    public float noteSpeed     = 5f;
    public float spawnInterval = 0.5f;
    public float hitRange      = 0.5f;
    
    public  float failDelay     = 0.5f;
    private float failDelayTimer;
    
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;
    
    [Header("Game Objects")]
    public SpriteRenderer successNodePrefab;
    public Transform      targetZone;
    public GameObject     attackNodePrefab; // 왼쪽 노드 프리팹
    public Transform      rightSpawnPoint;  // 오른쪽 스폰 포인트
    public Transform      spawnPoint;       // 왼쪽 스폰 포인트
    public GameObject     moveNotePrefab;   // 오른쪽 노드 프리팹
    
    private int score = 0;
    private NoteType currentNoteType = NoteType.LeftNote; // 번갈아가며 생성하기 위해

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // UI 초기화
        UpdateScoreUI();
        ShowResult("");
        
        // 노드 생성 시작
        InvokeRepeating("SpawnNote", 0.5f, spawnInterval);
    }
    
    void Update()
    {
        // 실패 입력 불가
        failDelayTimer -= Time.deltaTime;
        if (failDelayTimer >= 0)
            return;
        
        // 성공 노드 색 복구
        if(successNodePrefab.color != Color.white)
            successNodePrefab.color = Color.white;
        
        // 공격 노드 => 왼쪽 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            CheckHit(NoteType.LeftNote, "Mouse Click");
        }
        
        // 무브 노드 => ASDW 각각 구분
        if (Input.GetKeyDown(KeyCode.A))
        {
            CheckHit(NoteType.RightNote, "A");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            CheckHit(NoteType.RightNote, "S");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            CheckHit(NoteType.RightNote, "D");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            CheckHit(NoteType.RightNote, "W");
        }
    }
    
    void SpawnNote()
    {
        GameObject noteToSpawn   = null;
        Transform  spawnPosition = null;
        
        // 현재 노드 타입에 따라 프리팹과 스폰 포인트 선택
        if (currentNoteType == NoteType.LeftNote)
        {
            noteToSpawn   = attackNodePrefab;
            spawnPosition = spawnPoint;
        }
        else if (currentNoteType == NoteType.RightNote)
        {
            noteToSpawn   = moveNotePrefab;
            spawnPosition = rightSpawnPoint;
        }
        
        // 노드 생성
        if (noteToSpawn != null && spawnPosition != null)
        {
            GameObject note = Instantiate(noteToSpawn, spawnPosition.position, Quaternion.identity);
            Note noteScript = note.GetComponent<Note>();
            if (noteScript != null)
            {
                noteScript.Initialize(noteSpeed, targetZone.position.x, currentNoteType);
            }
        }
        
        // 다음 노드 타입으로 변경 (번갈아가며)
        currentNoteType = (currentNoteType == NoteType.LeftNote) ? NoteType.RightNote : NoteType.LeftNote;
    }
    
    void CheckHit(NoteType inputType, string keyPressed)
    {
        // 타겟 존 근처에 있는 노드들을 찾기
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");
        bool hit = false;
        
        foreach (GameObject noteObj in notes)
        {
            Note noteScript = noteObj.GetComponent<Note>();
            if (noteScript == null) continue;
            
            // 입력 타입과 노드 타입이 일치하는지 확인
            if (noteScript.GetNoteType() != inputType) continue;
            
            float distance = Mathf.Abs(noteObj.transform.position.x - targetZone.position.x);
            
            if (distance <= hitRange)
            {
                // 성공!
                score += 100;
                ShowResult($"Success! ({keyPressed} key)");
                Destroy(noteObj);
                hit = true;
                break;
            }
        }
        
        // 실패
        if (!hit)
        {
            successNodePrefab.color = new Color(0.54f, 0.54f, 0.54f);
            failDelayTimer = failDelay; // 타이머 ON
            ShowResult($"Fail! ({keyPressed} key)");
        }
        
        // UI
        UpdateScoreUI();
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score.ToString();
        }
    }
    
    void ShowResult(string result)
    {
        if (resultText != null)
        {
            resultText.text = result;
            
            // // 2초 후에 결과 텍스트 지우기
            // if (result != "")
            // {
            //     Invoke("ClearResult", 2f);
            // }
        }
    }
    
    void ClearResult()
    {
        if (resultText != null)
        {
            resultText.text = "";
        }
    }
} 