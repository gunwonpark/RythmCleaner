using UnityEngine;

public class PatternGenerator : MonoBehaviour
{
    public static PatternGenerator instance;
    
    [Header("설정")]
    public GameObject circlePrefab;
    public Transform  centerPoint;
    
    [Header("간격 설정")]
    public float distanceFromCenter = 10f;  // 중심에서 거리

    [Header("특수 로직")]
    public bool FirstMonsterMoveForce = true;
    private bool isFirstMove = false; // 첫 번째 몬스터 이동 여부

    // 각 방향별 현재 인덱스
    private int currentUpRow;           // 위: 행 인덱스 (마지막→0으로 감소)
    private int currentDownRow = 0;     // 아래: 행 인덱스 (0→마지막으로 증가)
    private int currentLeftCol;         // 왼쪽: 열 인덱스 (마지막→0으로 감소)
    private int currentRightCol = 0;    // 오른쪽: 열 인덱스 (0→마지막으로 증가)
    
    // StringData 인덱스 관리
    private int currentStringDataIndex = 0; // 현재 사용 중인 StringData 인덱스
    
    // 각 방향별 패턴 데이터
    private string[] upLines;
    private string[] downLines;
    private string[] leftLines;
    private string[] rightLines;
    private int maxLeftCols = 0;
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
        if (GameManager.instance.currentLevelData == null || GameManager.instance.currentLevelData.stringData.Count == 0) return;
        
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
        Debug.Log($"📋 StringData [{currentStringDataIndex}] 로드 시작!");
        
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
        currentUpRow    = upLines.Length - 1;   // 위: 마지막 행부터 시작
        currentDownRow  = 0;                    // 아래: 첫 번째 행부터 시작
        currentLeftCol  = maxLeftCols - 1;     // 왼쪽: 마지막 열부터 시작
        currentRightCol = 0;                    // 오른쪽: 첫 번째 열부터 시작
        
