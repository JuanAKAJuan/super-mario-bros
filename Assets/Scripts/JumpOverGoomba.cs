using System;
using UnityEngine;

public class JumpOverGoomba : MonoBehaviour
{
    public Transform enemyLocation;
    public Vector3 boxSize;
    public float maxDistance;
    public LayerMask layerMask;

    private bool _onGroundState;
    private bool _countScoreState;
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameObject.FindGameObjectWithTag("Manager")?.GetComponent<GameManager>();
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
                _gameManager.IncreaseScore(1);
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
