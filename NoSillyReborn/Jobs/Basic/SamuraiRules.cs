using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class SamuraiRules : IJobRuleSet
{
    public JobID JobId => JobID.Samurai;

    public IReadOnlyList<ActionRule> Rules { get; }

    public SamuraiRules()
    {
        Rules =
        [
            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.MeikyoShisuiPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.MeikyoShisui),
                "Meikyo Shisui → Blocked (Meikyo Shisui active)"),
        ];
    }
}
