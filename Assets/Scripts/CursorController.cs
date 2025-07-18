using UnityEngine;

public class CursorController : MonoBehaviour
{
    public Texture2D AttackCursurTexture;

    [ContextMenu("Set Attack Cursor")]
    public void SetAttackCursor()
    {
        Vector2 centerHotspot = new Vector2(AttackCursurTexture.width / 2f, AttackCursurTexture.height / 2f);
        Cursor.SetCursor(AttackCursurTexture, centerHotspot, CursorMode.Auto);
    }

    [ContextMenu("Set Default Cursor")]
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
