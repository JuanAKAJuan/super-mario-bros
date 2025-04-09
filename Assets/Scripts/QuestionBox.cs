using System.Collections;
using UnityEngine;

/// <summary>
/// Represents an interactive Question Box that can be hit by the player from below.
/// Contains an item (like a coin) that is revealed upon being hit.
/// Can be reset to its initial state via the ResetBox method.
/// </summary>
public class QuestionBox : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("The AudioSource component holding the sound for when the box is hit.")]
    public AudioSource questionBoxHitAudio;

    [Header("Contents & Effects")]
    [Tooltip("Reference to the QuestionBoxCoin script/object associated with this box.")]
    public QuestionBoxCoin coin;

    [Tooltip("The initial upward force applied to the box when hit.")]
    public float initialBumpForce = 10.0f;

    [Tooltip("Delay in seconds before the box returns to its original position after being bumped.")]
    public float bumpResetDelay = 0.3f;

    /// <summary>
    /// The amount of score this coin gives when collected.
    /// </summary>
    private const int _ScoreValue = 200;

    /// <summary>
    /// Tracks if the box has already been hit.
    /// </summary>
    private bool _boxHasBeenHit = false;

    /// <summary>
    /// The initial world position of the box, stored for resetting after the bump animation.
    /// </summary>
    private Vector3 _startPosition;

    /// <summary>
    /// The name of the Animator state representing the box *before* it's hit (the flashing question mark).
    /// </summary>
    private const string _QuestionBoxAnimatorState = "question-box";

    // Cached component references
    private Animator _animator;
    private GameManager _gameManager;
    private Rigidbody2D _rigidBody;

    private static readonly int _HitTriggerHash = Animator.StringToHash("hit");

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Caches component references and sets up the Rigidbody.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();

        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _startPosition = transform.position;

    }

    /// <summary>
    /// Called when another Collider2D enters this trigger or collision.
    /// Checks if the player hit the bottom of the box and triggers the hit sequence if applicable.
    /// </summary>
    /// <param name="collision">Data about the collision event.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_boxHasBeenHit)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the player hit the BOTTOM of the question box
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // `-contact.normal.y` is a vector that points downwards from the question box.
                // A threshold (< -0.9f) is used to account for slight angle variations.
                if (-contact.normal.y < -0.9)
                {
                    TriggerHit();
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Plays the sound effect assigned to the questionBoxHitAudio source.
    /// Includes null checks for safety.
    /// </summary>
    private void PlayQuestionBoxHitSound()
    {
        questionBoxHitAudio.PlayOneShot(questionBoxHitAudio.clip);
    }

    /// <summary>
    /// Initiates the sequence when the box is successfully hit by the player.
    /// Marks the box as hit, triggers animations, plays sound, awards score, and starts the bump coroutine.
    /// </summary>
    private void TriggerHit()
    {
        _boxHasBeenHit = true;
        _animator.SetTrigger(_HitTriggerHash);
        coin.TriggerHit();
        PlayQuestionBoxHitSound();
        _gameManager.IncreaseScore(_ScoreValue);
        StartCoroutine(BumpSequence());
    }

    /// <summary>
    /// Coroutine that handles the physical "bump" movement of the box when hit.
    /// Briefly makes the box dynamic, applies an upward force, waits, then resets it to kinematic at its start position.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator BumpSequence()
    {
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _rigidBody.AddForce(Vector2.up * initialBumpForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(bumpResetDelay);

        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _rigidBody.linearVelocity = Vector2.zero;
        _rigidBody.angularVelocity = 0f;
        transform.position = _startPosition;
    }

    /// <summary>
    /// Resets the Question Box back to its initial, hittable state.
    /// </summary>
    public void ResetBox()
    {
        Debug.Log($"Resetting QuestionBox: {gameObject.name}");
        _boxHasBeenHit = false;

        _animator.Play(_QuestionBoxAnimatorState, -1, 0f); // Play from the beginning of the state

        coin.ResetCoin();
    }
}