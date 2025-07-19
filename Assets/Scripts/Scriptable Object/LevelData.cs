using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Scriptable Object/Level Data", order = int.MaxValue)]
public class LevelData : ScriptableObject
{
    public int level;                   // 레벨
    
    public AudioClip audioClip;         // 사용 음악

    public int countBeat;               // 생성 및 이동 비트 간격
    
    public List<StringData> stringData; // 스트링 배열 패턴

    public int monsterCount;             // 생성될 몬스터 수
}

[Serializable]
public class StringData
{
    [TextArea(10, 1)]
    public string upData;
    
    [TextArea(10, 1)]
    public string downData;
    
    [TextArea(10, 1)]
    public string leftData;
    
    [TextArea(10, 1)]
    public string rightData;
}