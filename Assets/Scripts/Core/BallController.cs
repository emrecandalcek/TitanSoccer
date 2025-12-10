using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    public event Action<PlayerController> PossessionChanged;

    [Header("Passing")] public float BasePassSpeed = 16f;
    public float LoftBonus = 2.5f;
    public float MaxLoftMultiplier = 1.8f;
    public List<PlayerController> Defenders = new();
    [Header("Shot")] public float BaseShotForce = 22f;
    public float CurveForce = 8f;
    public float SpinDuration = 2f;

    public PlayerController CurrentPossessor { get; private set; }

    private Rigidbody2D _rigidbody;
    private float _spinTimer;
    private Vector2 _spinAxis;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (_spinTimer > 0f)
        {
            _spinTimer -= Time.fixedDeltaTime;
            _rigidbody.AddForce(_spinAxis * CurveForce);
        }
    }

    public void SetPossession(PlayerController player)
    {
        if (CurrentPossessor == player) return;

        CurrentPossessor?.SetPossession(false);
        CurrentPossessor = player;
        CurrentPossessor?.SetPossession(true);
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

        Vector2 predicted = target.PredictTargetPosition(0.65f);
        Vector2 direction = (predicted - (Vector2)transform.position);
        direction.Normalize();

        float speed = BasePassSpeed * passer.Attributes.PassPower;
        Vector2 velocity = direction * speed;

        if (IsInterceptionLikely(Defenders, predicted))
        {
            velocity *= 0.9f;
            predicted += (predicted - (Vector2)transform.position).normalized * 0.5f;
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

        Vector2 predicted = target.PredictTargetPosition(0.85f);
        Vector2 toTarget = predicted - (Vector2)transform.position;
        float distance = toTarget.magnitude;
        toTarget.Normalize();

        float lift = LoftBonus + Mathf.Clamp(distance * 0.05f, 0f, LoftBonus * (MaxLoftMultiplier - 1f));
        float launchSpeed = Mathf.Lerp(BasePassSpeed, BasePassSpeed * MaxLoftMultiplier, Mathf.Clamp01(holdSeconds / 2f));

        Vector2 velocity = toTarget * launchSpeed;
        velocity += Vector2.up * lift * 0.35f;

        if (IsInterceptionLikely(Defenders, predicted))
        {
            velocity += Vector2.up * 1.5f;
        }

        FireBall(passer, velocity, predicted, true);
        passer.LockMovement(0.35f);
    }

    public void Shoot(PlayerController shooter, Vector2 gestureDir, float power, float curvature)
    {
        float force = BaseShotForce * shooter.Attributes.ShotPower * power;
        Vector2 velocity = gestureDir.normalized * force;
        velocity += Vector2.up * Mathf.Lerp(0.25f, 1.25f, curvature) * force * 0.05f;

        FireBall(shooter, velocity, (Vector2)transform.position + gestureDir, true);
        Vector3 cross = Vector3.Cross(Vector3.forward, new Vector3(gestureDir.x, gestureDir.y, 0f));
        _spinAxis = new Vector2(-cross.y, cross.x) * curvature * shooter.Attributes.SpinControl;
        _spinTimer = SpinDuration;
        shooter.LockMovement(0.4f);
    }

    private void FireBall(PlayerController passer, Vector2 velocity, Vector2 target, bool aerial)
    {
        Release();
        _rigidbody.velocity = velocity;
        _rigidbody.angularVelocity = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out PlayerController controller))
        {
            SetPossession(controller);
        }
    }

    public bool IsInterceptionLikely(IEnumerable<PlayerController> defenders, Vector2 target)
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

    private static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector2 lineEnd)
    {
        Vector2 point2D = new Vector2(point.x, point.y);
        Vector2 lineStart2D = new Vector2(lineStart.x, lineStart.y);
        Vector2 line = lineEnd - lineStart2D;
        Vector2 projected = Vector2.Dot(point2D - lineStart2D, line.normalized) * line.normalized;
        Vector2 closest = lineStart2D + projected;
        return Vector2.Distance(point2D, closest);
    }
}
