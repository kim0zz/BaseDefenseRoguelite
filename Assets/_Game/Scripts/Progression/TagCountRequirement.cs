using System;
using UnityEngine;

/// <summary>
/// Próg liczby tagów (np. AOE &gt;= 2).
/// </summary>
[Serializable]
public struct TagCountRequirement
{
    [SerializeField] private TalentTag tag;
    [SerializeField] private int minCount;

    public TalentTag Tag => tag;
    public int MinCount => minCount;

    public static TagCountRequirement Create(TalentTag tag, int minCount)
    {
        return new TagCountRequirement { tag = tag, minCount = minCount };
    }
}
