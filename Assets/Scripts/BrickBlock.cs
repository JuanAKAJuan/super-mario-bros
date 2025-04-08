using System.Collections;
using UnityEngine;

public class BrickBlock : MonoBehaviour
{
    public AudioSource brickHitAudio;
    public float initialBumpForce = 10.0f;
    public float bumpResetDelay = 0.3f;

    private Rigidbody2D _rigidBody;
    private Vector3 _startPosition;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the player hit the BOTTOM of the brick
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // `-contact.normal.y` is a vector that points downwards from the brick.
                // A threshold (< -0.9f) is used to account for slight angle variations.
                if (-contact.normal.y < -0.9)
                {
                    TriggerHit();
                    break;
                }
            }
        }
    }

    private void PlayBrickHitSound()
    {
        brickHitAudio.PlayOneShot(brickHitAudio.clip);
    }

    private void TriggerHit()
    {
        PlayBrickHitSound();
        StartCoroutine(BumpSequence());
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