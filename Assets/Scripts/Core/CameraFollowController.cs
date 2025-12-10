using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowController : MonoBehaviour
{
    public BallController Ball;
    public List<PlayerController> Players = new();
    public float FollowHeight = 12f;
    public float FollowDistance = 8f;
    public float SmoothTime = 0.25f;
    public float MinFov = 45f;
    public float MaxFov = 65f;
    public float DensityRadius = 12f;

    private Camera _camera;
    private Vector3 _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (Ball != null)
        {
            Ball.PossessionChanged += OnPossessionChanged;
        }
    }

    private void OnDisable()
    {
        if (Ball != null)
        {
            Ball.PossessionChanged -= OnPossessionChanged;
        }
    }

    private void LateUpdate()
    {
        if (Ball == null) return;

        Vector3 targetPos = Ball.transform.position - Ball.transform.forward * FollowDistance;
        targetPos += Vector3.up * FollowHeight;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, SmoothTime);

        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(Ball.transform.position - transform.position, Vector3.up),
            Time.deltaTime * 3f);

        UpdateZoom();
    }

    private void UpdateZoom()
    {
        if (_camera == null || Players.Count == 0) return;
        int nearby = Players.Count(p => Vector3.Distance(p.transform.position, Ball.transform.position) < DensityRadius);
        float t = Mathf.Clamp01(nearby / Mathf.Max(1f, Players.Count));
        float targetFov = Mathf.Lerp(MaxFov, MinFov, t);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFov, Time.deltaTime * 2f);
    }

    private void OnPossessionChanged(PlayerController possessor)
    {
        if (possessor == null) return;
        Vector3 snapPosition = possessor.transform.position - possessor.transform.forward * FollowDistance + Vector3.up * FollowHeight;
        transform.position = Vector3.Lerp(transform.position, snapPosition, 0.4f);
    }
}
