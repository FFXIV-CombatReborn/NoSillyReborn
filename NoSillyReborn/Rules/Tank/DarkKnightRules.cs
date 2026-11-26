namespace NoSillyReborn.Rules.Tank;

internal sealed class DarkKnightRules : JobRules
{
	public override Job Job => Job.DRK;

	public override JobRole Role => JobRole.Tank;

	protected override GuardRule[] Define() =>
	[
		ComboStep("combo-syphon-strike", "Continue to Syphon Strike",
			"Turns Hard Slash or Souleater into Syphon Strike right after Hard Slash.",
			"Hard Slash was just used", [ActionID.HardSlashPvE, ActionID.SouleaterPvE],
			ActionID.SyphonStrikePvE, ActionID.SyphonStrikePvE, ActionID.HardSlashPvE),
		ComboStep("combo-souleater", "Continue to Souleater",
			"Turns Hard Slash or Syphon Strike into Souleater right after Syphon Strike.",
			"Syphon Strike was just used", [ActionID.HardSlashPvE, ActionID.SyphonStrikePvE],
			ActionID.SouleaterPvE, ActionID.SouleaterPvE, ActionID.SyphonStrikePvE),
		ComboStep("combo-stalwart-soul", "Continue to Stalwart Soul",
			"Turns Unleash into Stalwart Soul right after Unleash.",
			"Unleash was just used", [ActionID.UnleashPvE],
			ActionID.StalwartSoulPvE, ActionID.StalwartSoulPvE, ActionID.UnleashPvE),
		KeepTargetCharge("keep-oblation", ActionID.OblationPvE, "Oblation", [StatusID.Oblation]),
		PreserveCombo(),
		KeepStance(ActionID.ReleaseGritPvE, "Grit"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