        // Debug.Log($"패턴 로드 완료 - Up:{upLines.Length}행, Down:{downLines.Length}행, Left:{maxLeftCols}열, Right:{maxRightCols}열");
    }
    
    // 비트 관리에서 실행
    // countBeat에 도달하면 다음 패턴 생성 + 쓰레기 이동
    public void GenerateNextPattern()
    {
        Vector3 center = centerPoint ? centerPoint.position : transform.position;
        
        // Debug.Log($"=== Beat {levelData.createAndMoveCountBeat} 도달! 패턴 생성 ===");
        // Debug.Log($"현재 인덱스 - Up:{currentUpRow}, Down:{currentDownRow}, Left:{currentLeftCol}, Right:{currentRightCol}");
        
        // 각 방향별로 현재 줄/열 생성
        GenerateUpLine(center);      // 위: 행 우선
        GenerateDownLine(center);    // 아래: 행 우선
        GenerateLeftColumn(center);  // 왼쪽: 열 우선 (세로)
        GenerateRightColumn(center); // 오른쪽: 열 우선 (세로)
        
        // 다음 줄/열로 이동 (각 방향별로 다르게)
        currentUpRow--;     // 위: 감소 (마지막→0)
        currentDownRow++;   // 아래: 증가 (0→마지막)
        currentLeftCol--;   // 왼쪽: 감소 (마지막→0)
        currentRightCol++;  // 오른쪽: 증가 (0→마지막)
        
        isFirstMove = true; // 첫 번째 이동 완료
        // Debug.Log($"다음 인덱스로 이동 - Up:{currentUpRow}, Down:{currentDownRow}, Left:{currentLeftCol}, Right:{currentRightCol}");

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
        bool leftFinished  = (currentLeftCol < 0);
        bool rightFinished = (currentRightCol >= maxRightCols);
        
        bool allFinished = upFinished && downFinished && leftFinished && rightFinished;
        
        if (allFinished)
        {
            // Debug.Log($"✅ 현재 StringData [{currentStringDataIndex}] 패턴 완료!");
            // Debug.Log($"   - Up: {upFinished}, Down: {downFinished}, Left: {leftFinished}, Right: {rightFinished}");
        }
        
        return allFinished;
    }
    
    void MoveToNextStringData()
    {
        currentStringDataIndex++;
       // Debug.Log($"🔄 다음 StringData [{currentStringDataIndex}]로 이동 시도...");
        
        if (currentStringDataIndex >= GameManager.instance.currentLevelData.stringData.Count)
        {
            //Debug.Log("🏁 모든 StringData 패턴이 종료되었습니다!");
            CancelInvoke("GenerateNextPattern");
        }
        else
        {
            LoadCurrentStringData();
            // Debug.Log($"🆕 StringData [{currentStringDataIndex}] 시작!");
        }
    }
    
    void GenerateUpLine(Vector3 center)
    {
        if (upLines == null || currentUpRow < 0 || currentUpRow >= upLines.Length) 
        {
            // Debug.Log($"[위쪽] 생성 중단 - currentUpRow: {currentUpRow}, 총 행 수: {(upLines != null ? upLines.Length : 0)}");
            return;
        }
        
        string line = upLines[currentUpRow].Trim();
        if (string.IsNullOrEmpty(line)) 
        {
            //Debug.Log($"[위쪽] 빈 줄 - Row {currentUpRow}");
            return;
        }
        
        //Debug.Log($"[위쪽] Row {currentUpRow} 생성 - 패턴: '{line}'");
        int circleCount = 0;
        
        for (int col = 0; col < line.Length; col++)
        {
            if (line[col] != '0')
            {
                float x = center.x + (col - (line.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                float y = center.y + distanceFromCenter;
                Vector3 pos = new Vector3(x, y, center.z);
                CreateCircle(pos, Vector3Int.down, line[col]);    // 위쪽은 아래로 이동
                circleCount++;
                //Debug.Log($"  → 원 생성: Col {col}, 위치 ({x:F1}, {y:F1})");
            }
        }
        //Debug.Log($"[위쪽] 총 {circleCount}개 원 생성 완료");
    }
    
    void GenerateDownLine(Vector3 center)
    {
        if (downLines == null || currentDownRow < 0 || currentDownRow >= downLines.Length) 
        {
            //Debug.Log($"[아래쪽] 생성 중단 - currentDownRow: {currentDownRow}, 총 행 수: {(downLines != null ? downLines.Length : 0)}");
            return;
        }
        
        string line = downLines[currentDownRow].Trim();
        if (string.IsNullOrEmpty(line)) 
        {
            Debug.Log($"[아래쪽] 빈 줄 - Row {currentDownRow}");
            return;
        }
        
        //Debug.Log($"[아래쪽] Row {currentDownRow} 생성 - 패턴: '{line}'");
        int circleCount = 0;
        
        for (int col = 0; col < line.Length; col++)
        {
            if (line[col] != '0')
            {
                float x = center.x + (col - (line.Length - 1) * 0.5f) * 2f; // 간격 없이, 중앙 정렬
                float y = center.y - distanceFromCenter;
                Vector3 pos = new Vector3(x, y, center.z);
                CreateCircle(pos, Vector3Int.up, line[col]); // 아래는 위로 이동
                circleCount++;
                //Debug.Log($"  → 원 생성: Col {col}, 위치 ({x:F1}, {y:F1})");
            }
        }
        //Debug.Log($"[아래쪽] 총 {circleCount}개 원 생성 완료");
    }
    
    void GenerateLeftColumn(Vector3 center)
    {
        if (leftLines == null || currentLeftCol < 0 || currentLeftCol >= maxLeftCols) 
        {
            //Debug.Log($"[왼쪽] 생성 중단 - currentLeftCol: {currentLeftCol}, 최대 열 수: {maxLeftCols}");
            return;
        }
        
        //Debug.Log($"[왼쪽] Col {currentLeftCol} 생성 시작");
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
                CreateCircle(pos, Vector3Int.right, line[currentLeftCol]); // 왼쪽은 오른쪽으로 이동
                circleCount++;
                //Debug.Log($"  → 원 생성: Row {row} (패턴: '{line[currentLeftCol]}'), 위치 ({x:F1}, {y:F1})");
            }
        }
        //Debug.Log($"[왼쪽] 총 {circleCount}개 원 생성 완료");
    }
    
    void GenerateRightColumn(Vector3 center)
    {
        if (rightLines == null || currentRightCol < 0 || currentRightCol >= maxRightCols) 
        {
            //Debug.Log($"[오른쪽] 생성 중단 - currentRightCol: {currentRightCol}, 최대 열 수: {maxRightCols}");
            return;
        }
        
        //Debug.Log($"[오른쪽] Col {currentRightCol} 생성 시작");
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
                CreateCircle(pos, Vector3Int.left, line[currentRightCol]);    // 오른쪽은 왼쪽으로 이동
                circleCount++;
                //Debug.Log($"  → 원 생성: Row {row} (패턴: '{line[currentRightCol]}'), 위치 ({x:F1}, {y:F1})");
            }
        }
        //Debug.Log($"[오른쪽] 총 {circleCount}개 원 생성 완료");
    }
    
    void CreateCircle(Vector3 position, Vector3Int direction, int id)
    {
        id = id - '0';
        if (circlePrefab != null)
        {
            GameObject circle = Instantiate(circlePrefab, position, Quaternion.identity);
            circle.transform.SetParent(transform);
            Monster _monster = circle.GetComponent<Monster>();
            if (_monster != null)
            {
                _monster.SetMonsterData(direction, id, GameManager.instance.currentLevelData.createAndMoveCountBeat,11);
                if (isFirstMove == false && FirstMonsterMoveForce)
                {
                    _monster.MoveForce(); // 첫 번째 턴 몬스터 강제이동
                }
            }
            TestManager.Instance.Monsters.Add(_monster);
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