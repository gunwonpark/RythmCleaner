using UnityEngine;

public class DynamicTextAreaAttribute : PropertyAttribute
{
    public string gridSizeFieldName;
    public int minLines;
    
    public DynamicTextAreaAttribute(string gridSizeFieldName, int minLines = 1)
    {
        this.gridSizeFieldName = gridSizeFieldName;
        this.minLines = minLines;
    }
}