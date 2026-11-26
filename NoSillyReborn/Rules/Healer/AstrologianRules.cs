using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Healer;

internal sealed class AstrologianRules : JobRules
{
	public override Job Job => Job.AST;

	public override JobRole Role => JobRole.Healer;

	protected override GuardRule[] Define() =>
	[
		KeepCharge(ActionID.LightspeedPvE, "Lightspeed", [StatusID.Lightspeed]),
		KeepTargetCharge("keep-celestial-intersection", ActionID.CelestialIntersectionPvE, "Celestial Intersection", [StatusID.Intersection, StatusID.Intersection_4040]),
		Block("play-card-first", "Play your damage card first",
			"Blocks Astral Draw and Umbral Draw in combat while you still hold The Balance or The Spear, so you don't overwrite an unplayed damage buff.",
			"a damage card is still unplayed", [ActionID.AstralDrawPvE, ActionID.UmbralDrawPvE],
			_ => InCombat && Gauge<ASTGauge>().Card1 is CardType.Balance or CardType.Spear),
		DoubleRaise(ActionID.AscendPvE, "Ascend"),
		BlockRescue(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
