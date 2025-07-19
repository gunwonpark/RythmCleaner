using DG.Tweening;
using UnityEngine;

public class TailFollower : MonoBehaviour
{
    public float JumpHieght = 0.3f; // 점프 높이
    public void MoveTo(Vector3 targetPosition, float moveDelay)
    {
        transform.DOJump(targetPosition, JumpHieght, 1, moveDelay)
            .SetEase(Ease.OutQuad);
    }
}
