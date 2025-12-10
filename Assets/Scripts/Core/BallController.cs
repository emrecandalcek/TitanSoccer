using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public event Action<PlayerController> PossessionChanged;

    [Header("Flight")] public float GravityScale = 1f;
    [Header("Passing")] public float BasePassSpeed = 16f;
    public float LoftArcHeight = 2.5f;
    public float MaxLoftMultiplier = 1.8f;
    public List<PlayerController> Defenders = new();
    [Header("Shot")] public float BaseShotForce = 22f;
    public float CurveForce = 8f;
    public float SpinDuration = 2f;

    public PlayerController CurrentPossessor { get; private set; }

    private Rigidbody _rigidbody;
    private float _spinTimer;
    private Vector3 _spinAxis;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_spinTimer > 0f)
        {
            _spinTimer -= Time.fixedDeltaTime;
            _rigidbody.AddForce(_spinAxis * CurveForce, ForceMode.Acceleration);
        }

        _rigidbody.AddForce(Physics.gravity * (GravityScale - 1f), ForceMode.Acceleration);
    }

    public void SetPossession(PlayerController player)
    {
        if (CurrentPossessor == player) return;
        CurrentPossessor = player;
        PossessionChanged?.Invoke(player);
    }

    public void Release()
    {
        SetPossession(null);
    }

    public void PassGround(PlayerController passer, PlayerController target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 predicted = target.PredictTargetPosition(0.65f);
        Vector3 direction = (predicted - transform.position);
        direction.Normalize();

        float speed = BasePassSpeed * passer.Attributes.PassPower;
        Vector3 velocity = direction * speed;

        if (IsInterceptionLikely(Defenders, predicted))
        {
            velocity *= 0.9f;
            predicted += (predicted - transform.position).normalized * 0.5f;
        }

        FireBall(passer, velocity, predicted, false);
        passer.LockMovement(0.1f);
    }

    public void PassLofted(PlayerController passer, PlayerController target, float holdSeconds)
    {
        if (target == null)
        {
            return;
        }

        Vector3 predicted = target.PredictTargetPosition(0.85f);
        Vector3 toTarget = predicted - transform.position;
        float distance = toTarget.magnitude;
        toTarget.Normalize();

        float arcHeight = LoftArcHeight + Mathf.Clamp(distance * 0.05f, 0f, LoftArcHeight * (MaxLoftMultiplier - 1f));
        float launchSpeed = Mathf.Lerp(BasePassSpeed, BasePassSpeed * MaxLoftMultiplier, Mathf.Clamp01(holdSeconds / 2f));

        Vector3 velocity = toTarget * launchSpeed;
        velocity.y += Mathf.Sqrt(2f * Physics.gravity.magnitude * arcHeight);

        if (IsInterceptionLikely(Defenders, predicted))
        {
            velocity += Vector3.up * 1.5f;
        }

        FireBall(passer, velocity, predicted, true);
        passer.LockMovement(0.35f);
    }

    public void Shoot(PlayerController shooter, Vector3 gestureDir, float power, float curvature)
    {
        float force = BaseShotForce * shooter.Attributes.ShotPower * power;
        Vector3 velocity = gestureDir.normalized * force;
        velocity.y += Mathf.Lerp(0.25f, 1.25f, curvature) * force * 0.15f;

        FireBall(shooter, velocity, transform.position + gestureDir, true);
        _spinAxis = Vector3.Cross(Vector3.up, gestureDir.normalized) * curvature * shooter.Attributes.SpinControl;
        _spinTimer = SpinDuration;
        shooter.LockMovement(0.4f);
    }

    private void FireBall(PlayerController passer, Vector3 velocity, Vector3 target, bool aerial)
    {
        Release();
        _rigidbody.velocity = velocity;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public bool IsInterceptionLikely(IEnumerable<PlayerController> defenders, Vector3 target)
    {
        foreach (var defender in defenders)
        {
            float distToLine = DistancePointLine(defender.transform.position, transform.position, target);
            float anticipation = defender.Attributes.EvaluateSkill(defender.Attributes.Awareness, 0.5f, 1f);
            if (distToLine < 1.5f * anticipation)
            {
                return true;
            }
        }

        return false;
    }

    private static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 line = lineEnd - lineStart;
        Vector3 projected = Vector3.Project(point - lineStart, line.normalized);
        Vector3 closest = lineStart + projected;
        return Vector3.Distance(point, closest);
    }
}
