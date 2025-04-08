using UnityEngine;

public class FloatingCoin : MonoBehaviour
{
    /// <summary>
    /// The sound effect to play when the coin is collected.
    /// </summary>
    public AudioClip pickupSound;

    /// <summary>
    /// The amount of score this coin gives when collected.
    /// </summary>
    private const int _ScoreValue = 100;

    /// <summary>
    /// Reference to the GameManager to update the score.
    /// </summary>
    private GameManager _gameManager;

    /// <summary>
    /// Tracks whether or not a coin has been collected to prevent double collection.
    /// </summary>
    private bool _collected = false;

    private void Awake()
    {
        _gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();

        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_collected && other.gameObject.CompareTag("Player"))
        {
            TriggerCollected(other);
        }
    }

    private void TriggerCollected(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        _collected = true;
        _gameManager.IncreaseScore(_ScoreValue);
        player.PlaySoundEffect(pickupSound);
        Destroy(gameObject);
    }
}