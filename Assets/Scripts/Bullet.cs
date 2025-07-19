using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("총알 속도")]
    public float speed = 20f;
    public string monsterTag = "Monster"; // 충돌을 감지할 대상의 태그

    public Rigidbody2D rb;

    public bool IsAttack = false;

    public GameObject DamageEffect;

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
        if (IsAttack) return; // 이미 한번의 공격이 되었으면 추가적으로 공격을 적용하지 않는다.
        if (other.CompareTag(monsterTag))
        {
            IsAttack = true;
            
            Debug.Log(other.name + " 몬스터와 충돌!");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Water_Crashed);
           
            // 🚀 최적화: GetComponent 캐싱과 null 체크 추가
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(1);
                Instantiate(DamageEffect, transform.position, Quaternion.identity); // 데미지 이펙트 생성
            }

            Destroy(gameObject); // 충돌 시 총알 제거
        }
    }
}