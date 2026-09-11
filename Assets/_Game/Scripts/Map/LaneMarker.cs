using UnityEngine;

/// <summary>
/// Marker linii natarcia — wizualna ścieżka od krawędzi mapy do bazy (M2 greybox).
/// </summary>
[DisallowMultipleComponent]
public class LaneMarker : MonoBehaviour
{
    [SerializeField] private AttackLineId attackLine = AttackLineId.Center;

    public AttackLineId AttackLine => attackLine;

    public void Configure(AttackLineId line)
    {
        attackLine = line;
    }
}
