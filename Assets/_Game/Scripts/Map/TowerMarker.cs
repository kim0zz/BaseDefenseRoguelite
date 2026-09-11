using UnityEngine;

/// <summary>
/// Marker wieży greybox — identyfikacja linii i roli (M2 placeholder, bez walki).
/// </summary>
[DisallowMultipleComponent]
public class TowerMarker : MonoBehaviour
{
    [SerializeField] private AttackLineId attackLine = AttackLineId.Center;
    [SerializeField] private TowerRole role = TowerRole.LineTower;

    public AttackLineId AttackLine => attackLine;
    public TowerRole Role => role;

    public void Configure(AttackLineId line, TowerRole towerRole)
    {
        attackLine = line;
        role = towerRole;
    }
}
