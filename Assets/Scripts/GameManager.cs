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

    /// <summary>
    /// Event to signal HUD update for the time.
    /// </summary>
    public UnityEvent<int> timeChange;

    /// <summary>
    /// The AudioSource component dedicated to playing background music.
    /// </summary>
    [Tooltip("Assign the AudioSource component used for background music here.")]
    public AudioSource backgroundMusicAudioSource;

    /// <summary>
    /// The background music track to play while the player is alive.
    /// </summary>
    public AudioClip backgroundMusicClip;

    /// <summary>
    /// Time limit for the player to beat the level..
    /// </summary>
    private const float _LevelTimeLimit = 400f;

    private float _timeRemaining;
    private bool _isTimerRunning = false;

    private int _score = 0;
    private int _lives = 0;
    private const int _ScoreForExtraLife = 2000;

    /// <summary>
    /// Tracks the next score target for a life.
    /// </summary>
    private int _nextLifeScoreThreshold = 0;

    /// <summary>
    /// The AudioSource component dedicated to playing sound effects.
    /// </summary>
    private AudioSource _sfxAudioSource;

    /// <summary>
    /// Sets up and starts the initial game state.
    /// </summary>
    public void GameStart()
    {
        Application.targetFrameRate = 30;

        _score = 0;
        SetScore(_score);

        _lives = startingLives;
        SetLives(_lives);

        _nextLifeScoreThreshold = _ScoreForExtraLife;

        _timeRemaining = _LevelTimeLimit;
        _isTimerRunning = true;
        SetTimeDisplay(_timeRemaining);

        gameStart.Invoke();

        PlayBackgroundMusic();
    }

    /// <summary>
    /// Called after losing a life (if lives > 0).
    /// </summary>
    public void GameRestart()
    {
        _score = 0;
        SetScore(_score);
        _nextLifeScoreThreshold = _ScoreForExtraLife;

        _timeRemaining = _LevelTimeLimit;
        _isTimerRunning = true;
        SetTimeDisplay(_timeRemaining);

        gameRestart.Invoke();

        PlayBackgroundMusic();
    }

    /// <summary>
    /// Called when lives reach 0.
    /// </summary>
    public void GameOver()
    {
        _isTimerRunning = false;
        gameOver.Invoke();
        StopBackgroundMusic();
    }

    public void IncreaseScore(int increment)
    {
        if (!_isTimerRunning) return;

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

    /// <summary>
    /// Plays the extra life sound effect.
    /// </summary>
    public void GainLife()
    {
        _lives++;
        SetLives(_lives);
        _sfxAudioSource.PlayOneShot(extraLifeSound);
    }

    /// <summary>
    /// Processes the player losing a life.
    /// </summary>
    public void LoseLife()
    {
        _isTimerRunning = false;
        _lives--;
        SetLives(_lives);

        if (_lives <= 0)
        {
            GameOver();
        }
        GameRestart();
    }

    /// <summary>
    /// Starts playing the background music if available and configured.
    /// Ensures music is stopped first to prevent overlapping playback issues if called rapidly.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (!backgroundMusicAudioSource.isPlaying)
            backgroundMusicAudioSource.Play();
    }

    /// <summary>
    /// Stops the background music if it's playing.
    /// This should be called immediately when the player starts dying.
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (backgroundMusicAudioSource.isPlaying)
            backgroundMusicAudioSource.Stop();
    }

    public void LevelComplete()
    {
        _isTimerRunning = false;
        Debug.Log($"LEVEL COMPLETE! Time Remaining: {_timeRemaining}");
    }

    private void TimeExpired()
    {
        if (!_isTimerRunning) return;

        _isTimerRunning = false;
        _timeRemaining = 0f;
        SetTimeDisplay(_timeRemaining);
        LoseLife();
    }

    /// <summary>
    /// Helper to update the time display via event.
    /// </summary>
    /// <param name="time">The time to display.</param>
    private void SetTimeDisplay(float time)
    {
        int displayTime = Mathf.CeilToInt(Mathf.Max(time, 0f));
        timeChange.Invoke(displayTime);
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        _sfxAudioSource = GetComponent<AudioSource>();
        backgroundMusicAudioSource.clip = backgroundMusicClip;
        backgroundMusicAudioSource.loop = true;
        backgroundMusicAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _timeRemaining -= Time.deltaTime;
            SetTimeDisplay(_timeRemaining);

            if (_timeRemaining <= 0f)
                TimeExpired();

        }
    }
}