using UnityEngine;

public class QuestionBoxCoin : MonoBehaviour
{
    /// <summary>
    /// Reference to the animator.
    /// </summary>
    private Animator _animator;

    private static readonly int HitTriggerHash = Animator.StringToHash("hit");

    public void TriggerHit()
    {
        _animator.SetTrigger(HitTriggerHash);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
}