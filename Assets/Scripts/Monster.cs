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

    public bool IsDead = false;
    [Header("변하는 몬스터 데이터")]
    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp <= 0)
            {
                if (IsDead) return; // 이미 죽은 몬스터는 다시 죽지 않도록
                IsDead = true;
                Collider.enabled = false; // 몬스터가 죽으면 Collider 비활성화
                OnMonsterDie();
                SpriteRenderer.DOFade(0, 0.5f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
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

    public Collider2D Collider;

    public bool IsMoveOnce = false; // 한번 움직였는지 체크하는 변수
    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<Collider2D>();
        Collider.enabled = false;
        Color color = SpriteRenderer.color;
        color.a = 0.5f; // 초기 투명도 설정
        SpriteRenderer.color = color; // 초기 투명도 적용
    }

    // 처음 세팅해 줄때 왼쪽에서 생성된 오브젝트면 moveDirection을 오른쪽으로 해주면 된다
    // string에 있는 id에 따라 몬스터 데이터를 설정해 준다
    public void SetMonsterData(Vector3Int moveDirection, int id, int perMoveInterval, int gridSize)
    {
        MoveDirection = moveDirection * MoveDistance;
        Data = GameManager.instance.monsterData.GetMonsterData(id);
        PerMoveInterval = perMoveInterval;
        LimitMoveDistance = gridSize;
        try
        {
            SpriteRenderer.sprite = Data.Sprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"몬스터 스프라이트 설정 실패: {id}" + e);
            // 기본 스프라이트로 설정하거나 다른 처리를 할 수 있습니다.
            SpriteRenderer.sprite = null; // 또는 기본 스프라이트로 설정
        }
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

        if(Collider.enabled == false)
        {
            Collider.enabled = true; // 이동 시작 시 Collider 활성화
            Color color = SpriteRenderer.color;
            color.a = 1f; // 이동 시작 시 투명도 1로 설정
            SpriteRenderer.color = color; // 투명도 적용
        }

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
    
    public void OnMonsterDie()
    {
        Collider.enabled = false;
        GameManager.instance.KillDustCount++;
        PlayerController.instance.AddTail();
    }
}
