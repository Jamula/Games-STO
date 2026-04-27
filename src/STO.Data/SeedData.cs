using Microsoft.EntityFrameworkCore;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Data.Context;

namespace STO.Data;

/// <summary>
/// Provides seed data for the STO Account Manager database.
/// All methods are idempotent — safe to call on every startup.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seeds reference items and traits that all players commonly track.
    /// </summary>
    public static async Task SeedAsync(StoDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        await SeedItemsAsync(context);
        await SeedTraitsAsync(context);
    }

    /// <summary>
    /// Seeds demo accounts, characters, and builds for testing.
    /// </summary>
    public static async Task SeedDemoDataAsync(StoDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Ensure reference data exists first
        await SeedAsync(context);

        if (await context.Accounts.AnyAsync())
            return;

        var accounts = CreateDemoAccounts();
        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();

        // Initialize reputations for every demo character
        foreach (var account in accounts)
        {
            foreach (var character in account.Characters)
            {
                InitializeReputations(character);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Initializes all 13 reputation factions at Tier 0 for a new character.
    /// Call this whenever a new character is created.
    /// </summary>
    /// <remarks>
    /// The 13 STO reputation factions:
    ///   1. Task Force Omega
    ///   2. New Romulus
    ///   3. Nukara Strike Force
    ///   4. Dyson Joint Command
    ///   5. 8472 Counter-Command
    ///   6. Delta Alliance
    ///   7. Iconian Resistance
    ///   8. Terran Task Force
    ///   9. Temporal Defense Initiative
    ///  10. Lukari Restoration
    ///  11. Competitive Wargames
    ///  12. Gamma Task Force
    ///  13. Discovery Legends
    /// </remarks>
    public static void InitializeReputations(Character character)
    {
        var existing = character.Reputations
            .Select(r => r.Faction)
            .ToHashSet();

        foreach (ReputationFaction faction in Enum.GetValues<ReputationFaction>())
        {
            if (existing.Contains(faction))
                continue;

            character.Reputations.Add(new Reputation
            {
                CharacterId = character.Id,
                Faction = faction,
                Tier = 0,
                Progress = 0
            });
        }
    }

    #region Items

    private static async Task SeedItemsAsync(StoDbContext context)
    {
        if (await context.Items.AnyAsync())
            return;

        var items = new List<Item>();

        // --- Iconian Resistance Set ---
        items.Add(MakeItem("Iconian Resistance Resilient Shield Array", ItemType.SpaceShield,
            ItemRarity.VeryRare, "Iconian Resistance", "Iconian Resistance reputation (T5)"));
        items.Add(MakeItem("Iconian Resistance Deflector Array", ItemType.SpaceDeflector,
            ItemRarity.VeryRare, "Iconian Resistance", "Iconian Resistance reputation (T4)"));
        items.Add(MakeItem("Iconian Resistance Hyper-Impulse Engines", ItemType.SpaceEngine,
            ItemRarity.VeryRare, "Iconian Resistance", "Iconian Resistance reputation (T3)"));
        items.Add(MakeItem("Iconian Resistance Warp Core", ItemType.SpaceWarpCore,
            ItemRarity.VeryRare, "Iconian Resistance", "Iconian Resistance reputation (T2)"));

        // --- Competitive Wargames Set ---
        items.Add(MakeItem("Prevailing Innervated Impulse Engines", ItemType.SpaceEngine,
            ItemRarity.VeryRare, "Competitive Wargames", "Competitive Wargames reputation"));
        items.Add(MakeItem("Prevailing Fortified Impulse Engines", ItemType.SpaceEngine,
            ItemRarity.VeryRare, "Competitive Wargames", "Competitive Wargames reputation"));

        // --- Colony Deflector ---
        items.Add(MakeItem("Elite Fleet Intervention Protomatter Deflector Array", ItemType.SpaceDeflector,
            ItemRarity.UltraRare, null, "Colony world fleet holding"));

        // --- Temporal Defense Initiative Set ---
        items.Add(MakeItem("Temporal Defense Initiative Deflector Array", ItemType.SpaceDeflector,
            ItemRarity.VeryRare, "Temporal Defense Initiative", "Temporal Defense Initiative reputation"));
        items.Add(MakeItem("Temporal Defense Initiative Resilient Shield Array", ItemType.SpaceShield,
            ItemRarity.VeryRare, "Temporal Defense Initiative", "Temporal Defense Initiative reputation"));
        items.Add(MakeItem("Temporal Defense Initiative Combat Impulse Engines", ItemType.SpaceEngine,
            ItemRarity.VeryRare, "Temporal Defense Initiative", "Temporal Defense Initiative reputation"));
        items.Add(MakeItem("Temporal Defense Initiative Overcharged Warp Core", ItemType.SpaceWarpCore,
            ItemRarity.VeryRare, "Temporal Defense Initiative", "Temporal Defense Initiative reputation"));

        // --- Popular Weapons ---
        items.Add(MakeItem("Terran Task Force Phaser Beam Array", ItemType.SpaceWeapon,
            ItemRarity.VeryRare, "Terran Task Force Munitions", "Terran Task Force reputation"));
        items.Add(MakeItem("Prolonged Engagement Phaser Beam Array", ItemType.SpaceWeapon,
            ItemRarity.VeryRare, null, "Phoenix Prize Pack (Ultra Rare)"));

        // --- Popular Consoles ---
        items.Add(MakeItem("Bioneural Infusion Circuits", ItemType.SpaceConsoleUniversal,
            ItemRarity.Epic, null, "Lobi Store"));
        items.Add(MakeItem("Assimilated Module", ItemType.SpaceConsoleUniversal,
            ItemRarity.VeryRare, "Omega Adapted Borg Technology", "Task Force Omega reputation (T1)"));
        items.Add(MakeItem("Dynamic Power Redistributor Module", ItemType.SpaceConsoleUniversal,
            ItemRarity.Epic, null, "Promotional lockbox / Lobi Store"));
        items.Add(MakeItem("Lorca's Custom Fire Controls", ItemType.SpaceConsoleTactical,
            ItemRarity.VeryRare, "Lorca's Ambition", "Discovery Legends reputation"));

        context.Items.AddRange(items);
        await context.SaveChangesAsync();
    }

    private static Item MakeItem(string name, ItemType type, ItemRarity rarity,
        string? setName, string source)
    {
        return new Item
        {
            Name = name,
            Type = type,
            DefaultRarity = rarity,
            SetName = setName,
            Source = source,
            IsFromWiki = false
        };
    }

    #endregion

    #region Traits

    private static async Task SeedTraitsAsync(StoDbContext context)
    {
        if (await context.Traits.AnyAsync())
            return;

        var traits = new List<Trait>();

        // --- Personal Space Traits ---
        traits.Add(MakeTrait("Inspirational Leader", TraitType.PersonalSpace,
            "10% chance to gain a team-wide bonus to all skills on Bridge Officer ability use",
            "Lock Box / Exchange"));
        traits.Add(MakeTrait("A Good Day to Die", TraitType.PersonalSpace,
            "Go Down Fighting can be used at any hull value",
            "K-13 Fleet Holding"));
        traits.Add(MakeTrait("Context is for Kings", TraitType.PersonalSpace,
            "Bonus damage and damage resistance from flanking",
            "Discovery Legends reputation"));
        traits.Add(MakeTrait("Terran Targeting Systems", TraitType.PersonalSpace,
            "Crit severity boost against targets below 50% hull",
            "Terran Task Force reputation"));
        traits.Add(MakeTrait("Intelligence Agent Attache", TraitType.PersonalSpace,
            "Weapon critical strikes reduce Bridge Officer ability recharge",
            "Lock Box / Exchange"));
        traits.Add(MakeTrait("Unconventional Systems", TraitType.PersonalSpace,
            "Control Bridge Officer abilities reduce cooldowns of Universal Consoles",
            "Infinity Lock Box"));

        // --- Starship Traits ---
        traits.Add(MakeTrait("Emergency Weapon Cycle", TraitType.Starship,
            "Activating Emergency Power to Weapons reduces weapon power cost and boosts firing speed",
            "Arbiter / Morrigu / Kurak (T6 Battlecruiser)"));
        traits.Add(MakeTrait("Entwined Tactical Matrices", TraitType.Starship,
            "Using Fire at Will or Torpedo Spread triggers the other for free",
            "Gagarin / Qugh (T6 Miracle Worker Battlecruiser)"));
        traits.Add(MakeTrait("Superweapon Ingenuity", TraitType.Starship,
            "Firing a torpedo while Torpedo: High Yield is active adds a second charge",
            "Xindi-Primate Ateleth Dreadnought Cruiser"));
        traits.Add(MakeTrait("History Will Remember", TraitType.Starship,
            "Stacking damage and damage resistance buff in combat",
            "Khaleri / Qoj / Vor'ral (T6 Support Cruiser)"));
        traits.Add(MakeTrait("Strike From Shadows", TraitType.Starship,
            "Bonus stealth and damage after using non-weapon abilities",
            "Shran / M'Chla (T6 Light Pilot Escort)"));

        // --- Personal Ground Traits ---
        traits.Add(MakeTrait("Adrenal Release", TraitType.PersonalGround,
            "Chance on being hit to gain a damage buff",
            "Innate trait"));
        traits.Add(MakeTrait("Lucky", TraitType.PersonalGround,
            "Improved critical hit chance for ground combat",
            "Innate trait"));
        traits.Add(MakeTrait("Vicious", TraitType.PersonalGround,
            "Improved critical severity for ground combat",
            "Innate trait"));

        context.Traits.AddRange(traits);
        await context.SaveChangesAsync();
    }

    private static Trait MakeTrait(string name, TraitType type, string description, string source)
    {
        return new Trait
        {
            Name = name,
            Type = type,
            Description = description,
            Source = source,
            IsFromWiki = false
        };
    }

    #endregion

    #region Demo Data

    private static List<Account> CreateDemoAccounts()
    {
        return
        [
            new Account
            {
                Gamertag = "CaptainJaneway@handle1",
                Nickname = "Janeway",
                Notes = "Main account",
                Characters =
                [
                    new Character
                    {
                        Name = "Kathryn Janeway",
                        Career = Career.Science,
                        Faction = Faction.Federation,
                        Level = 65,
                        ActiveShip = "Intrepid-class Science Vessel",
                        Builds =
                        [
                            new Build
                            {
                                Name = "Exotic Particle Build",
                                Type = BuildType.Space,
                                ShipName = "U.S.S. Voyager",
                                Notes = "EPG focused build"
                            },
                            new Build
                            {
                                Name = "Delta Ground",
                                Type = BuildType.Ground,
                                Notes = "Science ground kit build"
                            }
                        ]
                    },
                    new Character
                    {
                        Name = "Seven of Nine",
                        Career = Career.Tactical,
                        Faction = Faction.Federation,
                        Level = 65,
                        ActiveShip = "Defiant-class Tactical Escort",
                        Builds =
                        [
                            new Build
                            {
                                Name = "Cannon Scatter Volley",
                                Type = BuildType.Space,
                                ShipName = "U.S.S. Dark Horse",
                                Notes = "DEW cannon build"
                            }
                        ]
                    }
                ]
            },
            new Account
            {
                Gamertag = "Worf@warrior42",
                Nickname = "Worf",
                Notes = "KDF-focused account",
                Characters =
                [
                    new Character
                    {
                        Name = "Worf, Son of Mogh",
                        Career = Career.Tactical,
                        Faction = Faction.KlingonDefenseForce,
                        Level = 65,
                        ActiveShip = "Vor'cha-class Battle Cruiser",
                        Builds =
                        [
                            new Build
                            {
                                Name = "Beam Fire at Will",
                                Type = BuildType.Space,
                                ShipName = "I.K.S. Rotarran",
                                Notes = "FAW broadside build"
                            }
                        ]
                    },
                    new Character
                    {
                        Name = "K'Ehleyr",
                        Career = Career.Engineering,
                        Faction = Faction.KlingonDefenseForce,
                        Level = 65,
                        ActiveShip = "Bortasqu' War Cruiser",
                        Builds =
                        [
                            new Build
                            {
                                Name = "Tank Support",
                                Type = BuildType.Space,
                                ShipName = "I.K.S. Hammer",
                                Notes = "Threat tank build"
                            }
                        ]
                    }
                ]
            },
            new Account
            {
                Gamertag = "Sela@shadow99",
                Nickname = "Sela",
                Notes = "Romulan alt account",
                Characters =
                [
                    new Character
                    {
                        Name = "Sela",
                        Career = Career.Tactical,
                        Faction = Faction.RomulanRepublic,
                        Level = 65,
                        ActiveShip = "Scimitar Dreadnought Warbird",
                        Builds =
                        [
                            new Build
                            {
                                Name = "Surgical Strikes Torp",
                                Type = BuildType.Space,
                                ShipName = "R.R.W. Shadow",
                                Notes = "Torpedo + surgical strikes build"
                            },
                            new Build
                            {
                                Name = "Ground Assault",
                                Type = BuildType.Ground,
                                Notes = "Romulan operative ground build"
                            }
                        ]
                    }
                ]
            }
        ];
    }

    #endregion
}
