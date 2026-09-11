using UnityEngine;

/// <summary>
/// Pierścień aury piekielnej / formy Kolosa na graczu (M9.1a).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerPersistentEffects))]
public class PersistentAuraFeedbackView : MonoBehaviour
{
    private const string AuraRingChildName = "AuraFeedbackRing";
    private const string KolosRingChildName = "KolosFeedbackRing";

    private PlayerPersistentEffects _persistents;
    private LineRenderer _auraRing;
    private LineRenderer _kolosRing;
    private GameObject _auraPrefabInstance;
    private GameObject _kolosPrefabInstance;

    private void Awake()
    {
        _persistents = GetComponent<PlayerPersistentEffects>();
    }

    private void Update()
    {
        RefreshKolos();
        RefreshAura();
    }

    private void RefreshKolos()
    {
        var active = _persistents != null && _persistents.KolosActive;
        if (!active)
        {
            HideKolos();
            return;
        }

        var skin = CombatFeedbackSkin.GetOrDefault();
        if (SkillFeedbackResolver.HasAuthored(skin.KolosPrefab))
        {
            HideKolosRing();
            if (_kolosPrefabInstance == null)
                _kolosPrefabInstance = Instantiate(skin.KolosPrefab, transform);
            _kolosPrefabInstance.SetActive(true);
            return;
        }

        DestroyKolosPrefab();
        var bodyRadius = Mathf.Max(transform.localScale.x, transform.localScale.z) * 0.45f;
        _kolosRing = FeedbackRingHelper.CreateRing(transform, KolosRingChildName, skin.KolosColor, bodyRadius);
        FeedbackRingHelper.SetVisible(_kolosRing, true);
    }

    private void RefreshAura()
    {
        var active = _persistents != null && _persistents.HasHellAuraRamping;
        if (!active)
        {
            HideAura();
            return;
        }

        var radius = _persistents.GetHellAuraRadius();
        var skin = CombatFeedbackSkin.GetOrDefault();
        if (SkillFeedbackResolver.HasAuthored(skin.AuraPrefab))
        {
            HideAuraRing();
            if (_auraPrefabInstance == null)
                _auraPrefabInstance = Instantiate(skin.AuraPrefab, transform);
            _auraPrefabInstance.SetActive(true);
            return;
        }

        DestroyAuraPrefab();
        _auraRing = FeedbackRingHelper.CreateRing(transform, AuraRingChildName, skin.AuraColor, radius);
        FeedbackRingHelper.SetVisible(_auraRing, true);
        FeedbackRingHelper.UpdateRing(_auraRing, radius);
    }

    private void HideKolos()
    {
        HideKolosRing();
        if (_kolosPrefabInstance != null)
            _kolosPrefabInstance.SetActive(false);
    }

    private void HideAura()
    {
        HideAuraRing();
        if (_auraPrefabInstance != null)
            _auraPrefabInstance.SetActive(false);
    }

    private void HideKolosRing() => FeedbackRingHelper.SetVisible(_kolosRing, false);
    private void HideAuraRing() => FeedbackRingHelper.SetVisible(_auraRing, false);

    private void DestroyKolosPrefab()
    {
        if (_kolosPrefabInstance == null) return;
        Destroy(_kolosPrefabInstance);
        _kolosPrefabInstance = null;
    }

    private void DestroyAuraPrefab()
    {
        if (_auraPrefabInstance == null) return;
        Destroy(_auraPrefabInstance);
        _auraPrefabInstance = null;
    }
}
