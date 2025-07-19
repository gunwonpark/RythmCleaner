using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Monster : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    [Header("참조되고 있는 몬스터 데이터")]
    public MonsterData Data;

    private int hp = 1;

    [Header("변하는 몬스터 데이터")]
    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp <= 0)
            {
                TestManager.Instance.OnMonsterDie();
                Destroy(gameObject); // 몬스터가 죽으면 오브젝트 제거
            }
        }
    }

    [Header("어떻게 움직이는지 보여주는 변수들")] 
    public Vector3Int MoveDirection;
    public Ease MoveEase = Ease.OutQuad;
    public bool IsMoving = false;
    public int MoveDistance = 1; // 이동할 거리
    public float MoveDelay = 0.15f; // 움직임에 걸리는 시간
    public int PerMoveInterval; // 몇번 비트에 한번씩 움직일지
    public int currentMoveInterval = 0; // 현재 몇번 비트인지

    public int LimitMoveDistance;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 처음 세팅해 줄때 왼쪽에서 생성된 오브젝트면 moveDirection을 오른쪽으로 해주면 된다
    // string에 있는 id에 따라 몬스터 데이터를 설정해 준다
    public void SetMonsterData(Vector3Int moveDirection, int id, int perMoveInterval, int gridSize)
    {
        MoveDirection = moveDirection * MoveDistance;
        Data = TestManager.Instance.MonsterDatas.GetMonsterData(id);
        PerMoveInterval = perMoveInterval;
        LimitMoveDistance = gridSize;
        SpriteRenderer.sprite = Data.Sprite;
    }

    public void Move(float moveDelay)
    {
        // 이게 가능하지 못할거다
        if (IsMoving || MoveDirection == Vector3Int.zero)
        {
            Debug.LogError("Monster 이 영역에 들어오는 것은 불가능하다");
            return; 
        }
        
        currentMoveInterval++;
        
        if (currentMoveInterval == PerMoveInterval)
        {
            currentMoveInterval = 0;
        }
        else
        {
            return;
        }

        IsMoving = true;

        Vector3 targetPosition = transform.position + MoveDirection;
        
        transform.DOMove(targetPosition, moveDelay)
            .SetEase(MoveEase)
            .OnComplete(() =>
            {
                IsMoving = false; // 이동 완료 후 IsMoving 상태 해제
            });
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
    }
}
