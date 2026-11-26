using Dalamud.Game.ClientState.JobGauge.Types;

namespace NoSillyReborn.Rules.Magical;

internal sealed class PictomancerRules : JobRules
{
	public override Job Job => Job.PCT;

	public override JobRole Role => JobRole.RangedMagical;

	protected override GuardRule[] Define() =>
	[
		Block("keep-striking-muse", "Spend Hammer Time first",
			"Blocks Steel Muse and Striking Muse while Hammer Time is still active, so a double press doesn't waste stacks or burn both charges.",
			"Hammer Time is still active", [ActionID.SteelMusePvE, ActionID.StrikingMusePvE],
			_ => Self(StatusID.HammerTime) || JustUsed(ActionID.StrikingMusePvE)),
		Block("keep-portrait", "Spend your Portrait first",
			"Blocks Winged Muse and Fanged Muse while Moogle or Madeen Portrait is already ready to cast.",
			"a Portrait is waiting to be cast", [ActionID.WingedMusePvE, ActionID.FangedMusePvE],
			_ => Gauge<PCTGauge>().MooglePortraitReady || Gauge<PCTGauge>().MadeenPortraitReady),
		Block("keep-subtractive", "Don't double Subtractive Palette",
			"Blocks Subtractive Palette while Subtractive Palette stacks are still active.",
			"Subtractive Palette is already active", ActionID.SubtractivePalettePvE,
			_ => Self(StatusID.SubtractivePalette, StatusID.SubtractivePalette_4102) || JustUsed(ActionID.SubtractivePalettePvE)),
	];

	protected override GuardRule[] DefineEvolved() => [];
}
