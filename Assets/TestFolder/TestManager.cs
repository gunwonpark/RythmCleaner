using UnityEngine;

public class TestManager : MonoBehaviour
{
    private static TestManager _instance;
    public static TestManager Instance
    {
        get
        {
            return _instance;
        }
    }
    public PlayerController player;

    private void Awake()
    {
        _instance = this;
    }

    public void OnMonsterDie()
    {
        player.AddTail();
    }
}