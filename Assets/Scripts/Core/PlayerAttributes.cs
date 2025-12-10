using System;
using UnityEngine;

[Serializable]
public class PlayerAttributes
{
    [Range(1f, 100f)] public float Overall = 50f;
    [Range(1f, 100f)] public float Passing = 50f;
    [Range(1f, 100f)] public float Shooting = 50f;
    [Range(1f, 100f)] public float Dribbling = 50f;
    [Range(1f, 100f)] public float Goalkeeping = 50f;
    [Range(1f, 100f)] public float Speed = 50f;
    [Range(1f, 100f)] public float Stamina = 50f;
    [Range(1f, 100f)] public float Awareness = 50f;
    [Range(1f, 100f)] public float Marking = 50f;
    [Range(1f, 100f)] public float Agility = 50f;

    private const float CurveTightness = 0.4f;

    public float EvaluateSkill(float stat, float min = 0f, float max = 1f)
    {
        var normalized = Mathf.Clamp01(stat / 100f);
        var curved = Mathf.Pow(normalized, CurveTightness);
        return Mathf.Lerp(min, max, curved);
    }

    public float PassAccuracy => EvaluateSkill(Passing, 0.6f, 1.05f);
    public float PassPower => EvaluateSkill(Passing, 0.5f, 1.2f);
    public float ShotPower => EvaluateSkill(Shooting, 0.5f, 1.3f);
    public float ShotScatter => Mathf.Lerp(4f, 0.25f, EvaluateSkill(Shooting));
    public float SpinControl => EvaluateSkill(Shooting, 0f, 1f);
    public float Acceleration => EvaluateSkill(Speed, 2f, 6f);
    public float TurnSpeed => EvaluateSkill(Agility, 180f, 540f);
    public float ReactionTime => Mathf.Lerp(0.35f, 0.08f, EvaluateSkill(Awareness));
}
