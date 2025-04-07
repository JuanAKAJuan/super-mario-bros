using UnityEngine;

public class BrickBlock : MonoBehaviour
{
    public AudioSource brickHitAudio;

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
    }
}