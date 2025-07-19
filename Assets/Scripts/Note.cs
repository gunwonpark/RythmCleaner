using UnityEngine;

public enum NoteType
{
    LeftNote,  // 왼쪽에서 생성   => 마우스로 타격
    RightNote  // 오른쪽에서 생성 => 방향키로 타격
}

public class Note : MonoBehaviour
{
    public  float    speed;
    private float    targetX;
    private bool     isMoving = false;
    private NoteType noteType;
    
    void Update()
    {
        if (isMoving)
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
    }
    
    public void Initialize(float moveSpeed, float target, NoteType type)
    {
        // Note 태그 설정
        gameObject.tag = "Note";
        speed          = moveSpeed;
        targetX        = target;
        noteType       = type;
        isMoving       = true;
    }
    
    public NoteType GetNoteType()
    {
        return noteType;
    }
} 