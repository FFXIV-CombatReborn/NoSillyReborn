using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class RedMageRules : IJobRuleSet
{
    public JobID JobId => JobID.RedMage;

    public IReadOnlyList<ActionRule> Rules { get; }

    public RedMageRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.AccelerationPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Acceleration, StatusID.GrandImpactReady),
                "Acceleration → Blocked (Acceleration or Grand Impact Ready active)"),
        ];
    }
}
