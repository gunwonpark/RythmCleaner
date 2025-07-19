using UnityEngine;

public class Effect : MonoBehaviour
{
    public float DistroyTime = 0.31f; // 이펙트가 사라지는 시간

    private void Start()
    {
        // 이펙트가 사라지는 시간 후에 오브젝트를 파괴
        Destroy(gameObject, DistroyTime);
    }
}
