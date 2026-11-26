using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Magical;

internal sealed class BlackMageRules : JobRules
{
	public override Job Job => Job.BLM;

	public override Job BaseClass => Job.THM;

	public override JobRole Role => JobRole.RangedMagical;

	protected override GuardRule[] Define() =>
	[
		KeepCharge(ActionID.TriplecastPvE, "Triplecast", [StatusID.Triplecast]),
		KeepCharge("keep-ley-lines", ActionID.LeyLinesPvE, "Ley Lines", [StatusID.LeyLines],
			"Blocks Ley Lines while your circle of power is already active, so a double press doesn't burn both charges."),
		Block("polyglot-overcap", "Spend Polyglot first",
			"Blocks Amplifier while your Polyglot stacks are already full.",
			"Polyglot is already full", ActionID.AmplifierPvE,
			_ => Gauge<BLMGauge>().PolyglotStacks >= (TraitID.EnhancedPolyglotIi.IsUnlocked() ? 3 : TraitID.EnhancedPolyglot.IsUnlocked() ? 2 : 1)),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
