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
    public float JumpHeight = 0.3f; // 점프 높이
    public int MoveDistance = 1; // 이동할 거리
    public Ease moveEase = Ease.OutQuad;
    public bool IsMoving;
    public Vector3Int moveDirection = Vector3Int.up; // 이번 비트에 이동할 방향(처음은 위로 이동)

    [Header("공격로직")]
    public Bullet AttackBullet;
    public float AttackDelay = 0.5f; // 공격에 걸리는 시간
    public Vector2 AttackDirection = Vector2.right; // 공격 방향
    public float spreadAngle = 15f;
    public Transform AttackPoint; // 공격이 시작되는 위치

    [Header("쓰레기 꼬리")]
    public TailFollower tailPrefab;
    public List<TailFollower> followers; // 플레이어를 따라다닐 오브젝트들
    public List<Vector3> positionHistory = new List<Vector3>();
    public int PerCreateTailCount = 4; // 몇개의 몬스터를 잡아야 꼬리를 생성할지
    public int curCatchedMonsterCount = 0; // 현재 잡은 몬스터 개수

    [Header("플레이어 애니메이션")]
    public Animator Animator;
    void Update()
    {
        LookAtMousePointer();

        // test
        if (IsTestMode == false)
            return;
    }

    public void LookAtMousePointer()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. 플레이어 위치에서 마우스 위치를 향하는 방향 벡터 계산
        Vector2 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

        transform.rotation = targetRotation; // 플레이어를 마우스 방향으로 회전시킴
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
        Vector3 targetPosition = transform.position + moveDirection * MoveDistance;

        transform.DOJump(targetPosition, JumpHeight, 1, moveDelay)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                IsMoving = false; // 이동 완료 후 IsMoving 상태 해제
            });
    }

    // 플레이어의 쓰레기 봉투 개수에 따라 한번에 여러개의 불렛을 원뿔 각도로 발사하도록 수정해야 된다.
    public void Attack(float attackDelay, Vector2 attackDirection)
    {
        AttackDelay = attackDelay;
        AttackDirection = attackDirection;
        
        Animator.SetTrigger("Attack");
    }

    // Attack애니메이션의 이벤트로 등록해둠 -> 애니메이션이 끝나기 전에 비트가 호출될 가능성이 없다
    public void AttackEvent()
    {
        int bulletCount = followers.Count;

        if (bulletCount <= 0 && IsTestMode == false)
        {
            return;
        }

        if (UseTestBullet && IsTestMode)
        {
            bulletCount = TestBulletCount;
        }

        // 꼬리가 1개일때는 그냥 한발만 발사
        if (bulletCount == 1)
        {
            float angle = Mathf.Atan2(AttackDirection.y, AttackDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);
            Bullet singleBullet = Instantiate(AttackBullet, AttackPoint.position, rotation);
            singleBullet.Shoot(AttackDirection);
            return;
        }

        float centerAngle = Mathf.Atan2(AttackDirection.y, AttackDirection.x) * Mathf.Rad2Deg;
        float totalSpreadAngle = (bulletCount - 1) * spreadAngle;
        float startAngle = centerAngle - totalSpreadAngle / 2f;

        // 꼬리 개수만큼 총알 발사
        for (int i = 0; i < bulletCount; i++)
        {
            // 현재 총알의 발사 각도 계산
            float currentAngle = startAngle + i * spreadAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle - 90f);

            // 총알 생성
            Bullet bullet = Instantiate(AttackBullet, AttackPoint.position, rotation);

            // 해당 각도로 발사
            float currentAngleRad = currentAngle * Mathf.Deg2Rad;
            Vector2 bulletDirection = new Vector2(Mathf.Cos(currentAngleRad), Mathf.Sin(currentAngleRad));
            bullet.Shoot(bulletDirection);
        }
    }

    // 몬스터가 죽은경우 꼬리를 추가해 주기 때문에 Manager단에서 이 이벤트를 호출하면 좋을거 같다
    public void AddTail()
    {
        curCatchedMonsterCount++;
        if (curCatchedMonsterCount < PerCreateTailCount)
        {
            return; // 꼬리를 추가할 조건이 안되면 그냥 리턴
        }

        curCatchedMonsterCount = 0; // 꼬리 추가 조건이 충족되었으므로 초기화

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

    // 몬스터, 자기 꼬리, 벽 충돌 시 게임 종료 -> 벽에 충돌은 Move부분에서 처리할수도 있다 미관상을 위하여
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Monster") || collision.CompareTag("Wall") || collision.CompareTag("Tail"))
        {
            Debug.Log("Game Over! Player collided with " + collision.gameObject.name);
            //TODO : GameManager에서 게임 오버 처리 로직을 구현해야 한다
            GameManager.instance.GameOver();
        }
    }
}