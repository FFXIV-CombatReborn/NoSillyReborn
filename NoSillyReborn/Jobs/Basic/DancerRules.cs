using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class DancerRules : IJobRuleSet
{
    public JobID JobId => JobID.Dancer;

    public IReadOnlyList<ActionRule> Rules { get; }

    public DancerRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.ShieldSambaPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.ShieldSamba, StatusID.Tactician_1951, StatusID.Troubadour),
                "Shield Samba → Blocked (Physranged mit already active)"),
        ];
    }
}
