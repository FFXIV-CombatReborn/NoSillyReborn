using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class DragoonRules : IJobRuleSet
{
    public JobID JobId => JobID.Dragoon;

    public IReadOnlyList<ActionRule> Rules { get; }

    public DragoonRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.LifeSurgePvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.LifeSurge),
                "Life Surge → Blocked (Life Surge active)"),
        ];
    }
}
