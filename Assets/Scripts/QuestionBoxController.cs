using UnityEngine;

public class QuestionBoxController : MonoBehaviour
{
    public AudioSource questionBoxHitAudio;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the player hit the BOTTOM of the question box
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // `-contact.normal.y` is a vector that points downwards from the question box.
                // A threshold (< -0.9f) is used to account for slight angle variations.
                if (-contact.normal.y < -0.9)
                {
                    PlayQuestionBoxHitSound();
                    break;
                }
            }
        }
    }

    private void PlayQuestionBoxHitSound()
    {
        questionBoxHitAudio.PlayOneShot(questionBoxHitAudio.clip);
    }
}