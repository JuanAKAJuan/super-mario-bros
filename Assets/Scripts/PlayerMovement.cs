using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls Mario's movement, jumping, animations, sound effects, and interactions.
/// Handles physics-based movement using Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Horizontal acceleration force.")]
    public float speed = 100;

    [Tooltip("Maximum horizontal speed.")]
    public float maxSpeed = 150;

    [Tooltip("Initial vertical force applied when jumping.")]
    public float upSpeed = 30;

    [Tooltip("How high Mario bounces after stomping an enemy.")]
    public float stompBounceForce = 15f;

    [Tooltip("How high Mario bounces vertically when he dies.")]
    public float deathImpulse = 15;

    [Tooltip("Layers considered as ground for jumping and landing checks.")]
    public LayerMask groundLayerMask = (1 << 3 | 1 << 6 | 1 << 7);

    [Header("Component References")]
    [Tooltip("Animator component for controlling Mario's animations.")]
    public Animator marioAnimator;

    [Tooltip("AudioSource for Mario's primary sounds (like jumping).")]
    public AudioSource marioAudio;

    [Tooltip("AudioSource for general sound effects triggered by Mario (like stomps, coin pickups).")]
    public AudioSource effectsAudioSource;

    [Header("Audio Clips")]
    [Tooltip("Sound effect played when Mario dies.")]
    public AudioClip marioDeath;

    [Tooltip("Sound effect played when Mario stomps an enemy.")]
    public AudioClip enemyStompSound;

    /// <summary>
    /// Tracks whether Mario is currently alive and controllable.
    /// Set to false during the death sequence.
    /// </summary>
    [NonSerialized]
    public bool alive = true;

    // Cached component references
    private Rigidbody2D _marioBody;
    private SpriteRenderer _marioSprite;
    private Collider2D _marioCollider;
    private GameManager _gameManager;

    // State variables
    private bool _faceRightState = true;
    private bool _onGroundState = true;

    private static readonly int _IsAliveHash = Animator.StringToHash("isAlive");
    private static readonly int _XSpeedHash = Animator.StringToHash("xSpeed");
    private static readonly int _OnGroundHash = Animator.StringToHash("onGround");
    private static readonly int _DiedHash = Animator.StringToHash("died");
    private static readonly int _OnSkidHash = Animator.StringToHash("onSkid");

    /// <summary>
    /// Resets Mario's state and position for a game restart (after losing a life).
    /// Called by the GameManager.
    /// </summary>
    public void GameRestart()
    {
        // Reset position and physics states
        _marioBody.transform.position = new Vector3(-6.52f, -2.47f, 0f);
        _marioBody.linearVelocity = Vector2.zero;
        _marioBody.angularVelocity = 0f;
        _marioBody.simulated = true;

        // Reset visual state
        _faceRightState = true;
        _marioSprite.flipX = false;
        _marioSprite.enabled = true;

        // Reset internal state
        alive = true;
        _onGroundState = true;

        // Reset collider
        if (_marioCollider == null)
            _marioCollider = GetComponent<Collider2D>();
        _marioCollider.enabled = true;

        // Reset animation
        marioAnimator.SetBool(_IsAliveHash, true);
        marioAnimator.SetBool(_OnGroundHash, _onGroundState);
        marioAnimator.SetFloat(_XSpeedHash, 0f);
        marioAnimator.Play("mario-idle", -1, 0f);
        marioAnimator.SetTrigger("gameRestart");
    }

    /// <summary>
    /// Plays a one-shot sound effect using the dedicated effects audio source.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="volume">The volume to play the clip at (0.0 to 1.0).</param>
    public void PlaySoundEffect(AudioClip clip, float volume = 1.0f)
    {
        effectsAudioSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Called once when the script instance is first loaded.
    /// Caches necessary component references. Finds the GameManager.
    /// </summary>
    private void Awake()
    {
        _marioBody = GetComponent<Rigidbody2D>();
        _marioSprite = GetComponent<SpriteRenderer>();
        _marioCollider = GetComponent<Collider2D>();

        _gameManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
    }

    /// <summary>
    /// Called once after Awake when the script is enabled.
    /// Sets the initial animation state based on the ground status.
    /// </summary>
    private void Start()
    {
        marioAnimator.SetBool(_IsAliveHash, true);
        marioAnimator.SetBool(_OnGroundHash, _onGroundState);
    }

    /// <summary>
    /// Called every frame.
    /// Handles input for jumping and updates animator parameters based on state.
    /// Also handles sprite flipping based on movement direction.
    /// </summary>
    private void Update()
    {
        if (!alive) return;

        HandleDirectionChange();

        marioAnimator.SetFloat(_XSpeedHash, Mathf.Abs(_marioBody.linearVelocity.x));
        marioAnimator.SetBool(_OnGroundHash, _onGroundState);

        if (Input.GetButtonDown("Jump") && _onGroundState)
        {
            Jump();
        }
    }

    /// <summary>
    /// Called every fixed framerate frame.
    /// Best place for physics calculations (movement forces, ground checks).
    /// </summary>
    private void FixedUpdate()
    {
        if (!alive) return;

        CheckGroundStatus();

        // --- Horizontal Movement ---
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        _marioBody.AddForce(Vector2.right * moveHorizontal * speed);

        if (Mathf.Abs(_marioBody.linearVelocity.x) > maxSpeed)
        {
            _marioBody.linearVelocity = new Vector2(Mathf.Sign(_marioBody.linearVelocity.x) * maxSpeed, _marioBody.linearVelocity.y);
        }

        // --- Friction/Deceleration ---
        if (Mathf.Abs(moveHorizontal) < 0.1f && _onGroundState)
        {
            float friction = 0.90f; // Closer to 1 = less friction
            _marioBody.linearVelocity = new Vector2(_marioBody.linearVelocity.x * friction, _marioBody.linearVelocity.y);
        }
    }

    /// <summary>
    /// Called when this collider/rigidbody has begun touching another rigidbody/collider.
    /// Handles landing on ground and interactions with enemies (stomp or die).
    /// </summary>
    /// <param name="collision">The Collision2D data associated with this collision.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!alive) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyMovement enemy = collision.gameObject.GetComponent<EnemyMovement>();
            Collider2D enemyCollider = collision.collider;

            if (enemy != null && enemyCollider != null && !enemy.IsStomped)
            {
                bool stompDetected = false;
                foreach (ContactPoint2D point in collision.contacts)
                {
                    bool hitTopSurface = point.normal.y > 0.5f;
                    bool movingDown = _marioBody.linearVelocity.y < 0.1f;

                    if (hitTopSurface && movingDown)
                    {
                        Debug.Log($"Stomp detected on: {enemy.name}");
                        enemy.Stomped();
                        StompBounce();
                        _gameManager.IncreaseScore(100);
                        PlaySoundEffect(enemyStompSound);
                        stompDetected = true;
                        break;
                    }
                }

                if (!stompDetected)
                {
                    if (_marioBody.linearVelocity.y > -0.5f)
                        Debug.Log($"Side/Bottom collision with: {enemy.name} - Mario Dies");

                    StartCoroutine(DieAndRestartSequence());
                }
                return;
            }
        }

        // --- Ground Landing Check ---
        bool landedOnGround = false;
        foreach (ContactPoint2D point in collision.contacts)
        {
            if (point.normal.y > 0.5f && ((groundLayerMask.value & (1 << collision.gameObject.layer)) > 0))
            {
                landedOnGround = true;
                break;
            }
        }

        if (landedOnGround && !_onGroundState)
        {
            Debug.Log("Landed on ground via OnCollisionEnter2D.");
            _onGroundState = true;
            marioAnimator.SetBool(_OnGroundHash, _onGroundState);
        }
    }

    /// <summary>
    /// Applies an upward force to Mario after stomping an enemy.
    /// Sets the state to not be on the ground.
    /// </summary>
    private void StompBounce()
    {
        _marioBody.linearVelocity = new Vector2(_marioBody.linearVelocity.x, 0);
        _marioBody.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
        _onGroundState = false;
        marioAnimator.SetBool(_OnGroundHash, false);
    }

    /// <summary>
    /// Coroutine that handles the sequence of events when Mario dies.
    /// Plays death animation, sound, disables controls, waits, then tells GameManager to handle the life loss/restart.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    private IEnumerator DieAndRestartSequence()
    {
        if (!alive) yield break;

        alive = false;
        marioAnimator.SetBool(_IsAliveHash, false);

        _gameManager.StopBackgroundMusic();
        marioAudio.PlayOneShot(marioDeath);

        marioAnimator.SetTrigger(_DiedHash);

        // --- Death "Jump" ---
        _marioCollider.enabled = false;
        _marioBody.linearVelocity = Vector2.zero;
        _marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);

        yield return new WaitForSeconds(3.0f);

        _gameManager.LoseLife();
    }

    /// <summary>
    /// Plays the jump sound effect using the primary Mario audio source.
    /// </summary>
    private void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
    }

    /// <summary>
    /// Checks the input axes and flips Mario's sprite accordingly.
    /// Triggers the skid animation if changing direction abruptly while on the ground.
    /// </summary>
    private void HandleDirectionChange()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        if (moveHorizontal < -0.1f && _faceRightState)
        {
            _faceRightState = false;
            _marioSprite.flipX = true;

            if (_onGroundState && _marioBody.linearVelocity.x > -0.1f)
                marioAnimator.SetTrigger(_OnSkidHash);
        }
        else if (moveHorizontal > 0.1f && !_faceRightState)
        {
            _faceRightState = true;
            _marioSprite.flipX = false;

            if (_onGroundState && _marioBody.linearVelocity.x < 0.1f)
                marioAnimator.SetTrigger(_OnSkidHash);
        }
    }

    /// <summary>
    /// Applies the jump force, plays jump sound, and updates ground state.
    /// </summary>
    private void Jump()
    {
        _marioBody.linearVelocity = new Vector2(_marioBody.linearVelocity.x, 0);
        _marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
        _onGroundState = false;
        marioAnimator.SetBool(_OnGroundHash, false);
        PlayJumpSound();
    }

    /// <summary>
    /// Performs a physics check downwards to see if Mario is touching the ground.
    /// Uses Physics2D.OverlapCircle for simplicity and reliability.
    /// Updates the _onGroundState flag.
    /// </summary>
    private void CheckGroundStatus()
    {
        float groundCheckRadius = 0.2f;

        // Point at feet
        Vector2 groundCheckPoint = (Vector2)transform.position + new Vector2(0, -_marioSprite.bounds.extents.y);

        bool wasOnGround = _onGroundState;
        _onGroundState = Physics2D.OverlapCircle(groundCheckPoint, groundCheckRadius, groundLayerMask);

        if (!wasOnGround && _onGroundState)
            marioAnimator.SetBool(_OnGroundHash, _onGroundState);
    }
}
