using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fabryka domyślnego contentu M6/M8.1b — używana gdy brak assetu katalogu.
/// </summary>
public static class BuildContentFactory
{
    public static BuildContentCatalog CreateDefaultCatalog()
    {
        var catalog = ScriptableObject.CreateInstance<BuildContentCatalog>();

        var sword = CreateMelee(10f, 0.8f, 2f, 120f, 3);
        var dagger = CreateMelee(5f, 0.35f, 1.6f, 90f, 1);
        var axe = CreateMelee(22f, 1.6f, 2.2f, 140f, 6);
        var bulawa = CreateBulawa();
        var hammer = CreateMelee(18f, 1.4f, 2f, 130f, 3);
        var pike = CreateMelee(13f, 1.1f, 2.8f, 80f, 3);
        var bow = CreateMelee(9f, 0.9f, 3f, 60f, 1);

        var weapons = new[]
        {
            CreateWeapon("miecz", "Miecz", LootRarity.Common, sword, Mod()),
            CreateWeapon("sztylet", "Sztylet", LootRarity.Common, dagger, Mod()),
            CreateWeapon("topor", "Wielki topór", LootRarity.Common, axe, Mod()),
            CreateWeapon("bulawa", "Buława", LootRarity.Common, bulawa, Mod()),
            CreateWeapon("mlot", "Młot wojenny", LootRarity.Common, hammer, Mod()),
            CreateWeapon("pika", "Pika", LootRarity.Common, pike, Mod()),
            CreateWeapon("luk", "Łuk", LootRarity.Common, bow, Mod()),
            CreateWeapon("knights_edge", "Knight's Edge", LootRarity.Rare, sword, Mod(damageMultiplier: 1.15f)),
            CreateWeapon("viper_fang", "Viper Fang", LootRarity.Rare, dagger,
                Mod(attackSpeedMultiplier: 1.05f)),
            CreateUniqueWeapon("ramhammer", "Ramhammer", hammer, Mod(damageMultiplier: 1.2f)),
            CreateUniqueWeapon("horn_of_momentum", "Horn of Momentum", dagger, Mod(moveSpeedMultiplier: 1.05f)),
            CreateUniqueWeapon("charging_pike", "Charging Pike", pike, Mod(damageMultiplier: 1.1f))
        };

        var items = new[]
        {
            CreateItem("blood_charm", "Blood Charm", LootRarity.Common, Mod(maxHealthBonus: 10f)),
            CreateItem("spiked_boots", "Spiked Boots", LootRarity.Common, Mod(moveSpeedMultiplier: 1.08f)),
            CreateItem("banner_fragment", "Banner Fragment", LootRarity.Common, Mod(damageMultiplier: 1.1f)),
            CreateItem("rusty_horn", "Rusty Horn", LootRarity.Common, Mod(moveSpeedMultiplier: 1.05f)),
            CreateItem("hunters_eye", "Hunter's Eye", LootRarity.Common, Mod(damageMultiplier: 1.08f)),
            CreateItem("iron_rosary", "Iron Rosary", LootRarity.Rare, Mod(maxHealthMultiplier: 1.15f))
        };

        var classes = new[]
        {
            CreateClass(PlayerClassId.Jamie, "Jamie", weapons[0], weapons[6], 100f, 5f),
            CreateClass(PlayerClassId.Cwel, "Cwel", weapons[1], weapons[6], 85f, 5.8f),
            CreateClass(PlayerClassId.Pudzian, "Pudzian", weapons[3], weapons[4], 135f, 4.2f),
            CreateClass(PlayerClassId.Cipak, "Cipak", weapons[6], weapons[1], 90f, 5.2f)
        };

        var talents = BuildPudzianTalents();
        var tables = new[] { CreatePudzianProgressionTable() };

        catalog.InitializeRuntime(classes, weapons, items, talents, tables);
        return catalog;
    }

    private static MeleeWeaponDefinition CreateBulawa()
    {
        var profile = ScriptableObject.CreateInstance<MeleeWeaponDefinition>();
        SetField(profile, "damage", 10f);
        SetField(profile, "attackInterval", 2.16f);
        SetField(profile, "range", 2.2f);
        SetField(profile, "arcDegrees", 140f);
        SetField(profile, "maxTargets", 6);
        SetField(profile, "comboProfile", AttackComboDefinition.CreateBulawaBaseline());
        return profile;
    }

