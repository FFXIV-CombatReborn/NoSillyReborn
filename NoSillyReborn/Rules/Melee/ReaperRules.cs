using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Melee;

internal sealed class ReaperRules : JobRules
{
	public override Job Job => Job.RPR;

	public override JobRole Role => JobRole.Melee;

	protected override GuardRule[] Define() =>
	[
		Block("protect-soul-reaver", "Spend Soul Reaver and Executioner first",
			"Blocks normal weaponskills and spells while Soul Reaver or Executioner is active, so you don't drop your stacks before spending them.",
			"Soul Reaver or Executioner is still up",
			[
				ActionID.SlicePvE, ActionID.WaxingSlicePvE, ActionID.InfernalSlicePvE,
				ActionID.SpinningScythePvE, ActionID.NightmareScythePvE, ActionID.ShadowOfDeathPvE, ActionID.WhorlOfDeathPvE,
				ActionID.SoulSlicePvE, ActionID.SoulScythePvE, ActionID.HarpePvE, ActionID.HarvestMoonPvE,
			],
			_ => Self(StatusID.SoulReaver, StatusID.SoulReaver_2854, StatusID.Executioner)),
		Block("keep-soul-reaver", "Don't overwrite Soul Reaver or Executioner",
			"Blocks Blood Stalk, Unveiled Gibbet/Gallows, Grim Swathe, and Gluttony while Soul Reaver or Executioner is already active.",
			"Soul Reaver or Executioner is already active",
			[ActionID.BloodStalkPvE, ActionID.UnveiledGibbetPvE, ActionID.UnveiledGallowsPvE, ActionID.GrimSwathePvE, ActionID.GluttonyPvE],
			_ => Self(StatusID.SoulReaver, StatusID.SoulReaver_2854, StatusID.Executioner)),
		ComboStep("combo-waxing-slice", "Continue to Waxing Slice",
			"Turns Slice or Infernal Slice into Waxing Slice right after Slice.",
			"Slice was just used", [ActionID.SlicePvE, ActionID.InfernalSlicePvE],
			ActionID.WaxingSlicePvE, ActionID.WaxingSlicePvE, ActionID.SlicePvE),
		ComboStep("combo-infernal-slice", "Continue to Infernal Slice",
			"Turns Slice or Waxing Slice into Infernal Slice right after Waxing Slice.",
			"Waxing Slice was just used", [ActionID.SlicePvE, ActionID.WaxingSlicePvE],
			ActionID.InfernalSlicePvE, ActionID.InfernalSlicePvE, ActionID.WaxingSlicePvE),
		ComboStep("combo-nightmare-scythe", "Continue to Nightmare Scythe",
			"Turns Spinning Scythe into Nightmare Scythe right after Spinning Scythe.",
			"Spinning Scythe was just used", [ActionID.SpinningScythePvE],
			ActionID.NightmareScythePvE, ActionID.NightmareScythePvE, ActionID.SpinningScythePvE),
		Replace("enhanced-gibbet", "Use Enhanced Gibbet",
			"Turns Gallows or Executioner's Gallows into Gibbet or Executioner's Gibbet while Enhanced Gibbet is up.",
			"Enhanced Gibbet is up", [ActionID.GallowsPvE, ActionID.ExecutionersGallowsPvE],
			ActionID.GibbetPvE, ActionID.GibbetPvE,
			_ => Self(StatusID.EnhancedGibbet, StatusID.EnhancedGibbet_2855) && !Self(StatusID.Enshrouded, StatusID.Enshrouded_2863)),
		Replace("enhanced-gallows", "Use Enhanced Gallows",
			"Turns Gibbet or Executioner's Gibbet into Gallows or Executioner's Gallows while Enhanced Gallows is up.",
			"Enhanced Gallows is up", [ActionID.GibbetPvE, ActionID.ExecutionersGibbetPvE],
			ActionID.GallowsPvE, ActionID.GallowsPvE,
			_ => Self(StatusID.EnhancedGallows) && !Self(StatusID.Enshrouded, StatusID.Enshrouded_2863)),
		Replace("enhanced-void-reaping", "Use Enhanced Void Reaping",
			"Turns Cross Reaping into Void Reaping while Enhanced Void Reaping is up.",
			"Enhanced Void Reaping is up", [ActionID.CrossReapingPvE],
			ActionID.GibbetPvE, ActionID.VoidReapingPvE,
			_ => Self(StatusID.Enshrouded, StatusID.Enshrouded_2863) && Self(StatusID.EnhancedVoidReaping, StatusID.EnhancedVoidReaping_2857)),
		Replace("enhanced-cross-reaping", "Use Enhanced Cross Reaping",
			"Turns Void Reaping into Cross Reaping while Enhanced Cross Reaping is up.",
			"Enhanced Cross Reaping is up", [ActionID.VoidReapingPvE],
			ActionID.GallowsPvE, ActionID.CrossReapingPvE,
			_ => Self(StatusID.Enshrouded, StatusID.Enshrouded_2863) && Self(StatusID.EnhancedCrossReaping)),
		Block("soul-gauge-overcap", "Spend Soul Gauge first",
			"Blocks Soul Slice and Soul Scythe above 50 Soul Gauge, since the 50 Soul they grant would overcap.",
			"Soul Gauge would overcap", [ActionID.SoulSlicePvE, ActionID.SoulScythePvE],
			_ => Gauge<RPRGauge>().Soul > 50),
		PreserveCombo(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
