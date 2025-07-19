using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Scriptable Object/Level Data", order = int.MaxValue)]
public class LevelData : ScriptableObject
{
    public int       level;                   // 레벨
    public AudioClip audioClip;               // 사용 음악
    public float     soundBeat;               // 사운드 비트 (ex 100, 95, 90)
    public int       createAndMoveCountBeat;  // 생성 및 이동 비트 간격
    public float     nodeSpeed;               // 비트와 노드가 중앙에 도착하는 시간을 맞추기 위해서 노드 스피드 조절
    
    public List<StringData> stringData; // 스트링 배열 패턴
}

[Serializable]
public class StringData
{
    [TextArea(30, 1)]
    public string upData;
    
    [TextArea(30, 1)]
    public string downData;
    
    [TextArea(30, 1)]
    public string leftData;
    
    [TextArea(30, 1)]
    public string rightData;
}