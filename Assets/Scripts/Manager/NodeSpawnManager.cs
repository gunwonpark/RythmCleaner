using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawnManager : MonoBehaviour
{
    // public static NodeSpawnManager Instance;
    //
    // [Header("Game Settings")]
    // public float hitRange  = 0.5f;
    // public float failRange = 0.2f;
    //
    // [Header("Game Objects")]
    // public SpriteRenderer successNode;
    // public Transform      targetZone;
    // public GameObject     attackNodePrefab; // 왼쪽 노드 프리팹
    // public Transform      rightSpawnPoint;  // 오른쪽 스폰 포인트
    // public Transform      spawnPoint;       // 왼쪽 스폰 포인트
    // public GameObject     moveNotePrefab;   // 오른쪽 노드 프리팹
    //
    // [Header("피드백 효과")]
    // public GameObject successEffectPrefab; // 성공 프리팹
    // public GameObject failEffectPrefab;    // 실패 프리팹
    //
    // [Header("최적화 - 노트 관리")]
    // private List<Node> leftNotes  = new List<Node>();   // 왼쪽 노트들 캐싱
    // private List<Node> rightNotes = new List<Node>();  // 오른쪽 노트들 캐싱
    //
    // //private int score = 0;
    //
    // private void Awake()
    // {
    //     Instance = this;
    // }
    //
    // public IEnumerator SpawnNotesOnBeat()
    // {
    //     // 🚀 최적화: BPM을 기반으로 1비트당 시간 간격 계산
    //     float beatInterval = 60f / GameManager.instance.currentLevelData.soundBeat; // 레벨에 따라 변경됨
    //     WaitForSeconds waitTime = new WaitForSeconds(beatInterval);                 // 캐싱으로 GC 방지
    //
    //     // 게임이 시작되고 끝나기 전까지 무한 반복
    //     while (GameManager.instance.isGameStart && !GameManager.instance.isGameOver)
    //     {
    //         // 2. 다음 비트까지 대기 (캐싱된 WaitForSeconds 사용)
    //         yield return waitTime;
    //
    //         // 3. 비트 시간에 맞춰 노드 생성 함수 호출
    //         SpawnNote();
    //     }
    // }
    //
    // void SpawnNote()
    // {
    //     // // 왼쪽 공격 노드 생성
    //     // if (attackNodePrefab != null && spawnPoint != null)
    //     // {
    //     //     GameObject leftNote = Instantiate(attackNodePrefab, spawnPoint.position, Quaternion.identity);
    //     //     Node leftNodeScript = leftNote.GetComponent<Node>();
    //     //     leftNodeScript.speed = GameManager.instance.currentLevelData.nodeSpeed; // 노드 속도 변경
    //     //     if (leftNodeScript != null)
    //     //     {
    //     //         leftNodeScript.Initialize(GameManager.instance.currentLevelData.nodeSpeed, targetZone.position.x, NodeType.LeftNote);
    //     //         leftNotes.Add(leftNodeScript); // 리스트에 추가하여 캐싱
    //     //     }
    //     // }
    //     //
    //     // // 오른쪽 무브 노드 생성 (동시에)
    //     // if (moveNotePrefab != null && rightSpawnPoint != null)
    //     // {
    //     //     GameObject rightNote = Instantiate(moveNotePrefab, rightSpawnPoint.position, Quaternion.identity);
    //     //     Node rightNodeScript = rightNote.GetComponent<Node>();
    //     //     rightNodeScript.speed = GameManager.instance.currentLevelData.nodeSpeed; // 노드 속도 변경
    //     //     if (rightNodeScript != null)
    //     //     {
    //     //         rightNodeScript.Initialize(GameManager.instance.currentLevelData.nodeSpeed, targetZone.position.x, NodeType.RightNote);
    //     //         rightNotes.Add(rightNodeScript); // 리스트에 추가하여 캐싱
    //     //     }
    //     // }
    // }
    //
    // public bool CheckHit(NodeType inputType, string keyPressed, Vector3Int playerMoveDirection = default)
    // {
    //     // 🚀 최적화: 캐싱된 리스트 사용 (FindGameObjectsWithTag 제거!)
    //     List<Node> targetNotes = (inputType == NodeType.LeftNote) ? leftNotes : rightNotes;
    //     bool hit = false;
    //     
    //     // 역순으로 순회하여 삭제 시 인덱스 문제 방지
    //     for (int i = targetNotes.Count - 1; i >= 0; i--)
    //     {
    //         Node nodeScript = targetNotes[i];
    //         if (nodeScript == null || nodeScript.gameObject == null)
    //         {
    //             targetNotes.RemoveAt(i); // null 참조 제거
    //             continue;
    //         }
    //         
    //         float distance = Mathf.Abs(nodeScript.transform.position.x - targetZone.position.x);
    //         
    //         if (distance <= hitRange)
    //         {
    //             // 성공!
    //             Instantiate(successEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //             
    //             // 이동 무브는 파괴 전 먼저 방향 바꿔줘야 함!
    //             if(inputType == NodeType.RightNote)
    //                 PlayerController.instance.moveDirection = playerMoveDirection;
    //             
    //             GameManager.instance.CurrnetNodeDestoryCheck(inputType);
    //             
    //             // 리스트에서 제거 후 오브젝트 삭제
    //             targetNotes.RemoveAt(i);
    //             Destroy(nodeScript.gameObject);
    //             hit = true;
    //             //Debug.Log("입력 성공");
    //             break;
    //         }
    //         // 실패 시 이펙트 호출
    //         else if(distance <= hitRange + failRange)
    //         {
    //             Instantiate(failEffectPrefab, nodeScript.transform.position, Quaternion.identity);
    //             
    //             GameManager.instance.CurrnetNodeDestoryCheck(inputType);
    //             
    //             // 리스트에서 제거 후 오브젝트 삭제
    //             targetNotes.RemoveAt(i);
    //             Destroy(nodeScript.gameObject);
    //             //Debug.Log("입력 실패");
    //             return false;
    //         }
    //     }
    //     
    //     // 실패
    //     if (!hit)
    //     {
    //         successNode.color = new Color(0.54f, 0.54f, 0.54f);
    //         InputManager.instance.failColorDelayTimer = InputManager.instance.failColorDelay; // 타이머 ON
    //         return false;
    //     }
    //     
    //     return true;
    // }
    //
    // // 노드가 중앙에 도착했을 때 호출되는 실패 처리 메서드
    // public void OnNoteMissed()
    // {
    //     // 왼쪽 노드 실패: 실패 처리 + 이전 방향으로 이동
    //     successNode.color = new Color(0.54f, 0.54f, 0.54f);
    //     InputManager.instance.failColorDelayTimer = InputManager.instance.failColorDelay;
    // }
    //
    // // 🚀 노트가 삭제될 때 리스트에서도 제거하는 메서드
    // public void RemoveNoteFromList(Node node)
    // {
    //     if (node.GetNoteType() == NodeType.LeftNote)
    //         leftNotes.Remove(node);
    //     else
    //         rightNotes.Remove(node);
    // }
    //
    // void OnDrawGizmos()
    // {
    //     Vector3 successNodePos = successNode.transform.position;
    //     
    //     // 중심점
    //     Gizmos.color = new Color(0.07f, 0f, 1f);
    //     Gizmos.DrawLine(successNodePos, successNodePos + Vector3.right * hitRange);
    //     Gizmos.color = new Color(1f, 0f, 0.04f);
    //     Gizmos.DrawLine(successNodePos, successNodePos + Vector3.left  * hitRange);
    // }
} 