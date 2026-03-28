using NoSillyReborn.Data;

namespace NoSillyReborn.Jobs.Basic;

public sealed class BardRules : IJobRuleSet
{
    public JobID JobId => JobID.Bard;

    public IReadOnlyList<ActionRule> Rules { get; }

    public BardRules()
    {
        Rules =
        [
            // ── GCD Replacements ─────────────────────────────────────────

            new ActionRule(
                ActionID.BurstShotPvE,
                ActionID.RefulgentArrowPvE,
                () => StatusHelper.PlayerHasStatus(true, StatusID.HawksEye_3861),
                "Burst Shot → Refulgent Arrow (Hawk's Eye active)"),

            new ActionRule(
                ActionID.WideVolleyPvE,
                ActionID.ShadowbitePvE,
                () => StatusHelper.PlayerHasStatus(true, StatusID.HawksEye_3861),
                "Wide Volley → Shadowbite (Hawk's Eye active)"),

            new ActionRule(
                ActionID.BurstShotPvE,
                ActionID.RefulgentArrowPvE,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Barrage),
                "Burst Shot → Refulgent Arrow (Barrage active)"),

            new ActionRule(
                ActionID.WideVolleyPvE,
                ActionID.ShadowbitePvE,
                () => StatusHelper.PlayerHasStatus(true, StatusID.Barrage),
                "Wide Volley → Shadowbite (Barrage active)"),

            new ActionRule(
                ActionID.ApexArrowPvE,
                ActionID.BlastArrowPvE,
                () => StatusHelper.PlayerHasStatus(true, StatusID.BlastArrowReady),
                "Apex Arrow → Blast Arrow (Blast Arrow Ready active)"),

            // ── oGCD Intercepts ─────────────────────────────────
            new ActionRule(
                ActionID.TroubadourPvE,
                0,
                () => StatusHelper.PlayerHasStatus(true, StatusID.ShieldSamba, StatusID.Tactician_1951, StatusID.Troubadour),
                "Troubadour → Blocked (Physranged mit already active)"),
        ];
    }
}
