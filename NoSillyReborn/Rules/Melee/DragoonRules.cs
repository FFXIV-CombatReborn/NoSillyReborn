namespace NoSillyReborn.Rules.Melee;

internal sealed class DragoonRules : JobRules
{
	public override Job Job => Job.DRG;

	public override Job BaseClass => Job.LNC;

	public override JobRole Role => JobRole.Melee;

	protected override GuardRule[] Define() =>
	[
		Replace("combo-spiral-blow", "Refresh Power Surge with Spiral Blow",
			"Turns off-step single-target weaponskills into Disembowel or Spiral Blow after True Thrust or Raiden Thrust when Power Surge has under 10 seconds left.",
			"Power Surge needs refreshing",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE, ActionID.DrakesbanePvE],
			ActionID.DisembowelPvE, ActionID.SpiralBlowPvE,
			_ => ComboIs(ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE) && ActionID.DisembowelPvE.IsUnlocked()
				&& StatusHelper.PlayerStatusTime(true, StatusID.PowerSurge, StatusID.PowerSurge_2720) < 10f),
		ComboStep("combo-lance-barrage", "Continue to Lance Barrage",
			"Turns off-step single-target weaponskills into Vorpal Thrust or Lance Barrage right after True Thrust or Raiden Thrust.",
			"True Thrust or Raiden Thrust was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE, ActionID.DrakesbanePvE],
			ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE,
			ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE),
		ComboStep("combo-heavens-thrust", "Continue to Heavens' Thrust",
			"Turns other single-target combo weaponskills into Full Thrust or Heavens' Thrust right after Vorpal Thrust or Lance Barrage.",
			"Vorpal Thrust or Lance Barrage was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE, ActionID.DisembowelPvE, ActionID.SpiralBlowPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE, ActionID.DrakesbanePvE],
			ActionID.FullThrustPvE, ActionID.HeavensThrustPvE,
			ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE),
		ComboStep("combo-chaotic-spring", "Continue to Chaotic Spring",
			"Turns other single-target combo weaponskills into Chaos Thrust or Chaotic Spring right after Disembowel or Spiral Blow.",
			"Disembowel or Spiral Blow was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE, ActionID.DisembowelPvE, ActionID.SpiralBlowPvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE, ActionID.DrakesbanePvE],
			ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE,
			ActionID.DisembowelPvE, ActionID.SpiralBlowPvE),
		ComboStep("combo-fang-and-claw", "Continue to Fang and Claw",
			"Turns other single-target combo weaponskills into Fang and Claw right after Full Thrust or Heavens' Thrust.",
			"Full Thrust or Heavens' Thrust was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.DisembowelPvE, ActionID.SpiralBlowPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.WheelingThrustPvE],
			ActionID.FangAndClawPvE, ActionID.FangAndClawPvE,
			ActionID.FullThrustPvE, ActionID.HeavensThrustPvE),
		ComboStep("combo-wheeling-thrust", "Continue to Wheeling Thrust",
			"Turns other single-target combo weaponskills into Wheeling Thrust right after Chaos Thrust or Chaotic Spring.",
			"Chaos Thrust or Chaotic Spring was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.DisembowelPvE, ActionID.SpiralBlowPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.FangAndClawPvE],
			ActionID.WheelingThrustPvE, ActionID.WheelingThrustPvE,
			ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE),
		Replace("combo-drakesbane", "Continue to Drakesbane",
			"Turns other single-target combo weaponskills into Drakesbane right after Fang and Claw or Wheeling Thrust.",
			"Fang and Claw or Wheeling Thrust was just used",
			[ActionID.TrueThrustPvE, ActionID.RaidenThrustPvE, ActionID.VorpalThrustPvE, ActionID.LanceBarragePvE, ActionID.DisembowelPvE, ActionID.SpiralBlowPvE, ActionID.FullThrustPvE, ActionID.HeavensThrustPvE, ActionID.ChaosThrustPvE, ActionID.ChaoticSpringPvE, ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE],
			ActionID.FangAndClawPvE, ActionID.DrakesbanePvE,
			_ => ComboIs(ActionID.FangAndClawPvE, ActionID.WheelingThrustPvE)
				&& ActionID.DrakesbanePvE.IsUnlocked()
				&& Service.GetAdjustedActionId((uint)ActionID.FangAndClawPvE) == (uint)ActionID.DrakesbanePvE),
		ComboStep("combo-sonic-thrust", "Continue to Sonic Thrust",
			"Turns Doom Spike, Draconian Fury, or Coerthan Torment into Sonic Thrust right after Doom Spike or Draconian Fury.",
			"Doom Spike or Draconian Fury was just used",
			[ActionID.DoomSpikePvE, ActionID.DraconianFuryPvE, ActionID.CoerthanTormentPvE],
			ActionID.SonicThrustPvE, ActionID.SonicThrustPvE,
			ActionID.DoomSpikePvE, ActionID.DraconianFuryPvE),
		ComboStep("combo-coerthan-torment", "Continue to Coerthan Torment",
			"Turns Doom Spike, Draconian Fury, or Sonic Thrust into Coerthan Torment right after Sonic Thrust.",
			"Sonic Thrust was just used",
			[ActionID.DoomSpikePvE, ActionID.DraconianFuryPvE, ActionID.SonicThrustPvE],
			ActionID.CoerthanTormentPvE, ActionID.CoerthanTormentPvE,
			ActionID.SonicThrustPvE),
		KeepCharge(ActionID.LifeSurgePvE, "Life Surge", [StatusID.LifeSurge]),
		PreserveCombo(),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
