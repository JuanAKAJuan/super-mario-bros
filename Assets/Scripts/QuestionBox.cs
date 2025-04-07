using System.Collections;
using UnityEngine;

public class QuestionBox : MonoBehaviour
{
    public AudioSource questionBoxHitAudio;
    public Coin coin;
    public float initialBumpForce = 2.0f;
    public float bumpResetDelay = 0.3f;

    private const int ScoreValue = 200;
    /// <summary>
    /// Reference to the animator.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// Tracks if the box has already been hit.
    /// </summary>
    private bool _boxHasBeenHit = false;
    private Rigidbody2D _rigidBody;
    private Vector3 _startPosition;
    private GameManager _gameManager;

    private static readonly int HitTriggerHash = Animator.StringToHash("hit");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();

        _startPosition = transform.position;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

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

    private void PlayQuestionBoxHitSound()
    {
        questionBoxHitAudio.PlayOneShot(questionBoxHitAudio.clip);
    }

    private void TriggerHit()
    {
        _boxHasBeenHit = true;
        StartCoroutine(BumpSequence());
        _animator.SetTrigger(HitTriggerHash);
        coin.TriggerHit();
        PlayQuestionBoxHitSound();
        _gameManager.IncreaseScore(ScoreValue);
    }

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
}