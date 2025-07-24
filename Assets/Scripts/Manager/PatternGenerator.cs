using UnityEngine;
using UnityEngine.Serialization;

public class PatternGenerator : MonoBehaviour
{
    public static PatternGenerator instance;
    
    [Header("설정")]
    public GameObject dustPrefab;
    public Transform  centerPoint;
    
    [Header("간격 설정")]
    public float distanceFromCenter = 10f;  // 중심에서 거리

    // 각 방향별 현재 인덱스
    private int currentUpRow;           // 위     : 행 인덱스 (마지막→0으로 감소)
    private int currentDownRow = 0;     // 아래   : 행 인덱스 (0→마지막으로 증가)
    private int currentLeftCol = 0;     // 왼쪽   : 열 인덱스 (0→마지막으로 증가)
    private int currentRightCol;        // 오른쪽 : 열 인덱스 (마지막→0으로 감소)
    
    // StringData 인덱스 관리
    private int currentStringDataIndex = 0; // 현재 사용 중인 StringData 인덱스
    
    // 각 방향별 패턴 데이터
    private string[] upLines;
    private string[] downLines;
    private string[] leftLines;
    private string[] rightLines;
    
    private int maxLeftCols  = 0;
    private int maxRightCols = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializePatterns();
        Debug.Log("🎮 PatternGenerator 초기화 완료! GameManager에서 beatCounter 관리합니다.");
    }
    
    void InitializePatterns()
    {
        if (GameManager.instance.currentLevelData == null || GameManager.instance.currentLevelData.stringData.Count == 0) 
            return;
        
        LoadCurrentStringData();
    }
    
    void LoadCurrentStringData()
    {
        if (currentStringDataIndex >= GameManager.instance.currentLevelData.stringData.Count)
        {
            Debug.Log("🎉 모든 StringData 패턴이 종료되었습니다!");
            CancelInvoke("GenerateNextPattern");
            return;
        }
        
        StringData data = GameManager.instance.currentLevelData.stringData[currentStringDataIndex];
        // Debug.Log($"📋 StringData [{currentStringDataIndex}] 로드 시작!");
        
        // 각 방향별 패턴 데이터를 줄별로 분리
        upLines    = data.upData.Split('\n');
        downLines  = data.downData.Split('\n');
        leftLines  = data.leftData.Split('\n');
        rightLines = data.rightData.Split('\n');
        
        // 좌우 최대 열 수 계산 (초기화)
        maxLeftCols  = 0;
        maxRightCols = 0;
        for (int i = 0; i < leftLines.Length; i++)
        {
            maxLeftCols = Mathf.Max(maxLeftCols, leftLines[i].Trim().Length);
        }
        for (int i = 0; i < rightLines.Length; i++)
        {
            maxRightCols = Mathf.Max(maxRightCols, rightLines[i].Trim().Length);
        }
        
        // 시작 인덱스 설정
        currentUpRow    = upLines.Length - 1;   // 위     : 마지막 행부터 시작
        currentDownRow  = 0;                    // 아래   : 첫 번째 행부터 시작
        currentLeftCol  = 0;                    // 왼쪽   : 첫 번째 열부터 시작
        currentRightCol = maxRightCols - 1;     // 오른쪽 : 마지막 열부터 시작
    }
    
    // 비트 관리에서 실행
    // countBeat에 도달하면 다음 패턴 생성 + 쓰레기 이동
    public void GenerateNextPattern()
    {
        Vector3 center = centerPoint ? centerPoint.position : transform.position;
        // 각 방향별로 현재 줄/열 생성
        GenerateUpLine(center);      // 위: 행 우선
        GenerateDownLine(center);    // 아래: 행 우선
        GenerateLeftColumn(center);  // 왼쪽: 열 우선 (세로)
        GenerateRightColumn(center); // 오른쪽: 열 우선 (세로)
        
        // 다음 줄/열로 이동 (각 방향별로 다르게)
        currentUpRow--;     // 위     : 감소 (마지막→0)
        currentDownRow++;   // 아래   : 증가 (0→마지막)
        currentLeftCol++;   // 왼쪽   : 증가 (0→마지막)
        currentRightCol--;  // 오른쪽 : 감소 (마지막→0)
        
        // 현재 패턴이 모두 끝났는지 확인
        if (IsCurrentPatternFinished())
        {
            MoveToNextStringData();
        }
    }
    
    bool IsCurrentPatternFinished()
    {
        bool upFinished    = (currentUpRow < 0);
        bool downFinished  = (currentDownRow >= downLines.Length);
        bool leftFinished  = (currentLeftCol >= maxLeftCols);
        bool rightFinished = (currentRightCol < 0);
        
        bool allFinished = upFinished && downFinished && leftFinished && rightFinished;
        
        return allFinished;
    }
    
    void MoveToNextStringData()
    {
        currentStringDataIndex++;
        
        if (currentStringDataIndex >= GameManager.instance.currentLevelData.stringData.Count)
        {
            CancelInvoke("GenerateNextPattern");
        }
        else
        {
            LoadCurrentStringData();
        }
    }
    
    void GenerateUpLine(Vector3 center)
    {
        if (upLines == null || currentUpRow < 0 || currentUpRow >= upLines.Length) 
        {
            return;
        }
        
        string line = upLines[currentUpRow].Trim();
        if (string.IsNullOrEmpty(line)) 
        {
            return;
        }
        
        int circleCount = 0;
        
        for (int col = 0; col < line.Length; col++)
        {
            if (line[col] != '0')
            {
                float x = center.x + (col - (line.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                float y = center.y + distanceFromCenter;
                Vector3 pos = new Vector3(x, y, center.z);
                CreateDust(pos, Vector3Int.down, line[col]); // 위쪽은 아래로 이동
                circleCount++;
            }
        }
    }
    
    void GenerateDownLine(Vector3 center)
    {
        if (downLines == null || currentDownRow < 0 || currentDownRow >= downLines.Length) 
        {
            return;
        }
        
        string line = downLines[currentDownRow].Trim();
        if (string.IsNullOrEmpty(line)) 
        {
            Debug.Log($"[아래쪽] 빈 줄 - Row {currentDownRow}");
            return;
        }
        
        int circleCount = 0;
        
        for (int col = 0; col < line.Length; col++)
        {
            if (line[col] != '0')
            {
                float x = center.x + (col - (line.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                float y = center.y - distanceFromCenter;
                Vector3 pos = new Vector3(x, y, center.z);
                CreateDust(pos, Vector3Int.up, line[col]); // 아래는 위로 이동
                circleCount++;
            }
        }
    }
    
    void GenerateLeftColumn(Vector3 center)
    {
        if (leftLines == null || currentLeftCol < 0 || currentLeftCol >= maxLeftCols) 
        {
            return;
        }
        
        int circleCount = 0;
        
        // 현재 열(currentLeftCol)에 해당하는 모든 행을 세로로 처리
        for (int row = 0; row < leftLines.Length; row++)
        {
            string line = leftLines[row].Trim();
            if (currentLeftCol < line.Length && line[currentLeftCol] != '0')
            {
                float x = center.x - distanceFromCenter;
                float y = center.y + (row - (leftLines.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                Vector3 pos = new Vector3(x, y, center.z);
                CreateDust(pos, Vector3Int.right, line[currentLeftCol]); // 왼쪽은 오른쪽으로 이동
                circleCount++;
            }
        }
    }
    
    void GenerateRightColumn(Vector3 center)
    {
        if (rightLines == null || currentRightCol < 0 || currentRightCol >= maxRightCols) 
        {
            return;
        }
        
        int circleCount = 0;
        
        // 현재 열(currentRightCol)에 해당하는 모든 행을 세로로 처리
        for (int row = 0; row < rightLines.Length; row++)
        {
            string line = rightLines[row].Trim();
            if (currentRightCol < line.Length && line[currentRightCol] != '0')
            {
                float x = center.x + distanceFromCenter;
                float y = center.y + (row - (rightLines.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                Vector3 pos = new Vector3(x, y, center.z);
                CreateDust(pos, Vector3Int.left, line[currentRightCol]);    // 오른쪽은 왼쪽으로 이동
                circleCount++;
            }
        }
    }
    
    void CreateDust(Vector3 position, Vector3Int direction, int id)
    {
        id = id - '0';
        if (dustPrefab != null)
        {
            GameObject circle = Instantiate(dustPrefab, position, Quaternion.identity);
            circle.transform.SetParent(transform);
            Monster _monster = circle.GetComponent<Monster>();
            if (_monster != null)
            {
                _monster.SetMonsterData(direction, id, GameManager.instance.currentLevelData.createAndMoveCountBeat,11);
            }
            GameManager.instance.monsters.Add(_monster);
        }
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