using UnityEngine;

public class Monster : MonoBehaviour
{
    private int hp = 1;

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
    public void TakeDamage(int damage)
    {
        HP -= damage;
    }
}
