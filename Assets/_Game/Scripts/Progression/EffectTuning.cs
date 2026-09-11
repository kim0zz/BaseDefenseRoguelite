using System;
using UnityEngine;

/// <summary>
/// Named float/int tuning per persistent/skill effect (M8.1b).
/// </summary>
[Serializable]
public class EffectTuning
{
    [SerializeField] private float float0;
    [SerializeField] private float float1;
    [SerializeField] private float float2;
    [SerializeField] private float float3;
    [SerializeField] private float float4;
    [SerializeField] private float float5;
    [SerializeField] private float float6;
    [SerializeField] private float float7;
    [SerializeField] private int int0;
    [SerializeField] private int int1;
    [SerializeField] private int int2;

    public float Float0 => float0;
    public float Float1 => float1;
    public float Float2 => float2;
    public float Float3 => float3;
    public float Float4 => float4;
    public float Float5 => float5;
    public float Float6 => float6;
    public float Float7 => float7;
    public int Int0 => int0;
    public int Int1 => int1;
    public int Int2 => int2;

    public static EffectTuning Create(
        float f0 = 0f, float f1 = 0f, float f2 = 0f, float f3 = 0f,
        float f4 = 0f, float f5 = 0f, float f6 = 0f, float f7 = 0f,
        int i0 = 0, int i1 = 0, int i2 = 0)
    {
        return new EffectTuning
        {
            float0 = f0, float1 = f1, float2 = f2, float3 = f3,
            float4 = f4, float5 = f5, float6 = f6, float7 = f7,
            int0 = i0, int1 = i1, int2 = i2
        };
    }
}
