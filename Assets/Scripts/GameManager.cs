using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent gameStart;

    /// <summary>
    /// Represents restarting AFTER losing a life (if lives remain).
    /// </summary>
    public UnityEvent gameRestart;

    /// <summary>
    /// This will be called when lives run out.
    /// </summary>
    public UnityEvent gameOver;

    /// <summary>
    /// Event to signal HUD update for score changes.
    /// </summary>
    public UnityEvent<int> scoreChange;

    /// <summary>
    /// The amount of lives that the player starts with in a new game over after a game over.
    /// </summary>
    public int startingLives = 3;

    /// <summary>
    /// Event to signal HUD update for lives.
    /// </summary>
    public UnityEvent<int> livesChange;

    /// <summary>
    /// Sound effect to play when gaining and extra life.
    /// </summary>
    public AudioClip extraLifeSound;


    private int _score = 0;
    private int _lives = 0;
    private const int _ScoreForExtraLife = 2000;

    /// <summary>
    /// Tracks the next score target for a life.
    /// </summary>
    private int _nextLifeScoreThreshold = 0;

    /// <summary>
    /// To play the sound effect.
    /// </summary>
    private AudioSource _audioSource;

    public void GameStart()
    {
        Application.targetFrameRate = 30;

        _score = 0;
        SetScore(_score);

        _lives = startingLives;
        SetLives(_lives);

        _nextLifeScoreThreshold = _ScoreForExtraLife;

        gameStart.Invoke();
    }

    /// <summary>
    /// Called after losing a life (if lives > 0).
    /// </summary>
    public void GameRestart()
    {
        gameRestart.Invoke();
    }

    /// <summary>
    /// Called when lives reach 0.
    /// </summary>
    public void GameOver()
    {
        gameOver.Invoke();
    }

    public void IncreaseScore(int increment)
    {
        _score += increment;
        SetScore(_score);

        if (_score >= _nextLifeScoreThreshold)
        {
            GainLife();
            _nextLifeScoreThreshold += _ScoreForExtraLife;
        }
    }

    public void SetScore(int newScore)
    {
        scoreChange.Invoke(newScore);
    }

    public void SetLives(int newLives)
    {
        livesChange.Invoke(newLives);
    }

    public void GainLife()
    {
        _lives++;
        SetLives(_lives);
        _audioSource.PlayOneShot(extraLifeSound);
    }

    public void LoseLife()
    {
        _lives--;
        SetLives(_lives);

        if (_lives <= 0)
        {
            GameOver();
        }
        else
        {
            GameRestart();
        }
    }

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameStart();
    }
}