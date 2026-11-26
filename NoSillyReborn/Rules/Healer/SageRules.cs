using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Healer;

internal sealed class SageRules : JobRules
{
	private const int MaxAddersgall = 3;

	public override Job Job => Job.SGE;

	public override JobRole Role => JobRole.Healer;

	protected override GuardRule[] Define() =>
	[
		Block("addersgall-overcap", "Don't overcap Addersgall",
			"Blocks Rhizomata while you already hold 3 Addersgall, since the stack it grants would be lost.",
			"Addersgall is already full", ActionID.RhizomataPvE,
			_ => Gauge<SGEGauge>().Addersgall >= MaxAddersgall),
		DoubleRaise(ActionID.EgeiroPvE, "Egeiro"),
		BlockRescue(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
