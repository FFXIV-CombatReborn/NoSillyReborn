using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Melee;

internal sealed class MonkRules : JobRules
{
	public override Job Job => Job.MNK;

	public override Job BaseClass => Job.PGL;

	public override JobRole Role => JobRole.Melee;

	protected override GuardRule[] Define() =>
	[
		Block("preserve-form", "Keep your Raptor or Coeurl form",
			"Blocks Opo-opo weaponskills while you are in Raptor or Coeurl form without Perfect Balance or Formless Fist, so a stray press doesn't drop your form. Off by default.",
			"you're in Raptor or Coeurl form",
			[ActionID.BootshinePvE, ActionID.DragonKickPvE, ActionID.ArmOfTheDestroyerPvE, ActionID.ShadowOfTheDestroyerPvE, ActionID.LeapingOpoPvE],
			_ => Self(StatusID.RaptorForm, StatusID.CoeurlForm) && !Self(StatusID.PerfectBalance, StatusID.FormlessFist), false),
		Replace("dragon-kick-for-fury", "Dragon Kick when Opo-opo's Fury is empty",
			"Turns Bootshine or Leaping Opo into Dragon Kick when you have no Opo-opo's Fury stacks.",
			"Opo-opo's Fury is empty", [ActionID.BootshinePvE, ActionID.LeapingOpoPvE],
			ActionID.DragonKickPvE, ActionID.DragonKickPvE,
			_ => Gauge<MNKGauge>().OpoOpoFury == 0),
		Replace("leaping-opo-on-fury", "Leaping Opo when Opo-opo's Fury is ready",
			"Turns Dragon Kick into Bootshine or Leaping Opo while you already hold Opo-opo's Fury.",
			"Opo-opo's Fury is ready", [ActionID.DragonKickPvE],
			ActionID.BootshinePvE, ActionID.LeapingOpoPvE,
			_ => Gauge<MNKGauge>().OpoOpoFury > 0),
		Replace("twin-snakes-for-fury", "Twin Snakes when Raptor's Fury is empty",
			"Turns True Strike or Rising Raptor into Twin Snakes when you have no Raptor's Fury stacks.",
			"Raptor's Fury is empty", [ActionID.TrueStrikePvE, ActionID.RisingRaptorPvE],
			ActionID.TwinSnakesPvE, ActionID.TwinSnakesPvE,
			_ => Gauge<MNKGauge>().RaptorFury == 0),
		Replace("rising-raptor-on-fury", "Rising Raptor when Raptor's Fury is ready",
			"Turns Twin Snakes into True Strike or Rising Raptor while you already hold Raptor's Fury.",
			"Raptor's Fury is ready", [ActionID.TwinSnakesPvE],
			ActionID.TrueStrikePvE, ActionID.RisingRaptorPvE,
			_ => Gauge<MNKGauge>().RaptorFury > 0),
		Replace("demolish-for-fury", "Demolish when Coeurl's Fury is empty",
			"Turns Snap Punch or Pouncing Coeurl into Demolish when you have no Coeurl's Fury stacks.",
			"Coeurl's Fury is empty", [ActionID.SnapPunchPvE, ActionID.PouncingCoeurlPvE],
			ActionID.DemolishPvE, ActionID.DemolishPvE,
			_ => Gauge<MNKGauge>().CoeurlFury == 0),
		Replace("pouncing-coeurl-on-fury", "Pouncing Coeurl when Coeurl's Fury is ready",
			"Turns Demolish into Snap Punch or Pouncing Coeurl while you already hold Coeurl's Fury.",
			"Coeurl's Fury is ready", [ActionID.DemolishPvE],
			ActionID.SnapPunchPvE, ActionID.PouncingCoeurlPvE,
			_ => Gauge<MNKGauge>().CoeurlFury > 0),
		Block("keep-charge", "Keep your second Perfect Balance charge",
			"Blocks Perfect Balance while Perfect Balance, Formless Fist, or an active Masterful Blitz window is already up, so a double press doesn't burn both charges.",
			"it's already active", ActionID.PerfectBalancePvE,
			_ => Self(StatusID.PerfectBalance, StatusID.FormlessFist) || Gauge<MNKGauge>().BlitzTimeRemaining > 0 || JustUsed(ActionID.PerfectBalancePvE)),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
