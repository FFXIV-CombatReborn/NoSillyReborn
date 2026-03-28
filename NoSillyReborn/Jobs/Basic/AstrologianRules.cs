using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class AstrologianRules : IJobRuleSet
{
    public JobID JobId => JobID.Astrologian;

    public IReadOnlyList<ActionRule> Rules { get; }

    public AstrologianRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.LightspeedPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Lightspeed),
                "Lightspeed → Blocked (Lightspeed active)"),
        ];
    }
}
