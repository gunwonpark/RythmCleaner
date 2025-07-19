using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isGameStart = false;
    public bool isGameOver  = false;
    
    [Header("비트관리")]
    public int  beatCounter      = 0;       // 노드 생성 때, 카운트 증가
    public bool leftNodeDestory  = false;   // 좌우 노드 다 삭제되야, 비트 증가
    public bool rightNodeDestory = false;   // 좌우 노드 다 삭제되야, 비트 증가
    
    [Header("플레이어 이동 관리")]
    public float playerMoveInterval = 0.25f;

    [Header("커서 관리")]
    public Texture2D AttackCursurTexture;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // yield return new waitforseconds 3 2 1 GO 애니메이션 진행
        // 
        //
        
        isGameStart = true;

        StartCoroutine(BeatManagement()); // 비트 관리
        StartCoroutine(PlayerMoveCo());   // 플레이어 계속 움직이기
    }

    // 지속 비트 체크 및 비트 작업 진행
    IEnumerator BeatManagement()
    {
        // 커서 변환 적용
        SetAttackCursor();

        while (isGameStart && !isGameOver)
        {
            Debug.Log("실행 중");
            if (beatCounter >= PatternGenerator.instance.levelData.countBeat)
            {
                Debug.Log($"🎯 Beat 목표 달성! beatCounter:{beatCounter} >= countBeat:{PatternGenerator.instance.levelData.countBeat}");
                
                // 쓰레기 이동 진행
                PatternGenerator.instance.GenerateNextPattern();
                
                // 비트 초기화
                beatCounter = 0;
                Debug.Log($"🔄 beatCounter 리셋: {beatCounter}");
            }
            yield return null;
        }
    }

    IEnumerator PlayerMoveCo()
    {
        // 게임 시작하고, 게임 끝나기 전 까지 이동
        while (isGameStart && !isGameOver)
        {
            TestManager.Instance.player.Move(TestManager.Instance.player.moveDirection, TestManager.Instance.player.MoveDelay);
            
            yield return new WaitForSeconds(playerMoveInterval);
        }
    }
    
    // 좌우 노드 체크(=> 비트 관리)
    public void CurrnetNodeDestoryCheck(NoteType inputType)
    {
        Debug.Log($"🎵 NodeDestroy 체크: {inputType} | Left:{leftNodeDestory} | Right:{rightNodeDestory}");
        
        // 좌우 노드 삭제 체크 
        if (inputType == NoteType.LeftNote)
            leftNodeDestory  = true;
        else if (inputType == NoteType.RightNote)
            rightNodeDestory = true;
            
        Debug.Log($"📋 업데이트 후: Left:{leftNodeDestory} | Right:{rightNodeDestory}");
        
        // 초기화
        if (rightNodeDestory && leftNodeDestory)
        {
            leftNodeDestory  = false;
            rightNodeDestory = false;
            beatCounter++;
            
            // 기존 몬스터 모두 각자 방향으로 이동(monster.Move에서 beatCounter 체크)
            if (TestManager.Instance.Monsters.Count != 0)
            {
                foreach (Monster monster in TestManager.Instance.Monsters)
                {
                    if(monster != null)
                        monster.Move(0.15f);
                }
            }
            Debug.Log($"✅ beatCounter 증가! 현재: {beatCounter} | 목표: {PatternGenerator.instance.levelData.countBeat}");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("게임 오버!");
        
        // 커서 초기화
        ResetCursor();

        //TODO : 다른 필요한 로직들 ex) 노드 생성 중지, UI 팝업 띄어주기 등


    }

    #region 커서 변경 함수
    public void SetAttackCursor()
    {
        Vector2 centerHotspot = new Vector2(AttackCursurTexture.width / 2f, AttackCursurTexture.height / 2f);
        Cursor.SetCursor(AttackCursurTexture, centerHotspot, CursorMode.Auto);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    #endregion
}