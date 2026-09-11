/// <summary>
/// Hak meta-unlocków. MVP: bazowy kit odblokowany; puste id = zawsze.
/// </summary>
public interface IMetaUnlockQuery
{
    bool IsUnlocked(string metaUnlockId);
}

/// <summary>MVP — wszystko odblokowane.</summary>
public sealed class AlwaysUnlockedMetaQuery : IMetaUnlockQuery
{
    public static readonly AlwaysUnlockedMetaQuery Instance = new();

    public bool IsUnlocked(string metaUnlockId) => true;
}

/// <summary>Testy — konkretne id zablokowane.</summary>
public sealed class LockedMetaQuery : IMetaUnlockQuery
{
    private readonly string _lockedId;

    public LockedMetaQuery(string lockedId)
    {
        _lockedId = lockedId ?? "";
    }

    public bool IsUnlocked(string metaUnlockId)
    {
        if (string.IsNullOrEmpty(metaUnlockId)) return true;
        return metaUnlockId != _lockedId;
    }
}
