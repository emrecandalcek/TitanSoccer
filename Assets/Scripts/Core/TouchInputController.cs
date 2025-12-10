using System.Collections.Generic;
using UnityEngine;

public class TouchInputController : MonoBehaviour
{
    public LayerMask PitchMask;
    public Camera WorldCamera;
    public PlayerController ControlledPlayer;
    public BallController Ball;
    public float HoldThreshold = 2f;
    public float GestureMinDistance = 0.6f;

    private float _pressStartTime;
    private bool _pressingPlayer;
    private readonly List<Vector3> _gesturePoints = new();

    private void Update()
    {
        if (WorldCamera == null) return;

        if (Input.touchSupported)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginPress(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            ContinuePress(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndPress(Input.mousePosition);
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginPress(touch.position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                ContinuePress(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndPress(touch.position);
                break;
        }
    }

    private void BeginPress(Vector2 screenPos)
    {
        _pressStartTime = Time.time;
        _pressingPlayer = HitPlayer(screenPos, out var player) && player == ControlledPlayer;
        _gesturePoints.Clear();
        _gesturePoints.Add(GetWorldPosition(screenPos));
    }

    private void ContinuePress(Vector2 screenPos)
    {
        Vector3 world = GetWorldPosition(screenPos);
        if (_pressingPlayer)
        {
            _gesturePoints.Add(world);
        }
    }

    private void EndPress(Vector2 screenPos)
    {
        float held = Time.time - _pressStartTime;
        Vector3 world = GetWorldPosition(screenPos);

        if (_pressingPlayer && held > 0.35f)
        {
            HandleShotGesture(held);
            return;
        }

        if (HitPlayer(screenPos, out var teammate) && teammate != ControlledPlayer)
        {
            if (held >= HoldThreshold)
            {
                Ball.PassLofted(ControlledPlayer, teammate, held);
            }
            else
            {
                Ball.PassGround(ControlledPlayer, teammate);
            }
            return;
        }

        ControlledPlayer.SetMovementTarget(world);
    }

    private void HandleShotGesture(float holdDuration)
    {
        if (_gesturePoints.Count < 2)
        {
            return;
        }

        Vector3 start = _gesturePoints[0];
        Vector3 end = _gesturePoints[_gesturePoints.Count - 1];
        Vector3 direction = end - start;
        float length = direction.magnitude;
        if (length < GestureMinDistance)
        {
            return;
        }

        float curvature = ComputeCurvature(_gesturePoints);
        float normalizedPower = Mathf.Clamp01(length / 10f);
        Ball.Shoot(ControlledPlayer, direction.normalized, normalizedPower, curvature);
    }

    private float ComputeCurvature(List<Vector3> points)
    {
        if (points.Count < 3) return 0f;
        float total = 0f;
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 a = (points[i] - points[i - 1]).normalized;
            Vector3 b = (points[i + 1] - points[i]).normalized;
            total += Vector3.SignedAngle(a, b, Vector3.up);
        }

        return Mathf.Clamp(total / 180f, -1f, 1f);
    }

    private bool HitPlayer(Vector2 screenPos, out PlayerController player)
    {
        Ray ray = WorldCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit))
        {
            player = hit.collider.GetComponentInParent<PlayerController>();
            return player != null;
        }

        player = null;
        return false;
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = WorldCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, 100f, PitchMask))
        {
            return hit.point;
        }

        return ray.GetPoint(15f);
    }
}
