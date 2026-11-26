namespace NoSillyReborn.Rules.Tank;

internal sealed class GunbreakerRules : JobRules
{
	public override Job Job => Job.GNB;

	public override JobRole Role => JobRole.Tank;

	protected override GuardRule[] Define() =>
	[
		ComboStep("combo-brutal-shell", "Continue to Brutal Shell",
			"Turns Keen Edge or Solid Barrel into Brutal Shell right after Keen Edge.",
			"Keen Edge was just used", [ActionID.KeenEdgePvE, ActionID.SolidBarrelPvE],
			ActionID.BrutalShellPvE, ActionID.BrutalShellPvE, ActionID.KeenEdgePvE),
		ComboStep("combo-solid-barrel", "Continue to Solid Barrel",
			"Turns Keen Edge or Brutal Shell into Solid Barrel right after Brutal Shell.",
			"Brutal Shell was just used", [ActionID.KeenEdgePvE, ActionID.BrutalShellPvE],
			ActionID.SolidBarrelPvE, ActionID.SolidBarrelPvE, ActionID.BrutalShellPvE),
		ComboStep("combo-demon-slaughter", "Continue to Demon Slaughter",
			"Turns Demon Slice into Demon Slaughter right after Demon Slice.",
			"Demon Slice was just used", [ActionID.DemonSlicePvE],
			ActionID.DemonSlaughterPvE, ActionID.DemonSlaughterPvE, ActionID.DemonSlicePvE),
		KeepTargetCharge("keep-aurora", ActionID.AuroraPvE, "Aurora", [StatusID.Aurora, StatusID.Aurora_2065]),
		PreserveCombo(),
		KeepStance(ActionID.ReleaseRoyalGuardPvE, "Royal Guard"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
