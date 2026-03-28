using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class WhiteMageRules : IJobRuleSet
{
    public JobID JobId => JobID.WhiteMage;

    public IReadOnlyList<ActionRule> Rules { get; }

    public WhiteMageRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.ThinAirPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.ThinAir),
                "Thin Air → Blocked (Thin Air active)"),
        ];
    }
}
