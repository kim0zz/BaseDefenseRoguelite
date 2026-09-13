using UnityEngine;

/// <summary>
/// Konfiguracja startowa walki M3 — przypisuje broń i HP graczom po spawnie.
/// </summary>
[DisallowMultipleComponent]
public class CombatBootstrap : MonoBehaviour
{
    [SerializeField] private MeleeWeaponDefinition defaultWeapon;
    [SerializeField] private MeleeWeaponDefinition heavyWeapon;
    [SerializeField] private MeleeWeaponDefinition quickWeapon;
    [SerializeField] private float playerMaxHealth = 100f;
    [SerializeField] private bool usePlaytestClassOverride;
    [SerializeField] private PlayerClassId playtestClassOverride = PlayerClassId.Bomberman;
    [SerializeField] private GameObject bombermanVisualPrefab;

    public MeleeWeaponDefinition DefaultWeapon => defaultWeapon;
    public float PlayerMaxHealth => playerMaxHealth;

    public void SetupPlayer(GameObject playerObject, int playerSlotIndex)
    {
        var health = playerObject.GetComponent<Health>();
        if (health == null)
            health = playerObject.AddComponent<Health>();

        if (playerObject.GetComponent<HitFlashFeedback>() == null)
            playerObject.AddComponent<HitFlashFeedback>();

        if (playerObject.GetComponent<StatusEffectReceiver>() == null)
            playerObject.AddComponent<StatusEffectReceiver>();

        if (playerObject.GetComponent<StunFeedbackView>() == null)
            playerObject.AddComponent<StunFeedbackView>();

        if (playerObject.GetComponent<WeaponPlaceholderView>() == null)
            playerObject.AddComponent<WeaponPlaceholderView>();

        if (playerObject.GetComponent<PlayerAttackController>() == null)
            playerObject.AddComponent<PlayerAttackController>();

        if (playerObject.GetComponent<PlayerWeaponController>() == null)
            playerObject.AddComponent<PlayerWeaponController>();

        var build = playerObject.GetComponent<PlayerBuildState>();
        if (build == null)
            build = playerObject.AddComponent<PlayerBuildState>();

        var catalog = BuildSystemBootstrap.GetCatalogOrDefault();
        var classDef = catalog.GetDefaultClassForPlayerSlot(playerSlotIndex);
        if (usePlaytestClassOverride && playerSlotIndex == 0)
        {
            var overrideClass = catalog.GetClass(playtestClassOverride);
            if (overrideClass != null)
                classDef = overrideClass;
        }

        var teamLevel = SharedRunState.Instance != null ? SharedRunState.Instance.TeamLevel : 1;
        build.Initialize(classDef, catalog, playerSlotIndex, teamLevel);

        if (playerObject.GetComponent<PlayerRespawn>() == null)
            playerObject.AddComponent<PlayerRespawn>();

        if (playerObject.GetComponent<ForcedMovementReceiver>() == null)
            playerObject.AddComponent<ForcedMovementReceiver>();

        if (playerObject.GetComponent<KnockbackReceiver>() == null)
            playerObject.AddComponent<KnockbackReceiver>();

        if (playerObject.GetComponent<PlayerUniqueWeaponEffects>() == null)
            playerObject.AddComponent<PlayerUniqueWeaponEffects>();

        if (playerObject.GetComponent<PlayerPersistentEffects>() == null)
            playerObject.AddComponent<PlayerPersistentEffects>();

        if (playerObject.GetComponent<PersistentAuraFeedbackView>() == null)
            playerObject.AddComponent<PersistentAuraFeedbackView>();

        if (playerObject.GetComponent<PlayerSkillController>() == null)
            playerObject.AddComponent<PlayerSkillController>();

        ConfigureSkillsForClass(playerObject, classDef);

        playerObject.GetComponent<PlayerRespawn>()?.Initialize();

        FindAnyObjectByType<GameFlowManager>()?.RegisterJoinedPlayer(playerObject.GetComponent<PlayerCharacter>());

        CombatFeelService.Ensure();
        EnsureSkillTelemetryOverlay();

        ApplyJamieVisualSlice(playerObject);
        if (classDef != null && classDef.ClassId == PlayerClassId.Bomberman && bombermanVisualPrefab != null
            && playerObject.GetComponentInChildren<BombermanModelView>() == null)
        {
            var visual = Instantiate(bombermanVisualPrefab, playerObject.transform, false);
            visual.transform.localPosition = new Vector3(0f, -1f, 0f);
            visual.GetComponent<BombermanModelView>()?.Initialize(playerObject);
        }
    }

    private static void ConfigureSkillsForClass(GameObject playerObject, ClassDefinition classDef)
    {
        var skills = playerObject.GetComponent<PlayerSkillController>();
        if (skills == null || classDef == null) return;

        if (classDef.ClassId == PlayerClassId.Pudzian)
        {
            skills.Configure(new[]
            {
                SkillContentFactory.CreateTrzasniecie(),
                SkillContentFactory.CreateNoChodzTu(),
                SkillContentFactory.CreateByk()
            });
        }
        else if (classDef.ClassId == PlayerClassId.Bomberman)
        {
            skills.Configure(new[]
            {
                SkillContentFactory.CreateBombermanBomba(),
                SkillContentFactory.CreateBombermanWybuchowyOdskok(),
                SkillContentFactory.CreateBombermanKopniak()
            });
        }
    }

    private static void EnsureSkillTelemetryOverlay()
    {
        if (FindAnyObjectByType<SkillTelemetryDebugOverlay>() != null) return;
        var go = new GameObject("SkillTelemetryDebugOverlay");
        go.AddComponent<SkillTelemetryDebugOverlay>();
    }

    /// <summary>Legacy — używa slotu 0.</summary>
    public void SetupPlayer(GameObject playerObject)
    {
        SetupPlayer(playerObject, 0);
    }

    /// <summary>M4 visual slice — Jamie (P1) z wyraźniejszą sylwetką low-poly.</summary>
    private static void ApplyJamieVisualSlice(GameObject playerObject)
    {
        var character = playerObject.GetComponent<PlayerCharacter>();
        if (character == null || character.PlayerIndex != 0) return;

        playerObject.transform.localScale = new Vector3(0.95f, 1.1f, 0.95f);
    }
}
