using System.Collections.Generic;
using UnityEngine;

public class SimplePatternGenerator : MonoBehaviour
{
    [Header("설정")]
    public LevelData levelData;
    public GameObject circlePrefab;
    public Transform centerPoint;
    
    [Header("간격 설정")]
    public float spacing = 2f;  // 간격을 2로 설정
    public float distanceFromCenter = 10f;  // 중심에서 거리도 늘림
    
    private List<GameObject> generatedCircles = new List<GameObject>();
    
    void Start()
    {
        GeneratePattern();
    }
    
    [ContextMenu("Generate Pattern")]
    public void GeneratePattern()
    {
        ClearCircles();
        
        if (levelData == null || levelData.stringData.Count == 0) return;
        
        StringData data = levelData.stringData[0];
        Vector3 center = centerPoint ? centerPoint.position : transform.position;
        
        // 위쪽 패턴 (10글자 x 5줄)
        GenerateUpPattern(data.upData, center);
        
        // 아래쪽 패턴 (10글자 x 5줄)
        GenerateDownPattern(data.downData, center);
        
        // 왼쪽 패턴 (6글자 x 10줄)
        GenerateLeftPattern(data.leftData, center);
        
        // 오른쪽 패턴 (6글자 x 10줄)
        GenerateRightPattern(data.rightData, center);
    }
    
    void GenerateUpPattern(string patternData, Vector3 center)
    {
        if (string.IsNullOrEmpty(patternData)) return;
        
        string[] lines = patternData.Split('\n');
        
        for (int row = 0; row < lines.Length; row++)
        {
            string line = lines[row].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] == '1')
                {
                    float x = center.x + (col - (line.Length - 1) * 0.5f) * spacing; // 중앙 정렬
                    float y = center.y + distanceFromCenter + (row * spacing);
                    Vector3 pos = new Vector3(x, y, center.z);
                    CreateCircle(pos);
                }
            }
        }
    }
    
    void GenerateDownPattern(string patternData, Vector3 center)
    {
        if (string.IsNullOrEmpty(patternData)) return;
        
        string[] lines = patternData.Split('\n');
        
        for (int row = 0; row < lines.Length; row++)
        {
            string line = lines[row].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] == '1')
                {
                    float x = center.x + (col - (line.Length - 1) * 0.5f) * spacing; // 중앙 정렬
                    float y = center.y - distanceFromCenter - (row * spacing);
                    Vector3 pos = new Vector3(x, y, center.z);
                    CreateCircle(pos);
                }
            }
        }
    }
    
    void GenerateLeftPattern(string patternData, Vector3 center)
    {
        if (string.IsNullOrEmpty(patternData)) return;
        
        string[] lines = patternData.Split('\n');
        
        for (int row = 0; row < lines.Length; row++)
        {
            string line = lines[row].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] == '1')
                {
                    float x = center.x - distanceFromCenter - (col * spacing);
                    float y = center.y + (row - (lines.Length - 1) * 0.5f) * spacing; // 중앙 정렬
                    Vector3 pos = new Vector3(x, y, center.z);
                    CreateCircle(pos);
                }
            }
        }
    }
    
    void GenerateRightPattern(string patternData, Vector3 center)
    {
        if (string.IsNullOrEmpty(patternData)) return;
        
        string[] lines = patternData.Split('\n');
        
        for (int row = 0; row < lines.Length; row++)
        {
            string line = lines[row].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] == '1')
                {
                    float x = center.x + distanceFromCenter + (col * spacing);
                    float y = center.y + (row - (lines.Length - 1) * 0.5f) * spacing; // 중앙 정렬
                    Vector3 pos = new Vector3(x, y, center.z);
                    CreateCircle(pos);
                }
            }
        }
    }
    
    void CreateCircle(Vector3 position)
    {
        if (circlePrefab != null)
        {
            GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);
            circle.transform.SetParent(transform);
            generatedCircles.Add(circle);
        }
    }
    
    void ClearCircles()
    {
        foreach (GameObject circle in generatedCircles)
        {
            if (circle != null)
            {
                DestroyImmediate(circle);
            }
        }
        generatedCircles.Clear();
    }
    
    void OnDrawGizmos()
    {
        Vector3 center = centerPoint ? centerPoint.position : transform.position;
        
        // 중심점
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, 1f);
        
        // 영역 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, Vector3.one * distanceFromCenter * 2);
    }
} 