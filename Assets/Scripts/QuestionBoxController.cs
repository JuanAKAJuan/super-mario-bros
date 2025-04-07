using UnityEngine;

public class QuestionBoxController : MonoBehaviour
{
    public AudioSource questionBoxHitAudio;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// Reference to the animator.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// Tracks if the box has already been hit.
    /// </summary>
    private bool _boxHasBeenHit = false;

    private static readonly int HitTriggerHash = Animator.StringToHash("hit");

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
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
        _animator.SetTrigger(HitTriggerHash);
        PlayQuestionBoxHitSound();
    }
}