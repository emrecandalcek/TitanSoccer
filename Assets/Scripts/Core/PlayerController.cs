using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Identity")]
    public string DisplayName = "Player";
    public PlayerAttributes Attributes = new PlayerAttributes();

    [Header("Control")]
    public bool IsUserControlled;
    public bool IsDefender;
    public Transform AimOrigin;

    [Header("Movement")]
    public float StopDistance = 0.45f;
    public float MovementLockDuration = 0.25f;
    public AnimationCurve SlowdownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Rigidbody2D _rigidbody;
    private float _lockTimer;
    private Vector2 _pendingTarget;
    private readonly Queue<Vector2> _steeringPoints = new();

    public bool HasBall { get; private set; }

    public Vector2 Velocity { get; private set; }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _pendingTarget = transform.position;
        ConfigureBody();
    }

    private void ConfigureBody()
    {
        if (_rigidbody == null) return;
        _rigidbody.gravityScale = 0f;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (_lockTimer > 0f)
        {
            _lockTimer -= Time.deltaTime;
            return;
        }
    }

    private void FixedUpdate()
    {
        if (_rigidbody == null) return;

        if (_lockTimer > 0f)
        {
            _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, Vector2.zero, Time.fixedDeltaTime * 6f);
            Velocity = _rigidbody.velocity;
            return;
        }

        if (_steeringPoints.Count > 0 && Vector2.Distance(transform.position, _pendingTarget) <= StopDistance)
        {
            _pendingTarget = _steeringPoints.Dequeue();
        }

        Vector2 toTarget = _pendingTarget - (Vector2)transform.position;
        if (toTarget.magnitude <= StopDistance)
        {
            _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }
        else
        {
            Vector2 desired = toTarget.normalized * Attributes.EvaluateSkill(Attributes.Speed, 2.5f, 7.5f);
            _rigidbody.velocity = Vector2.MoveTowards(_rigidbody.velocity, desired, Attributes.Acceleration * Time.fixedDeltaTime);
        }

        Velocity = _rigidbody.velocity;
    }

    public void SetMovementTarget(Vector2 worldPosition, bool clearQueue = true)
    {
        if (_lockTimer > 0f) return;
        _pendingTarget = worldPosition;

        if (clearQueue)
        {
            _steeringPoints.Clear();
        }

        _steeringPoints.Enqueue(worldPosition);
    }

    public void LockMovement(float duration)
    {
        _lockTimer = Mathf.Max(_lockTimer, duration);
    }

    public void SetPossession(bool hasBall)
    {
        HasBall = hasBall;
        if (hasBall)
        {
            LockMovement(0.05f);
        }
    }

    public Vector2 PredictTargetPosition(float lookahead)
    {
        return (Vector2)transform.position + Velocity * lookahead;
    }

    public void BlendToAnimation(Animator animator)
    {
        if (animator == null) return;
        float speed = Velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("HasBall", HasBall);
    }
}
