using UnityEngine;
using DG.Tweening;

public class FeedbackEffect : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public float moveAmount = 1.7f;   // 위로 올라갈 거리
    public float duration = 1f;     // 애니메이션 지속 시간
    public Ease moveEase = Ease.InQuad; // 움직임 Easing 효과

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Sequence mySequence = DOTween.Sequence();

        mySequence.Join(transform.DOMoveY(transform.position.y + moveAmount, duration).SetEase(Ease.OutQuad));

        mySequence.Join(spriteRenderer.material.DOFade(0, duration).SetEase(Ease.InExpo));

        mySequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}