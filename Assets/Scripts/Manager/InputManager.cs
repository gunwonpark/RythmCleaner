using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    [Header("Game Settings")]
    public float hitRange  = 0.5f;
    public float failRange = 0.25f;
    
    [Header("피드백 효과")]
    public GameObject successEffectPrefab; // 성공 프리팹
    public GameObject failEffectPrefab;    // 실패 프리팹
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Update()
    {
        if(Time.timeScale == 0)
        {
            return; // 게임이 일시정지 상태면 입력 처리 중단
        }

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
    // 🎯 노드 타입에 따라 캐싱된 리스트 선택 (FindGameObjectsWithTag 제거)
    List<RhythmNode> targetNotes = (inputType == NodeType.LeftNote) ? AudioSyncManager.instance.leftNodes 
                                                                    : AudioSyncManager.instance.rightNodes;

    bool hit = false;

    // 리스트 역순 순회 (중간 삭제 시 인덱스 오류 방지)
    for (int i = targetNotes.Count - 1; i >= 0; i--)
    {
        RhythmNode node = targetNotes[i];

        // 💥 null 노드 제거
        if (node == null || node.gameObject == null)
        {
            targetNotes.RemoveAt(i);
            continue;
        }

        // 🎯 현재 노드와 기준 노드(정답 위치) 사이의 거리 계산
        float distance = Mathf.Abs(node.transform.position.x - AudioSyncManager.instance.successNodeGameObject.transform.position.x);

        // 성공 여부와 실패 여부 판단용 플래그
        bool isSuccess = false;
        bool isFail    = false;

        // 🟥 오른쪽 노드(공격 입력)
        if (inputType == NodeType.RightNote)
        {
            isSuccess = distance <= hitRange;                           // 거리 안 입력 = 성공 
            isFail    = !isSuccess && distance <= hitRange + failRange; // 거리 밖 입력 = 실패
        }
        // 🟦 왼쪽 노드(이동 입력)
        else if (inputType == NodeType.LeftNote)
        {
            bool isSameDirection = PlayerController.instance.previousMoveDirection == playerMoveDirection;

            // 성공: 범위 안이고 이전 이동방향과 다를 때
            isSuccess = distance <= hitRange && !isSameDirection;

            // 실패: 실패 범위 내이고 이전 이동 방향과 같을 때
            isFail = distance <= hitRange + failRange && isSameDirection;
        }

        // 성공 or 실패 둘 중 하나라도 조건 만족 시 처리
        if (isSuccess || isFail)
        {
            HandleHit(node, inputType, isSuccess, playerMoveDirection); // 공통 처리 함수로 위임
            targetNotes.RemoveAt(i);                                    // 리스트에서 제거
            hit = isSuccess;                                            // 성공 여부 저장
            break;                                                      // 한 번에 하나의 노드만 처리
        }
    }

    // 결과 반환
    return hit;
}

