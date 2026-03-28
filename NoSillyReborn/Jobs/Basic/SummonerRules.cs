using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class SummonerRules : IJobRuleSet
{
    public JobID JobId => JobID.Summoner;

    public IReadOnlyList<ActionRule> Rules { get; }

    public SummonerRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.RadiantAegisPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.RadiantAegis),
                "Radiant Aegis → Blocked (Radiant Aegis active)"),
        ];
    }
}
