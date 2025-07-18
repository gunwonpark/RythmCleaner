 using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

[CustomPropertyDrawer(typeof(DynamicTextAreaAttribute))]
public class DynamicTextAreaDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DynamicTextAreaAttribute dynamicTextArea = (DynamicTextAreaAttribute)attribute;
        int gridSize = GetGridSize(property, dynamicTextArea.gridSizeFieldName);
        int maxLines = Mathf.Max(gridSize, dynamicTextArea.minLines);

        // 기존 값
        string value = property.stringValue;

        // 줄 단위로 분리
        string[] lines = value.Split('\n');
        List<string> newLines = new List<string>();

        for (int i = 0; i < Mathf.Min(lines.Length, gridSize); i++)
        {
            string line = lines[i];
            newLines.Add(TrimToByteLength(line, gridSize));
        }
        // 줄 수 제한
        while (newLines.Count < gridSize)
            newLines.Add("");

        string limitedValue = string.Join("\n", newLines);

        // TextArea 표시
        EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label);
        float textAreaHeight = EditorGUIUtility.singleLineHeight * maxLines;
        Rect textAreaRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, textAreaHeight);

        GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
        textAreaStyle.wordWrap = false;

        string edited = EditorGUI.TextArea(textAreaRect, limitedValue, textAreaStyle);

        // 다시 제한 적용 (붙여넣기 등)
        string[] editedLines = edited.Split('\n');
        List<string> finalLines = new List<string>();
        for (int i = 0; i < Mathf.Min(editedLines.Length, gridSize); i++)
        {
            string line = editedLines[i];
            finalLines.Add(TrimToByteLength(line, gridSize));
        }
        while (finalLines.Count < gridSize)
            finalLines.Add("");
        property.stringValue = string.Join("\n", finalLines);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        DynamicTextAreaAttribute dynamicTextArea = (DynamicTextAreaAttribute)attribute;
        int gridSize = GetGridSize(property, dynamicTextArea.gridSizeFieldName);
        int maxLines = Mathf.Max(gridSize, dynamicTextArea.minLines);
        float textAreaHeight = EditorGUIUtility.singleLineHeight * maxLines;
        return EditorGUIUtility.singleLineHeight + textAreaHeight + 4;
    }

    private int GetGridSize(SerializedProperty property, string fieldName)
    {
        SerializedProperty gridSizeProperty = property.serializedObject.FindProperty(fieldName);
        if (gridSizeProperty != null && gridSizeProperty.propertyType == SerializedPropertyType.Integer)
        {
            return gridSizeProperty.intValue;
        }
        return 10;
    }

    // 한글 2바이트, 영문/숫자 1바이트로 계산해서 자르기
    private string TrimToByteLength(string input, int maxBytes)
    {
        int byteCount = 0;
        StringBuilder sb = new StringBuilder();
        foreach (char c in input)
        {
            int charBytes = (c > 127) ? 2 : 1; // 한글 등 멀티바이트 문자는 2, 나머지는 1
            if (byteCount + charBytes > maxBytes)
                break;
            sb.Append(c);
            byteCount += charBytes;
        }
        return sb.ToString();
    }
}