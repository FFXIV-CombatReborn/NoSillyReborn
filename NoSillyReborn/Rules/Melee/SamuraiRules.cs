using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Melee;

internal sealed class SamuraiRules : JobRules
{
	public override Job Job => Job.SAM;

	public override JobRole Role => JobRole.Melee;

	protected override GuardRule[] Define() =>
	[
		Replace("combo-jinpu", "Continue to Jinpu",
			"Turns off-step single-target weaponskills into Jinpu after Hakaze or Gyofu when Fugetsu or Getsu needs refreshing.",
			"Hakaze or Gyofu was just used",
			[ActionID.HakazePvE, ActionID.GyofuPvE, ActionID.GekkoPvE, ActionID.KashaPvE],
			ActionID.JinpuPvE, ActionID.JinpuPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.HakazePvE, ActionID.GyofuPvE)
				&& (!ActionID.ShifuPvE.IsUnlocked() || ShouldPreferFugetsu())),
		Replace("combo-shifu", "Continue to Shifu",
			"Turns off-step single-target weaponskills into Shifu after Hakaze or Gyofu when Fuka needs refreshing.",
			"Hakaze or Gyofu was just used",
			[ActionID.HakazePvE, ActionID.GyofuPvE, ActionID.GekkoPvE, ActionID.KashaPvE],
			ActionID.ShifuPvE, ActionID.ShifuPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.HakazePvE, ActionID.GyofuPvE)),
		Replace("combo-gekko", "Continue to Gekko",
			"Turns other single-target combo weaponskills into Gekko right after Jinpu.",
			"Jinpu was just used",
			[ActionID.HakazePvE, ActionID.GyofuPvE, ActionID.JinpuPvE, ActionID.ShifuPvE, ActionID.YukikazePvE, ActionID.KashaPvE],
			ActionID.GekkoPvE, ActionID.GekkoPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.JinpuPvE)),
		Replace("combo-kasha", "Continue to Kasha",
			"Turns other single-target combo weaponskills into Kasha right after Shifu.",
			"Shifu was just used",
			[ActionID.HakazePvE, ActionID.GyofuPvE, ActionID.JinpuPvE, ActionID.ShifuPvE, ActionID.YukikazePvE, ActionID.GekkoPvE],
			ActionID.KashaPvE, ActionID.KashaPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.ShifuPvE)),
		Replace("combo-mangetsu", "Continue to Mangetsu",
			"Turns Fuga or Fuko into Mangetsu right after Fuga or Fuko when Fugetsu or Getsu needs refreshing.",
			"Fuga or Fuko was just used",
			[ActionID.FugaPvE, ActionID.FukoPvE],
			ActionID.MangetsuPvE, ActionID.MangetsuPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.FugaPvE, ActionID.FukoPvE)
				&& (!ActionID.OkaPvE.IsUnlocked() || ShouldPreferFugetsu())),
		Replace("combo-oka", "Continue to Oka",
			"Turns Fuga or Fuko into Oka right after Fuga or Fuko when Fuka or Ka needs refreshing.",
			"Fuga or Fuko was just used",
			[ActionID.FugaPvE, ActionID.FukoPvE],
			ActionID.OkaPvE, ActionID.OkaPvE,
			_ => !Self(StatusID.MeikyoShisui) && ComboIs(ActionID.FugaPvE, ActionID.FukoPvE)),
		KeepCharge(ActionID.MeikyoShisuiPvE, "Meikyo Shisui", [StatusID.MeikyoShisui]),
		Block("kenki-overcap", "Spend Kenki first",
			"Blocks Ikishoten above 50 Kenki, since the 50 Kenki it grants would overcap.",
			"Kenki Gauge would overcap", ActionID.IkishotenPvE,
			_ => Gauge<SAMGauge>().Kenki > 50),
		PreserveCombo(when: _ => !Self(StatusID.MeikyoShisui)),
	];

	private static bool ShouldPreferFugetsu()
	{
		if (!Self(StatusID.Fugetsu))
		{
			return true;
		}

		if (!Self(StatusID.Fuka))
		{
			return false;
		}

		var gauge = Gauge<SAMGauge>();
		if (gauge.HasGetsu != gauge.HasKa)
		{
			return !gauge.HasGetsu;
		}

		return StatusHelper.PlayerStatusTime(true, StatusID.Fugetsu) <= StatusHelper.PlayerStatusTime(true, StatusID.Fuka);
	}

	protected override GuardRule[] DefineEvolved() => [];
}
