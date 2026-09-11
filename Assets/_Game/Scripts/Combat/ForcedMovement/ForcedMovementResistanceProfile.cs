using UnityEngine;

/// <summary>
/// Profil odporności na wymuszony ruch — na definicji wroga, nie w skillu (M7.5-T1).
/// </summary>
[DisallowMultipleComponent]
public class ForcedMovementResistanceProfile : MonoBehaviour
{
    [SerializeField] private ForcedMovementResistanceCategory category = ForcedMovementResistanceCategory.Normal;

    public ForcedMovementResistanceCategory Category => category;

    public void Configure(ForcedMovementResistanceCategory resistanceCategory)
    {
        category = resistanceCategory;
    }
}
