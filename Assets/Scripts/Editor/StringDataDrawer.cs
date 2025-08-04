using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(StringData))]
public class StringDataDrawer : PropertyDrawer
{
    private const int   PATTERN_LENGTH = 11;
    private const float BUTTON_SIZE    = 18f;
    private const float SPACING        = 2f;
    private const float LABEL_WIDTH    = 50f;
    
    // 각 패턴의 foldout 상태를 저장하는 Dictionary
    private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 속성들 가져오기
        SerializedProperty upData    = property.FindPropertyRelative("upData");
        SerializedProperty downData  = property.FindPropertyRelative("downData");
        SerializedProperty leftData  = property.FindPropertyRelative("leftData");
        SerializedProperty rightData = property.FindPropertyRelative("rightData");

        // 시작 위치
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // Element 번호 추출 및 고유 키 생성
        string elementNumber = ExtractElementNumber(property.propertyPath);
        string foldoutKey    = property.propertyPath; // 고유한 키로 사용
        
        // foldout 상태 가져오기 (기본값: true로 펼쳐진 상태)
        if (!foldoutStates.ContainsKey(foldoutKey))
        {
            foldoutStates[foldoutKey] = true;
        }
        
        // Foldout 그리기
        bool foldout                  = foldoutStates[foldoutKey];
        foldout                       = EditorGUI.Foldout(rect, foldout, $"패턴 {elementNumber}", true, EditorStyles.foldoutHeader);
        foldoutStates[foldoutKey]     = foldout;
        
        rect.y += EditorGUIUtility.singleLineHeight + SPACING;

        // foldout이 펼쳐진 경우에만 패턴 그리기
        if (foldout)
        {
            // 각 방향별로 패턴 그리기
            rect = DrawPatternRow(rect, "Up", upData, false);    // 가로
            rect = DrawPatternRow(rect, "Down", downData, false); // 가로
            rect = DrawPatternRow(rect, "Left", leftData, true);  // 세로
            rect = DrawPatternRow(rect, "Right", rightData, true); // 세로
        }