    private static TalentDefinition[] BuildPudzianTalents()
    {
        var skok = SkillContentFactory.CreateSkok();
        var zryj = SkillContentFactory.CreateZryjMnie();
        var spychacz = SkillContentFactory.CreateSpychacz();
        var boss = SkillContentFactory.CreateJaJestemBoss();
        var trzesienie = SkillContentFactory.CreateTrzesienieSwiata();
        var aura = SkillContentFactory.CreatePiekielnaAuraPulse();
        var lastStandSkill = SkillContentFactory.CreateNieZabijecieMnie();

        var furyTuning = EffectTuning.Create(45f, 0.25f, 0.20f, 0.15f, 0.4f, 1.5f, 0.10f, 6f);
        var kolosTuning = EffectTuning.Create(1.7f, 0f, 1.35f, 0.35f, 0.6f, 0.15f);
        var hartTuning = EffectTuning.Create(2f, 1f, 0.5f, 0f, 0f, 0f, i0: 5);
        var reflectTuning = EffectTuning.Create(0.5f);
        var lastChanceTuning = EffectTuning.Create(6f, 0.30f);
        var auraTuning = EffectTuning.Create(3.75f, 0.5f, 3f, 1.5f, 0f, 0f, i0: 0, i1: 5);
        var zryjTuning = EffectTuning.Create(0f, 0f, 0f, 0f, 0f, 0f, i0: 3);
        var pozeraczTuning = EffectTuning.Create(8f, 0.4f, 4f);
        var wkurwKolosTuning = EffectTuning.Create(0.20f, 0.30f, 0.15f);
        var kazdyKrokTuning = EffectTuning.Create(6f, 1.6f, 0.3f, 1.2f, 0.45f);
        var wstrzasyTuning = EffectTuning.Create(0.7f, 0.5f, 0.7f, 2f);
        var rozpadlinaTuning = EffectTuning.Create(4f, 3f, 0.5f);
        var krwawaTuning = EffectTuning.Create(4f, 32f);
        var staggerBoostTuning = EffectTuning.Create(1.6f, 1.5f);
        var vampireTuning = EffectTuning.Create(0.20f, 12f);
        var przegrzanieTuning = EffectTuning.Create(0.35f, 1.4f, 2f);
        var sladTuning = EffectTuning.Create(2f, 3f, 0.5f, 0.8f, 6f);
        var pozarTuning = EffectTuning.Create(3f, 2.5f, 4f, 2f, 0.5f);
        var nieChceTuning = EffectTuning.Create(1.75f, 0.20f);
        var agoniaTuning = EffectTuning.Create(3.5f, 10f, 1f);
        var drugaSzansaTuning = EffectTuning.Create(0.25f, 0.15f, 4f);
        var prawdziwyBossTuning = EffectTuning.Create(6f, 2f);
        var wScianeTuning = EffectTuning.Create(14f, 2f, 1f, 16f);

        return new[]
        {
            Card("pudzian_skok", "Skok z Pierdolnięciem",
                "Stomp staje się skokiem w cel. Lądujesz z tym samym uderzeniem; w locie jesteś nietrafialny.",
                2, Tags(TalentTag.Aoe, TalentTag.Mobility),
                TalentRequirement.Create(),
                TalentEffect.Mutate("pudzian_trzasniecie", 0, skok, OfferGenerator.GroupMutation)),

            Card("pudzian_zryj_mnie", "ŻRYJ MNIE",
                "Prowokacja: pierwsze 3 hity od sprowokowanych leczą zamiast ranić. Nadmiar HP przepada.",
                2, Tags(TalentTag.Taunt, TalentTag.Heal),
                TalentRequirement.Create(),
                TalentEffect.Mutate("pudzian_no_chodz_tu", 1, zryj, OfferGenerator.GroupMutation,
                    persistent: PersistentEffectKind.ConvertTauntHitsToHeal, persistentTuning: zryjTuning)),

            Card("pudzian_spychacz", "Spychacz",
                "Byk zbiera zwykłych wrogów przed sobą i zrzuca ich na końcu, zamiast rozpychać na boki.",
                2, Tags(TalentTag.Control, TalentTag.Aoe),
                TalentRequirement.Create(),
                TalentEffect.Mutate("pudzian_byk", 2, spychacz, OfferGenerator.GroupMutation)),

            Card("pudzian_wkurw", "Wkurw",
                "Obrywanie nabija Furię. Po pełnym pasku atak przyspiesza, ale bierzesz więcej obrażeń.",
                3, Tags(TalentTag.Berserker, TalentTag.Risk),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.FuryMeter, OfferGenerator.GroupCore, furyTuning)),

            Card("pudzian_hart", "Hart",
                "Każdy hit daje stack Harta. Przy 5 następny hit Cię nie rani i ogłusza atakującego.",
                3, Tags(TalentTag.Tank, TalentTag.Defence),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.HartStacks, OfferGenerator.GroupCore, hartTuning)),

