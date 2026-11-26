using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Ranged;

internal sealed class BardRules : JobRules
{
	public override Job Job => Job.BRD;

	public override Job BaseClass => Job.ARC;

	public override JobRole Role => JobRole.RangedPhysical;

	protected override GuardRule[] Define() =>
	[
		Replace("refulgent-arrow", "Refulgent Arrow on Hawk's Eye",
			"Turns Heavy Shot and Burst Shot into Straight Shot or Refulgent Arrow while Hawk's Eye or Barrage is up.",
			"Hawk's Eye or Barrage is up", [ActionID.HeavyShotPvE, ActionID.BurstShotPvE], ActionID.StraightShotPvE, ActionID.RefulgentArrowPvE,
			_ => Self(StatusID.HawksEye_3861, StatusID.Barrage)),
		Replace("shadowbite", "Shadowbite on Hawk's Eye",
			"Turns Quick Nock and Ladonsbite into Wide Volley or Shadowbite while Hawk's Eye or Barrage is up.",
			"Hawk's Eye or Barrage is up", [ActionID.QuickNockPvE, ActionID.LadonsbitePvE], ActionID.WideVolleyPvE, ActionID.ShadowbitePvE,
			_ => Self(StatusID.HawksEye_3861, StatusID.Barrage)),
		Block("keep-song", "Don't clip a fresh song",
			"Blocks Mage's Ballad, Army's Paeon, and The Wanderer's Minuet when a song has more than 42 seconds remaining.",
			"a fresh song is still playing", [ActionID.MagesBalladPvE, ActionID.ArmysPaeonPvE, ActionID.TheWanderersMinuetPvE],
			_ => Gauge<BRDGauge>().Song != 0 && Gauge<BRDGauge>().SongTimer > 42000),
		MitigationOverlap(ActionID.TroubadourPvE, "Troubadour"),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
