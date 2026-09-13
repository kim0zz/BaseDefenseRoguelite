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

        var petarda = CreateWeapon("petarda", "Petarda", LootRarity.Common, CreatePetardaProfile(), Mod());
        var allWeapons = new List<WeaponDefinition>(weapons) { petarda };
        weapons = allWeapons.ToArray();

        var classes = new[]
        {
            CreateClass(PlayerClassId.Jamie, "Jamie", weapons[0], weapons[6], 100f, 5f),
            CreateClass(PlayerClassId.Cwel, "Cwel", weapons[1], weapons[6], 85f, 5.8f),
            CreateClass(PlayerClassId.Pudzian, "Pudzian", weapons[3], weapons[4], 135f, 4.2f),
            CreateClass(PlayerClassId.Cipak, "Cipak", weapons[6], weapons[1], 90f, 5.2f),
            CreateBombermanClass(petarda)
        };

        var pudzianTalents = BuildPudzianTalents();
        var bombermanTalents = BuildBombermanTalents();
        var talents = new TalentDefinition[pudzianTalents.Length + bombermanTalents.Length];
        pudzianTalents.CopyTo(talents, 0);
        bombermanTalents.CopyTo(talents, pudzianTalents.Length);

        var tables = new[] { CreatePudzianProgressionTable(), CreateBombermanProgressionTable() };

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

    private static MeleeWeaponDefinition CreatePetardaProfile()
    {
        var profile = CreateMelee(9f, 0.70f, 8f, 0f, 5);
        SetField(profile, "splashRadius", 1.1f);
        SetField(profile, "arcHeight", 0.55f);
        return profile;
    }

    private static ClassDefinition CreateBombermanClass(WeaponDefinition petarda)
    {
        var cls = CreateClass(PlayerClassId.Bomberman, "Bomberman", petarda, petarda, 95f, 5.3f);
        SetField(cls, "maxHealthPerTeamLevel", 6f);
        return cls;
    }

    private static TalentDefinition[] BuildBombermanTalents()
    {
        var szybkostrzelnosc = SkillContentFactory.CreateBombermanSzybkostrzelnosc();
        var orbitale = SkillContentFactory.CreateBombermanOrbitale();
        var nalot = SkillContentFactory.CreateBombermanNalot();

        var kasetowaTuning = EffectTuning.Create(0.8f, 7f, 1.4f, 1.2f, 1.8f, i0: 3);
        var hukTuning = EffectTuning.Create(2.2f);
        var ogluszajacyTuning = EffectTuning.Create(1.0f, 0.5f, 0f, 8f);
        var saperTuning = EffectTuning.Create(1.0f, 1.6f);
        var piromanTuning = EffectTuning.Create(2f, 0.5f, 2.0f);
        var torpedyTuning = EffectTuning.Create(8f, 0.45f, 8f, i0: 3);
        var ladunekTuning = EffectTuning.Create(1.75f, 1.20f, 1.40f, 1.60f, i0: 4);
        var kulaTuning = EffectTuning.Create(11f, 0.85f, 6.5f, i0: 3);
        var rapidTuning = EffectTuning.Create(0.22f, 6f);
        var termicznaTuning = EffectTuning.Create(0.5f, 8f, 2.0f, 0.40f, i0: 0);
        var feniksTuning = EffectTuning.Create(6f, 1.2f, 0.60f, i0: 2, i1: 6);
        var polowanieTuning = EffectTuning.Create(8f, 1.20f, 1.44f, i0: 3);
        var termobarycznyTuning = EffectTuning.Create(1.0f, 8f, 1.5f, i0: 1);
        var karabinTuning = EffectTuning.Create(0.22f, 0.45f);
        var przelamanieTuning = EffectTuning.Create(0.45f, 22f, 2.8f);
        var treserTuning = EffectTuning.Create(4.5f);
        var taranTuning = EffectTuning.Create(0.40f, 3.5f, i0: 4);
        var breakTuning = EffectTuning.Create(16f, 6f, 1.35f, i0: 6);
        var lancuchTuning = EffectTuning.Create(0.12f, 0.15f, i0: 5);
        var przeladowanyTuning = EffectTuning.Create(2.4f, 3.2f, 0.60f, 2f);
        var reakcjaOrbitalnaTuning = EffectTuning.Create(1.0f);
        var planetarnyTuning = EffectTuning.Create(3.0f, 200f, i0: 6);
        var ostatniaBombaTuning = EffectTuning.Create(18f, 2.6f, 4f, 0.20f, i0: 8);
        var capBonusTuning = EffectTuning.Create(i0: 10);

        return new[]
        {
            BombermanCard("bomberman_kasetowa", "Bomba kasetowa",
                "Wybuch głównej bomby rozrzuca trzy miniatury z krótkim lontem.",
                2, Tags(TalentTag.Demolition, TalentTag.Aoe, TalentTag.Chain),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.ClusterOnExplode, OfferGenerator.GroupMutation, kasetowaTuning)),

            BombermanCard("bomberman_wiekszy_huk", "Większy Huk",
                "Petarda i wybuchy podstawowe zyskują większy promień rażenia.",
                2, Tags(TalentTag.Basic, TalentTag.Aoe),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.BasicSplashBonus, OfferGenerator.GroupMutation, hukTuning)),

            BombermanCard("bomberman_wybuch_ogluszajacy", "Wybuch ogłuszający",
                "Kopnięta bomba ogłusza wrogów przy wybuchu na kontakcie.",
                2, Tags(TalentTag.Control, TalentTag.Physics),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.KickExplodeStun, OfferGenerator.GroupMutation, ogluszajacyTuning)),

            BombermanCard("bomberman_saper", "Saper",
                "Postawione bomby uzbrajają się i wybuchają, gdy wróg wejdzie w zasięg. Działa równolegle z lontem.",
                3, Tags(TalentTag.Trapper, TalentTag.Control),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.ArmToProximityMine, OfferGenerator.GroupCore, saperTuning)),

            BombermanCard("bomberman_piroman", "Piroman",
                "Twoje ofensywne trafienia podpalają wrogów na krótki czas.",
                3, Tags(TalentTag.Fire),
                TalentRequirement.Create(),
                TalentEffect.Persistent(PersistentEffectKind.ApplyBurnOnPlayerDamage, OfferGenerator.GroupCore, piromanTuning)),

            BombermanCard("bomberman_mini_torpedy", "Mini-torpedy",
                "Miniatury z kasetowej lecą w najbliższe cele i wybuchają na kontakcie.",
                3, Tags(TalentTag.Seeking, TalentTag.Chain, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_kasetowa" }),
                TalentEffect.Persistent(PersistentEffectKind.ChildHoming, OfferGenerator.GroupFollowup, torpedyTuning)),

            BombermanCard("bomberman_ladunek_kumulacyjny", "Ładunek kumulacyjny",
                "Kolejne trafienia tego samego celu petardą zwiększają obrażenia.",
                3, Tags(TalentTag.Basic, TalentTag.Demolition),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wiekszy_huk" }),
                TalentEffect.Persistent(PersistentEffectKind.PerTargetHitStacks, OfferGenerator.GroupFollowup, ladunekTuning)),

            BombermanCard("bomberman_kula_bilardowa", "Kula bilardowa",
                "Kopnięta bomba odbija się od wrogów zamiast wybuchać od razu.",
                3, Tags(TalentTag.Physics, TalentTag.Collision),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wybuch_ogluszajacy" }),
                TalentEffect.Persistent(PersistentEffectKind.BilliardOnKick, OfferGenerator.GroupFollowup, kulaTuning)),

            BombermanCard("bomberman_szybkostrzelnosc", "Szybkostrzelność",
                "Aktywne ulti: przez kilka sekund petarda strzela znacznie szybciej.",
                4, Tags(TalentTag.RapidFire, TalentTag.Basic),
                TalentRequirement.Create(),
                TalentEffect.Grant(szybkostrzelnosc, OfferGenerator.GroupUltimate,
                    PersistentEffectKind.TimedAttackIntervalOverride, persistentTuning: rapidTuning)),

            BombermanCard("bomberman_orbitale", "Orbitale",
                "Aktywne ulti: wokół ciebie krążą bomby gotowe do kontaktu z wrogami.",
                4, Tags(TalentTag.Orbital, TalentTag.Physics, TalentTag.Aoe),
                TalentRequirement.Create(),
                TalentEffect.Grant(orbitale, OfferGenerator.GroupUltimate)),

            BombermanCard("bomberman_nalot", "Nalot",
                "Aktywne ulti: seria wybuchów w pasie przed tobą po krótkiej zapowiedzi.",
                4, Tags(TalentTag.Airstrike, TalentTag.Aoe, TalentTag.Zone),
                TalentRequirement.Create(),
                TalentEffect.Grant(nalot, OfferGenerator.GroupUltimate)),

            BombermanCard("bomberman_reakcja_termiczna", "Reakcja termiczna",
                "Podpalone cele mogą wybuchnąć, zużywając część pozostałego burnu.",
                5, Tags(TalentTag.Fire, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_kasetowa", "bomberman_piroman" }),
                TalentEffect.Persistent(PersistentEffectKind.ConsumeBurnOnHit, OfferGenerator.GroupCapstone, termicznaTuning)),

            BombermanCard("bomberman_feniks", "Feniks",
                "Zabójstwo podpalonego wroga może zostawić krótką bombę-feniksa.",
                5, Tags(TalentTag.Fire, TalentTag.Chain),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_kasetowa", "bomberman_piroman" }),
                TalentEffect.Persistent(PersistentEffectKind.PhoenixOnBurnKill, OfferGenerator.GroupCapstone, feniksTuning)),

            BombermanCard("bomberman_polowanie_stadne", "Polowanie stadne",
                "Homingi i wybuchy preferują elity i bossy w zasięgu, z rosnącym focusem.",
                5, Tags(TalentTag.Seeking, TalentTag.Damage),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_mini_torpedy" }),
                TalentEffect.Persistent(PersistentEffectKind.PackHuntFocus, OfferGenerator.GroupCapstone, polowanieTuning)),

            BombermanCard("bomberman_termobaryczny", "Termobaryczny",
                "Podstawowe wybuchy zużywają burn i zyskują mocniejszy splash.",
                5, Tags(TalentTag.Basic, TalentTag.Fire, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wiekszy_huk", "bomberman_piroman" }),
                TalentEffect.Persistent(PersistentEffectKind.ConsumeBurnOnHit, OfferGenerator.GroupCapstone, termobarycznyTuning)),

            BombermanCard("bomberman_rozniecanie", "Rozniecanie",
                "Splash petardy rozprzestrzenia burn na trafione cele.",
                5, Tags(TalentTag.Basic, TalentTag.Fire, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wiekszy_huk", "bomberman_piroman" }),
                TalentEffect.Persistent(PersistentEffectKind.SpreadBurnOnSplash, OfferGenerator.GroupCapstone)),

            BombermanCard("bomberman_karabin_maszynowy", "Karabin maszynowy",
                "Petarda na stałe strzela szybciej, ale każdy strzał rani słabiej.",
                5, Tags(TalentTag.Basic, TalentTag.RapidFire),
                TalentRequirement.Create(requiresTagCounts: new[] { TagCountRequirement.Create(TalentTag.Basic, 2) }),
                TalentEffect.Persistent(PersistentEffectKind.PermanentAttackIntervalOverride, OfferGenerator.GroupCapstone, karabinTuning)),

            BombermanCard("bomberman_przelamanie", "Przełamanie",
                "Pełne stacki ładunku wyzwalają opóźniony wybuch na celu.",
                5, Tags(TalentTag.Basic, TalentTag.Demolition),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_ladunek_kumulacyjny" }),
                TalentEffect.Persistent(PersistentEffectKind.MarkedDelayedBlast, OfferGenerator.GroupCapstone, przelamanieTuning)),

            BombermanCard("bomberman_treser_bomb", "Treser bomb",
                "Kopniak dodatkowo wystrzeliwuje wszystkie pobliskie bomby wokół ciebie, nie tylko te przed tobą.",
                5, Tags(TalentTag.Control, TalentTag.Trapper),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wybuch_ogluszajacy", "bomberman_saper" }),
                TalentEffect.Persistent(PersistentEffectKind.MultiLaunchOwned, OfferGenerator.GroupCapstone, treserTuning)),

            BombermanCard("bomberman_plonacy_taran", "Płonący taran",
                "Kopnięta bomba niesie płonącego wroga i wybucha po krótkim locie.",
                5, Tags(TalentTag.Fire, TalentTag.Physics),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_wybuch_ogluszajacy", "bomberman_piroman" }),
                TalentEffect.Persistent(PersistentEffectKind.CarryBurningOnKick, OfferGenerator.GroupCapstone, taranTuning)),

            BombermanCard("bomberman_break", "BREAK!",
                "Kula bilardowa dłużej jeździ, odbija się więcej razy i mocniej kończy.",
                5, Tags(TalentTag.Physics, TalentTag.Collision),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_kula_bilardowa" }),
                TalentEffect.Persistent(PersistentEffectKind.BilliardBreakUpgrade, OfferGenerator.GroupCapstone, breakTuning)),

            BombermanCard("bomberman_lancuch_kolizji", "Łańcuch kolizji",
                "Każde trafienie kuli bilardowej powiększa finalny wybuch.",
                5, Tags(TalentTag.Collision, TalentTag.Aoe),
                TalentRequirement.Create(requiresTalents: new[] { "bomberman_kula_bilardowa" }),
                TalentEffect.Persistent(PersistentEffectKind.CollisionScalingExplosion, OfferGenerator.GroupCapstone, lancuchTuning)),

            BombermanCard("bomberman_przeladowany_magazynek", "Przeładowany magazynek",
                "Podczas Szybkostrzelności splash petardy rośnie i zostawia krótki żar.",
                5, Tags(TalentTag.RapidFire, TalentTag.Aoe),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_szybkostrzelnosc",
                    requiresTags: new[] { TalentTag.Basic }),
                TalentEffect.Persistent(PersistentEffectKind.RapidSplashDuringOverride, OfferGenerator.GroupCapstone, przeladowanyTuning)),

            BombermanCard("bomberman_reakcja_orbitalna", "Reakcja orbitalna",
                "Wybuchy skracają czas przeładowania orbitujących bomb.",
                5, Tags(TalentTag.Orbital, TalentTag.Chain),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_orbitale",
                    requiresTags: new[] { TalentTag.Orbital }),
                TalentEffect.Persistent(PersistentEffectKind.OrbitalRechargeOnExplode, OfferGenerator.GroupCapstone, reakcjaOrbitalnaTuning)),

            BombermanCard("bomberman_kasetowe_satelity", "Kasetowe satelity",
                "Orbitujące bomby rozrzucają miniatury jak kasetowa przy wybuchu.",
                5, Tags(TalentTag.Orbital, TalentTag.Chain),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_orbitale",
                    requiresTalents: new[] { "bomberman_kasetowa" }),
                TalentEffect.Persistent(PersistentEffectKind.OrbitalInheritsCluster, OfferGenerator.GroupCapstone)),

            BombermanCard("bomberman_uklad_planetarny", "Układ planetarny",
                "Więcej orbitujących bomb na szerszym pierścieniu i szybszej rotacji.",
                5, Tags(TalentTag.Orbital, TalentTag.Physics),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_orbitale",
                    requiresTags: new[] { TalentTag.Physics }),
                TalentEffect.Persistent(PersistentEffectKind.PlanetaryOrbitUpgrade, OfferGenerator.GroupCapstone, planetarnyTuning)),

            BombermanCard("bomberman_nalot_opozniony", "Nalot z opóźnionym zapłonem",
                "Część wybuchów nalotu zostawia normalne bomby na ziemi.",
                5, Tags(TalentTag.Airstrike, TalentTag.Trapper),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_nalot",
                    requiresTags: new[] { TalentTag.Airstrike }),
                TalentEffect.Persistent(PersistentEffectKind.AirstrikeLeavesNormals, OfferGenerator.GroupCapstone)),

            BombermanCard("bomberman_ostatnia_bomba", "Ostatnia bomba",
                "Końcowy wybuch nalotu rośnie z liczbą unikalnych celów trafionych w sekwencji.",
                5, Tags(TalentTag.Airstrike, TalentTag.Zone),
                TalentRequirement.Create(
                    requiresUltimateId: "bomberman_nalot",
                    requiresTags: new[] { TalentTag.Zone }),
                TalentEffect.Persistent(PersistentEffectKind.AirstrikeFinisher, OfferGenerator.GroupCapstone, ostatniaBombaTuning)),

            BombermanCard("bomberman_10_bomb", "10 BOMB",
                "Możesz utrzymać do dziesięciu bomb; nadmiar detonuje najstarszą.",
                5, Tags(TalentTag.Demolition, TalentTag.Trapper),
                TalentRequirement.Create(requiresAnyTags: new[] { TalentTag.Demolition, TalentTag.Trapper }),
                TalentEffect.Persistent(PersistentEffectKind.DeployableCapBonus, OfferGenerator.GroupCapstone, capBonusTuning))
        };
    }

    private static ClassProgressionTable CreateBombermanProgressionTable()
    {
        var table = ScriptableObject.CreateInstance<ClassProgressionTable>();
        table.InitializeRuntime(
            PlayerClassId.Bomberman,
            new[] { "bomberman_bomba", "bomberman_wybuchowy_odskok", "bomberman_kopniak" },
            new[]
            {
                LevelOfferRule.Create(2, OfferRecipeKind.AllEligibleInGroup, 3, 3, OfferGenerator.GroupMutation),
                LevelOfferRule.Create(3, OfferRecipeKind.MixIndependentAndFollowup, 3, 3,
                    OfferGenerator.GroupCore, 2),
                LevelOfferRule.Create(4, OfferRecipeKind.AllEligibleGrantUltimate, 3, 3),
                LevelOfferRule.Create(5, OfferRecipeKind.AllEligible, 1, 8)
            });
        return table;
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
        int level, TalentTag[] tags, TalentRequirement req, TalentEffect effect,
        PlayerClassId classId = PlayerClassId.Pudzian)
    {
        var talent = ScriptableObject.CreateInstance<TalentDefinition>();
        talent.ConfigureRuntime(id, displayName, description, classId, level, tags, req, effect);
        return talent;
    }

    private static TalentDefinition BombermanCard(
        string id, string displayName, string description,
        int level, TalentTag[] tags, TalentRequirement req, TalentEffect effect)
    {
        return Card(id, displayName, description, level, tags, req, effect, PlayerClassId.Bomberman);
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
