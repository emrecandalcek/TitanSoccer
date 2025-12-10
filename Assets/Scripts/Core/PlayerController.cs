using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
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

    private NavMeshAgent _agent;
    private float _lockTimer;
    private Vector3 _pendingTarget;
    private readonly Queue<Vector3> _steeringPoints = new();

    public bool HasBall { get; private set; }

    public Vector3 Velocity => _agent != null ? _agent.velocity : Vector3.zero;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
    }

    private void ConfigureAgent()
    {
        _agent.acceleration = Attributes.Acceleration;
        _agent.angularSpeed = Attributes.TurnSpeed;
        _agent.stoppingDistance = StopDistance;
        _agent.updateRotation = true;
    }

    private void Update()
    {
        if (_agent == null) return;

        if (_lockTimer > 0f)
        {
            _lockTimer -= Time.deltaTime;
            return;
        }

        if (_steeringPoints.Count > 0 && _agent.remainingDistance <= StopDistance)
        {
            _agent.SetDestination(_steeringPoints.Dequeue());
        }

        if (_agent.remainingDistance <= StopDistance && _agent.velocity.magnitude > 0.01f)
        {
            _agent.velocity = Vector3.Lerp(_agent.velocity, Vector3.zero, Time.deltaTime * 4f);
        }
    }

    public void SetMovementTarget(Vector3 worldPosition, bool clearQueue = true)
    {
        if (_lockTimer > 0f) return;
        _pendingTarget = worldPosition;

        if (clearQueue)
        {
            _steeringPoints.Clear();
        }

        _steeringPoints.Enqueue(worldPosition);
        _agent.SetDestination(worldPosition);
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

    public Vector3 PredictTargetPosition(float lookahead)
    {
        return transform.position + Velocity * lookahead;
    }

    public void BlendToAnimation(Animator animator)
    {
        if (animator == null) return;
        float speed = Velocity.magnitude;
        animator.SetFloat("Speed", speed);
        animator.SetBool("HasBall", HasBall);
    }
}
