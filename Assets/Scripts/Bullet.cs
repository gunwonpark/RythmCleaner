using UnityEditorInternal;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public string monsterTag = "Monster"; // 충돌을 감지할 대상의 태그

    public Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Shoot(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed; // 방향을 정규화하여 속도 적용
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(monsterTag))
        {
            Debug.Log(other.name + " 몬스터와 충돌!");
            Destroy(gameObject); // 충돌 시 총알 제거
        }
    }
}