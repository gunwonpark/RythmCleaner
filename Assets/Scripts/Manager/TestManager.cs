using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private static TestManager _instance;
    public static TestManager Instance
    {
        get
        {
            return _instance;
        }
    }
    // 현재 플레이어
    public PlayerController player;
    // 몬스터 데이터
    public MonsterDatas MonsterDatas;
    
    public List<Monster> Monsters = new List<Monster>();
    private void Awake()
    {
        _instance = this;
    }
   
    public void OnMonsterDie()
    {
        player.AddTail();
    }
}