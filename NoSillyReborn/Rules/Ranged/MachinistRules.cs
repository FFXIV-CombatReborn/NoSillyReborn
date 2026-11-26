using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Ranged;

internal sealed class MachinistRules : JobRules
{
	public override Job Job => Job.MCH;

	public override JobRole Role => JobRole.RangedPhysical;

	protected override GuardRule[] Define() =>
	[
		ComboStep("combo-slug-shot", "Continue to Heated Slug Shot",
			"Turns Split Shot or Clean Shot into Slug Shot or Heated Slug Shot right after Split Shot.",
			"Split Shot was just used",
			[ActionID.SplitShotPvE, ActionID.HeatedSplitShotPvE, ActionID.CleanShotPvE, ActionID.HeatedCleanShotPvE],
			ActionID.SlugShotPvE, ActionID.HeatedSlugShotPvE,
			ActionID.SplitShotPvE, ActionID.HeatedSplitShotPvE),
		ComboStep("combo-clean-shot", "Continue to Heated Clean Shot",
			"Turns Split Shot or Slug Shot into Clean Shot or Heated Clean Shot right after Slug Shot.",
			"Slug Shot was just used",
			[ActionID.SplitShotPvE, ActionID.HeatedSplitShotPvE, ActionID.SlugShotPvE, ActionID.HeatedSlugShotPvE],
			ActionID.CleanShotPvE, ActionID.HeatedCleanShotPvE,
			ActionID.SlugShotPvE, ActionID.HeatedSlugShotPvE),
		KeepCharge(ActionID.ReassemblePvE, "Reassemble", [StatusID.Reassembled]),
		Block("keep-overheated", "Don't clip Overheated",
			"Blocks Hypercharge while Overheated is already active.",
			"Overheated is already active", ActionID.HyperchargePvE,
			_ => Gauge<MCHGauge>().IsOverheated || Self(StatusID.Overheated, StatusID.Overheated_3149) || JustUsed(ActionID.HyperchargePvE)),
		Block("keep-hypercharged", "Spend Hypercharged first",
			"Blocks Barrel Stabilizer while Hypercharged is still active.",
			"Hypercharged is already active", ActionID.BarrelStabilizerPvE,
			_ => Self(StatusID.Hypercharged)),
		MitigationOverlap(ActionID.TacticianPvE, "Tactician"),
		PreserveCombo(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
