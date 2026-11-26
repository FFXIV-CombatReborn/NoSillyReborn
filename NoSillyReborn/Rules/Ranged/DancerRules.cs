using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Ranged;

internal sealed class DancerRules : JobRules
{
	public override Job Job => Job.DNC;

	public override JobRole Role => JobRole.RangedPhysical;

	protected override GuardRule[] Define() =>
	[
		Replace("combo-fountain", "Continue to Fountain",
			"Turns Cascade into Fountain right after Cascade.",
			"Cascade was just used", [ActionID.CascadePvE],
			ActionID.FountainPvE, ActionID.FountainPvE,
			_ => !Gauge<DNCGauge>().IsDancing && ComboIs(ActionID.CascadePvE)),
		Replace("combo-bladeshower", "Continue to Bladeshower",
			"Turns Windmill into Bladeshower right after Windmill.",
			"Windmill was just used", [ActionID.WindmillPvE],
			ActionID.BladeshowerPvE, ActionID.BladeshowerPvE,
			_ => !Gauge<DNCGauge>().IsDancing && ComboIs(ActionID.WindmillPvE)),
		Block("wrong-dance-step", "Block the wrong dance step",
			"Blocks any step action that doesn't match the next required step while dancing.",
			"that isn't the next dance step",
			[ActionID.EmboitePvE, ActionID.EntrechatPvE, ActionID.JetePvE, ActionID.PirouettePvE],
			context => Gauge<DNCGauge>().IsDancing && Gauge<DNCGauge>().NextStep != 0 && (uint)context.Action != Gauge<DNCGauge>().NextStep),
		Block("complete-dance", "Finish all dance steps first",
			"Blocks incomplete Standard Finish and Technical Finish presses until every step has been performed.",
			"your dance isn't finished yet",
			[
				ActionID.StandardFinishPvE, ActionID.SingleStandardFinishPvE,
				ActionID.TechnicalFinishPvE, ActionID.SingleTechnicalFinishPvE,
				ActionID.DoubleTechnicalFinishPvE, ActionID.TripleTechnicalFinishPvE,
			],
			_ => true),
		Block("keep-fan-procs", "Spend Fan Dance procs first",
			"Blocks Flourish while Threefold Fan Dance or Fourfold Fan Dance is still waiting to be spent.",
			"Fan Dance procs are still unspent", ActionID.FlourishPvE,
			_ => Self(StatusID.ThreefoldFanDance, StatusID.FourfoldFanDance)),
		MitigationOverlap(ActionID.ShieldSambaPvE, "Shield Samba"),
		PreserveCombo(when: _ => !Gauge<DNCGauge>().IsDancing),
		Block("keep-partner", "Keep your dance partner in combat",
			"Blocks Ending during combat, so a stray press doesn't drop your dance partner. Off by default, since changing partners needs it.",
			"your dance partner stays in combat", ActionID.EndingPvE, _ => InCombat, false),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
