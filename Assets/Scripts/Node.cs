using UnityEngine;

public enum NodeType
{
    LeftNode,  // 왼쪽에서 생성   => 마우스로 타격
    RightNode  // 오른쪽에서 생성 => 방향키로 타격
}

public class Node : MonoBehaviour
{
    public  float    speed;
    private float    targetX;
    private bool     isMoving = false;
    private NodeType _nodeType;
    
    void Update()
    {
        if (isMoving)
        {
            if (_nodeType == NodeType.LeftNode)
            {
                // 왼쪽 노드: 오른쪽으로 이동
                transform.Translate(Vector3.right * (speed * Time.deltaTime));
                
                // 중앙에 도착했는지 확인 (타겟존을 지나쳤을 때)
                if (transform.position.x > targetX + NodeSpawnManager.Instance.hitRange)
                {
                    // 중앙에 도착했으므로 실패 처리 후 삭제
                    NodeSpawnManager.Instance.OnNoteMissed();
                    NodeSpawnManager.Instance.RemoveNoteFromList(this);
                    GameManager.instance.CurrnetNodeDestoryCheck(_nodeType);
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
            else if (_nodeType == NodeType.RightNode)
            {
                // 오른쪽 노드: 왼쪽으로 이동
                transform.Translate(Vector3.left * (speed * Time.deltaTime));
                
                // 중앙에 도착했는지 확인 (타겟존을 지나쳤을 때)
                if (transform.position.x < targetX - NodeSpawnManager.Instance.hitRange)
                {
                    // 중앙에 도착했으므로 실패 처리 후 삭제
                    NodeSpawnManager.Instance.OnNoteMissed();
                    NodeSpawnManager.Instance.RemoveNoteFromList(this);
                    GameManager.instance.CurrnetNodeDestoryCheck(_nodeType);
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
    
    public void Initialize(float moveSpeed, float target, NodeType type)
    {
        // Note 태그 설정
        gameObject.tag = "Node";
        speed          = moveSpeed;
        targetX        = target;
        _nodeType      = type;
        isMoving       = true;
    }
    
    public NodeType GetNodeType()
    {
        return _nodeType;
    }
} 