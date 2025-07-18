using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�̵� ����")]
    public float MoveDelay = 0.15f; // �����ӿ� �ɸ��� �ð�
    public float MoveDistance = 1f; // �̵��� �Ÿ�
    public Ease moveEase = Ease.OutQuad;
    public bool IsMoving;

    private Vector3Int moveDirection; // �̹� ��Ʈ�� �̵��� ����

    [Header("���ݷ���")]
    public Bullet AttackBullet;
    public float AttackDelay = 0.5f; // ���ݿ� �ɸ��� �ð�
    public Vector2 AttackDirection = Vector2.right; // ���� ����


    [Header("������ ����")]
    public List<TailFollower> followers; // �÷��̾ ����ٴ� ������Ʈ��
    public List<Vector3> positionHistory = new List<Vector3>();

    void Update()
    {
        // test��
        moveDirection = Vector3Int.zero; // �� ������ �ʱ�ȭ
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

    // ��ó�� �������� moveDuration�� ���� �����ǹǷ� �����̴� �߿� �����̴� ���� �������̴�
    // moveDuration�� ��Ʈ�� �ӵ��� ���� �ڵ����� �����Ǿ�� �Ѵ�.
    public void Move(Vector3Int moveDirection, float moveDuration)
    {
        if(IsMoving || moveDirection == Vector3Int.zero)
        {
            return; // �̵� ������ ������ �ƹ��͵� ���� ����
        }

        IsMoving = true;
        Debug.Log("Player IsMoving");

        // 1. ��ǥ ��ġ ��� (���� ��ġ + ����)
        Vector3 targetPosition = transform.position + moveDirection;

        transform.DOMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                IsMoving = false; // �̵� �Ϸ� �� IsMoving ���� ����
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