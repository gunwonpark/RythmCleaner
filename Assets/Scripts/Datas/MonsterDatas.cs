using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterData
{
    public int ID;
    public int HP;
}

[CreateAssetMenu(fileName = "MonsterDatas", menuName = "Scriptable Objects/MonsterDatas")]
public class MonsterDatas : ScriptableObject
{
    public List<MonsterData> monsterDataList;

    public MonsterData GetMonsterData(int id)
    {
        return monsterDataList.Find(data => data.ID == id);
    }
}
