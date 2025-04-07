using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Vector3 startPosition = new Vector3(0.0f, 0.0f, 0.0f);

    private float _originalX;
    private const float MaxOffset = 5.0f;
    private const float EnemyPatrolTime = 2.0f;
    private int _moveRight = -1;
    private Vector2 _velocity;
    private Rigidbody2D _enemyBody;

    public void GameRestart()
    {
        transform.localPosition = startPosition;
        _originalX = transform.position.x;
        _moveRight = -1;
        ComputeVelocity();
    }

    private void Start()
    {
        _enemyBody = GetComponent<Rigidbody2D>();
        _originalX = transform.position.x;
        ComputeVelocity();
    }

    private void Update()
    {
        if (Mathf.Abs(_enemyBody.position.x - _originalX) < MaxOffset)
        {
            MoveGoomba();
        }
        else
        {
            _moveRight *= -1;
            ComputeVelocity();
            MoveGoomba();
        }
    }

    private void ComputeVelocity()
    {
        _velocity = new Vector2((_moveRight) * MaxOffset / EnemyPatrolTime, 0);
    }

    private void MoveGoomba()
    {
        _enemyBody.MovePosition(_enemyBody.position + _velocity * Time.fixedDeltaTime);
    }
}
