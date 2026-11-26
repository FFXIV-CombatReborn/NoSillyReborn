using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Tank;

internal sealed class WarriorRules : JobRules
{
	public override Job Job => Job.WAR;

	public override Job BaseClass => Job.MRD;

	public override JobRole Role => JobRole.Tank;

	protected override GuardRule[] Define() =>
	[
		ComboStep("combo-maim", "Continue to Maim",
			"Turns Heavy Swing, Storm's Path, or Storm's Eye into Maim right after Heavy Swing.",
			"Heavy Swing was just used", [ActionID.HeavySwingPvE, ActionID.StormsPathPvE, ActionID.StormsEyePvE],
			ActionID.MaimPvE, ActionID.MaimPvE, ActionID.HeavySwingPvE),
		Replace("combo-storms-eye", "Keep Surging Tempest up",
			"Turns Heavy Swing or Maim into Storm's Eye after Maim when Surging Tempest has under 10 seconds left.",
			"Surging Tempest needs refreshing", [ActionID.HeavySwingPvE, ActionID.MaimPvE],
			ActionID.StormsEyePvE, ActionID.StormsEyePvE,
			_ => ComboIs(ActionID.MaimPvE) && ActionID.StormsEyePvE.IsUnlocked()
				&& StatusHelper.PlayerStatusTime(true, StatusID.SurgingTempest) < 10f),
		ComboStep("combo-storms-path", "Continue to Storm's Path",
			"Turns Heavy Swing or Maim into Storm's Path right after Maim.",
			"Maim was just used", [ActionID.HeavySwingPvE, ActionID.MaimPvE],
			ActionID.StormsPathPvE, ActionID.StormsPathPvE, ActionID.MaimPvE),
		ComboStep("combo-mythril-tempest", "Continue to Mythril Tempest",
			"Turns Overpower into Mythril Tempest right after Overpower.",
			"Overpower was just used", [ActionID.OverpowerPvE],
			ActionID.MythrilTempestPvE, ActionID.MythrilTempestPvE, ActionID.OverpowerPvE),
		KeepCharge(ActionID.InfuriatePvE, "Infuriate", [StatusID.NascentChaos],
			"Blocks Infuriate while Nascent Chaos is still active, so a double press doesn't waste the proc or burn both charges."),
		Block("beast-gauge-overcap", "Spend Beast Gauge first",
			"Blocks Infuriate above 50 Beast Gauge, since the 50 gauge it grants would overcap.",
			"Beast Gauge would overcap", ActionID.InfuriatePvE,
			_ => Gauge<WARGauge>().BeastGauge > 50),
		PreserveCombo(),
		KeepStance(ActionID.ReleaseDefiancePvE, "Defiance"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
