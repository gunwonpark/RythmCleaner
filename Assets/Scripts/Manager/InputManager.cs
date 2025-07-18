using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public float failDelay = 0.5f;
    [HideInInspector] public float failDelayTimer;

    public Vector3Int previousDirection = Vector3Int.zero;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 실패 입력 불가
        failDelayTimer -= Time.deltaTime;
        if (failDelayTimer >= 0)
            return;
        
        // 성공 노드 색 복구
        if(NodeSpawnManager.Instance.successNodePrefab.color != Color.white)
            NodeSpawnManager.Instance.successNodePrefab.color = Color.white;
        
        // 공격 노드 => 왼쪽 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            bool attackHitSuccess = NodeSpawnManager.Instance.CheckHit(NoteType.LeftNote, "mouse click");
            
            if (attackHitSuccess)
            {
                // 성공: 마우스 방향으로 공격 실행
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction  = (mousePosition - TestManager.Instance.player.transform.position).normalized;
                TestManager.Instance.player.AttackDirection = direction;
                TestManager.Instance.player.Attack(TestManager.Instance.player.AttackDelay, direction);
                //Debug.Log("공격 true");
            }
            else
            {
                //Debug.Log("공격 false");
                // 공격 실패
            }
        }
        
        // 무브 노드 => ASDW 각각 구분 (최적화된 버전)
        Vector3Int moveDirection = Vector3Int.zero;
        string keyPressed = "";
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Vector3Int.left;
            keyPressed = "A";
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Vector3Int.down;
            keyPressed = "S";
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Vector3Int.right;
            keyPressed = "D";
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Vector3Int.up;
            keyPressed = "W";
        }
        
        // 키가 눌렸다면 처리
        if (keyPressed != "")
        {
            bool hitSuccess = NodeSpawnManager.Instance.CheckHit(NoteType.RightNote, keyPressed);
            
            if (hitSuccess)
            {
                // 성공: 새로운 방향으로 이동
                TestManager.Instance.player.Move(moveDirection, TestManager.Instance.player.MoveDelay);
                previousDirection = moveDirection;
            }
            else
            {
                // 실패: 이전 방향으로 강제 이동 (역방향)
                if (previousDirection != Vector3.zero)
                {
                    TestManager.Instance.player.Move(previousDirection, TestManager.Instance.player.MoveDelay);
                }
            }
        }
    }
}
