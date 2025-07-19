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
    public float JumpHeight = 0.3f; // 점프 높이
    public float MoveDelay = 0.15f; // 움직임에 걸리는 시간
    public int PerMoveInterval; // 몇번 비트에 한번씩 움직일지
    public int currentMoveInterval = 0; // 현재 몇번 비트인지
    public int CurrentMoveCount = 0; // 현재 몇번 움직였는지
    public bool IsEnd = false; // 움직임이 끝났는지 체크하는 변수

    public int LimitMoveDistance;
    public float minScale = 0.3f; // 최소 크기


    public bool IsMoveOnce = false; // 한번 움직였는지 체크하는 변수
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
        // 🚀 최적화: 불필요한 에러 로그 제거 (성능 향상)
        if (IsMoving || MoveDirection == Vector3Int.zero)
        {
            return; 
        }
        
        currentMoveInterval++;
        
        if (currentMoveInterval == PerMoveInterval)
        {
            IsMoveOnce = true;
            currentMoveInterval = 0;
            CurrentMoveCount++;
            if (CurrentMoveCount == LimitMoveDistance + 1)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.Join(transform.DOJump(transform.position + MoveDirection, JumpHeight, 1, moveDelay)
                    .SetEase(MoveEase));
                sequence.Join(transform.DOScale(0, moveDelay)).SetEase(MoveEase);
                sequence.OnComplete(() =>
                                {
                   Destroy(gameObject); // 몬스터가 죽으면 오브젝트 제거
                                });

                return;
            }
        }
        else
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Join(transform.DOJump(transform.position, JumpHeight, 1, moveDelay)
                .SetEase(MoveEase));

            if (!IsMoveOnce)
            {
                float progress = (float)currentMoveInterval / (PerMoveInterval - 1);
                float targetScale = Mathf.Lerp(minScale, 1f, progress);

                sequence.Join(transform.DOScale(targetScale, moveDelay).SetEase(MoveEase));
            }

            return;
        }

        IsMoving = true;

        Vector3 targetPosition = transform.position + MoveDirection;
        
        transform.DOJump(targetPosition, JumpHeight, 1, moveDelay)
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
