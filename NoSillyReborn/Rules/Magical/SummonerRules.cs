using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Magical;

internal sealed class SummonerRules : JobRules
{
	public override Job Job => Job.SMN;

	public override Job BaseClass => Job.ACN;

	public override JobRole Role => JobRole.RangedMagical;

	protected override GuardRule[] Define() =>
	[
		KeepCharge(ActionID.RadiantAegisPvE, "Radiant Aegis", [StatusID.RadiantAegis]),
		Block("aetherflow-overcap", "Spend Aetherflow first",
			"Blocks Energy Drain and Energy Siphon while you still have Aetherflow stacks, since the new stacks would replace them.",
			"Aetherflow stacks are still unspent", [ActionID.EnergyDrainPvE_16508, ActionID.EnergySiphonPvE],
			_ => Gauge<SMNGauge>().AetherflowStacks > 0),
		Block("keep-attunement", "Spend primal attunements first",
			"Blocks summoning another primal while you still have Elemental Attunement charges or an Astral Flow favor active. Off by default.",
			"you still have primal attunements left",
			[
				ActionID.SummonRubyPvE, ActionID.SummonTopazPvE, ActionID.SummonEmeraldPvE,
				ActionID.SummonIfritPvE, ActionID.SummonTitanPvE, ActionID.SummonGarudaPvE,
				ActionID.SummonIfritIiPvE, ActionID.SummonTitanIiPvE, ActionID.SummonGarudaIiPvE,
			],
			_ => Gauge<SMNGauge>().AttunementCount > 0
				|| Self(StatusID.IfritsFavor, StatusID.CrimsonStrikeReady, StatusID.CrimsonStrikeReady_4403, StatusID.TitansFavor, StatusID.GarudasFavor),
			false),
		DoubleRaise(ActionID.ResurrectionPvE, "Resurrection"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
