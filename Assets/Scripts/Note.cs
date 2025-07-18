using UnityEngine;

public enum NoteType
{
    LeftNote,  // 왼쪽에서 생성   => 마우스로 타격
    RightNote  // 오른쪽에서 생성 => 방향키로 타격
}

public class Note : MonoBehaviour
{
    private float    speed;
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
                
                // 화면을 벗어나면 삭제
                if (transform.position.x > targetX + 10f)
                {
                    Destroy(gameObject);
                }
            }
            else if (noteType == NoteType.RightNote)
            {
                // 오른쪽 노드: 왼쪽으로 이동
                transform.Translate(Vector3.left * (speed * Time.deltaTime));
                
                // 화면을 벗어나면 삭제
                if (transform.position.x < targetX - 10f)
                {
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