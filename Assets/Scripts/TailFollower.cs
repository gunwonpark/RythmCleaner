using DG.Tweening;
using UnityEngine;

public class TailFollower : MonoBehaviour
{
    public void MoveTo(Vector3 targetPosition, float moveDelay)
    {
        transform.DOMove(targetPosition, moveDelay)
            .SetEase(Ease.OutQuad);
    }
}
