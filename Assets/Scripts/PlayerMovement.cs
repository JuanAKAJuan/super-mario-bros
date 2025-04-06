using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 300;
    public float maxSpeed = 350;
    public float upSpeed = 30;
    public TextMeshProUGUI scoreText;
    public GameObject enemies;
    public JumpOverGoomba jumpOverGoomba;
    public Animator marioAnimator;
    public AudioSource marioAudio;
    public AudioClip marioDeath;
    public float deathImpulse = 15;
    public Transform gameCamera;

    [NonSerialized]
    public bool alive = true;

    private Rigidbody2D _marioBody;
    private SpriteRenderer _marioSprite;
    private bool _faceRightState = true;
    private bool _onGroundState = true;

    private void Start()
    {
        _marioSprite = GetComponent<SpriteRenderer>();

        Application.targetFrameRate = 30;

        _marioBody = GetComponent<Rigidbody2D>();

        marioAnimator.SetBool("onGround", _onGroundState);
    }

    private void Update()
    {
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
            float moveHorizontal = Input.GetAxisRaw("Horizontal");

            if (Mathf.Abs(moveHorizontal) > 0)
            {
                Vector2 movement = new Vector2(moveHorizontal, 0);

                if (_marioBody.linearVelocity.magnitude < maxSpeed)
                    _marioBody.AddForce(movement * speed);
            }

            if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
                _marioBody.linearVelocity = Vector2.zero;

            if (Input.GetKeyDown("space") && _onGroundState)
            {
                _marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
                _onGroundState = false;
                marioAnimator.SetBool("onGround", _onGroundState);
            }
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
            Debug.Log("Collided with Goomba");

            StartCoroutine(DieAndResetSequence());
        }
    }

    private void ResetGame()
    {
        // Reset position
        _marioBody.transform.position = new Vector3(-0.023f, -1.07f, 0f);

        // Reset sprite direction
        _faceRightState = true;
        _marioSprite.flipX = false;

        // Reset score
        scoreText.text = "Score: 0";

        // Reset Goomba
        foreach (Transform eachChild in enemies.transform)
        {
            eachChild.transform.localPosition = eachChild.GetComponent<EnemyMovement>().startPosition;
        }

        jumpOverGoomba.score = 0;

        // Reset animation
        marioAnimator.SetTrigger("gameRestart");
        alive = true;

        // Reset camera position
        gameCamera.position = new Vector3(0, 0, -10);
    }

    private IEnumerator DieAndResetSequence()
    {
        marioAnimator.Play("mario-die");
        marioAudio.PlayOneShot(marioDeath);
        alive = false;

        yield return new WaitForSeconds(5.0f);

        ResetGame();
    }

    private void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
    }

    private void PlayDeathImpulse()
    {
        _marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);
    }
}
