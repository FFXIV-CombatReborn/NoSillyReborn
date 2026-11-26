using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Magical;

internal sealed class RedMageRules : JobRules
{
	public override Job Job => Job.RDM;

	public override JobRole Role => JobRole.RangedMagical;

	protected override GuardRule[] Define() =>
	[
		Replace("combo-zwerchhau", "Continue to Enchanted Zwerchhau",
			"Turns Riposte or Redoublement into Enchanted Zwerchhau right after Enchanted Riposte.",
			"Enchanted Riposte was just used",
			[
				ActionID.RipostePvE, ActionID.EnchantedRipostePvE, ActionID.EnchantedRipostePvE_45960,
				ActionID.RedoublementPvE, ActionID.EnchantedRedoublementPvE, ActionID.EnchantedRedoublementPvE_45962,
			],
			ActionID.ZwerchhauPvE, ActionID.EnchantedZwerchhauPvE,
			_ => ComboIs(ActionID.RipostePvE, ActionID.EnchantedRipostePvE, ActionID.EnchantedRipostePvE_45960) && HasEnchantedMana()),
		Replace("combo-redoublement", "Continue to Enchanted Redoublement",
			"Turns Riposte or Zwerchhau into Enchanted Redoublement right after Enchanted Zwerchhau.",
			"Enchanted Zwerchhau was just used",
			[
				ActionID.RipostePvE, ActionID.EnchantedRipostePvE, ActionID.EnchantedRipostePvE_45960,
				ActionID.ZwerchhauPvE, ActionID.EnchantedZwerchhauPvE, ActionID.EnchantedZwerchhauPvE_45961,
			],
			ActionID.RedoublementPvE, ActionID.EnchantedRedoublementPvE,
			_ => ComboIs(ActionID.ZwerchhauPvE, ActionID.EnchantedZwerchhauPvE, ActionID.EnchantedZwerchhauPvE_45961) && HasEnchantedMana()),
		Replace("dualcast-verthunder", "Use Verthunder III on Dualcast",
			"Turns Jolt, Verfire, and Verstone into Verthunder or Verthunder III while Dualcast, Acceleration, or Swiftcast is up and Black Mana is at or below White Mana.",
			"Dualcast, Acceleration, or Swiftcast is up",
			[ActionID.JoltPvE, ActionID.JoltIiPvE, ActionID.JoltIiiPvE, ActionID.VerfirePvE, ActionID.VerstonePvE],
			ActionID.VerthunderPvE, ActionID.VerthunderIiiPvE,
			_ => HasInstantCast() && !Self(StatusID.GrandImpactReady) && Gauge<RDMGauge>().ManaStacks < 3
				&& (!ActionID.VeraeroPvE.IsUnlocked() || ShouldPreferVerthunder())),
		Replace("dualcast-veraero", "Use Veraero III on Dualcast",
			"Turns Jolt, Verfire, and Verstone into Veraero or Veraero III while Dualcast, Acceleration, or Swiftcast is up.",
			"Dualcast, Acceleration, or Swiftcast is up",
			[ActionID.JoltPvE, ActionID.JoltIiPvE, ActionID.JoltIiiPvE, ActionID.VerfirePvE, ActionID.VerstonePvE],
			ActionID.VeraeroPvE, ActionID.VeraeroIiiPvE,
			_ => HasInstantCast() && !Self(StatusID.GrandImpactReady) && Gauge<RDMGauge>().ManaStacks < 3),
		Block("block-unenchanted-melee", "Don't use unenchanted melee actions",
			"Blocks unenchanted Riposte, Zwerchhau, Redoublement, Moulinet, and Reprise from level 2 onward, so insufficient Balance Gauge doesn't spend a weak physical GCD.",
			"you don't have enough Balance Gauge for an enchanted strike",
			[ActionID.ZwerchhauPvE, ActionID.RedoublementPvE, ActionID.MoulinetPvE, ActionID.ReprisePvE, ActionID.RipostePvE],
			_ => ActionID.JoltPvE.IsUnlocked()),
		KeepCharge(ActionID.AccelerationPvE, "Acceleration", [StatusID.Acceleration, StatusID.GrandImpactReady],
			"Blocks Acceleration while Acceleration or Grand Impact Ready is still up, so a double press doesn't burn both charges."),
		DoubleRaise(ActionID.VerraisePvE, "Verraise"),
		PreserveCombo("Blocks spells and off-step weaponskills while a melee or finisher combo is active, so a stray press doesn't break your chain. Off by default.",
			_ => !IsEnchantedMeleeCombo() || HasEnchantedMana()),
	];

	private static bool HasInstantCast()
	{
		return Self(StatusID.Dualcast, StatusID.Dualcast_1393, StatusID.Acceleration, StatusID.Swiftcast);
	}

	private static bool ShouldPreferVerthunder()
	{
		var gauge = Gauge<RDMGauge>();
		return gauge.BlackMana < gauge.WhiteMana
			|| (gauge.BlackMana == gauge.WhiteMana && (!Self(StatusID.VerfireReady) || Self(StatusID.VerstoneReady)));
	}

	private static bool IsEnchantedMeleeCombo()
	{
		return ComboIs(
			ActionID.RipostePvE, ActionID.EnchantedRipostePvE, ActionID.EnchantedRipostePvE_45960,
			ActionID.ZwerchhauPvE, ActionID.EnchantedZwerchhauPvE, ActionID.EnchantedZwerchhauPvE_45961,
			ActionID.MoulinetPvE, ActionID.EnchantedMoulinetPvE, ActionID.EnchantedMoulinetDeuxPvE);
	}

	private static bool HasEnchantedMana()
	{
		if (Self(StatusID.MagickedSwordplay))
		{
			return true;
		}

		var gauge = Gauge<RDMGauge>();
		return gauge.WhiteMana >= 15 && gauge.BlackMana >= 15;
	}

	protected override GuardRule[] DefineEvolved() => [];
}
