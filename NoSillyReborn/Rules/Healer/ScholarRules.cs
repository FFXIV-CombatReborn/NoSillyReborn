using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Healer;

internal sealed class ScholarRules : JobRules
{
	public override Job Job => Job.SCH;

	public override JobRole Role => JobRole.Healer;

	protected override GuardRule[] Define() =>
	[
		Block("aetherflow-overcap", "Spend Aetherflow first",
			"Blocks Aetherflow while you still hold unspent Aetherflow stacks, since the new stacks would overwrite them.",
			"Aetherflow stacks are still unspent", ActionID.AetherflowPvE,
			_ => Gauge<SCHGauge>().Aetherflow > 0),
		Block("keep-emergency-tactics", "Don't double Emergency Tactics",
			"Blocks Emergency Tactics while its effect is still waiting on your next shield spell.",
			"Emergency Tactics is already active", [ActionID.EmergencyTacticsPvE, ActionID.EmergencyTacticsPvE_37037],
			_ => Self(StatusID.EmergencyTactics) || JustUsed(ActionID.EmergencyTacticsPvE) || JustUsed(ActionID.EmergencyTacticsPvE_37037)),
		DoubleRaise(ActionID.ResurrectionPvE, "Resurrection"),
		BlockRescue(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
