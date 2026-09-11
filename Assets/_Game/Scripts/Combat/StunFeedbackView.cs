using UnityEngine;

/// <summary>
/// Żółty pierścień lub slot stunPrefab gdy StatusEffectReceiver.IsStunned (M9.1a).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(StatusEffectReceiver))]
public class StunFeedbackView : MonoBehaviour
{
    private const string RingChildName = "StunFeedbackRing";
    private const float RingRadius = 0.75f;

    private StatusEffectReceiver _status;
    private LineRenderer _ring;
    private GameObject _prefabInstance;
    private bool _wasStunned;

    private void Awake()
    {
        _status = GetComponent<StatusEffectReceiver>();
        _status.StatusChanged += OnStatusChanged;
        Refresh();
    }

    private void OnDestroy()
    {
        if (_status != null)
            _status.StatusChanged -= OnStatusChanged;
    }

    private void OnStatusChanged() => Refresh();

    private void Refresh()
    {
        var stunned = _status != null && StunFeedbackRules.ShouldShow(_status.IsStunned);
        if (stunned == _wasStunned)
            return;

        if (stunned)
            SkillFeedbackAudio.Ensure().PlayStun();

        _wasStunned = stunned;
        if (!stunned)
        {
            Hide();
            return;
        }

        Show();
    }

    private void Show()
    {
        var skin = CombatFeedbackSkin.GetOrDefault();
        if (SkillFeedbackResolver.HasAuthored(skin.StunPrefab))
        {
            HideRing();
            if (_prefabInstance == null)
                _prefabInstance = Instantiate(skin.StunPrefab, transform);
            _prefabInstance.SetActive(true);
            return;
        }

        DestroyPrefabInstance();
        var scale = Mathf.Max(transform.localScale.x, transform.localScale.z);
        _ring = FeedbackRingHelper.CreateRing(transform, RingChildName, skin.StunColor, RingRadius * scale);
        FeedbackRingHelper.SetVisible(_ring, true);
    }

    private void Hide()
    {
        HideRing();
        if (_prefabInstance != null)
            _prefabInstance.SetActive(false);
    }

    private void HideRing()
    {
        FeedbackRingHelper.SetVisible(_ring, false);
    }

    private void DestroyPrefabInstance()
    {
        if (_prefabInstance == null) return;
        Destroy(_prefabInstance);
        _prefabInstance = null;
    }
}
