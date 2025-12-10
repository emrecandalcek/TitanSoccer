using System.Collections.Generic;
using UnityEngine;

public enum PlayerRole
{
    SF, MOO, MDO, SĞK, SLK, SĞO, SLO, STP, SLB, SĞB, KL
}

public enum AiState
{
    Positioning,
    Mark,
    Intercept,
    Press,
    SupportRun,
    Retreat
}

[RequireComponent(typeof(PlayerController))]
public class AIStateController : MonoBehaviour
{
    public PlayerRole Role;
    public Transform MarkTarget;
    public Transform PressTarget;
    public Transform Ball;
    public float ZoneWeight = 0.35f;
    public float PressDistance = 9f;

    private PlayerController _player;
    private AiState _currentState;
    private Vector2 _homePosition;
    private readonly Dictionary<PlayerRole, Rect> _roleZones = new();

    public bool IsUserTeam;
    public bool HasPossession;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _homePosition = transform.position;
        BuildRoleZones();
    }

    private void Update()
    {
        if (Ball != null && !HasPossession && PressTarget == null)
        {
            PressTarget = Ball;
        }

        EvaluateState();
        ExecuteState();
    }

    private void EvaluateState()
    {
        Vector2 zoneCenter = GetZone(Role).center;
        float distanceFromZone = Vector2.Distance(transform.position, zoneCenter);
        float pressModifier = Mathf.Lerp(0.6f, 1.25f, _player.Attributes.EvaluateSkill(_player.Attributes.Awareness));
        float staminaFactor = Mathf.Lerp(0.75f, 1.1f, _player.Attributes.EvaluateSkill(_player.Attributes.Stamina));
        float dynamicPressDistance = PressDistance * pressModifier * staminaFactor;

        if (HasPossession)
        {
            if (distanceFromZone > PressDistance)
            {
                _currentState = AiState.SupportRun;
            }
            else
            {
                _currentState = AiState.Positioning;
            }
            return;
        }

        if (PressTarget != null && Vector2.Distance(transform.position, PressTarget.position) < dynamicPressDistance * 0.8f)
        {
            _currentState = AiState.Press;
            return;
        }

        if (MarkTarget != null && Vector2.Distance(transform.position, MarkTarget.position) < 6f)
        {
            _currentState = AiState.Mark;
            return;
        }

        _currentState = distanceFromZone > dynamicPressDistance ? AiState.Retreat : AiState.Positioning;
    }

    private void ExecuteState()
    {
        switch (_currentState)
        {
            case AiState.Positioning:
                MoveToZoneCenter();
                break;
            case AiState.Mark:
                if (MarkTarget != null)
                {
                    _player.SetMovementTarget(GetClampedTarget(MarkTarget.position));
                }
                break;
            case AiState.Intercept:
                _player.SetMovementTarget(_homePosition + (Vector2)transform.up * 2f);
                break;
            case AiState.Press:
                if (PressTarget != null)
                {
                    _player.SetMovementTarget(GetClampedTarget(PressTarget.position));
                }
                break;
            case AiState.SupportRun:
                MoveToZoneEdge();
                break;
            case AiState.Retreat:
                MoveToZoneCenter();
                break;
        }
    }

    private void MoveToZoneCenter()
    {
        Rect zone = GetZone(Role);
        Vector2 target = zone.center;
        _player.SetMovementTarget(target);
    }

    private void MoveToZoneEdge()
    {
        Rect zone = GetZone(Role);
        Vector2 offset = new(Mathf.Sign(zone.center.x) * zone.width * 0.25f, zone.height * 0.2f);
        Vector2 target = new(zone.center.x + offset.x, zone.center.y + offset.y);
        _player.SetMovementTarget(GetClampedTarget(target));
    }

    private Vector2 GetClampedTarget(Vector2 target)
    {
        Rect zone = GetZone(Role);
        Vector2 zoneCenter = zone.center;
        Vector2 blended = Vector2.Lerp(target, zoneCenter, ZoneWeight);

        float clampedX = Mathf.Clamp(blended.x, zone.xMin, zone.xMax);
        float clampedZ = Mathf.Clamp(blended.y, zone.yMin, zone.yMax);
        return new Vector2(clampedX, clampedZ);
    }

    private void BuildRoleZones()
    {
        // Simplified pitch: width 50 (-25 to 25), length 100 (-50 to 50)
        _roleZones[PlayerRole.SF] = new Rect(-10f, 20f, 20f, 20f);
        _roleZones[PlayerRole.MOO] = new Rect(-15f, 5f, 30f, 15f);
        _roleZones[PlayerRole.MDO] = new Rect(-15f, -10f, 30f, 15f);
        _roleZones[PlayerRole.SĞK] = new Rect(10f, 20f, 10f, 20f);
        _roleZones[PlayerRole.SLK] = new Rect(-20f, 20f, 10f, 20f);
        _roleZones[PlayerRole.SĞO] = new Rect(10f, 0f, 15f, 20f);
        _roleZones[PlayerRole.SLO] = new Rect(-25f, 0f, 15f, 20f);
        _roleZones[PlayerRole.STP] = new Rect(-10f, 30f, 20f, 20f);
        _roleZones[PlayerRole.SLB] = new Rect(-25f, -20f, 15f, 20f);
        _roleZones[PlayerRole.SĞB] = new Rect(10f, -20f, 15f, 20f);
        _roleZones[PlayerRole.KL] = new Rect(-5f, -40f, 10f, 15f);
    }

    private Rect GetZone(PlayerRole role)
    {
        if (_roleZones.TryGetValue(role, out var zone))
        {
            return zone;
        }

        return new Rect(-5f, -5f, 10f, 10f);
    }
}
