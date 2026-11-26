using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace NoSillyReborn.Rules.Melee;

internal sealed class NinjaRules : JobRules
{
	private const byte TenBit = 1;
	private const byte ChiBit = 2;
	private const byte JinBit = 4;
	private const long MudraDurationMs = 8000;
	private const long MudraPendingMs = 1500;

	private static byte _mudraMask;
	private static long _lastMudraTick;

	public override Job Job => Job.NIN;

	public override Job BaseClass => Job.ROG;

	public override JobRole Role => JobRole.Melee;

	internal static void Reset()
	{
		_mudraMask = 0;
		_lastMudraTick = 0;
	}

	internal static void RecordExecutedAction(ActionID action)
	{
		if (action is ActionID.NinjutsuPvE
			or ActionID.FumaShurikenPvE or ActionID.FumaShurikenPvE_18873 or ActionID.FumaShurikenPvE_18874 or ActionID.FumaShurikenPvE_18875 or ActionID.FumaShurikenPvE_49062
			or ActionID.KatonPvE or ActionID.KatonPvE_18876
			or ActionID.RaitonPvE or ActionID.RaitonPvE_18877
			or ActionID.HyotonPvE or ActionID.HyotonPvE_18878
			or ActionID.HutonPvE or ActionID.HutonPvE_18879
			or ActionID.DotonPvE or ActionID.DotonPvE_18880
			or ActionID.SuitonPvE or ActionID.SuitonPvE_18881
			or ActionID.GokaMekkyakuPvE
			or ActionID.HyoshoRanryuPvE
			or ActionID.RabbitMediumPvE)
		{
			Reset();
			return;
		}

		if (Self(StatusID.TenChiJin))
		{
			return;
		}

		var bit = MudraBit(action);
		if (bit == 0)
		{
			return;
		}

		var now = Environment.TickCount64;
		if (!Self(StatusID.Mudra) && now - _lastMudraTick > MudraPendingMs)
		{
			_mudraMask = bit;
		}
		else
		{
			_mudraMask |= bit;
		}

		_lastMudraTick = now;
	}

	private static byte MudraBit(ActionID action) => action switch
	{
		ActionID.TenPvE or ActionID.TenPvE_18805 => TenBit,
		ActionID.ChiPvE or ActionID.ChiPvE_18806 => ChiBit,
		ActionID.JinPvE or ActionID.JinPvE_18807 => JinBit,
		_ => 0,
	};

