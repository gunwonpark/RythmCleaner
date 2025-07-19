using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NodeSpawnManager : MonoBehaviour
{
    public static NodeSpawnManager Instance;

    [Header("Game Settings")]
    public float hitRange  = 0.5f;
    public float failRange = 0.2f;
    
    [Header("Game Objects")]
    public SpriteRenderer successNodePrefab;
    public Transform      targetZone;
    public GameObject     attackNodePrefab; // 왼쪽 노드 프리팹
    public Transform      rightSpawnPoint;  // 오른쪽 스폰 포인트
    public Transform      spawnPoint;       // 왼쪽 스폰 포인트
    public GameObject     moveNotePrefab;   // 오른쪽 노드 프리팹

    [Header("UI References")]
    public TextMeshProUGUI resultText;

    [Header("피드백 효과")]
    public GameObject successEffectPrefab; // 성공 프리팹
    public GameObject failEffectPrefab;    // 실패 프리팹

    [Header("최적화 - 노트 관리")]
    private List<Note> leftNotes = new List<Note>();   // 왼쪽 노트들 캐싱
    private List<Note> rightNotes = new List<Note>();  // 오른쪽 노트들 캐싱

    // 🎯 타이밍 측정용 변수들
    [Header("타이밍 디버그")]
    private float lastSpawnTime = 0f;        // 마지막 생성 시간
    private float expectedInterval = 0f;     // 예상 간격
    private int spawnCount = 0;              // 생성 횟수
    private float totalError = 0f;           // 총 오차
    private float maxError = 0f;             // 최대 오차
    private float minError = float.MaxValue; // 최소 오차

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowResult("");
        
        // ★ 노드 생성 시작(=> 이것도 나중에 중앙 gamemanager 관리로 이동)
        // InvokeRepeating("SpawnNote", 0.5f, spawnInterval);
        StartCoroutine(SpawnNotesOnBeat());
    }

    IEnumerator SpawnNotesOnBeat()
    {
        // 🚀 최적화: BPM을 기반으로 1비트당 시간 간격 계산
        float beatInterval = 60f / GameManager.instance.currentLevelData.soundBeat; // 레벨에 따라 변경됨
        WaitForSeconds waitTime = new WaitForSeconds(beatInterval);                 // 캐싱으로 GC 방지

        // 🎯 타이밍 측정 초기화
        expectedInterval = beatInterval;
        lastSpawnTime = Time.time;
        spawnCount = 0;
        totalError = 0f;
        maxError = 0f;
        minError = float.MaxValue;
        
        Debug.Log($"📊 [타이밍 측정] 시작 - 예상 간격: {expectedInterval:F4}초 ({GameManager.instance.currentLevelData.soundBeat} BPM)");

        // 게임이 시작되고 끝나기 전까지 무한 반복
        while (GameManager.instance.isGameStart && !GameManager.instance.isGameOver)
        {
            // 2. 다음 비트까지 대기 (캐싱된 WaitForSeconds 사용)
            yield return waitTime;

            // 3. 비트 시간에 맞춰 노드 생성 함수 호출
            SpawnNote();
        }
        
        // 🎯 최종 통계 출력
        if (spawnCount > 1)
        {
            float avgError = totalError / (spawnCount - 1);
            Debug.Log($"📊 [최종 타이밍 통계] 총 생성: {spawnCount}회, 평균 오차: {avgError * 1000:F2}ms, " +
                      $"최대 오차: {maxError * 1000:F2}ms, 최소 오차: {minError * 1000:F2}ms");
        }
    }

    void SpawnNote()
    {
        // 🎯 타이밍 측정 및 오차 계산
        float currentTime = Time.time;
        spawnCount++;
        
        if (spawnCount > 1) // 첫 번째는 기준점이므로 제외
        {
            float actualInterval = currentTime - lastSpawnTime;
            float error = Mathf.Abs(actualInterval - expectedInterval);
            
            totalError += error;
            maxError = Mathf.Max(maxError, error);
            minError = Mathf.Min(minError, error);
            
            // 🎯 실시간 오차 로그 (매 5번째마다 출력)
            if (spawnCount % 5 == 0)
            {
                float avgError = totalError / (spawnCount - 1);
                Debug.Log($"📊 [타이밍 #{spawnCount:D2}] 실제간격: {actualInterval * 1000:F2}ms, " +
                          $"예상간격: {expectedInterval * 1000:F2}ms, " +
                          $"오차: {error * 1000:F2}ms, " +
                          $"평균오차: {avgError * 1000:F2}ms");
            }
        }
        
        lastSpawnTime = currentTime;

        // 🎯 비트에 맞는 정확한 이동 시간 계산
        float beatInterval = 60f / GameManager.instance.currentLevelData.soundBeat; // 1비트당 시간
        
        Debug.Log($"🎵 BPM: {GameManager.instance.currentLevelData.soundBeat}, 이동시간: {beatInterval:F3}초");
        
        // 왼쪽 공격 노드 생성
        if (attackNodePrefab != null && spawnPoint != null)
        {
            GameObject leftNote = Instantiate(attackNodePrefab, spawnPoint.position, Quaternion.identity);
            Note leftNoteScript = leftNote.GetComponent<Note>();
            leftNoteScript.speed = GameManager.instance.currentLevelData.nodeSpeed; // 노드 속도 변경
            if (leftNoteScript != null)
            {
                // 시작위치, 목표위치, 이동시간으로 초기화
                leftNoteScript.InitializeWithTime(spawnPoint.position, new Vector3(targetZone.position.x, spawnPoint.position.y, spawnPoint.position.z), beatInterval, NoteType.LeftNote);
                leftNotes.Add(leftNoteScript); // 리스트에 추가하여 캐싱
            }
        }
        
        // 오른쪽 무브 노드 생성 (동시에)
        if (moveNotePrefab != null && rightSpawnPoint != null)
        {
            GameObject rightNote = Instantiate(moveNotePrefab, rightSpawnPoint.position, Quaternion.identity);
            Note rightNoteScript = rightNote.GetComponent<Note>();
            rightNoteScript.speed = GameManager.instance.currentLevelData.nodeSpeed; // 노드 속도 변경
            if (rightNoteScript != null)
            {
                // 시작위치, 목표위치, 이동시간으로 초기화
                rightNoteScript.InitializeWithTime(rightSpawnPoint.position, new Vector3(targetZone.position.x, rightSpawnPoint.position.y, rightSpawnPoint.position.z), beatInterval, NoteType.RightNote);
                rightNotes.Add(rightNoteScript); // 리스트에 추가하여 캐싱
            }
        }
    }
    
    public bool CheckHit(NoteType inputType, string keyPressed, Vector3Int playerMoveDirection = default)
    {
        // 🚀 최적화: 캐싱된 리스트 사용 (FindGameObjectsWithTag 제거!)
        List<Note> targetNotes = (inputType == NoteType.LeftNote) ? leftNotes : rightNotes;
        bool hit = false;
        
        // 역순으로 순회하여 삭제 시 인덱스 문제 방지
        for (int i = targetNotes.Count - 1; i >= 0; i--)
        {
            Note noteScript = targetNotes[i];
            if (noteScript == null || noteScript.gameObject == null)
            {
                targetNotes.RemoveAt(i); // null 참조 제거
                continue;
            }
            
            float distance = Mathf.Abs(noteScript.transform.position.x - targetZone.position.x);
            
            if (distance <= hitRange)
            {
                // 성공!
                GameManager.instance.Score += 100f;
                ShowResult($"Success! ({keyPressed} key)");
                Instantiate(successEffectPrefab, noteScript.transform.position, Quaternion.identity);
                
                // 이동 무브는 파괴 전 먼저 방향 바꿔줘야 함!
                if(inputType == NoteType.RightNote)
                    TestManager.Instance.player.moveDirection = playerMoveDirection;
                
                GameManager.instance.CurrnetNodeDestoryCheck(inputType);
                
                // 리스트에서 제거 후 오브젝트 삭제
                targetNotes.RemoveAt(i);
                Destroy(noteScript.gameObject);
                hit = true;
                Debug.Log("입력 성공");
                break;
            }
            // 실패 시 이펙트 호출
            else if(distance <= hitRange + failRange)
            {
                ShowResult($"Fail! ({keyPressed} key)");
                Instantiate(failEffectPrefab, noteScript.transform.position, Quaternion.identity);
                
                GameManager.instance.CurrnetNodeDestoryCheck(inputType);
                
                // 리스트에서 제거 후 오브젝트 삭제
                targetNotes.RemoveAt(i);
                Destroy(noteScript.gameObject);
                Debug.Log("입력 실패");
                return false;
            }
        }
        
        // 실패
        if (!hit)
        {
            successNodePrefab.color = new Color(0.54f, 0.54f, 0.54f);
            InputManager.instance.failColorDelayTimer = InputManager.instance.failColorDelay; // 타이머 ON
            ShowResult($"Fail! ({keyPressed} key)");
            return false;
        }
        
        return true;
    }
    
    // 노드가 중앙에 도착했을 때 호출되는 실패 처리 메서드
    public void OnNoteMissed()
    {
        // 왼쪽 노드 실패: 실패 처리 + 이전 방향으로 이동
        successNodePrefab.color = new Color(0.54f, 0.54f, 0.54f);
        InputManager.instance.failColorDelayTimer = InputManager.instance.failColorDelay;
        ShowResult("Fail! (Missed Attack Node)");
    }
    
    // 🚀 노트가 삭제될 때 리스트에서도 제거하는 메서드
    public void RemoveNoteFromList(Note note)
    {
        if (note.GetNoteType() == NoteType.LeftNote)
            leftNotes.Remove(note);
        else
            rightNotes.Remove(note);
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