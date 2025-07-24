using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class AudioSyncManager : MonoBehaviour
{
    public static AudioSyncManager instance;

    [Header("오디오 설정")]
    public AudioSource audioSource;
    
    [Header("노드 설정")]
    public GameObject successNodeGameObject; // 중앙 도착 지점
    public GameObject leftPrefab;
    public Transform  leftSpawnPoint;        // 왼쪽 생성 지점
    public GameObject rightPrefab;
    public Transform  rightSpawnPoint;       // 오른쪽 생성 지점
    public float      nodeSpeed = 5f;        // 노드 이동 속도 (초당 유닛)
    
    [Header("타이밍 설정")]
    public float spawnOffset = 2f; // 노드가 목표에 도착하기 몇 초 전에 생성될지
    
    [Header("최적화 - 노트 관리")]
    public List<RhythmNode> rightNodes = new List<RhythmNode>();  // 오른쪽 노트들 
    public List<RhythmNode> leftNodes  = new List<RhythmNode>();  // 왼쪽 노트들 캐싱
    
    // 내부 변수들
    private double songStartTime;   // 노래가 시작된 dspTime
    private double gameStartTime;   // 게임이 시작된 dspTime
    private double nextBeatTime;    // 다음 비트가 나올 dspTime
    private float  secondsPerBeat;  // 한 비트당 시간 (초)
    private int    currentBeat = 0; // 현재 비트 카운터
    // private List<RhythmNode> activeNodes = new List<RhythmNode>();
    
    // 타겟 표시기 관련
    private GameObject     targetIndicator;
    private SpriteRenderer targetRenderer;
    private Coroutine      colorResetCoroutine;
    public bool            musicStarted = false; // 음악이 시작되었는지 확인
    
    [Header("디버그")]
    public bool showDebugInfo = true;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // BPM을 초로 변환(GameManager.instance.currentLevelData.soundBeat 기반)
        secondsPerBeat = 60f / GameManager.instance.currentLevelData.soundBeat;
    }
    
    public void PrepareGame()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource가 할당되지 않았습니다!");
            return;
        }
        
        if (leftSpawnPoint == null || rightSpawnPoint == null || successNodeGameObject == null)
        {
            Debug.LogError("SpawnPoint들이나 TargetPoint가 할당되지 않았습니다!");
            return;
        }
        
        // 게임 시작 시간 기록
        gameStartTime = AudioSettings.dspTime; // 준비 시간
        
        // 첫 번째 노드들이 중앙에 도착하는 시간 계산
        float distanceLeft  = Vector3.Distance(leftSpawnPoint.position,  successNodeGameObject.transform.position);
        float distanceRight = Vector3.Distance(rightSpawnPoint.position, successNodeGameObject.transform.position);
        float travelTime    = Mathf.Max(distanceLeft, distanceRight) / nodeSpeed;
        
        // 음악 시작 시간 = 첫 번째 노드 도착 시간
        songStartTime = gameStartTime + travelTime;
        nextBeatTime  = songStartTime + secondsPerBeat; // 비트 1이 도착하는 시간으로 설정
        
        // 음악을 예약된 시간에 시작
        audioSource.PlayScheduled(songStartTime);
        
        // 첫 번째 노드들 생성 (비트 0)
        SpawnFirstNodes();
        
        // 음악 시작 시점을 정확히 감지하는 코루틴 시작
        StartCoroutine(WaitForMusicStart());
        
        Debug.Log($"게임 시작: {gameStartTime:F2}");
        Debug.Log($"첫 노드 이동 시간: {travelTime:F2}초");
        Debug.Log($"음악 시작 예정 시간: {songStartTime:F2}");
    }
    
    void SpawnFirstNodes()
    {
        // 첫 번째 비트(비트 0)의 노드들을 양쪽에서 생성
        double firstBeatHitTime = songStartTime; // 첫 번째 비트는 음악 시작과 동시에
        
        // 왼쪽에서 노드 생성
        CreateNodeFromPosition(leftSpawnPoint.position,  leftPrefab, firstBeatHitTime, 0, "Left", NodeType.LeftNote);
        
        // 오른쪽에서 노드 생성  
        CreateNodeFromPosition(rightSpawnPoint.position, rightPrefab, firstBeatHitTime, 0, "Right", NodeType.RightNote);
        
        // 다음 비트는 1부터 시작
        currentBeat = 1;
    }
    
    // 음악 시작 시점을 정확히 감지하는 코루틴
    IEnumerator WaitForMusicStart()
    {
        // 음악 시작 시간까지 정확히 대기
        while (AudioSettings.dspTime < songStartTime)
        {
            yield return null; // 다음 프레임까지 대기
        }
        
        // 음악이 시작된 정확한 순간에 디버그 출력
        musicStarted = true;
        Debug.Log($"🎵 음악 시작! 정확한 시간: {AudioSettings.dspTime:F2} (예정: {songStartTime:F2})");
    }

    void Update()
    {
        // 음악이 끝났으면 노드 생성 중단
        if (musicStarted && !audioSource.isPlaying)
        {
            return;
        }
        
        // 현재 오디오 시간 계산 (음악 시작 이전에는 음수가 됨)
        double currentAudioTime = AudioSettings.dspTime - songStartTime;
        
        // 게임 시작부터 일정한 간격으로 노드 생성 (음악 시작 여부와 무관)
        if (AudioSettings.dspTime >= nextBeatTime - spawnOffset)
        {
            SpawnNodeForBeat(currentBeat);
            
            // 다음 비트 시간 계산
            currentBeat++;
            nextBeatTime = songStartTime + (currentBeat * secondsPerBeat);
        }
    }
    
    // 특정 위치에서 노드를 생성하는 메서드
    void CreateNodeFromPosition(Vector3 spawnPos,GameObject nodePrefabs, double targetHitTime, int beatNumber, string side, NodeType nodeType)
    {
        if (leftPrefab == null || rightPrefab == null || successNodeGameObject == null) return;
        
        // 노드 생성
        GameObject nodeObj = Instantiate(nodePrefabs, spawnPos, Quaternion.identity);
        RhythmNode node    = nodeObj.GetComponent<RhythmNode>();
        
        if (node == null)
        {
            node = nodeObj.AddComponent<RhythmNode>();
        }
        
        // 노드에 타이밍 정보 설정
        node.Initialize(spawnPos, successNodeGameObject.transform.position, targetHitTime, nodeSpeed,nodeType);
        node.name = $"Node_Beat{beatNumber}_{side}";
        
        // activeNodes.Add(node);
        if      (side == "Left")  leftNodes.Add(node);   // 왼쪽 노드는 leftNodes에 추가
        else if (side == "Right") rightNodes.Add(node);  // 오른쪽 노드는 rightNodes에 추가
        
        // Debug.Log($"비트 {beatNumber} ({side}): 노드 생성, 목표 도착 시간: {targetHitTime:F2}");
    }
    
    void SpawnNodeForBeat(int beatNumber)
    {
        if (leftPrefab == null || rightPrefab == null ||  leftSpawnPoint == null || rightSpawnPoint == null || successNodeGameObject == null)
        {
            Debug.LogWarning("노드 생성에 필요한 오브젝트들이 할당되지 않았습니다!");
            return;
        }
        
        // 이 비트의 목표 도착 시간 계산
        double targetHitTime = songStartTime + (beatNumber * secondsPerBeat);
        
        // 양쪽에서 노드 생성
        CreateNodeFromPosition(leftSpawnPoint.position,  leftPrefab,  targetHitTime, beatNumber, "Left",  NodeType.LeftNote);
        CreateNodeFromPosition(rightSpawnPoint.position, rightPrefab, targetHitTime, beatNumber, "Right", NodeType.RightNote);
    }
    
    // 노드가 타겟에 도착했을 때 호출되는 메서드
    public void OnNodeReachedTarget(RhythmNode node, double actualHitTime)
    {
        return; 
    
        double timingError = actualHitTime - node.GetTargetHitTime();
        Debug.Log($"노드 도착! 타이밍 오차: {timingError * 1000:F1}ms");
        
        // 타이밍에 따른 타겟 표시기 색상 변경
        if (Mathf.Abs((float)timingError) < 0.05f) // 50ms 이내면 Perfect
        {
            Debug.Log("Perfect Hit!");
        }
        else if (Mathf.Abs((float)timingError) < 0.1f) // 100ms 이내면 Good
        {
            Debug.Log("Good Hit!");
        }
        else if (Mathf.Abs((float)timingError) < 0.2f) // 200ms 이내면 OK
        {
            Debug.Log("OK Hit!");
        }
        else
        {
            Debug.Log("Miss...");
            // Miss는 색상 변경 없음
        }
    }
} 