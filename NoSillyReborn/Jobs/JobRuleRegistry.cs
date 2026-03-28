using NoSillyReborn.Data;
using NoSillyReborn.Jobs.Basic;

namespace NoSillyReborn.Jobs;

/// <summary>
/// Central registry that maps a <see cref="JobID"/> to its <see cref="IJobRuleSet"/>.
/// Add new job rule-sets here to register them with the intercept system.
/// </summary>
public static class JobRuleRegistry
{
    private static readonly Dictionary<uint, IJobRuleSet> Registry = [];

    static JobRuleRegistry()
    {
        Register(new AstrologianRules());
        Register(new BardRules());
        Register(new BlackMageRules());
        Register(new DancerRules());
        Register(new DragoonRules());
        Register(new MachinistRules());
        Register(new RedMageRules());
        Register(new SamuraiRules());
        Register(new SummonerRules());
        Register(new WhiteMageRules());
    }

    private static void Register(IJobRuleSet ruleSet)
    {
        Registry[(uint)ruleSet.JobId] = ruleSet;
        var baseClassId = GetBaseClassId(ruleSet.JobId);
        if (baseClassId <= 0U)
            return;
        Registry[baseClassId] = ruleSet;
    }

    /// <summary>
    /// Returns the rule-set for the given ClassJob row ID, or <c>null</c> if
    /// no rules are registered for that job.
    /// </summary>
    public static IJobRuleSet? GetRulesForJob(uint classJobId)
    {
        Registry.TryGetValue(classJobId, out var rulesForJob);
        return rulesForJob;
    }

    /// <summary>
    /// Returns all registered rule-sets (one per job, base classes de-duplicated).
    /// </summary>
    public static IEnumerable<IJobRuleSet> AllRuleSets
    {
        get => Registry.Values.Distinct<IJobRuleSet>();
    }

    private static uint GetBaseClassId(JobID jobId) =>
        jobId switch
        {
            JobID.Bard => (uint)JobID.Archer,
            JobID.Dragoon => (uint)JobID.Lancer,
            JobID.Monk => (uint)JobID.Pugilist,
            JobID.Ninja => (uint)JobID.Rogue,
            JobID.Samurai => 0,
            JobID.Warrior => (uint)JobID.Marauder,
            JobID.Paladin => (uint)JobID.Gladiator,
            JobID.WhiteMage => (uint)JobID.Conjurer,
            JobID.BlackMage => (uint)JobID.Thaumaturge,
            JobID.Summoner => (uint)JobID.Arcanist,
            _ => 0
        };
}
