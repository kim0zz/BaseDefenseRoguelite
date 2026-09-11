using System.Collections.Generic;

/// <summary>
/// Index living-only buffers used by enemy separation.
/// EnemyRegistry keeps corpses during DeathRoutine; buffer indices must skip them.
/// </summary>
public static class EnemyLivingBuffer
{
    public static int IndexOf(IReadOnlyList<EnemyController> registry, EnemyController self)
    {
        if (registry == null || self == null)
            return -1;

        var livingIndex = 0;
        for (var i = 0; i < registry.Count; i++)
        {
            var enemy = registry[i];
            if (enemy == null) continue;

            var health = enemy.GetComponent<Health>();
            if (health != null && !health.IsAlive) continue;

            if (enemy == self)
                return livingIndex;

            livingIndex++;
        }

        return -1;
    }

    /// <summary>
    /// Maps a registry index onto a living-only list (dead entries skipped).
    /// Returns -1 if that registry slot is dead or out of range.
    /// </summary>
    public static int IndexOf(IReadOnlyList<bool> isAlive, int registryIndex)
    {
        if (isAlive == null || registryIndex < 0 || registryIndex >= isAlive.Count)
            return -1;
        if (!isAlive[registryIndex])
            return -1;

        var livingIndex = 0;
        for (var i = 0; i < registryIndex; i++)
        {
            if (isAlive[i])
                livingIndex++;
        }

        return livingIndex;
    }
}
