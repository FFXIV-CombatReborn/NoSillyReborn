namespace NoSillyReborn.Rules.Healer;

internal sealed class WhiteMageRules : JobRules
{
	public override Job Job => Job.WHM;

	public override Job BaseClass => Job.CNJ;

	public override JobRole Role => JobRole.Healer;

	protected override GuardRule[] Define() =>
	[
		KeepCharge(ActionID.ThinAirPvE, "Thin Air", [StatusID.ThinAir]),
		KeepTargetCharge("keep-divine-benison", ActionID.DivineBenisonPvE, "Divine Benison", [StatusID.DivineBenison, StatusID.DivineBenison_1404]),
		DoubleRaise(ActionID.RaisePvE, "Raise"),
		BlockRescue(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