        EditorGUI.EndProperty();
    }

    private string ExtractElementNumber(string propertyPath)
    {
        // propertyPath는 "stringData.Array.data[0]" 형태
        // [숫자] 패턴을 찾아서 추출
        var match = System.Text.RegularExpressions.Regex.Match(propertyPath, @"\[(\d+)\]");
        if (match.Success)
        {
            if (int.TryParse(match.Groups[1].Value, out int index))
            {
                return (index + 1).ToString();
            }
        }
        return "1";
    }

    private Rect DrawPatternRow(Rect rect, string label, SerializedProperty stringProperty, bool isVertical)
    {
        // 라벨 그리기
        Rect labelRect = new Rect(rect.x, rect.y, LABEL_WIDTH, BUTTON_SIZE);
        EditorGUI.LabelField(labelRect, label);

        float startX   = rect.x + LABEL_WIDTH + SPACING;
        float currentX = startX;
        float currentY = rect.y;

        // 버튼들 그리기
        for (int i = 0; i < PATTERN_LENGTH; i++)
        {
            // 매번 현재 패턴 상태 가져오기 (실시간 업데이트 반영)
            string currentPattern = stringProperty.stringValue;
            if (string.IsNullOrEmpty(currentPattern) || currentPattern.Length != PATTERN_LENGTH)
            {
                currentPattern = new string('0', PATTERN_LENGTH);
                stringProperty.stringValue = currentPattern;
            }

            // 현재 위치의 값 가져오기
            int currentValue = 0;
            if (i < currentPattern.Length && char.IsDigit(currentPattern[i]))
            {
                currentValue = currentPattern[i] - '0';
                currentValue = Mathf.Clamp(currentValue, 0, 4);
            }

            Rect buttonRect;
            
            if (isVertical)
            {
                // 세로 배치 (Left, Right)
                buttonRect = new Rect(currentX, currentY + i * (BUTTON_SIZE + SPACING), BUTTON_SIZE, BUTTON_SIZE);
            }
            else
            {
                // 가로 배치 (Up, Down)
                buttonRect = new Rect(currentX + i * (BUTTON_SIZE + SPACING), currentY, BUTTON_SIZE, BUTTON_SIZE);
            }
            
            // 버튼 색상 설정
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = (currentValue >= 1 && currentValue <= 4) ? Color.green : Color.red;

            // 버튼 클릭 처리 (0->1->2->3->4->0 순환)
            if (GUI.Button(buttonRect, currentValue.ToString()))
            {
                int newValue = (currentValue + 1) % 5; // 0,1,2,3,4 순환
                
                // 문자열의 특정 위치만 업데이트
                char[] patternChars = currentPattern.ToCharArray();
                patternChars[i] = (char)('0' + newValue);
                stringProperty.stringValue = new string(patternChars);
            }

            GUI.backgroundColor = originalColor;
        }

        // 텍스트 필드에서 사용할 최신 패턴 가져오기
        string pattern = stringProperty.stringValue;

        // 문자열 필드도 추가로 표시 (직접 편집 가능)
        if (isVertical)
        {
            // 복붙용 멀티라인 텍스트 에어리어 (버튼 바로 옆에)
            float multiTextX      = startX + BUTTON_SIZE + 10f;
            float textAreaHeight  = (BUTTON_SIZE + SPACING) * PATTERN_LENGTH - SPACING;
            Rect  textAreaRect    = new Rect(multiTextX, currentY, 60f, textAreaHeight);
            
            // 커스텀 GUIStyle 생성 (패딩과 폰트 크기만 조정)
            GUIStyle customTextAreaStyle  = new GUIStyle(EditorStyles.textArea);
            customTextAreaStyle.padding   = new RectOffset(2, 2, 2, 2);
            customTextAreaStyle.fontSize  = 16;
            customTextAreaStyle.alignment = TextAnchor.UpperLeft;
            
            // 현재 패턴을 멀티라인 형태로 변환
            string multilinePattern = "";
            for (int i = 0; i < pattern.Length; i++)
            {
                multilinePattern += pattern[i];
                if (i < pattern.Length - 1)
                    multilinePattern += "\n";
            }
            
            // 멀티라인 텍스트 에어리어 (복붙용) - 커스텀 스타일 적용
            string newMultilineValue = EditorGUI.TextArea(textAreaRect, multilinePattern, customTextAreaStyle);
            
            // 멀티라인 입력을 패턴으로 변환
            if (newMultilineValue != multilinePattern)
            {
                string[] lines      = newMultilineValue.Split('\n');
                string   newPattern = "";
                
                for (int i = 0; i < PATTERN_LENGTH; i++)
                {
                    if (i < lines.Length && lines[i].Length > 0)
                    {
                        char firstChar = lines[i][0];
                        // 0~4 범위 체크
                        if (firstChar >= '0' && firstChar <= '4')
                        {
                            newPattern += firstChar;
                        }
                        else
                        {
                            newPattern += "0";
                        }
                    }
                    else
                    {
                        newPattern += "0";
                    }
                }
                
                stringProperty.stringValue = newPattern;
            }
        }
        else
        {
            // 가로 배치일 때는 기존 방식
            float textFieldX    = startX + PATTERN_LENGTH * (BUTTON_SIZE + SPACING) + 10f;
            float textFieldY    = currentY;
            
            Rect  textFieldRect = new Rect(textFieldX, textFieldY, 100f, BUTTON_SIZE);
            
            string newStringValue = EditorGUI.TextField(textFieldRect, stringProperty.stringValue);
            
            // 입력 검증: 11자리, 0~4만 허용
            if (newStringValue.Length <= PATTERN_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(newStringValue, "^[0-4]*$"))
            {
                // 부족한 자리는 0으로 채움
                while (newStringValue.Length < PATTERN_LENGTH)
                {
                    newStringValue += "0";
                }
                stringProperty.stringValue = newStringValue;
            }
        }

        // 다음 줄 위치 계산
        if (isVertical)
        {
            // 세로 배치는 더 많은 높이 필요
            return new Rect(rect.x, rect.y + (BUTTON_SIZE + SPACING) * PATTERN_LENGTH + SPACING, rect.width, rect.height);
        }
        else
        {
            // 가로 배치는 한 줄만
            return new Rect(rect.x, rect.y + BUTTON_SIZE + SPACING * 2, rect.width, rect.height);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 제목 높이 (foldout 헤더)
        float titleHeight = EditorGUIUtility.singleLineHeight + SPACING;
        
        // foldout 상태 확인
        string foldoutKey = property.propertyPath;
        bool   foldout    = foldoutStates.ContainsKey(foldoutKey) ? foldoutStates[foldoutKey] : true;
        
        // 접혀있으면 제목 높이만 반환
        if (!foldout)
        {
            return titleHeight;
        }
        
        // 펼쳐져 있으면 전체 높이 계산
        // Up, Down (가로) 높이
        float horizontalRowHeight = (BUTTON_SIZE + SPACING * 2) * 2; // 2개 패턴 줄
        
        // Left, Right (세로) 높이
        float verticalRowHeight   = ((BUTTON_SIZE + SPACING) * PATTERN_LENGTH + SPACING) * 2; // 2개 패턴 세로 줄
        
        return titleHeight + horizontalRowHeight + verticalRowHeight;
    }
}