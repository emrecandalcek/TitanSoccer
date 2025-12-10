using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowController : MonoBehaviour
{
    public BallController Ball;
    public List<PlayerController> Players = new();
    public float FollowDistance = 10f;
    public float SmoothTime = 0.25f;
    public float MinOrthoSize = 9f;
    public float MaxOrthoSize = 13f;
    public float DensityRadius = 8f;
    public float RecenterLerp = 0.65f;
    public float RecenterHeightBoost = 1.25f;

    private Camera _camera;
    private Vector3 _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
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

        Vector3 targetPos = new Vector3(Ball.transform.position.x, Ball.transform.position.y, -FollowDistance);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, SmoothTime);

        transform.rotation = Quaternion.identity;

        UpdateZoom();
    }

    private void UpdateZoom()
    {
        if (_camera == null || Players.Count == 0) return;
        int nearby = Players.Count(p => Vector2.Distance(p.transform.position, Ball.transform.position) < DensityRadius);
        float t = Mathf.Clamp01(nearby / Mathf.Max(1f, Players.Count));
        float targetSize = Mathf.Lerp(MaxOrthoSize, MinOrthoSize, t);
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetSize, Time.deltaTime * 2f);
    }

    private void OnPossessionChanged(PlayerController possessor)
    {
        if (possessor == null) return;
        Vector3 snapPosition = new Vector3(possessor.transform.position.x, possessor.transform.position.y + RecenterHeightBoost, -FollowDistance);
        transform.position = Vector3.Lerp(transform.position, snapPosition, RecenterLerp);
    }
}
