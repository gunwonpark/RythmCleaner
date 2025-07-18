using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Test Mode일경우")]
    public bool IsTestMode = false;
    public bool UseTestBullet = false; // 테스트 모드일때 꼬리개수만큼 총알을 발사할지 테스트 총알개수를 발사할지
    public int TestBulletCount = 3; // 테스트 모드일 때 발사할 총알 개수

    [Header("이동 로직")]
    public float MoveDelay = 0.15f; // 움직임에 걸리는 시간
    public float MoveDistance = 1f; // 이동할 거리
    public Ease moveEase = Ease.OutQuad;
    public bool IsMoving;
    private Vector3Int moveDirection; // 이번 비트에 이동할 방향

    [Header("공격로직")]
    public Bullet AttackBullet;
    public float AttackDelay = 0.5f; // 공격에 걸리는 시간
    public Vector2 AttackDirection = Vector2.right; // 공격 방향
    public float spreadAngle = 15f;

    [Header("쓰레기 꼬리")]
    public TailFollower tailPrefab;
    public List<TailFollower> followers; // 플레이어를 따라다닐 오브젝트들
    public List<Vector3> positionHistory = new List<Vector3>();

    void Update()
    {
        // test
        if (IsTestMode == false)
            return;

        // moveDirection = Vector3Int.zero; // 매 프레임 초기화
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     moveDirection = Vector3Int.up;
        // }
        // else if (Input.GetKeyDown(KeyCode.S))
        // {
        //     moveDirection = Vector3Int.down;
        // }
        // else if (Input.GetKeyDown(KeyCode.A))
        // {
        //     moveDirection = Vector3Int.left;
        // }
        // else if (Input.GetKeyDown(KeyCode.D))
        // {
        //     moveDirection = Vector3Int.right;
        // }
        //
        // Move(moveDirection, MoveDelay);

        // if(Input.GetMouseButtonDown(0))
        // {
        //     Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     Vector2 direction = (mousePosition - transform.position).normalized;
        //     AttackDirection = direction;
        //
        //     Attack(AttackDelay, AttackDirection);
        // }
    }

    // 어처피 움직임은 moveDelay 따라 결정되므로 움직이는 중에 움직이는 경우는 없을것이다
    // moveDelay 비트의 속도에 따라 자동으로 조정되어야 한다.
    public void Move(Vector3Int moveDirection, float moveDelay)
    {
        if(IsMoving || moveDirection == Vector3Int.zero)
        {
            return; // 이동 방향이 없으면 아무것도 하지 않음
        }

        IsMoving = true;
        //Debug.Log("Player IsMoving");
        
        positionHistory.Insert(0, transform.position);

        if (positionHistory.Count > followers.Count + 1)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }

        for (int i = 0; i < followers.Count; i++)
        {
            if(i >= positionHistory.Count)
            {
                break; 
            }
            Vector3 targetPos = positionHistory[i];
            followers[i].MoveTo(targetPos, moveDelay);
        }

        // 1. 목표 위치 계산 (현재 위치 + 방향)
        Vector3 targetPosition = transform.position + moveDirection;

        transform.DOMove(targetPosition, moveDelay)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                IsMoving = false; // 이동 완료 후 IsMoving 상태 해제
            });
    }

    // 플레이어의 쓰레기 봉투 개수에 따라 한번에 여러개의 불렛을 원뿔 각도로 발사하도록 수정해야 된다.
    public void Attack(float attackDelay, Vector2 attackDirection)
    {
        int bulletCount = followers.Count;

        if (bulletCount <= 0 && IsTestMode == false)
        {
            return;
        }

        if(UseTestBullet && IsTestMode)
        {
            bulletCount = TestBulletCount;    
        }

        // 꼬리가 1개일때는 그냥 한발만 발사
        if (bulletCount == 1)
        {
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);
            Bullet singleBullet = Instantiate(AttackBullet, transform.position, rotation);
            singleBullet.Shoot(attackDirection);
            return;
        }

        float centerAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float totalSpreadAngle = (bulletCount - 1) * spreadAngle;
        float startAngle = centerAngle - totalSpreadAngle / 2f;

        // 꼬리 개수만큼 총알 발사
        for (int i = 0; i < bulletCount; i++)
        {
            // 현재 총알의 발사 각도 계산
            float currentAngle = startAngle + i * spreadAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle - 90f);

            // 총알 생성
            Bullet bullet = Instantiate(AttackBullet, transform.position, rotation);

            // 해당 각도로 발사
            float currentAngleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 bulletDirection = new Vector2(Mathf.Cos(currentAngleRad), Mathf.Sin(currentAngleRad));
            bullet.Shoot(bulletDirection);
        }
    }

    // 몬스터가 죽은경우 꼬리를 추가해 주기 때문에 Manager단에서 이 이벤트를 호출하면 좋을거 같다
    public void AddTail()
    {
        Vector3 spawnPosition;

        // 히스토리가 부족할 경우, 마지막꼬리 위치에 중첩해서 생성해둔다
        if (positionHistory.Count <= followers.Count)
        {
            // 꼬리가 없다면 플레이어 바로 뒤 위치에 생성
            if (followers.Count == 0)
            {
                spawnPosition = transform.position - Vector3.right * MoveDistance; // 플레이어 위치에서 왼쪽으로 이동
            }
            else
            {
                spawnPosition = followers[followers.Count - 1].transform.position; // 마지막 꼬리 위치에 생성
            }
        }
        else
        {
            // 히스토리가 충분하면 기존 방식대로 위치 계산
            spawnPosition = positionHistory[followers.Count];
        }

        TailFollower newTailObject = Instantiate(tailPrefab, spawnPosition, Quaternion.identity);

        followers.Add(newTailObject);
    }
}