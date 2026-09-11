using UnityEngine;

/// <summary>
/// Spawnuje testowe moby na końcach linii (M3 — zastąpione przez WaveManager w M4).
/// </summary>
[DisallowMultipleComponent]
public class CombatTestSpawner : MonoBehaviour
{
    [SerializeField] private EnemyDefinition gruntDefinition;
    [SerializeField] private int gruntsPerLane = 1;
    [SerializeField] private MapGreyboxBuilder mapBuilder;

    private static readonly AttackLineId[] Lanes =
    {
        AttackLineId.Center,
        AttackLineId.Left,
        AttackLineId.Right
    };

    private void Start()
    {
        if (gruntDefinition == null)
        {
            Debug.LogWarning("[CombatTestSpawner] Brak EnemyDefinition — moby nie zostaną utworzone.");
            return;
        }

        if (mapBuilder == null)
            mapBuilder = FindAnyObjectByType<MapGreyboxBuilder>();

        for (var laneIndex = 0; laneIndex < Lanes.Length; laneIndex++)
        {
            for (var i = 0; i < gruntsPerLane; i++)
            {
                var lane = Lanes[laneIndex];
                var position = mapBuilder != null
                    ? mapBuilder.GetLaneSpawnPosition(lane, i)
                    : MapGreyboxLayout.GetFarEnd(lane) + Vector3.up;
                EnemySpawner.Spawn(gruntDefinition, position, laneIndex * gruntsPerLane + i, lane);
            }
        }
    }
}
