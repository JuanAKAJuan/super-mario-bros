using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public GameObject scoreText;

    private Vector3[] scoreTextPosition = {
        new Vector3(-161, 529, 0),
        new Vector3(0, 0, 0)
    };

    public void GameStart()
    {
        scoreText.transform.localPosition = scoreTextPosition[0];
    }

    public void SetScore(int score)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }

    public void GameOver()
    {
        scoreText.transform.localPosition = scoreTextPosition[1];
    }
}