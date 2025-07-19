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

    [Header("피드백 효과")]
    public GameObject successEffectPrefab; // 성공 프리팹
    public GameObject failEffectPrefab;    // 실패 프리팹

    [Header("최적화 - 노트 관리")]
    private List<Note> leftNotes = new List<Note>();   // 왼쪽 노트들 캐싱
    private List<Note> rightNotes = new List<Note>();  // 오른쪽 노트들 캐싱

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator SpawnNotesOnBeat()
    {
        // 🚀 최적화: BPM을 기반으로 1비트당 시간 간격 계산
        float beatInterval = 60f / GameManager.instance.currentLevelData.soundBeat; // 레벨에 따라 변경됨
        WaitForSeconds waitTime = new WaitForSeconds(beatInterval);                 // 캐싱으로 GC 방지

        // 게임이 시작되고 끝나기 전까지 무한 반복
        while (GameManager.instance.isGameStart && !GameManager.instance.isGameOver)
        {
            // 2. 다음 비트까지 대기 (캐싱된 WaitForSeconds 사용)
            yield return waitTime;

            // 3. 비트 시간에 맞춰 노드 생성 함수 호출
            SpawnNote();
        }
    }

    void SpawnNote()
    {
        // 왼쪽 공격 노드 생성
        if (attackNodePrefab != null && spawnPoint != null)
        {
            GameObject leftNote = Instantiate(attackNodePrefab, spawnPoint.position, Quaternion.identity);
            Note leftNoteScript = leftNote.GetComponent<Note>();
            leftNoteScript.speed = GameManager.instance.currentLevelData.nodeSpeed; // 노드 속도 변경
            if (leftNoteScript != null)
            {
                leftNoteScript.Initialize(GameManager.instance.currentLevelData.nodeSpeed, targetZone.position.x, NoteType.LeftNote);
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
                rightNoteScript.Initialize(GameManager.instance.currentLevelData.nodeSpeed, targetZone.position.x, NoteType.RightNote);
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
    }
    
    // 🚀 노트가 삭제될 때 리스트에서도 제거하는 메서드
    public void RemoveNoteFromList(Note note)
    {
        if (note.GetNoteType() == NoteType.LeftNote)
            leftNotes.Remove(note);
        else
            rightNotes.Remove(note);
    }
} 