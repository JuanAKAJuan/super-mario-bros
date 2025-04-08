using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 300;
    public float maxSpeed = 350;
    public float upSpeed = 30;
    public Animator marioAnimator;
    public AudioSource marioAudio;
    public AudioSource effectsAudioSource;
    public AudioClip marioDeath;
    public float deathImpulse = 15;
    public Transform gameCamera;

    [NonSerialized]
    public bool alive = true;

    private Rigidbody2D _marioBody;
    private SpriteRenderer _marioSprite;
    private bool _faceRightState = true;
    private bool _onGroundState = true;
    private GameManager _gameManager;

    public void GameRestart()
    {
        // Reset position and stop movement
        _marioBody.transform.position = new Vector3(-6.52f, -2.47f, 0f);
        _marioBody.linearVelocity = Vector2.zero;
        _marioBody.angularVelocity = 0f;

        // Reset sprite direction
        _faceRightState = true;
        _marioSprite.flipX = false;

        // Reset animation
        marioAnimator.SetTrigger("gameRestart");
        alive = true;
    }

    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f)
    {
        effectsAudioSource.PlayOneShot(clip, volume);
    }

    private void Awake()
    {
        _gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();
        _marioSprite = GetComponent<SpriteRenderer>();
        _marioBody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        marioAnimator.SetBool("onGround", _onGroundState);
    }

    private void Update()
    {
        if (!alive) return;

        if (Input.GetKeyDown("a") && _faceRightState)
        {
            _faceRightState = false;
            _marioSprite.flipX = true;

            if (_marioBody.linearVelocity.x > -0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        if (Input.GetKeyDown("d") && !_faceRightState)
        {
            _faceRightState = true;
            _marioSprite.flipX = false;

            if (_marioBody.linearVelocity.x < 0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        marioAnimator.SetFloat("xSpeed", Mathf.Abs(_marioBody.linearVelocity.x));
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            // --- Horizontal Movement ---
            float moveHorizontal = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(moveHorizontal) > 0)
            {
                Vector2 movement = new Vector2(moveHorizontal, 0);

                _marioBody.AddForce(movement * speed);
                _marioBody.linearVelocity = new Vector2(Mathf.Clamp(_marioBody.linearVelocity.x, -maxSpeed, maxSpeed), _marioBody.linearVelocity.y);
            }
            else
            {
                // If no input, gradually slow down (simple friction)
                _marioBody.linearVelocity = new Vector2(_marioBody.linearVelocity.x * 0.9f, _marioBody.linearVelocity.y);
            }

            // --- Jumping ---
            if (Input.GetKeyDown("space") && _onGroundState)
            {
                _marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
                _onGroundState = false;
                marioAnimator.SetBool("onGround", _onGroundState);
            }
        }
        else
        {
            _marioBody.linearVelocity = Vector2.zero;
        }
    }

    int collisonLayerMask = ((1 << 3) | (1 << 6) | (1 << 7));
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((collisonLayerMask & (1 << collision.transform.gameObject.layer)) > 0) & !_onGroundState)
        {
            _onGroundState = true;
            marioAnimator.SetBool("onGround", _onGroundState);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && alive)
        {
            // Check if player is stomping an enemy
            if (_marioBody.linearVelocity.y < -0.1f && transform.position.y > other.transform.position.y + (other.bounds.size.y * 0.3f))
            {
                Debug.Log("Stomped Goomba!");
                _marioBody.AddForce(Vector2.up * (upSpeed * 0.5f), ForceMode2D.Impulse);
                _gameManager.IncreaseScore(100);
            }
            else
            {
                Debug.Log("Collided with Goomba - Mario Dies");
                StartCoroutine(DieAndResetSequence());
            }
        }
    }

    private IEnumerator DieAndResetSequence()
    {
        if (!alive) yield break;

        alive = false;
        marioAnimator.Play("mario-die");
        marioAudio.PlayOneShot(marioDeath);

        yield return new WaitForSeconds(5.0f);

        _gameManager.LoseLife();
    }

    private void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
    }

    private void PlayDeathImpulse()
    {
        _marioBody.linearVelocity = Vector2.zero;
        _marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);
    }
}
