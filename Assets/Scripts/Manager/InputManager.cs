using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    [Header("Game Settings")]
    public float hitRange  = 0.5f;
    
    [Header("피드백 효과")]
    public GameObject successEffectPrefab; // 성공 프리팹
    public GameObject failEffectPrefab;    // 실패 프리팹
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Update()
    {
        if (GameManager.instance.isGameOver)
            return;
        
        // 공격 노드 => 왼쪽 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            bool attackHitSuccess = CheckHit(NodeType.RightNote);  // 공격 무브 방향 미사용 
            
            if (attackHitSuccess)
            {
                // 성공: 마우스 방향으로 공격 실행
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction  = (mousePosition - PlayerController.instance.transform.position).normalized;
                PlayerController.instance.AttackDirection = direction;
                PlayerController.instance.Attack(PlayerController.instance.AttackDelay, direction);
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
            CheckHit(NodeType.LeftNote, playerMoveDirection);
        }
    }
    
    public bool CheckHit(NodeType inputType, Vector3Int playerMoveDirection = default)
    {
        // 🚀 최적화: 캐싱된 리스트 사용 (FindGameObjectsWithTag 제거!)
        List<RhythmNode> targetNotes = (inputType == NodeType.LeftNote) ? AudioSyncManager.instance.leftNodes : AudioSyncManager.instance.rightNodes;
        bool hit = false;
        
        // 역순으로 순회하여 삭제 시 인덱스 문제 방지
        for (int i = targetNotes.Count - 1; i >= 0; i--)
        {
            RhythmNode nodeScript = targetNotes[i];
            if (nodeScript == null || nodeScript.gameObject == null)
            {
                targetNotes.RemoveAt(i); // null 참조 제거
                continue;
            }
            
            float distance = Mathf.Abs(nodeScript.transform.position.x - AudioSyncManager.instance.successNodeGameObject.transform.position.x);
            
            if (distance <= hitRange)
            {
                // 성공!
                Instantiate(successEffectPrefab, nodeScript.transform.position, Quaternion.identity);
                
                // 이동 무브는 파괴 전 먼저 방향 바꿔줘야 함!
                if(inputType == NodeType.LeftNote)
                    PlayerController.instance.moveDirection = playerMoveDirection;
                
                GameManager.instance.CurrentNodeDestroyCheck(inputType);
                
                // 리스트에서 제거 후 오브젝트 삭제
                targetNotes.RemoveAt(i);
                Destroy(nodeScript.gameObject);
                hit = true;
                //Debug.Log("입력 성공");
                break;
            }
            // 실패 시 이펙트 호출
            else if(distance <= hitRange)
            {
                Instantiate(failEffectPrefab, nodeScript.transform.position, Quaternion.identity);
                
                GameManager.instance.CurrentNodeDestroyCheck(inputType);
                
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
            return false;
        }
        
        return true;
    }
}
