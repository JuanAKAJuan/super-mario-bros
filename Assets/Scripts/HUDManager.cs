using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public GameObject scoreText;
    public GameObject livesText;

    private Vector3 _scoreTextPosition = new Vector3(-188, 529, 0);
    private Vector3 _livesTextPosition = new Vector3(1378, 529, 0);

    public void GameStart()
    {
        scoreText.transform.localPosition = _scoreTextPosition;
        livesText.transform.localPosition = _livesTextPosition;
    }

    public void SetScore(int score)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "SCORE: " + score.ToString();
    }

    public void SetLives(int lives)
    {
        livesText.GetComponent<TextMeshProUGUI>().text = "LIVES: " + lives.ToString();
    }

    public void GameOver()
    {
        scoreText.transform.localPosition = _scoreTextPosition;
        livesText.transform.localPosition = _livesTextPosition;
    }
}