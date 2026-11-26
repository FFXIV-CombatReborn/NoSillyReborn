namespace NoSillyReborn.Rules.Tank;

internal sealed class PaladinRules : JobRules
{
	public override Job Job => Job.PLD;

	public override Job BaseClass => Job.GLA;

	public override JobRole Role => JobRole.Tank;

	protected override GuardRule[] Define() =>
	[
		ComboStep("combo-riot-blade", "Continue to Riot Blade",
			"Turns Fast Blade, Rage of Halone, or Royal Authority into Riot Blade right after Fast Blade.",
			"Fast Blade was just used", [ActionID.FastBladePvE, ActionID.RageOfHalonePvE, ActionID.RoyalAuthorityPvE],
			ActionID.RiotBladePvE, ActionID.RiotBladePvE, ActionID.FastBladePvE),
		ComboStep("combo-royal-authority", "Continue to Royal Authority",
			"Turns Fast Blade or Riot Blade into Rage of Halone or Royal Authority right after Riot Blade.",
			"Riot Blade was just used", [ActionID.FastBladePvE, ActionID.RiotBladePvE],
			ActionID.RageOfHalonePvE, ActionID.RoyalAuthorityPvE, ActionID.RiotBladePvE),
		ComboStep("combo-prominence", "Continue to Prominence",
			"Turns Total Eclipse into Prominence right after Total Eclipse.",
			"Total Eclipse was just used", [ActionID.TotalEclipsePvE],
			ActionID.ProminencePvE, ActionID.ProminencePvE, ActionID.TotalEclipsePvE),
		Block("keep-sheltron", "Don't overlap Sheltron",
			"Blocks Sheltron and Holy Sheltron while their mitigation buff is still active.",
			"Sheltron is already active", [ActionID.SheltronPvE, ActionID.HolySheltronPvE],
			_ => Self(StatusID.Sheltron, StatusID.Sheltron_1856, StatusID.HolySheltron)
				|| JustUsed(ActionID.SheltronPvE) || JustUsed(ActionID.HolySheltronPvE)),
		PreserveCombo(),
		KeepStance(ActionID.ReleaseIronWillPvE, "Iron Will"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
