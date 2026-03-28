using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class MachinistRules : IJobRuleSet
{
    public JobID JobId => JobID.Machinist;

    public IReadOnlyList<ActionRule> Rules { get; }

    public MachinistRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.ReassemblePvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Reassembled),
                "Reassemble → Blocked (Reassembled active)"),

            new ActionRule(
                ActionID.TacticianPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.ShieldSamba, StatusID.Tactician_1951, StatusID.Troubadour),
                "Tactician → Blocked (Physranged mit already active)"),
        ];
    }
}
