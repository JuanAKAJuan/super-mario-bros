using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the movement, physics, and behavior of an enemy (e.g., Goomba).
/// Uses Rigidbody2D velocity for physics-based movement including gravity.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Maximum distance the enemy will patrol left/right from its spawn point.")]
    public float patrolDistance = 5.0f;

    [Tooltip("Time it takes for the enemy to patrol the full patrolDistance one way.")]
    public float patrolTime = 2.0f;

    [Header("Stomp Behavior")]
    [Tooltip("How long the flattened sprite stays before disappearing.")]
    public float stompAnimationDuration = 0.5f;

    [Header("Collision Settings")]
    [Tooltip("Layers that the enemy should treat as obstacles to turn around from (e.g., Ground, Pipes, Blocks). Select these layers.")]
    public LayerMask obstacleLayerMask;

    /// <summary>
    /// Provides public read-only access to the stomped state of the enemy.
    /// </summary>
    public bool IsStomped => _isStomped;

    /// <summary>
    /// The actual position where this enemy instance was spawned. Used for resetting.
    /// </summary>
    private Vector3 _spawnPosition;

    /// <summary>
    /// The center X-coordinate around which the enemy patrols.
    /// </summary>
    private float _patrolCenterX;

    /// <summary>
    /// Current horizontal movement speed calculated based on patrolDistance and patrolTime.
    /// </summary>
    private float _horizontalSpeed;

    /// <summary>
    /// Direction the enemy is currently moving (-1 for left, 1 for right).
    /// </summary>
    private int _moveDirection = -1; // Start moving left

    // Component References
    private Rigidbody2D _enemyBody;
    private Collider2D _collider;
    private Animator _animator;

    /// <summary>
    /// Tracks whether or not the enemy has been stomped.
    /// </summary>
    private bool _isStomped = false;

    /// <summary>
    /// Reference to the stomped trigger.
    /// </summary>
    private static readonly int StompedTriggerHash = Animator.StringToHash("stomped");


    /// <summary>
    /// Resets the enemy to its initial spawned state and position.
    /// Called by EnemyManager during a game restart (after player death).
    /// </summary>
    public void GameRestart()
    {
        if (gameObject == null) return;

        StopAllCoroutines();

        _isStomped = false;
        _animator.ResetTrigger(StompedTriggerHash);
        _animator.Play("goomba-walk", -1, 0f);

        // Reset position and physics state
        transform.position = _spawnPosition;
        _enemyBody.linearVelocity = Vector2.zero;
        _enemyBody.simulated = true;
        _collider.enabled = true;
        _enemyBody.angularVelocity = 0f;

        // Reset patrol state
        _patrolCenterX = _spawnPosition.x;
        _moveDirection = -1;
        ComputeHorizontalSpeed();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        enabled = true;
    }

    /// <summary>
    /// Called by the Player when this enemy is stomped.
    /// </summary>
    public void Stomped()
    {
        if (_isStomped) return;

        _isStomped = true;
        _animator.SetTrigger(StompedTriggerHash);

        // Stop all movement immediately
        _enemyBody.linearVelocity = Vector2.zero;
        _enemyBody.angularVelocity = 0f;
        _enemyBody.simulated = false;
        _collider.enabled = false;

        enabled = false;

        StartCoroutine(DieAfterDelay());
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Used to cache components and store the initial spawn position.
    /// </summary>
    private void Awake()
    {
        _enemyBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();

        _spawnPosition = transform.position;
        _patrolCenterX = _spawnPosition.x;

        ComputeHorizontalSpeed();
    }

    /// <summary>
    /// Called every fixed framerate frame. Best for physics calculations and manipulations.
    /// Handles the patrol logic and applies velocity.
    /// </summary>
    private void FixedUpdate()
    {
        if (_isStomped || !enabled) return;

        HandlePatrol();
    }

    /// <summary>
    /// Called when this collider/rigidbody has begun touching another rigidbody/collider.
    /// Checks for collisions with specified obstacle layers and triggers a turn-around if it's a side collision.
    /// </summary>
    /// <param name="collision">The Collision2D data associated with this collision.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_isStomped || !enabled) return;

        // Prevent turning if hitting the player
        if (collision.gameObject.CompareTag("Player")) return;

        // Check if the layer of the object we collided with is in our obstacleLayerMask
        // Uses bitwise operation: (1 shifted left by the layer number) ANDed with the mask's value
        if (((1 << collision.gameObject.layer) & obstacleLayerMask.value) > 0)
        {
            bool hitSide = false;
            foreach (ContactPoint2D point in collision.contacts)
            {
                // Check if the collision normal is mostly horizontal.
                // A normal points *out* from the surface hit. Hitting a vertical wall results in a horizontal normal.
                // Mathf.Abs(point.normal.x) > 0.5 means the angle is significantly horizontal.
                if (Mathf.Abs(point.normal.x) > 0.5f)
                {
                    hitSide = true;
                    break; // Found a side contact point, no need to check others
                }
            }

            if (hitSide)
            {
                Debug.Log($"Goomba hit obstacle: {collision.gameObject.name}. Turning around.");
                _moveDirection *= -1;
            }
        }
    }

    /// <summary>
    /// Coroutine to wait for the stomp animation duration and then destroy the enemy GameObject.
    /// Consider deactivating instead of destroying if you want them to respawn on GameRestart.
    /// </summary>
    private IEnumerator DieAfterDelay()
    {
        yield return new WaitForSeconds(stompAnimationDuration);
        Destroy(gameObject);
    }

    /// <summary>
    /// Calculates the horizontal speed based on public patrol settings.
    /// </summary>
    private void ComputeHorizontalSpeed()
    {
        if (patrolTime <= 0)
        {
            _horizontalSpeed = 0;
            Debug.LogWarning($"Enemy {gameObject.name}: Patrol Time is zero or negative. Enemy will not move.", this);
        }
        else
        {
            _horizontalSpeed = patrolDistance / patrolTime;
        }
    }

    /// <summary>
    /// Manages the enemy's patrol behavior, checking boundaries and applying movement.
    /// </summary>
    private void HandlePatrol()
    {
        float currentOffsetX = _enemyBody.position.x - _patrolCenterX;

        if ((_moveDirection == 1 && currentOffsetX >= patrolDistance) || (_moveDirection == -1 && currentOffsetX <= -patrolDistance))
        {
            // Turn around
            _moveDirection *= -1;
        }

        ApplyMovementVelocity();
    }

    /// <summary>
    /// Sets the enemy's horizontal velocity based on the current direction, preserving vertical velocity.
    /// </summary>
    private void ApplyMovementVelocity()
    {
        float targetVelocityX = _moveDirection * _horizontalSpeed;
        float currentVelocityY = _enemyBody.linearVelocity.y;
        _enemyBody.linearVelocity = new Vector2(targetVelocityX, currentVelocityY);
    }
}
