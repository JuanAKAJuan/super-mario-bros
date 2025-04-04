using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 300;
    public float maxSpeed = 350;
    public float upSpeed = 20;
    public TextMeshProUGUI scoreText;
    public GameObject enemies;
    public JumpOverGoomba jumpOverGoomba;
    public Animator marioAnimator;
    public AudioSource marioAudio;
    public AudioClip marioDeath;
    public float deathImpulse = 15;
    
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
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _onGroundState = true;
            marioAnimator.SetBool("onGround", _onGroundState);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Collided with Goomba");
            // Time.timeScale = 0.0f;
            ResetGame();
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
    }

    private void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
    }
}
