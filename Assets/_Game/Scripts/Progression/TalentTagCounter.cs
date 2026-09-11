using System.Collections.Generic;

/// <summary>
/// Suma tagów z wybranych kart — zawsze od nowa, nie persystowana.
/// </summary>
public static class TalentTagCounter
{
    public static Dictionary<TalentTag, int> Compute(IReadOnlyList<TalentDefinition> chosen)
    {
        var counts = new Dictionary<TalentTag, int>();
        if (chosen == null) return counts;

        foreach (var talent in chosen)
        {
            if (talent?.Tags == null) continue;
            foreach (var tag in talent.Tags)
            {
                if (tag == TalentTag.None) continue;
                counts.TryGetValue(tag, out var current);
                counts[tag] = current + 1;
            }
        }

        return counts;
    }
}
