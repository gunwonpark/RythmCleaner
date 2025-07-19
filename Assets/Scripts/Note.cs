using UnityEngine;

public enum NoteType
{
    LeftNote,  // 왼쪽에서 생성   => 마우스로 타격
    RightNote  // 오른쪽에서 생성 => 방향키로 타격
}

public class Note : MonoBehaviour
{
    public  float    speed;          // 기존 속도 (호환성 유지)
    private float    targetX;        // 기존 targetX (호환성 유지)
    private bool     isMoving = false;
    private NoteType noteType;
    
    // 🎯 새로운 시간 기반 이동 시스템
    [Header("시간 기반 이동")]
    private Vector3 startPosition;   // 시작 위치
    private Vector3 endPosition;     // 목표 위치  
    private float   moveTime;        // 이동하는데 걸리는 총 시간
    private float   elapsedTime;     // 경과 시간
    private bool    useTimeBasedMovement = false; // 시간 기반 이동 사용 여부
    
    void Update()
    {
        if (isMoving)
        {
            if (useTimeBasedMovement)
            {
                // 🎯 새로운 시간 기반 보간 이동
                UpdateTimeBasedMovement();
            }
            else
            {
                // 🔄 기존 속도 기반 이동 (호환성 유지)
                UpdateSpeedBasedMovement();
            }
        }
    }
    
    // 🎯 새로운 시간 기반 이동 업데이트
    void UpdateTimeBasedMovement()
    {
        elapsedTime += Time.deltaTime;
        
        // 진행도 계산 (0~1)
        float progress = elapsedTime / moveTime;
        
        if (progress >= 1f)
        {
            // 정확히 목표 지점에 도착
            transform.position = endPosition;
            
            // 중앙에 도착했으므로 실패 처리 후 삭제
            NodeSpawnManager.Instance.OnNoteMissed();
            NodeSpawnManager.Instance.RemoveNoteFromList(this);
            GameManager.instance.CurrnetNodeDestoryCheck(noteType);
            Destroy(gameObject);
            return;
        }
        
        // 보간으로 정확한 위치 계산
        transform.position = Vector3.Lerp(startPosition, endPosition, progress);
        
        // 히트 체크를 위한 targetX 업데이트
        targetX = endPosition.x;
    }
    
    // 🔄 기존 속도 기반 이동 (호환성 유지)
    void UpdateSpeedBasedMovement()
    {
        if (noteType == NoteType.LeftNote)
        {
            // 왼쪽 노드: 오른쪽으로 이동
            transform.Translate(Vector3.right * (speed * Time.deltaTime));
            
            // 중앙에 도착했는지 확인 (타겟존을 지나쳤을 때)
            if (transform.position.x > targetX + NodeSpawnManager.Instance.hitRange)
            {
                // 중앙에 도착했으므로 실패 처리 후 삭제
                NodeSpawnManager.Instance.OnNoteMissed();
                NodeSpawnManager.Instance.RemoveNoteFromList(this);
                GameManager.instance.CurrnetNodeDestoryCheck(noteType);
                Destroy(gameObject);
                return;
            }
            
            // 화면을 벗어나면 삭제
            if (transform.position.x > targetX + 10f)
            {
                NodeSpawnManager.Instance.RemoveNoteFromList(this);
                Destroy(gameObject);
            }
        }
        else if (noteType == NoteType.RightNote)
        {
            // 오른쪽 노드: 왼쪽으로 이동
            transform.Translate(Vector3.left * (speed * Time.deltaTime));
            
            // 중앙에 도착했는지 확인 (타겟존을 지나쳤을 때)
            if (transform.position.x < targetX - NodeSpawnManager.Instance.hitRange)
            {
                // 중앙에 도착했으므로 실패 처리 후 삭제
                NodeSpawnManager.Instance.OnNoteMissed();
                NodeSpawnManager.Instance.RemoveNoteFromList(this);
                GameManager.instance.CurrnetNodeDestoryCheck(noteType);
                Destroy(gameObject);
                return;
            }
            
            // 화면을 벗어나면 삭제
            if (transform.position.x < targetX - 10f)
            {
                NodeSpawnManager.Instance.RemoveNoteFromList(this);
                Destroy(gameObject);
            }
        }
    }
    
    // 🎯 새로운 시간 기반 초기화 메서드
    public void InitializeWithTime(Vector3 startPos, Vector3 endPos, float duration, NoteType type)
    {
        // Note 태그 설정
        gameObject.tag = "Note";
        
        startPosition = startPos;
        endPosition = endPos;
        moveTime = duration;
        noteType = type;
        elapsedTime = 0f;
        useTimeBasedMovement = true;
        isMoving = true;
        
        // 히트 체크를 위한 targetX 설정
        targetX = endPos.x;
        
        Debug.Log($"🎯 [{type}] 시간기반 이동 시작 - 시간: {duration:F3}초, 거리: {Vector3.Distance(startPos, endPos):F2}");
    }
    
    // 🔄 기존 초기화 메서드 (호환성 유지)
    public void Initialize(float moveSpeed, float target, NoteType type)
    {
        // Note 태그 설정
        gameObject.tag = "Note";
        speed          = moveSpeed;
        targetX        = target;
        noteType       = type;
        useTimeBasedMovement = false;
        isMoving       = true;
    }
    
    public NoteType GetNoteType()
    {
        return noteType;
    }
} 