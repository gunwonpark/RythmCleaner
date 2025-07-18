using UnityEngine;
using DG.Tweening;

public class FeedbackEffect : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float moveAmount = 1.5f;   // 위로 올라갈 거리
    public float duration = 0.5f;     // 애니메이션 지속 시간
    public Ease moveEase = Ease.OutQuad; // 움직임 Easing 효과

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // DOTween 시퀀스 생성
        Sequence mySequence = DOTween.Sequence();

        // 1. 위로 이동하는 애니메이션
        mySequence.Join(transform.DOMoveY(transform.position.y + moveAmount, duration).SetEase(moveEase));

        // 2. 투명해지며 사라지는 애니메이션
        mySequence.Join(spriteRenderer.material.DOFade(0, duration));

        // 3. 애니메이션이 모두 끝나면 오브젝트 파괴
        mySequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}