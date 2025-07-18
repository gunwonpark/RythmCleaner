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

        Sequence mySequence = DOTween.Sequence();

        mySequence.Join(transform.DOMoveY(transform.position.y + moveAmount, duration).SetEase(moveEase));

        mySequence.Join(spriteRenderer.material.DOFade(0, duration));

        mySequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}