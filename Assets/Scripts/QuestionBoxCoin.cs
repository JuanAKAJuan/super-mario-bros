using UnityEngine;

/// <summary>
/// Controls the animation of the coin that appears from a Question Box when hit.
/// Can be reset to its initial (hidden or idle) state.
/// </summary>
[RequireComponent(typeof(Animator))]
public class QuestionBoxCoin : MonoBehaviour
{
    /// <summary>
    /// Reference to the animator.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// Cached hash for the "hit" trigger parameter in the Animator.
    /// </summary>
    private static readonly int _HitTriggerHash = Animator.StringToHash("hit");

    /// <summary>
    /// The name of the Animator state representing the coin *before* it's revealed or after it resets.
    /// </summary>
    private const string _QuestionBoxCoinDormantAnimatorState = "question-box-coin-dormant";

    /// <summary>
    /// Triggers the coin's "hit" animation (e.g., popping up and spinning).
    /// Called by the parent QuestionBox when it is hit.
    /// </summary>
    public void TriggerHit()
    {
        _animator.SetTrigger(_HitTriggerHash);
    }

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Caches the Animator component reference.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Resets the coin's Animator back to its initial state.
    /// </summary>
    public void ResetCoin()
    {
        Debug.Log($"Resetting Coin for: {transform.parent.name ?? gameObject.name}");
        if (_animator != null)
        {
            _animator.Play(_QuestionBoxCoinDormantAnimatorState, -1, 0f);
        }
    }
}