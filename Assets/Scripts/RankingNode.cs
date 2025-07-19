using TMPro;
using UnityEngine;

public class RankingNode : MonoBehaviour
{
    public int rank;
    public string nickName;
    public int score;

    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string nickName, int score)
    {
        this.rank = rank;
        this.nickName = nickName;
        this.score = score;
        UpdateUI();
    }

    private void UpdateUI()
    {
        rankText.text = rank.ToString();
        nickNameText.text = nickName;
        scoreText.text = score.ToString();
    }
}
