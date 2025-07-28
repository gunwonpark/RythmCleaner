using UnityEngine;

public class RhythmNode : MonoBehaviour
{
    private NodeType _nodeType;
    
    // 이동 관련 변수들
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private double  targetHitTime; // 목표 도착 시간 (dspTime 기준)
    private float   moveSpeed;
    private bool    isInitialized    = false;
    private bool    hasReachedTarget = false;
    
    // 참조
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {   
        // 컴포넌트 설정
        SetupVisuals();
    }
    
    void SetupVisuals()
    {
        // 스프라이트 렌더러가 없으면 생성
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }
    
    public void Initialize(Vector3 start, Vector3 target, double hitTime, float speed, NodeType nodeType)
    {
        startPosition  = start;
        targetPosition = target;
        targetHitTime  = hitTime;
        moveSpeed      = speed;
        isInitialized  = true;
        _nodeType      = nodeType;
        
        transform.position = startPosition;
        
        // Debug.Log($"노드 초기화: 시작({start}) -> 목표({target}), 도착시간: {hitTime}");
    }
    
    void Update()
    {
        if (!isInitialized || hasReachedTarget) 
            return;
        
        // ★ 현재 오디오 시간 기준으로 이동
        double currentAudioTime = AudioSettings.dspTime;
        
        // 목표 시간과의 차이 계산
        double timeToTarget = targetHitTime - currentAudioTime;
        
        // 거리와 시간을 이용해 정확한 위치 계산
        float totalDistance = Vector3.Distance(startPosition, targetPosition);
        float totalTime     = totalDistance / moveSpeed;
        
        // 이동 진행도 계산 (0~1)
        double elapsedTime = totalTime - timeToTarget;
        float  progress    = Mathf.Clamp01((float)(elapsedTime / totalTime));
        
        // 위치 업데이트
        Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, progress);
        transform.position  = newPosition;
        
        // 목표 지점 도착 확인(노드가 중앙에 도착하고, 여유시간을 줘서, 너무 빡빡하게 사라지지 않도록 함...)
        // + 100ms 여유시간
        if (timeToTarget <= -0.1f)
        {
            if(progress >= 1.0f) // 도착 progress 확인인
            {
                // Debug.Log(progress + " 도달 및 " + timeToTarget + "시간 도달 제거");
                ReachTarget();
            }
        }
        
        // 너무 늦게 도착한 경우 제거
        if (timeToTarget < -1.0) // 1초 이상 지나면 제거
        {
            Debug.Log("노드가 너무 늦게 도착하여 제거됩니다.");
            Destroy(gameObject);
        }
    }
    
    void ReachTarget()
    {
        if (hasReachedTarget) 
            return;
        
        hasReachedTarget   = true;
        transform.position = targetPosition;
        
        // 매니저에게 도착 알림
        if (AudioSyncManager.instance != null)
        {
            AudioSyncManager.instance.OnNodeReachedTarget(this, AudioSettings.dspTime);
        }
        
        GameManager.instance.CurrentNodeDestroyCheck(_nodeType); // 바로 제거 (타겟 표시기가 피드백을 담당)
        Destroy(gameObject);                                     // 바로 제거 (타겟 표시기가 피드백을 담당)
    }
    
    // 외부에서 목표 시간을 가져올 수 있는 메서드
    public double GetTargetHitTime()
    {
        return targetHitTime;
    }
} 