	protected override GuardRule[] Define() =>
	[
		Block("prevent-rabbit-medium", "Prevent Rabbit Medium",
			"Blocks a Mudra input that would repeat a gesture or add a fourth Mudra and summon Rabbit Medium.",
			"that mudra would summon Rabbit Medium",
			[ActionID.TenPvE, ActionID.ChiPvE, ActionID.JinPvE, ActionID.TenPvE_18805, ActionID.ChiPvE_18806, ActionID.JinPvE_18807],
			context => WouldBunnyMudra(context.Action)),
		Block("protect-mudra", "Don't break Mudra mid-sequence",
			"Blocks weaponskills and Kassatsu while Mudra is active so you don't cancel your Ninjutsu before casting it.",
			"a Mudra sequence is active",
			[
				ActionID.SpinningEdgePvE, ActionID.GustSlashPvE, ActionID.AeolianEdgePvE, ActionID.ArmorCrushPvE,
				ActionID.DeathBlossomPvE, ActionID.HakkeMujinsatsuPvE, ActionID.ThrowingDaggerPvE,
				ActionID.PhantomKamaitachiPvE, ActionID.ForkedRaijuPvE, ActionID.FleetingRaijuPvE, ActionID.KassatsuPvE,
			],
			_ => Self(StatusID.Mudra)),
		Replace("keep-raiju", "Use Raiju Ready before combo weaponskills",
			"Turns single-target combo weaponskills into Fleeting Raiju while Raiju Ready is active, so a melee combo press doesn't waste your Raiju stacks.",
			"Raiju Ready is active",
			[ActionID.SpinningEdgePvE, ActionID.GustSlashPvE, ActionID.ArmorCrushPvE, ActionID.AeolianEdgePvE],
			ActionID.FleetingRaijuPvE, ActionID.FleetingRaijuPvE,
			_ => Self(StatusID.RaijuReady) && !Self(StatusID.Mudra)),
		ComboStep("combo-gust-slash", "Continue to Gust Slash",
			"Turns Spinning Edge, Aeolian Edge, or Armor Crush into Gust Slash right after Spinning Edge.",
			"Spinning Edge was just used", [ActionID.SpinningEdgePvE, ActionID.AeolianEdgePvE, ActionID.ArmorCrushPvE],
			ActionID.GustSlashPvE, ActionID.GustSlashPvE, ActionID.SpinningEdgePvE),
		Replace("combo-armor-crush", "Build Kazematoi with Armor Crush",
			"Turns Spinning Edge or Gust Slash into Armor Crush after Gust Slash when you have 0 Kazematoi stacks.",
			"Kazematoi is empty", [ActionID.SpinningEdgePvE, ActionID.GustSlashPvE],
			ActionID.ArmorCrushPvE, ActionID.ArmorCrushPvE,
			_ => ComboIs(ActionID.GustSlashPvE) && ActionID.ArmorCrushPvE.IsUnlocked() && Gauge<NINGauge>().Kazematoi == 0),
		Replace("combo-aeolian-edge", "Continue to Aeolian Edge",
			"Turns Spinning Edge or Gust Slash into Aeolian Edge after Gust Slash, or Armor Crush when Kazematoi would overcap (4 or more stacks).",
			"Gust Slash was just used", [ActionID.SpinningEdgePvE, ActionID.GustSlashPvE, ActionID.ArmorCrushPvE],
			ActionID.AeolianEdgePvE, ActionID.AeolianEdgePvE,
			context => ComboIs(ActionID.GustSlashPvE) && (context.Action != ActionID.ArmorCrushPvE || Gauge<NINGauge>().Kazematoi >= 4)),
		ComboStep("combo-hakke-mujinsatsu", "Continue to Hakke Mujinsatsu",
			"Turns Death Blossom into Hakke Mujinsatsu right after Death Blossom.",
			"Death Blossom was just used", [ActionID.DeathBlossomPvE],
			ActionID.HakkeMujinsatsuPvE, ActionID.HakkeMujinsatsuPvE, ActionID.DeathBlossomPvE),
		Block("ninki-overcap", "Spend Ninki first",
			"Blocks Meisui above 50 Ninki, since the 50 Ninki it grants would overcap.",
			"Ninki Gauge would overcap", ActionID.MeisuiPvE,
			_ => Gauge<NINGauge>().Ninki > 50),
		PreserveCombo(),
	];

	private static unsafe bool WouldBunnyMudra(ActionID action)
	{
		if (!Self(StatusID.Mudra) || Self(StatusID.TenChiJin))
		{
			return false;
		}

		var bit = MudraBit(action);
		if (bit != 0 && _mudraMask != 0 && Environment.TickCount64 - _lastMudraTick <= MudraDurationMs && (_mudraMask & bit) != 0)
		{
			return true;
		}

		var actionManager = ActionManager.Instance();
		if (actionManager == null)
		{
			return false;
		}

		var ninjutsu = (ActionID)actionManager->GetAdjustedActionId((uint)ActionID.NinjutsuPvE);
		if (ninjutsu is ActionID.HutonPvE or ActionID.DotonPvE or ActionID.SuitonPvE or ActionID.RabbitMediumPvE)
		{
			return true;
		}

		return action switch
		{
			ActionID.TenPvE or ActionID.TenPvE_18805 => ninjutsu is ActionID.KatonPvE or ActionID.GokaMekkyakuPvE
				|| (ninjutsu == ActionID.FumaShurikenPvE && (JustUsed(ActionID.TenPvE) || JustUsed(ActionID.TenPvE_18805))),
			ActionID.ChiPvE or ActionID.ChiPvE_18806 => ninjutsu == ActionID.RaitonPvE
				|| (ninjutsu == ActionID.FumaShurikenPvE && (JustUsed(ActionID.ChiPvE) || JustUsed(ActionID.ChiPvE_18806))),
			ActionID.JinPvE or ActionID.JinPvE_18807 => ninjutsu is ActionID.HyotonPvE or ActionID.HyoshoRanryuPvE
				|| (ninjutsu == ActionID.FumaShurikenPvE && (JustUsed(ActionID.JinPvE) || JustUsed(ActionID.JinPvE_18807))),
			_ => false,
		};
	}

	protected override GuardRule[] DefineEvolved() => [];
}
