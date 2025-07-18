using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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


    [Header("쓰레기 꼬리")]
    public List<TailFollower> followers; // 플레이어를 따라다닐 오브젝트들
    public List<Vector3> positionHistory = new List<Vector3>();

    void Update()
    {
        // test용
        moveDirection = Vector3Int.zero; // 매 프레임 초기화
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Vector3Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Vector3Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Vector3Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Vector3Int.right;
        }

        Move(moveDirection, MoveDelay);

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - transform.position).normalized;
            AttackDirection = direction;

            Attack(AttackDelay, AttackDirection);
        }
    }

    // 어처피 움직임은 moveDuration에 따라 결정되므로 움직이는 중에 움직이는 경우는 없을것이다
    // moveDuration은 비트의 속도에 따라 자동으로 조정되어야 한다.
    public void Move(Vector3Int moveDirection, float moveDuration)
    {
        if(IsMoving || moveDirection == Vector3Int.zero)
        {
            return; // 이동 방향이 없으면 아무것도 하지 않음
        }

        IsMoving = true;
        Debug.Log("Player IsMoving");

        // 1. 목표 위치 계산 (현재 위치 + 방향)
        Vector3 targetPosition = transform.position + moveDirection;

        transform.DOMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                IsMoving = false; // 이동 완료 후 IsMoving 상태 해제
            });
    }

    public void Attack(float attackDelay, Vector2 attackDirection)
    {
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

        Bullet bullet = Instantiate(AttackBullet, transform.position, rotation);
        bullet.Shoot(attackDirection);
    }
}