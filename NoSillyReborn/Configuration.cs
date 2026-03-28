using Dalamud.Configuration;
using NoSillyReborn.Jobs;

namespace NoSillyReborn;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    /// <summary>Master switch – when false the intercept hook is installed but no replacements fire.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Set of ClassJob row IDs for which interception is active.
    /// Populated by <see cref="InitialiseDefaults"/> from the registered rule-sets.
    /// </summary>
    public HashSet<uint> EnabledJobs { get; set; } = [];

    /// <summary>Whether the config window can be freely repositioned.</summary>
    public bool IsConfigWindowMovable { get; set; } = true;

    /// <summary>
    /// Fills <see cref="EnabledJobs"/> with all registered jobs the first time the
    /// plugin loads (or if the set is empty after a reset).
    /// </summary>
    public void InitialiseDefaults()
    {
        if (EnabledJobs.Count == 0)
        {
            foreach (var ruleSet in JobRuleRegistry.AllRuleSets)
                EnabledJobs.Add((uint)ruleSet.JobId);
        }
    }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}

