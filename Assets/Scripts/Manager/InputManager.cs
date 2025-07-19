using System;
using UnityEngine;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public float failColorDelay = 0.1f; // 단순히 컬러만 변환 // 키 입력 불가는 아님
    [HideInInspector] public float failColorDelayTimer;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 성공 노드 색 복구
        failColorDelayTimer -= Time.deltaTime;
        if (failColorDelayTimer < 0)
        {
            if(NodeSpawnManager.Instance.successNodePrefab.color != Color.white)
                NodeSpawnManager.Instance.successNodePrefab.color = Color.white;
        }
        
        // 공격 노드 => 왼쪽 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            bool attackHitSuccess = NodeSpawnManager.Instance.CheckHit(NoteType.LeftNote, "mouse click");  // 공격 무브 방향 미사용 
            
            if (attackHitSuccess)
            {
                // 성공: 마우스 방향으로 공격 실행
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction  = (mousePosition - TestManager.Instance.player.transform.position).normalized;
                TestManager.Instance.player.AttackDirection = direction;
                TestManager.Instance.player.Attack(TestManager.Instance.player.AttackDelay, direction);
            }
            else
            {
                // 공격 실패
            }
        }
        
        // 무브 노드 => ASDW 각각 구분 (최적화된 버전)
        Vector3Int playerMoveDirection = Vector3Int.zero;
        string keyPressed = "";
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            playerMoveDirection = Vector3Int.left;
            keyPressed = "A";
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            playerMoveDirection = Vector3Int.down;
            keyPressed = "S";
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            playerMoveDirection = Vector3Int.right;
            keyPressed = "D";
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            playerMoveDirection = Vector3Int.up;
            keyPressed = "W";
        }
        
        // 키가 눌렸다면 처리
        if (keyPressed != "")
        {
            // 키를 누르면, CheckHit를 하는데, CheckHit에서 노드가 삭제되기 전에 방향을 먼저 바꿔줘야
            // moveDirection이 먼저 바뀌어야, 알맞게 이동 비트에 맞춰서 이동함
            NodeSpawnManager.Instance.CheckHit(NoteType.RightNote, keyPressed, playerMoveDirection);
            // bool hitSuccess = NodeSpawnManager.Instance.CheckHit(NoteType.RightNote, keyPressed);
            //
            // if (hitSuccess)
            // {
            //     TestManager.Instance.player.moveDirection = moveDirection;
            // }
            // else
            // {
            //     // 실패
            // }
        }
    }
}