/// <summary>
/// 성공/실패 공통 처리 함수
/// </summary>
/// <param name="node">     타겟 노드             </param>
/// <param name="type">     노드 타입 (Left/Right)</param>
/// <param name="isSuccess">성공 여부             </param>
/// <param name="moveDir">  이동 방향 (LeftNote용)</param>
private void HandleHit(RhythmNode node, NodeType type, bool isSuccess, Vector3Int moveDir)
{
    // 🎆 이펙트 생성 (성공/실패에 따라 prefab 분기)
    GameObject prefab = isSuccess ? successEffectPrefab : failEffectPrefab;
    Instantiate(prefab, node.transform.position, Quaternion.identity);

    // 🚶 이동 노드일 경우: 방향 갱신 처리
    if (type == NodeType.LeftNote && isSuccess)
    {
        // 이동 직후 반대 방향을 잠금 (이전 쓰레기 위치로 돌아가지 않게 함)
        PlayerController.instance.previousMoveDirection = moveDir * -1;

        // 현재 이동 방향 저장
        PlayerController.instance.moveDirection = moveDir;
    }

    // 🧩 노드 파괴 체크 후 실제 오브젝트 파괴
    GameManager.instance.CurrentNodeDestroyCheck(type);
    Destroy(node.gameObject);
}
    
    // public bool CheckHit(NodeType inputType, Vector3Int playerMoveDirection = default)
    // {
    //     // 🚀 최적화: 캐싱된 리스트 사용 (FindGameObjectsWithTag 제거!)
    //     List<RhythmNode> targetNotes = (inputType == NodeType.LeftNote) ? AudioSyncManager.instance.leftNodes : AudioSyncManager.instance.rightNodes;
    //     bool hit = false;
    //     
    //     // 역순으로 순회하여 삭제 시 인덱스 문제 방지
    //     for (int i = targetNotes.Count - 1; i >= 0; i--)
    //     {
    //         RhythmNode nodeScript = targetNotes[i];
    //         if (nodeScript == null || nodeScript.gameObject == null)
    //         {
    //             targetNotes.RemoveAt(i); // null 참조 제거
    //             continue;
    //         }
    //         
    //         float distance = Mathf.Abs(nodeScript.transform.position.x - AudioSyncManager.instance.successNodeGameObject.transform.position.x);
    //         
    //         // 공격 노드 오른쪽
    //         if (inputType == NodeType.RightNote)
    //         {
    //             // 성공
    //             if (distance <= hitRange)
    //             {
    //                 Instantiate(successEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //             
    //                 GameManager.instance.CurrentNodeDestroyCheck(inputType);
    //             
    //                 // 리스트에서 제거 후 오브젝트 삭제
    //                 targetNotes.RemoveAt(i);
    //                 Destroy(nodeScript.gameObject);
    //                 hit = true;
    //                 
    //                 break;
    //             }
    //             // 실패
    //             else if(distance <= hitRange + failRange)
    //             {
    //                 Instantiate(failEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //             
    //                 GameManager.instance.CurrentNodeDestroyCheck(inputType);
    //             
    //                 // 리스트에서 제거 후 오브젝트 삭제
    //                 targetNotes.RemoveAt(i);
    //                 Destroy(nodeScript.gameObject);
    //                 
    //                 return false;
    //             }
    //         }
    //         // 무브 노드 왼쪽
    //         else if (inputType == NodeType.LeftNote)
    //         {
    //             // 성공(범위 알맞음 && 첫 쓰레기 위치 방향 이동 아님)
    //             if (distance <= hitRange && PlayerController.instance.previousMoveDirection != playerMoveDirection)
    //             {
    //                 Instantiate(successEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //                 
    //                 // 이동 무브는 파괴 전 먼저 방향 바꿔줘야 함!
    //                 PlayerController.instance.previousMoveDirection = playerMoveDirection * -1; // 다음 이동 때, 이동 방향의 반대 방향 잠금(첫 쓰레기 위치로 이동 불가하도록)
    //                 PlayerController.instance.moveDirection         = playerMoveDirection;      // 이동 방향
    //             
    //                 GameManager.instance.CurrentNodeDestroyCheck(inputType);
    //             
    //                 // 리스트에서 제거 후 오브젝트 삭제
    //                 targetNotes.RemoveAt(i);
    //                 Destroy(nodeScript.gameObject);
    //                 hit = true;
    //                 
    //                 break;
    //             }
    //             // 실패(실패 범위 && 첫 쓰레기 위치 방향)
    //             else if(distance <= hitRange + failRange && PlayerController.instance.previousMoveDirection == playerMoveDirection)
    //             {
    //                 Instantiate(failEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //             
    //                 GameManager.instance.CurrentNodeDestroyCheck(inputType);
    //             
    //                 // 리스트에서 제거 후 오브젝트 삭제
    //                 targetNotes.RemoveAt(i);
    //                 Destroy(nodeScript.gameObject);
    //                 
    //                 return false;
    //             }
    //         }
    //     }
    //     
    //     // 실패
    //     if (!hit)
    //     {
    //         return false;
    //     }
    //     
    //     return true;
    // }
}
