namespace NoSillyReborn.Data;

/// <summary>
/// FFXIV job (ClassJob) row IDs.
/// </summary>
public enum JobID : uint
{
    None = 0,

    // Tanks
    Gladiator = 1,
    Paladin = 19,
    Marauder = 3,
    Warrior = 21,
    DarkKnight = 32,
    Gunbreaker = 37,

    // Healers
    Conjurer = 6,
    WhiteMage = 24,
    Scholar = 28,
    Astrologian = 33,
    Sage = 40,

    // Melee DPS
    Pugilist = 2,
    Monk = 20,
    Lancer = 4,
    Dragoon = 22,
    Rogue = 29,
    Ninja = 30,
    Samurai = 34,
    Reaper = 39,
    Viper = 41,

    // Ranged Physical DPS
    Archer = 5,
    Bard = 23,
    Machinist = 31,
    Dancer = 38,

    // Ranged Magical DPS
    Thaumaturge = 7,
    BlackMage = 25,
    Arcanist = 26,
    Summoner = 27,
    RedMage = 35,
    Pictomancer = 42,
    BlueMage = 36,
}
