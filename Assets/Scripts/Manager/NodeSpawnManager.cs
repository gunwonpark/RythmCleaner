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

    [Header("피드백 효과")]
    public GameObject successEffectPrefab; // 성공 프리팹
    public GameObject failEffectPrefab;    // 실패 프리팹

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // UI 초기화
        UpdateScoreUI();
        ShowResult("");
        
        // ★ 노드 생성 시작(=> 이것도 나중에 중앙 gamemanager 관리로 이동)
        InvokeRepeating("SpawnNote", 0.5f, spawnInterval);
    }
    
    void SpawnNote()
    {
        // 왼쪽 공격 노드 생성
        if (attackNodePrefab != null && spawnPoint != null)
        {
            GameObject leftNote = Instantiate(attackNodePrefab, spawnPoint.position, Quaternion.identity);
            Note leftNoteScript = leftNote.GetComponent<Note>();
            if (leftNoteScript != null)
            {
                leftNoteScript.Initialize(noteSpeed, targetZone.position.x, NoteType.LeftNote);
            }
        }
        
        // 오른쪽 무브 노드 생성 (동시에)
        if (moveNotePrefab != null && rightSpawnPoint != null)
        {
            GameObject rightNote = Instantiate(moveNotePrefab, rightSpawnPoint.position, Quaternion.identity);
            Note rightNoteScript = rightNote.GetComponent<Note>();
            if (rightNoteScript != null)
            {
                rightNoteScript.Initialize(noteSpeed, targetZone.position.x, NoteType.RightNote);
            }
        }
    }
    
    public bool CheckHit(NoteType inputType, string keyPressed)
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
                // 성공 이펙트 생성
                Instantiate(successEffectPrefab, noteScript.transform.position, Quaternion.identity);
                
                // 현재 노드 파괴 체크
                GameManager.instance.CurrnetNodeDestoryCheck(inputType);
                
                Destroy(noteObj);
                hit = true;
                break;
            }
        }
        
        // 실패
        if (!hit)
        {
            successNodePrefab.color = new Color(0.54f, 0.54f, 0.54f);
            InputManager.instance.failDelayTimer = InputManager.instance.failDelay; // 타이머 ON
            ShowResult($"Fail! ({keyPressed} key)");
            return false;
        }
        
        // UI
        UpdateScoreUI();

        return true;
    }
    
    // 노드가 중앙에 도착했을 때 호출되는 실패 처리 메서드
    public void OnNoteMissed()
    {
        // 왼쪽 노드 실패: 실패 처리 + 이전 방향으로 이동
        successNodePrefab.color = new Color(0.54f, 0.54f, 0.54f);
        InputManager.instance.failDelayTimer = InputManager.instance.failDelay;
        ShowResult("Fail! (Missed Attack Node)");
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