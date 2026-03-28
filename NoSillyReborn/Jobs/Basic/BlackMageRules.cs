using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class BlackMageRules : IJobRuleSet
{
    public JobID JobId => JobID.BlackMage;

    public IReadOnlyList<ActionRule> Rules { get; }

    public BlackMageRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.TriplecastPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Triplecast),
                "Triplecast → Blocked (Triplecast active)"),
        ];
    }
}
