using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JumpOverGoomba : MonoBehaviour
{
    public Transform enemyLocation;
    public TextMeshProUGUI scoreText;
    public Vector3 boxSize;
    public float maxDistance;
    public LayerMask layerMask;
    
    // Don't want the score to show up in the inspector.
    [NonSerialized] public int score;
    
    private bool _onGroundState;
    private bool _countScoreState;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
    
    private void FixedUpdate()
    {
        if (Input.GetKeyDown("space") && OnGroundCheck())
        {
            _onGroundState = false;
            _countScoreState = true;
        }

        // When jumping, a Goomba is near Mario and the score hasn't been registered.
        if (!_onGroundState && _countScoreState)
        {
            if (Math.Abs(transform.position.x - enemyLocation.position.x) < 0.5f)
            {
                _countScoreState = false;
                score++;
                scoreText.text = "Score: " + score.ToString();
                Debug.Log(score);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            _onGroundState = true;
    }

    private bool OnGroundCheck()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, maxDistance, layerMask))
        {
            Debug.Log("On the ground");
            return true;
        }
        else
        {
            Debug.Log("Not on the ground");
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position - transform.up * maxDistance, boxSize);
    }
}
