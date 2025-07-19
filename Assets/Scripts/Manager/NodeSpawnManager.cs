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
    private List<Node> leftNotes = new List<Node>();   // 왼쪽 노트들 캐싱
    private List<Node> rightNotes = new List<Node>();  // 오른쪽 노트들 캐싱

    // 🎵 간단하고 확실한 600ms 리듬 시스템
    [Header("🎵 600ms 간격 리듬 시스템")]
    public double bpm      = 100.0;              // BPM
    public float  leadTime = 1.5f;               // 노트 생성 리드 타임 (초)
    
    private double musicStartDSPTime = 0.0;      // 음악 시작 DSP Time
    private double secPerBeat = 0.6;             // 100 BPM = 600ms = 0.6초
    private bool   isPlayingMusic = false;       // 음악 재생 중 여부
    private int    beatCount = 0;                // 비트 카운터

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowResult("");
        
        // GameManager 사운드 시작 신호 대기
        StartCoroutine(WaitForSoundStart());
    }

    // GameManager 사운드 시작 신호 대기
    IEnumerator WaitForSoundStart()
    {
        while (!GameManager.instance.isSountStart)
        {
            yield return null; // 매 프레임 체크
        }
        
        // 사운드 시작 신호를 받으면 리듬 시스템 시작
        StartRhythmSystem();
    }
    
    // 🎵 즉시 음악 재생 + 즉시 노드 생성 시스템
    void StartRhythmSystem()
    {
        // LevelData에서 BPM 가져오기
        bpm = GameManager.instance.currentLevelData.soundBeat;
        secPerBeat = 60.0 / bpm; // 100 BPM = 0.6초 = 600ms
        
        // 🎵 음악 즉시 재생!
        musicStartDSPTime = AudioSettings.dspTime;
        GameManager.instance.audioSource.Play();
        isPlayingMusic = true;
        
        Debug.Log($"🎵 [즉시 재생 리듬 시스템]");
        Debug.Log($"   BPM: {bpm}, 1비트: {secPerBeat * 1000:F0}ms");
        Debug.Log($"   음악 시작: 즉시!");
        Debug.Log($"   노드 생성: 0ms, 600ms, 1200ms...");
        
        // 즉시 노드 생성 코루틴 시작
        StartCoroutine(SpawnNodesEvery600ms());
    }
    
        // 🎯 즉시 시작 + 절대 시간 기준 완벽 비트 동기화
    IEnumerator SpawnNodesEvery600ms()
    {
        Debug.Log("🚀 즉시 노드 생성 시작! (0ms, 600ms, 1200ms...)");
        
        // 음악이 재생 중인 동안 계속 노드 생성
        while (isPlayingMusic && GameManager.instance.audioSource.isPlaying)
        {
            // 🔥 핵심: 절대 시간 기준 정확한 비트 계산 (드리프트 방지)
            double exactBeatTime = musicStartDSPTime + (beatCount * secPerBeat);
            double nodeArrivalTime = exactBeatTime + leadTime; // 노드가 중앙에 도착할 시간
            
            // 🎯 즉시 노드 생성: 노드가 nodeArrivalTime에 정확히 도착하도록
            SpawnNoteWithPerfectTiming(nodeArrivalTime);
            
            beatCount++;
            
            // 다음 비트까지 대기 (절대 시간 기준)
            double nextBeatTime = musicStartDSPTime + (beatCount * secPerBeat);
            while (AudioSettings.dspTime < nextBeatTime)
            {
                yield return null;
            }
            
            Debug.Log($"🎵 비트 {beatCount}: 노드 생성 완료, 다음 {secPerBeat * 1000:F0}ms 후");
        }
        
        Debug.Log("🎵 음악 종료 - 노드 생성 중지");
        isPlayingMusic = false;
    }
    
    // 🎯 즉시 생성 + 완벽한 도착 타이밍 계산
    public void SpawnNoteWithPerfectTiming(double nodeArrivalTime)
    {
        // 현재 DSP 시간
        double currentDSPTime = AudioSettings.dspTime;
        
        // 거리 계산
        float leftDistance = Vector3.Distance(spawnPoint.position, targetZone.position);
        float rightDistance = Vector3.Distance(rightSpawnPoint.position, targetZone.position);
        
        // 🔥 핵심: 정확한 도착 시간까지 남은 시간 계산
        double exactTravelTime = nodeArrivalTime - currentDSPTime;
        
        // 🎯 완벽 속도 계산: 속도 = 거리 / 정확한 남은 시간
        float perfectLeftSpeed = (float)(leftDistance / exactTravelTime);
        float perfectRightSpeed = (float)(rightDistance / exactTravelTime);
        
        Debug.Log($"🎯 [즉시 생성] 도착: {nodeArrivalTime:F3}, 이동시간: {exactTravelTime * 1000:F1}ms, " +
                  $"속도: L{perfectLeftSpeed:F1} R{perfectRightSpeed:F1}");
        
        // 왼쪽 노드 생성
        if (attackNodePrefab != null && spawnPoint != null)
        {
            GameObject leftNote = Instantiate(attackNodePrefab, spawnPoint.position, Quaternion.identity);
            Node leftNodeScript = leftNote.GetComponent<Node>();
            if (leftNodeScript != null)
            {
                leftNodeScript.speed = perfectLeftSpeed;
                leftNodeScript.Initialize(perfectLeftSpeed, targetZone.position.x, NodeType.LeftNode);
                leftNotes.Add(leftNodeScript);
            }
        }
        
        // 오른쪽 노드 생성
        if (moveNotePrefab != null && rightSpawnPoint != null)
        {
            GameObject rightNote = Instantiate(moveNotePrefab, rightSpawnPoint.position, Quaternion.identity);
            Node rightNodeScript = rightNote.GetComponent<Node>();
            if (rightNodeScript != null)
            {
                rightNodeScript.speed = perfectRightSpeed;
                rightNodeScript.Initialize(perfectRightSpeed, targetZone.position.x, NodeType.RightNode);
                rightNotes.Add(rightNodeScript);
            }
        }
    }
    
    public bool CheckHit(NodeType inputType, string keyPressed, Vector3Int playerMoveDirection = default)
    {
        // 🚀 최적화: 캐싱된 리스트 사용 (FindGameObjectsWithTag 제거!)
        List<Node> targetNotes = (inputType == NodeType.LeftNode) ? leftNotes : rightNotes;
        bool hit = false;
        
        // 역순으로 순회하여 삭제 시 인덱스 문제 방지
        for (int i = targetNotes.Count - 1; i >= 0; i--)
        {
            Node nodeScript = targetNotes[i];
            if (nodeScript == null || nodeScript.gameObject == null)
            {
                targetNotes.RemoveAt(i); // null 참조 제거
                continue;
            }
            
            float distance = Mathf.Abs(nodeScript.transform.position.x - targetZone.position.x);
            
            if (distance <= hitRange)
            {
                // 성공!
                GameManager.instance.Score += 100f;
                ShowResult($"Success! ({keyPressed} key)");
                Instantiate(successEffectPrefab, nodeScript.transform.position, Quaternion.identity);
                
                // 이동 무브는 파괴 전 먼저 방향 바꿔줘야 함!
                if(inputType == NodeType.RightNode)
                    TestManager.Instance.player.moveDirection = playerMoveDirection;
                
                GameManager.instance.CurrnetNodeDestoryCheck(inputType);
                
                // 리스트에서 제거 후 오브젝트 삭제
                targetNotes.RemoveAt(i);
                Destroy(nodeScript.gameObject);
                hit = true;
                //Debug.Log("입력 성공");
                break;
            }
            // 실패 시 이펙트 호출
            else if(distance <= hitRange + failRange)
            {
                ShowResult($"Fail! ({keyPressed} key)");
                Instantiate(failEffectPrefab, nodeScript.transform.position, Quaternion.identity);
                
                GameManager.instance.CurrnetNodeDestoryCheck(inputType);
                
                // 리스트에서 제거 후 오브젝트 삭제
                targetNotes.RemoveAt(i);
                Destroy(nodeScript.gameObject);
                //Debug.Log("입력 실패");
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
    public void RemoveNoteFromList(Node node)
    {
        if (node.GetNodeType() == NodeType.LeftNode)
            leftNotes.Remove(node);
        else
            rightNotes.Remove(node);
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