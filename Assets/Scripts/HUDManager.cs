using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public GameObject scoreText;
    public GameObject livesText;
    public GameObject timeText;

    private Vector3 _scoreTextPosition = new Vector3(-188, 529, 0);
    private Vector3 _livesTextPosition = new Vector3(1378, 529, 0);
    private Vector3 _timeTextPosition = new Vector3(549, 529, 0);

    public void GameStart()
    {
        scoreText.transform.localPosition = _scoreTextPosition;
        livesText.transform.localPosition = _livesTextPosition;
        timeText.transform.localPosition = _timeTextPosition;
    }

    /// <summary>
    /// Update the score display.
    /// </summary>
    /// <param name="score">Score to display.</param>
    public void SetScore(int score)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "SCORE: " + score.ToString();
    }

    /// <summary>
    /// Update the lives display.
    /// </summary>
    /// <param name="lives">Lives to display.</param>
    public void SetLives(int lives)
    {
        livesText.GetComponent<TextMeshProUGUI>().text = "LIVES: " + lives.ToString();
    }

    /// <summary>
    /// Update the time display.
    /// </summary>
    /// <param name="time">Time to display.</param>
    public void SetTime(int time)
    {
        timeText.GetComponent<TextMeshProUGUI>().text = "TIME: " + time.ToString();
    }

    public void GameOver()
    {
        scoreText.transform.localPosition = _scoreTextPosition;
        livesText.transform.localPosition = _livesTextPosition;
    }
}