            Card("pudzian_spalona_ziemia", "Spalona Ziemia",
                "Po lądowaniu Skoku zostaje paląca strefa na ziemi.",
                3, Tags(TalentTag.Aoe, TalentTag.Zone, TalentTag.Damage),
                TalentRequirement.Create(requiresTalents: new[] { "pudzian_skok" }),
                TalentEffect.AddEffect("pudzian_skok", 0, SkillEffectKind.SpawnDamageZone,
                    OfferGenerator.GroupFollowup)),

            Card("pudzian_najezony", "Najeżony",
                "Sprowokowany wróg, który Cię uderzy, dostaje część obrażeń z powrotem.",
                3, Tags(TalentTag.Taunt, TalentTag.Reflect),
                TalentRequirement.Create(requiresTalents: new[] { "pudzian_zryj_mnie" }),
                TalentEffect.Persistent(PersistentEffectKind.DamageReflect, OfferGenerator.GroupFollowup, reflectTuning)),

            Card("pudzian_w_sciane", "W ścianę",
                "Wbicie niesionej grupy w ścianę ogłusza ich i kończy Byka.",
                3, Tags(TalentTag.Control, TalentTag.Collision, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "pudzian_spychacz" }),
                TalentEffect.AddEffect("pudzian_spychacz", 2, SkillEffectKind.PulseRingOnWallStop,
                    OfferGenerator.GroupFollowup)),

            Card("pudzian_ja_jestem_boss", "JA JESTEM BOSS",
                "Aktywne ulti: urośniesz, szerszy zamach i Stomp. Zwykłych wrogów rozpychasz samym ruchem.",
                4, Tags(TalentTag.Kolos, TalentTag.Tank, TalentTag.Control),
                TalentRequirement.Create(),
                TalentEffect.Grant(boss, OfferGenerator.GroupUltimate, PersistentEffectKind.KolosForm,
                    persistentTuning: kolosTuning)),

            Card("pudzian_trzesienie", "Trzęsienie Świata",
                "Aktywne ulti: po zapowiedzi trzy fale lecą wzdłuż wszystkich linii.",
                4, Tags(TalentTag.Trzesienie, TalentTag.Aoe, TalentTag.Control),
                TalentRequirement.Create(),
                TalentEffect.Grant(trzesienie, OfferGenerator.GroupUltimate)),

            Card("pudzian_piekielna_aura", "Piekielna Aura",
                "Pasywne ulti: cały czas palisz wrogów wokół. Im dłużej stoją, tym mocniej.",
                4, Tags(TalentTag.Aura, TalentTag.Aoe, TalentTag.Zone),
                TalentRequirement.Create(),
                TalentEffect.Grant(aura, OfferGenerator.GroupUltimate, PersistentEffectKind.HellAuraRamping,
                    persistentTuning: auraTuning)),

            Card("pudzian_nie_zabijecie_mnie", "Nie zabijecie mnie",
                "Pasywne ulti: raz na falę śmiertelny hit nie zabija — wylecz się w oknie albo giniesz.",
                4, Tags(TalentTag.OstatniaSzansa, TalentTag.Tank, TalentTag.Heal, TalentTag.Risk),
                TalentRequirement.Create(),
                TalentEffect.Grant(lastStandSkill, OfferGenerator.GroupUltimate,
                    PersistentEffectKind.TimedLastChance, persistentTuning: lastChanceTuning)),

            Card("pudzian_prawdziwy_boss", "Prawdziwy Boss",
                "W formie Kolosa automatycznie ciągniesz aggro wokół siebie.",
                5, Tags(TalentTag.Taunt, TalentTag.Control),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_ja_jestem_boss",
                    requiresTags: new[] { TalentTag.Taunt }),
                TalentEffect.Persistent(PersistentEffectKind.PeriodicTauntInForm, OfferGenerator.GroupCapstone, prawdziwyBossTuning)),

            Card("pudzian_pozeracz", "Pożeracz",
                "Zabójstwa w formie leczą i trochę przedłużają Kolosa.",
                5, Tags(TalentTag.Heal, TalentTag.Kolos),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_ja_jestem_boss",
                    requiresTags: new[] { TalentTag.Heal }),
                TalentEffect.Persistent(PersistentEffectKind.OnKillFormExtend, OfferGenerator.GroupCapstone, pozeraczTuning)),

            Card("pudzian_wkurwiony_kolos", "Wkurwiony Kolos",
                "Kolos macha szybciej i mocniej, ale bierzesz więcej obrażeń.",
                5, Tags(TalentTag.Berserker, TalentTag.Risk),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_ja_jestem_boss",
                    requiresTags: new[] { TalentTag.Berserker }),
                TalentEffect.Persistent(PersistentEffectKind.WkurwionyKolos, OfferGenerator.GroupCapstone, wkurwKolosTuning)),

            Card("pudzian_kazdy_krok", "Każdy krok to problem",
                "Każdy krok w formie wypuszcza małą falę pod stopami.",
                5, Tags(TalentTag.Aoe, TalentTag.Kolos),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_ja_jestem_boss",
                    requiresTags: new[] { TalentTag.Aoe }),
                TalentEffect.Persistent(PersistentEffectKind.MoveShockwave, OfferGenerator.GroupCapstone, kazdyKrokTuning)),

            Card("pudzian_wstrzasy_wtorne", "Wstrząsy Wtórne",
                "Po głównym Trzęsieniu idą mniejsze fale wtórne.",
                5, Tags(TalentTag.Trzesienie, TalentTag.Aoe),
                TalentRequirement.Create(requiresUltimateId: "pudzian_trzesienie"),
                TalentEffect.Persistent(PersistentEffectKind.AftershockWaves, OfferGenerator.GroupCapstone, wstrzasyTuning)),

            Card("pudzian_rozpadlina", "Rozpadlina",
                "Na trafionych wrogach zostają pęknięcia ziemi, które dalej ranią.",
                5, Tags(TalentTag.Zone, TalentTag.Aoe, TalentTag.Damage),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_trzesienie",
                    requiresTags: new[] { TalentTag.Zone }),
                TalentEffect.Persistent(PersistentEffectKind.GroundCracks, OfferGenerator.GroupCapstone, rozpadlinaTuning)),

            Card("pudzian_krwawa_sejsmika", "Krwawa Sejsmika",
                "Trafieni Trzęsieniem leczą Cię (z limitem).",
                5, Tags(TalentTag.Heal, TalentTag.Aoe),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_trzesienie",
                    requiresTags: new[] { TalentTag.Heal }),
                TalentEffect.Persistent(PersistentEffectKind.KrwawaSejsmika, OfferGenerator.GroupCapstone, krwawaTuning)),

            Card("pudzian_zatrzymajcie_sie", "Zatrzymajcie się wszyscy",
                "Trzęsienie mocniej zachwia wrogów. Boss nie jest stun-lockowany.",
                5, Tags(TalentTag.Control),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_trzesienie",
                    requiresTags: new[] { TalentTag.Control }),
                TalentEffect.Persistent(PersistentEffectKind.StaggerBoost, OfferGenerator.GroupCapstone, staggerBoostTuning)),

            Card("pudzian_wampiryczny_ogien", "Wampiryczny Ogień",
                "Część obrażeń aury wraca jako leczenie.",
                5, Tags(TalentTag.Aura, TalentTag.Heal),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_piekielna_aura",
                    requiresTags: new[] { TalentTag.Heal }),
                TalentEffect.Persistent(PersistentEffectKind.VampireAura, OfferGenerator.GroupCapstone, vampireTuning)),

            Card("pudzian_przegrzanie", "Przegrzanie",
                "Przy niskim HP aura jest większa i szybciej wchodzi na pełny żar.",
                5, Tags(TalentTag.Aura, TalentTag.Berserker, TalentTag.Risk),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_piekielna_aura",
                    requiresAnyTags: new[] { TalentTag.Berserker, TalentTag.Risk }),
                TalentEffect.Persistent(PersistentEffectKind.OverheatAuraRamping, OfferGenerator.GroupCapstone, przegrzanieTuning)),

            Card("pudzian_slad_ognia", "Ślad Ognia",
                "Ruch zostawia krótki palący ślad.",
                5, Tags(TalentTag.Zone, TalentTag.Mobility),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_piekielna_aura",
                    requiresAnyTags: new[] { TalentTag.Zone, TalentTag.Mobility }),
                TalentEffect.Persistent(PersistentEffectKind.FireTrail, OfferGenerator.GroupCapstone, sladTuning)),

            Card("pudzian_pozar_lancuchowy", "Pożar Łańcuchowy",
                "Wróg długo w aurze zapala pobliskich nawet po wyjściu.",
                5, Tags(TalentTag.Aoe),
                TalentRequirement.Create(requiresUltimateId: "pudzian_piekielna_aura"),
                TalentEffect.Persistent(PersistentEffectKind.ChainIgnite, OfferGenerator.GroupCapstone, pozarTuning)),

            Card("pudzian_nie_chce_umierac", "Nie chcę umierać",
                "W Ostatniej Szansie leczenie jest mocniejsze — łatwiej przeżyć okno.",
                5, Tags(TalentTag.Heal, TalentTag.OstatniaSzansa),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_nie_zabijecie_mnie",
                    requiresTags: new[] { TalentTag.Heal }),
                TalentEffect.Persistent(PersistentEffectKind.HealAmpInLastChance, OfferGenerator.GroupCapstone, nieChceTuning)),

            Card("pudzian_agonia", "Agonia",
                "W oknie Ostatniej Szansy wychodzą fale obrażeń wokół Ciebie.",
                5, Tags(TalentTag.Aoe),
                TalentRequirement.Create(requiresUltimateId: "pudzian_nie_zabijecie_mnie"),
                TalentEffect.Persistent(PersistentEffectKind.AgonyPulses, OfferGenerator.GroupCapstone, agoniaTuning)),

            Card("pudzian_druga_szansa", "Druga szansa",
                "Jeśli przeżyjesz okno: reset CD aktywnych i krótki boost obrażeń.",
                5, Tags(TalentTag.Risk),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_nie_zabijecie_mnie",
                    requiresTags: new[] { TalentTag.Risk }),
                TalentEffect.Persistent(PersistentEffectKind.SecondWind, OfferGenerator.GroupCapstone, drugaSzansaTuning)),

            Card("pudzian_ostatni_wkurw", "Ostatni Wkurw",
                "Ostatnia Szansa sama odpala Furię.",
                5, Tags(TalentTag.Berserker, TalentTag.Risk),
                TalentRequirement.Create(
                    requiresUltimateId: "pudzian_nie_zabijecie_mnie",
                    requiresTags: new[] { TalentTag.Berserker }),
                TalentEffect.Persistent(PersistentEffectKind.AutoFuryOnLastChance, OfferGenerator.GroupCapstone)),

            Card("pudzian_pekniecie", "Pęknięcie",
                "Debug: strefa spowolnienia po stompie (F8).",
                2, Tags(TalentTag.Zone),
                TalentRequirement.Create(metaUnlockId: "debug_pekniecie"),
                TalentEffect.AddEffect("pudzian_trzasniecie", 0, SkillEffectKind.SpawnSlowZone, "debug"))
        };
    }

    private static ClassProgressionTable CreatePudzianProgressionTable()
    {
        var table = ScriptableObject.CreateInstance<ClassProgressionTable>();
        table.InitializeRuntime(
            PlayerClassId.Pudzian,
            new[] { "pudzian_trzasniecie", "pudzian_no_chodz_tu", "pudzian_byk" },
            new[]
            {
                LevelOfferRule.Create(2, OfferRecipeKind.OneMutationPerActive, 3, 3),
                LevelOfferRule.Create(3, OfferRecipeKind.MixIndependentAndFollowup, 3, 3,
                    OfferGenerator.GroupCore, 2),
                LevelOfferRule.Create(4, OfferRecipeKind.AllEligibleGrantUltimate, 2, 4),
                LevelOfferRule.Create(5, OfferRecipeKind.AllEligible, 1, 6)
            });
        return table;
    }

    private static TalentTag[] Tags(params TalentTag[] tags) => tags;

    private static TalentDefinition Card(
        string id, string displayName, string description,
        int level, TalentTag[] tags, TalentRequirement req, TalentEffect effect)
    {
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ConfigureRuntime(id, displayName, description, PlayerClassId.Pudzian, level, tags, req, effect);
        return talent;
    }

    private static MeleeWeaponDefinition CreateMelee(
        float damage, float interval, float range, float arc, int maxTargets)
    {
        var profile = ScriptableObject.CreateInstance<MeleeWeaponDefinition>();
        SetField(profile, "damage", damage);
        SetField(profile, "attackInterval", interval);
        SetField(profile, "range", range);
        SetField(profile, "arcDegrees", arc);
        SetField(profile, "maxTargets", maxTargets);
        return profile;
    }

    private static WeaponDefinition CreateWeapon(
        string id, string displayName, LootRarity rarity,
        MeleeWeaponDefinition profile, StatModifiers bonus)
    {
        return CreateWeaponInternal(id, displayName, rarity, profile, bonus, false);
    }

    private static WeaponDefinition CreateUniqueWeapon(
        string id, string displayName,
        MeleeWeaponDefinition profile, StatModifiers bonus)
    {
        return CreateWeaponInternal(id, displayName, LootRarity.Unique, profile, bonus, true);
    }

    private static WeaponDefinition CreateWeaponInternal(
        string id, string displayName, LootRarity rarity,
        MeleeWeaponDefinition profile, StatModifiers bonus, bool isBossUnique)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
        SetField(weapon, "weaponId", id);
        SetField(weapon, "displayName", displayName);
        SetField(weapon, "rarity", rarity);
        SetField(weapon, "meleeProfile", profile);
        SetField(weapon, "bonusStats", bonus);
        SetField(weapon, "family", WeaponFamilyCatalog.FromId(id));
        SetField(weapon, "isBossUnique", isBossUnique);
        return weapon;
    }

    private static ItemDefinition CreateItem(string id, string displayName, LootRarity rarity, StatModifiers mods)
    {
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        SetField(item, "itemId", id);
        SetField(item, "displayName", displayName);
        SetField(item, "rarity", rarity);
        SetField(item, "statModifiers", mods);
        return item;
    }

    private static ClassDefinition CreateClass(
        PlayerClassId classId, string displayName,
        WeaponDefinition primary, WeaponDefinition secondary,
        float baseHp, float baseMove)
    {
        var cls = ScriptableObject.CreateInstance<ClassDefinition>();
        SetField(cls, "classId", classId);
        SetField(cls, "displayName", displayName);
        SetField(cls, "startingWeaponPrimary", primary);
        SetField(cls, "startingWeaponSecondary", secondary);
        SetField(cls, "baseMaxHealth", baseHp);
        SetField(cls, "baseMoveSpeed", baseMove);
        SetField(cls, "maxHealthPerTeamLevel", 8f);
        SetField(cls, "moveSpeedPerTeamLevel", 0.05f);
        return cls;
    }

    private static StatModifiers Mod(
        float damageMultiplier = 1f,
        float attackSpeedMultiplier = 1f,
        float maxHealthBonus = 0f,
        float maxHealthMultiplier = 1f,
        float moveSpeedMultiplier = 1f)
    {
        return StatModifiers.Create(
            damageMultiplier, attackSpeedMultiplier, maxHealthBonus, maxHealthMultiplier, moveSpeedMultiplier);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}
