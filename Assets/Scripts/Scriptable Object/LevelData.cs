using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "Scriptable Object/Level Data", order = int.MaxValue)]
public class LevelData : ScriptableObject
{
    public int level;                   // 레벨
    
    public AudioClip audioClip;         // 사용 음악

    public int countBeat;               // 생성 및 이동 비트 간격
    
    public int gridSize;                // 열 제한
    
    public List<StringData> stringData; // 스트링 배열 패턴
}

[Serializable]
public class StringData
{
    [DynamicTextArea("gridSize", 1)]
    public string upData;
    
    [DynamicTextArea("gridSize", 1)]
    public string downData;
    
    [DynamicTextArea("gridSize", 1)]
    public string leftData;
    
    [DynamicTextArea("gridSize", 1)]
    public string rightData;
}